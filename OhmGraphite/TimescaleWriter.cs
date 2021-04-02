using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NLog;
using Npgsql;
using NpgsqlTypes;

namespace OhmGraphite
{
    public class TimescaleWriter : IWriteMetrics
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly string _connStr;
        private readonly string _localHost;
        private readonly bool _setupTable;
        private bool _failure = true;

        public TimescaleWriter(string connStr, bool setupTable, string localHost)
        {
            _connStr = connStr;
            _localHost = localHost;
            _setupTable = setupTable;
        }

        public Task ReportMetrics(DateTime reportTime, IEnumerable<ReportedValue> sensors)
        {
            try
            {
                if (_failure)
                {
                    Logger.Debug("Clearing connection pool");
                    NpgsqlConnection.ClearPool(new NpgsqlConnection(_connStr));
                }

                using (var conn = new NpgsqlConnection(_connStr))
                {
                    conn.Open();
                    if (_failure)
                    {
                        // The reason behind unpreparing is a doozy.
                        //
                        // Npgsql persists prepared statements across connections, reason: "This allows
                        // you to benefit from statements prepared in previous lifetimes, providing all
                        // the performance benefits to applications using connection pools" -
                        // http://www.roji.org/prepared-statements-in-npgsql-3-2. I have found this to
                        // be the correct behavior in 99% situations when either client or server is
                        // restarted, as the normal flow of exceptions reported on the client when the
                        // server restarts seems to be:
                        //
                        // - System.IO.EndOfStreamException: Attempted to read past the end of the stream
                        // - 57P03: the database system is starting up
                        // - Back to normal
                        //
                        // However, on 2018-11-29 while upgrading timescale db (0.12.1 to 1.0.0) I
                        // encountered a bizarre sequence of events
                        //
                        // - <start upgrade by restarting server>
                        // - System.IO.EndOfStreamException: Attempted to read past the end of the stream
                        // - 57P03: the database system is starting up
                        // - 58P01: could not access file "timescaledb-0.12.1": No such file or directory
                        // - <finished with: "ALTER EXTENSION timescaledb UPDATE;">
                        // - 26000: prepared statement "_p1" does not exist
                        //
                        // OhmGraphite could never recover because Npgsql seemed adamant that the
                        // prepared statement existed. And since Npgsql persists prepared statements in
                        // it's connection pool all future connections are "poisoned" with this
                        // prepared statement. The best solution appears to be unpreparing everything on
                        // db failure. For our use case, recreating these prepared statements is a small
                        // price to pay even if preparation is redundant.
                        conn.UnprepareAll();

                        if (_setupTable)
                        {
                            var assembly = Assembly.GetExecutingAssembly();
                            var path = assembly.GetManifestResourceNames()
                                .Single(str => str.EndsWith("schema.sql"));

                            using (var stream = assembly.GetManifestResourceStream(path))
                            using (var reader = new StreamReader(stream))
                            {
                                var setupSql = reader.ReadToEnd();
                                using (var cmd = new NpgsqlCommand(setupSql, conn))
                                {
                                    cmd.ExecuteNonQuery();

                                }
                            }
                        }
                    }

                    var values = sensors.ToList();
                    using (var cmd = new NpgsqlCommand(BatchedInsertSql(values), conn))
                    {
                        // Note that all parameters must be set before calling Prepare()
                        // they are part of the information transmitted to PostgreSQL
                        // and used to effectively plan the statement. You must also set
                        // the DbType or NpgsqlDbType on your parameters to unambiguously
                        // specify the data type (setting the value isn't support)
                        for (int i = 0; i < values.Count; i++)
                        {
                            cmd.Parameters.Add($"time{i}", NpgsqlDbType.TimestampTz);
                            cmd.Parameters.Add($"host{i}", NpgsqlDbType.Text);
                            cmd.Parameters.Add($"hardware{i}", NpgsqlDbType.Text);
                            cmd.Parameters.Add($"hardware_type{i}", NpgsqlDbType.Text);
                            cmd.Parameters.Add($"identifier{i}", NpgsqlDbType.Text);
                            cmd.Parameters.Add($"sensor{i}", NpgsqlDbType.Text);
                            cmd.Parameters.Add($"sensor_type{i}", NpgsqlDbType.Text);
                            cmd.Parameters.Add($"value{i}", NpgsqlDbType.Real);
                            cmd.Parameters.Add($"sensor_index{i}", NpgsqlDbType.Integer);
                        }

                        // A majority of the time, the same number of sensors will be
                        // reported on, so it's important to prepare the statement
                        cmd.Prepare();

                        for (int i = 0; i < values.Count; i++)
                        {
                            var sensor = values[i];
                            cmd.Parameters[$"time{i}"].Value = reportTime;
                            cmd.Parameters[$"host{i}"].Value = _localHost;
                            cmd.Parameters[$"hardware{i}"].Value = sensor.Hardware;
                            cmd.Parameters[$"hardware_type{i}"].Value = Enum.GetName(typeof(HardwareType), sensor.HardwareType);
                            cmd.Parameters[$"identifier{i}"].Value = sensor.Identifier;
                            cmd.Parameters[$"sensor{i}"].Value = sensor.Sensor;
                            cmd.Parameters[$"sensor_type{i}"].Value = Enum.GetName(typeof(SensorType), sensor.SensorType);
                            cmd.Parameters[$"value{i}"].Value = sensor.Value;
                            cmd.Parameters[$"sensor_index{i}"].Value = sensor.SensorIndex;
                        }

                        cmd.ExecuteNonQuery();
                    }

                    _failure = false;

                    // The synchronous versions of npgsql are more battle tested than asynchronous:
                    // https://github.com/npgsql/npgsql/issues/2266
                    return Task.CompletedTask;
                }
            }
            catch (Exception)
            {
                _failure = true;
                throw;
            }
        }

        // Returns a SQL INSERT statement that will insert all the reported values in one go.
        // Since there is no batched insert API that is part of Npgsql, we simulate one by
        // creating a unique set of sql parameters for each reported value by it's index.
        // Sending one insert of 70 values was nearly 10x faster than 70 inserts of 1 value,
        // so this circumnavigation around a lack of native batched insert statements is
        // worth it.
        private static string BatchedInsertSql(IEnumerable<ReportedValue> values)
        {
            var sqlColumns = values.Select((x, i) =>
                $"(@time{i}, @host{i}, @hardware{i}, @hardware_type{i}, @identifier{i}, @sensor{i}, @sensor_type{i}, @sensor_index{i}, @value{i})");
            var columns = string.Join(", ", sqlColumns);
            return "INSERT INTO ohm_stats " +
                   "(time, host, hardware, hardware_type, identifier, sensor, sensor_type, sensor_index, value) VALUES " +
                   columns;
        }

        public void Dispose()
        {
            NpgsqlConnection.ClearPool(new NpgsqlConnection(_connStr));
        }
    }
}
