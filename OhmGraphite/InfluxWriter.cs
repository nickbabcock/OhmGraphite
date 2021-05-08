using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using InfluxDB.LineProtocol.Payload;
using NLog;

namespace OhmGraphite
{
    public class InfluxWriter : IWriteMetrics
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly HttpClient _client = new HttpClient();
        private readonly InfluxConfig _config;
        private readonly string _localHost;

        public InfluxWriter(InfluxConfig config, string localHost)
        {
            _config = config;
            _localHost = localHost;

            if (!string.IsNullOrEmpty(_config.User) && !string.IsNullOrEmpty(_config.Password))
            {
                var raw = Encoding.UTF8.GetBytes($"{_config.User}:{_config.Password}");
                var encoded = Convert.ToBase64String(raw);
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encoded);
            }
        }

        public async Task ReportMetrics(DateTime reportTime, IEnumerable<ReportedValue> sensors)
        {
            var payload = new LineProtocolPayload();
            foreach (var point in sensors.Select(x => NewPoint(reportTime, x)))
            {
                payload.Add(point);
            }

            // can't use influx db client as they don't have one that is both 1.x and 2.x compatible
            // so we implement our own compatible client
            var formattedData = new StringWriter();
            payload.Format(formattedData);
            formattedData.Flush();
            var outData = Encoding.UTF8.GetBytes(formattedData.ToString());
            var content = new ByteArrayContent(outData);

            var queries = new List<string>();

            // passwordless authentication
            if (!string.IsNullOrEmpty(_config.User) && string.IsNullOrEmpty(_config.Password))
            {
                queries.Add($"u={_config.User}&p=");
            }

            if (!string.IsNullOrEmpty(_config.Db))
            {
                queries.Add($"db={_config.Db}");
            }

            var qs = string.Join("&", queries);
            qs = !string.IsNullOrEmpty(qs) ? $"?{qs}" : "";
            var addrPath = new Uri(_config.Address, "/write");
            var url = $"{addrPath}{qs}";
            var response = await _client.PostAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                Logger.Error("Influxdb encountered an error: {0}", err);
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
