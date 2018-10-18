using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using Npgsql;
using NpgsqlTypes;
using OpenHardwareMonitor.Hardware;

namespace OhmGraphite
{
    public class TimescaleWriter : IWriteMetrics
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly string _connStr;
        private readonly string _localHost;
        private readonly bool _setupTable;
        private NpgsqlConnection _conn;
        private bool _failure = true;

        public TimescaleWriter(string connStr, bool setupTable, string localHost)
        {
            _connStr = connStr;
            _localHost = localHost;
            _setupTable = setupTable;
            _conn = new NpgsqlConnection(_connStr);
        }

        public async Task ReportMetrics(DateTime reportTime, IEnumerable<ReportedValue> sensors)
        {
            try
            {
                if (_failure)
                {
                    _conn.Close();
                    _conn = new NpgsqlConnection(_connStr);
                    Logger.Debug("New timescale connection");
                    await _conn.OpenAsync();

                    if (_setupTable)
                    {
                        using (var cmd = new NpgsqlCommand(
                            "CREATE TABLE IF NOT EXISTS ohm_stats (" +
                            "   time TIMESTAMPTZ NOT NULL," +
                            "   host TEXT," +
                            "   hardware TEXT," +
                            "   hardware_type TEXT," +
                            "   identifier TEXT," +
                            "   sensor TEXT," +
                            "   sensor_type TEXT," +
                            "   sensor_index INT," +
                            "   value REAL" +
                            ");" +
                            "" +
                            @"SELECT create_hypertable('ohm_stats', 'time', if_not_exists => TRUE);" +
                            "CREATE INDEX IF NOT EXISTS idx_ohm_host ON ohm_stats (host);" +
                            "CREATE INDEX IF NOT EXISTS idx_ohm_identifier ON ohm_stats (identifier);",
                            _conn))
                        {
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }

                var values = sensors.ToList();
                using (var cmd = new NpgsqlCommand(BatchedInsertSql(values), _conn))
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
                    await cmd.PrepareAsync();

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

                    await cmd.ExecuteNonQueryAsync();
                }

                _failure = false;
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
    }
}
