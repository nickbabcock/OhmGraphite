using System;
using NLog;
using OpenHardwareMonitor.Hardware;
using Prometheus;
using Prometheus.Advanced;

namespace OhmGraphite
{
    public class PrometheusCollection : IOnDemandCollector
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly SensorCollector _collector;
        private readonly string _localHost;
        private MetricFactory _metrics;

        public PrometheusCollection(SensorCollector collector, string localHost)
        {
            _collector = collector;
            _localHost = localHost;
        }

        public void RegisterMetrics(ICollectorRegistry registry)
        {
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
                        sensor.Identifier.Substring(1).Replace('/', '_'),
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
