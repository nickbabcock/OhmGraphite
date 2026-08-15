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

        public Task ReportMetrics(IEnumerable<MetricReport> reports)
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

                    // Every report becomes its own statement, but they are all sent in a
                    // single batch, so the whole flush is one round trip and one commit.
                    using (var transaction = conn.BeginTransaction())
                    using (var batch = new NpgsqlBatch(conn, transaction))
                    {
                        foreach (var report in reports)
                        {
                            batch.BatchCommands.Add(InsertCommand(report));
                        }

                        // A majority of the time, the same number of sensors will be
                        // reported on, so it's important to prepare the statements
                        batch.Prepare();
                        batch.ExecuteNonQuery();
                        transaction.Commit();
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

        // Returns a statement that inserts every sensor of a report in one go.
        private NpgsqlBatchCommand InsertCommand(MetricReport report)
        {
            var sensors = report.Sensors;
            var reportTime = report.ReportTime.ToUniversalTime();

            var sqlColumns = sensors.Select((x, i) =>
                $"(@time{i}, @host{i}, @hardware{i}, @hardware_type{i}, @identifier{i}, @sensor{i}, @sensor_type{i}, @sensor_index{i}, @value{i})");
            var columns = string.Join(", ", sqlColumns);
            var cmd = new NpgsqlBatchCommand("INSERT INTO ohm_stats " +
                   "(time, host, hardware, hardware_type, identifier, sensor, sensor_type, sensor_index, value) VALUES " +
                   columns);

            // You must set the DbType or NpgsqlDbType on the parameters to unambiguously
            // specify the data type, as the type is part of the information transmitted to
            // PostgreSQL and used to effectively plan the statement.
            for (int i = 0; i < sensors.Count; i++)
            {
                var sensor = sensors[i];
                cmd.Parameters.Add(Param($"time{i}", NpgsqlDbType.TimestampTz, reportTime));
                cmd.Parameters.Add(Param($"host{i}", NpgsqlDbType.Text, _localHost));
                cmd.Parameters.Add(Param($"hardware{i}", NpgsqlDbType.Text, sensor.Hardware));
                cmd.Parameters.Add(Param($"hardware_type{i}", NpgsqlDbType.Text, Enum.GetName(typeof(HardwareType), sensor.HardwareType)));
                cmd.Parameters.Add(Param($"identifier{i}", NpgsqlDbType.Text, sensor.Identifier));
                cmd.Parameters.Add(Param($"sensor{i}", NpgsqlDbType.Text, sensor.Sensor));
                cmd.Parameters.Add(Param($"sensor_type{i}", NpgsqlDbType.Text, Enum.GetName(typeof(SensorType), sensor.SensorType)));
                cmd.Parameters.Add(Param($"sensor_index{i}", NpgsqlDbType.Integer, sensor.SensorIndex));
                cmd.Parameters.Add(Param($"value{i}", NpgsqlDbType.Real, sensor.Value));
            }

            return cmd;
        }

        private static NpgsqlParameter Param(string name, NpgsqlDbType type, object value) =>
            new NpgsqlParameter(name, type) { Value = value };

        public void Dispose()
        {
            NpgsqlConnection.ClearPool(new NpgsqlConnection(_connStr));
        }
    }
}
