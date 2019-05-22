using System;
using System.Text.RegularExpressions;
using NLog;
using OpenHardwareMonitor.Hardware;
using Prometheus;

namespace OhmGraphite
{
    public class PrometheusCollection
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly Regex rx = new Regex("[^a-zA-Z0-9_:]", RegexOptions.Compiled);
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

        private (string, double) BaseReport(ReportedValue report)
        {
            // Convert reported value into a base value by converting MB and GB into bytes, etc.
            // Flow rate is still liters per hour, even though liters per second may seem more
            // "base-unity", as grafana contained the former but not the latter. Fan speed remains
            // revolutions per minute, as I'm unaware of any manufacturer reporting fan speed as
            // revolutions per second.
            double BaseValue()
            {
                double value = report.Value;
                switch (report.SensorType)
                {
                    case SensorType.Data: // GB = 2^30 Bytes
                        return value * (1L << 30);
                    case SensorType.SmallData: // MB = 2^20 Bytes
                        return value * (1L << 20);
                    case SensorType.Clock: // MHz
                        return value * 1000000;
                    default:
                        return value;
                }
            }

            string BaseUnit()
            {
                switch (report.SensorType)
                {
                    case SensorType.Voltage: // V
                        return "volts";
                    case SensorType.Frequency: // Hz
                    case SensorType.Clock: // MHz
                        return "hertz";
                    case SensorType.Temperature: // °C
                        return "celsius";
                    case SensorType.Power: // W
                        return "watts";
                    case SensorType.Data: // GB = 2^30 Bytes
                    case SensorType.SmallData: // MB = 2^20 Bytes
                        return "bytes";
                    case SensorType.Throughput: // B/s
                        return "bytes_per_second";
                    case SensorType.Load: // %
                    case SensorType.Control: // %
                    case SensorType.Level: // %
                        return "percent";
                    case SensorType.Fan: // RPM
                        return "revolutions_per_minute";
                    case SensorType.Flow: // L/h
                        return "liters_per_hour";
                    case SensorType.Factor: // 1
                    default:
                        return report.SensorType.ToString().ToLowerInvariant();
                }
            }

            return (BaseUnit(), BaseValue());
        }

        private void PollSensors()
        {
            foreach (var sensor in _collector.ReadAllSensors())
            {
                var (unit, value) = BaseReport(sensor);
                var hw = Enum.GetName(typeof(HardwareType), sensor.HardwareType).ToLowerInvariant();
                var name = rx.Replace($"ohm_{hw}_{unit}", "_");
                _metrics.CreateGauge(name, "Metric reported by open hardware sensor", "hardware", "sensor")
                    .WithLabels(sensor.Hardware, sensor.Sensor)
                    .Set(value);
            }
        }
    }
}
