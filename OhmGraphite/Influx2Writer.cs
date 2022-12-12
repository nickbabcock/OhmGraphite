using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using NLog;

namespace OhmGraphite
{
    public class Influx2Writer : IWriteMetrics
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly Influx2Config _config;
        private readonly string _localHost;

        public Influx2Writer(Influx2Config config, string localHost)
        {
            _config = config;
            _localHost = localHost;
        }

        public async Task ReportMetrics(DateTime reportTime, IEnumerable<ReportedValue> sensors)
        {
            var influxDbClient = new InfluxDBClient(_config.Options);
            var writeApi = influxDbClient.GetWriteApiAsync();
            var points = sensors.Select(x => NewPoint(reportTime, x)).ToList();
            await writeApi.WritePointsAsync(points);
        }

        private PointData NewPoint(DateTime reportTime, ReportedValue sensor)
        {
            var sensorType = Enum.GetName(typeof(SensorType), sensor.SensorType);
            return PointData.Measurement(sensorType)
                .Tag("host", _localHost)
                .Tag("app", "ohm")
                .Tag("hardware", sensor.Hardware)
                .Tag("hardware_type", Enum.GetName(typeof(HardwareType), sensor.HardwareType))
                .Tag("identifier", sensor.Identifier)
                .Tag("sensor", sensor.Sensor)
                .Field("value", sensor.Value)
                .Field("sensor_index", sensor.SensorIndex)
                .Timestamp(reportTime.ToUniversalTime(), WritePrecision.S);
        }

        public void Dispose()
        {
        }
    }
}
