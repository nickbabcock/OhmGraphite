using System;
using System.Collections.Generic;
using System.Linq;
using InfluxDB.LineProtocol;
using InfluxDB.LineProtocol.Client;
using InfluxDB.LineProtocol.Payload;
using NLog;
using OpenHardwareMonitor.Hardware;

namespace OhmGraphite
{
    class InfluxWriter : IWriteMetrics
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly InfluxConfig _config;
        private readonly string _localHost;

        public InfluxWriter(InfluxConfig config, string localHost)
        {
            _config = config;
            _localHost = localHost;
        }

        public void ReportMetrics(DateTime reportTime, IEnumerable<ReportedValue> sensors)
        {
            var payload = new LineProtocolPayload();
            var client = new LineProtocolClient(_config.Address, _config.Db, _config.User, _config.Password);
            var writer = new LineProtocolWriter(Precision.Seconds);
            foreach (var point in sensors.Select(x => NewMethod(reportTime, x)))
            {
                payload.Add(point);
            }

            var task = client.SendAsync(writer);
            var result = task.GetAwaiter().GetResult();
            if (!result.Success)
            {
                Logger.Error("Influxdb encountered an error: {0}", result.ErrorMessage);
            }
        }

        private LineProtocolPoint NewMethod(DateTime reportTime, ReportedValue sensor)
        {
            var tags = new Dictionary<string, string>()
            {
                {"host", _localHost},
                {"app", "ohm"},
                {"hardware", sensor.Hardware},
                {"hardware_type", Enum.GetName(typeof(HardwareType), sensor.HardwareType)},
                {"sensor", sensor.Sensor},
                {"sensor_type", Enum.GetName(typeof(SensorType), sensor.SensorType)}
            };

            var fields = new Dictionary<string, object>()
            {
                {"value", sensor.Value},
                {"sensor_index", sensor.SensorIndex}
            };

            return new LineProtocolPoint(sensor.Identifier, fields, tags, reportTime.ToUniversalTime());
        }
    }
}
