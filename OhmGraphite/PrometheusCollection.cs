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
        private MetricFactory _metrics;

        public PrometheusCollection(IGiveSensors collector, CollectorRegistry registry)
        {
            _collector = collector;
            registry.AddBeforeCollectCallback(UpdateMetrics);
            _metrics = Metrics.WithCustomRegistry(registry);
        }

        public void UpdateMetrics()
        {
            Logger.LogAction("prometheus update metrics", PollSensors);
        }

        private string SuffixForSensorType(SensorType type, out float multiplier)
        {
            multiplier = 1.0f;
            switch (type)
            {
                case SensorType.Voltage: // V
                    return "voltage_volts";

                case SensorType.Clock: // MHz
                    multiplier = 1000000;
                    return "clock_hertz";

                case SensorType.Temperature: // °C
                    return "temperature_celsius";

                case SensorType.Frequency: // Hz
                    return "frequency_hertz";

                case SensorType.Power: // W
                    return "power_watts";

                case SensorType.Data: // GB = 2^30 Bytes
                    multiplier = 1073741824;
                    return "bytes";

                case SensorType.SmallData: // MB = 2^20 Bytes
                    multiplier = 1048576;
                    return "bytes";

                case SensorType.Throughput: // B/s
                    return "throughput_bytes_per_second";

                case SensorType.Load: // %
                case SensorType.Control: // %
                case SensorType.Level: // %
                case SensorType.Fan: // RPM
                case SensorType.Flow: // L/h
                case SensorType.Factor: // 1
                default:
                    return type.ToString().ToLower();
            }
        }

        private void PollSensors()
        {
            foreach (var sensor in _collector.ReadAllSensors())
            {
                string suffix = SuffixForSensorType(sensor.SensorType, out float multiplier);

                _metrics.CreateGauge(
                        String.Format(
                            "ohm_{0}_{1}",
                            Enum.GetName(typeof(HardwareType), sensor.HardwareType).ToLower(),
                            suffix
                        ),
                        "Metric reported by open hardware sensor",
                        "hardware", "sensor")
                    .WithLabels(sensor.Hardware, sensor.Sensor)
                    .Set(sensor.Value * multiplier);
            }
        }
    }
}
