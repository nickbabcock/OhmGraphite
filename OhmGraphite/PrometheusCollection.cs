using System;
using NLog;
using OpenHardwareMonitor.Hardware;
using Prometheus;

namespace OhmGraphite
{
    public class PrometheusCollection
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IGiveSensors _collector;
        private readonly string _localHost;
        private MetricFactory _metrics;

        public PrometheusCollection(IGiveSensors collector, string localHost, CollectorRegistry registry)
        {
            _collector = collector;
            _localHost = localHost;
            registry.AddBeforeCollectCallback(UpdateMetrics);
            _metrics = Metrics.WithCustomRegistry(registry);
        }

        public void UpdateMetrics()
        {
            Logger.LogAction("prometheus update metrics", PollSensors);
        }

        private void PollSensors()
        {
            foreach (var sensor in _collector.ReadAllSensors())
            {
                _metrics.CreateGauge(
                        "ohm_" + sensor.SensorType.ToString().ToLower(),
                        "Metric reported by open hardware sensor",
                        "host", "app", "hardware", "hardware_type", "sensor", "sensor_index")
                    .WithLabels(_localHost, "ohm", sensor.Hardware,
                        Enum.GetName(typeof(HardwareType), sensor.HardwareType),
                        sensor.Sensor,
                        sensor.SensorIndex.ToString())
                    .Set(sensor.Value);
            }
        }
    }
}
