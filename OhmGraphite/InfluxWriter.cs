using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfluxDB.LineProtocol.Client;
using InfluxDB.LineProtocol.Payload;
using NLog;

namespace OhmGraphite
{
    public class InfluxWriter : IWriteMetrics
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly InfluxConfig _config;
        private readonly string _localHost;

        public InfluxWriter(InfluxConfig config, string localHost)
        {
            _config = config;
            _localHost = localHost;
        }

        public async Task ReportMetrics(DateTime reportTime, IEnumerable<ReportedValue> sensors)
        {
            var payload = new LineProtocolPayload();
            var password = _config.User != null ? (_config.Password ?? "") : null;
            var client = new LineProtocolClient(_config.Address, _config.Db, _config.User, password);

            foreach (var point in sensors.Select(x => NewPoint(reportTime, x)))
            {
                payload.Add(point);
            }

            var result = await client.WriteAsync(payload);
            if (!result.Success)
            {
                Logger.Error("Influxdb encountered an error: {0}", result.ErrorMessage);
            }
        }

        private LineProtocolPoint NewPoint(DateTime reportTime, ReportedValue sensor)
        {
            var sensorType = Enum.GetName(typeof(SensorType), sensor.SensorType);
            var tags = new Dictionary<string, string>()
            {
                {"host", _localHost},
                {"app", "ohm"},
                {"hardware", sensor.Hardware},
                {"hardware_type", Enum.GetName(typeof(HardwareType), sensor.HardwareType)},
                {"identifier", sensor.Identifier },
                {"sensor", sensor.Sensor},
            };

            var fields = new Dictionary<string, object>()
            {
                {"value", sensor.Value},
                {"sensor_index", sensor.SensorIndex}
            };

            return new LineProtocolPoint(sensorType, fields, tags, reportTime.ToUniversalTime());
        }

        public void Dispose()
        {
        }
    }
}
