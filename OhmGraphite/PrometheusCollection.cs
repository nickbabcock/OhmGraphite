using System;
using System.Text.RegularExpressions;
using NLog;
using Prometheus;

namespace OhmGraphite
{
    public class PrometheusCollection
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly Regex Rx = new Regex("[^a-zA-Z0-9_:]", RegexOptions.Compiled);
        private readonly IGiveSensors _collector;
        private readonly MetricFactory _metrics;

        public PrometheusCollection(IGiveSensors collector, MetricFactory metrics)
        {
            _collector = collector;
            _metrics = metrics;
        }

        public static CollectorRegistry SetupDefault(IGiveSensors collector)
        {
            var registry = Metrics.DefaultRegistry;
            var metrics = Metrics.WithCustomRegistry(registry);
            var prometheusCollection = new PrometheusCollection(collector, metrics);
            registry.AddBeforeCollectCallback(() => prometheusCollection.UpdateMetrics());
            return registry;
        }

        public void UpdateMetrics()
        {
            Logger.LogAction("prometheus update metrics", PollSensors);
        }

        private static (string, double) BaseReport(ReportedValue report)
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
                    case SensorType.Energy: // Wh
                        return value / 1000.0;
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
                        return "load_percent";
                    case SensorType.Control: // %
                        return "control_percent";
                    case SensorType.Level: // %
                        return "level_percent";
                    case SensorType.Humidity: // %
                        return "humidity_percent";
                    case SensorType.Fan: // RPM
                        return "revolutions_per_minute";
                    case SensorType.Flow: // L/h
                        return "liters_per_hour";
                    case SensorType.Current:
                        return "amps";
                    case SensorType.TimeSpan:
                        return "seconds";
                    case SensorType.Energy:
                        return "watt_hours";
                    case SensorType.Noise:
                        return "dba";
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
                var hw = Enum.GetName(typeof(HardwareType), sensor.HardwareType)?.ToLowerInvariant();
                var name = Rx.Replace($"ohm_{hw}_{unit}", "_");
                _metrics.CreateGauge(name, "Metric reported by open hardware sensor", "hardware", "sensor", "hw_instance")
                    .WithLabels(sensor.Hardware, sensor.Sensor, sensor.HardwareInstance)
                    .Set(value);
            }
        }
    }
}
