using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NLog;
using Npgsql;
using NpgsqlTypes;
using OpenHardwareMonitor.Hardware;

namespace OhmGraphite
{
    class TimescaleWriter : IWriteMetrics
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly string _connStr;
        private readonly string _localHost;
        private NpgsqlConnection _conn;
        private bool _failure = true;

        public TimescaleWriter(string connStr, string localHost)
        {
            _connStr = connStr;
            _localHost = localHost;
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

                    using (var cmd = new NpgsqlCommand(
                        "CREATE TABLE IF NOT EXISTS ohm_stats (" +
                        "   time TIMESTAMPTZ NOT NULL," +
                        "   host TEXT,hardware TEXT," +
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

                using (var cmd = new NpgsqlCommand(
                    "INSERT INTO ohm_stats " +
                    "(time, host, hardware, hardware_type, identifier, sensor, sensor_type, sensor_index, value) VALUES " +
                    "(@time, @host, @hardware, @hardware_type, @identifier, @sensor, @sensor_type, @sensor_index, @value)",
                    _conn))
                {
                    // Note that all parameters must be set before calling Prepare()
                    // they are part of the information transmitted to PostgreSQL
                    // and used to effectively plan the statement. You must also set
                    // the DbType or NpgsqlDbType on your parameters to unambiguously
                    // specify the data type (setting the value isn't support)
                    cmd.Parameters.Add("time", NpgsqlDbType.TimestampTz);
                    cmd.Parameters.Add("host", NpgsqlDbType.Text);
                    cmd.Parameters.Add("hardware", NpgsqlDbType.Text);
                    cmd.Parameters.Add("hardware_type", NpgsqlDbType.Text);
                    cmd.Parameters.Add("identifier", NpgsqlDbType.Text);
                    cmd.Parameters.Add("sensor", NpgsqlDbType.Text);
                    cmd.Parameters.Add("sensor_type", NpgsqlDbType.Text);
                    cmd.Parameters.Add("value", NpgsqlDbType.Real);
                    cmd.Parameters.Add("sensor_index", NpgsqlDbType.Integer);

                    await cmd.PrepareAsync();

                    foreach (var sensor in sensors)
                    {
                        cmd.Parameters["time"].Value = reportTime;
                        cmd.Parameters["host"].Value = _localHost;
                        cmd.Parameters["hardware"].Value = sensor.Hardware;
                        cmd.Parameters["hardware_type"].Value = Enum.GetName(typeof(HardwareType), sensor.HardwareType);
                        cmd.Parameters["identifier"].Value = sensor.Identifier;
                        cmd.Parameters["sensor"].Value = sensor.Sensor;
                        cmd.Parameters["sensor_type"].Value = Enum.GetName(typeof(SensorType), sensor.SensorType);
                        cmd.Parameters["value"].Value = sensor.Value;
                        cmd.Parameters["sensor_index"].Value = sensor.SensorIndex;

                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                _failure = false;
            }
            catch (Exception)
            {
                _failure = true;
                throw;
            }
        }
    }
}
