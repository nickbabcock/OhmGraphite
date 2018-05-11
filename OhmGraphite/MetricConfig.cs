using System;
using System.Configuration;

namespace OhmGraphite
{
    public class MetricConfig
    {
        public MetricConfig(TimeSpan interval, GraphiteConfig graphite, InfluxConfig influx)
        {
            Interval = interval;
            Graphite = graphite;
            Influx = influx;
        }

        public TimeSpan Interval { get; }
        public GraphiteConfig Graphite { get; }
        public InfluxConfig Influx { get; }

        public static MetricConfig ParseAppSettings()
        {
            if (!int.TryParse(ConfigurationManager.AppSettings["interval"], out int seconds))
            {
                seconds = 5;
            }

            var interval = TimeSpan.FromSeconds(seconds);

            var type = ConfigurationManager.AppSettings["type"] ?? "graphite";
            GraphiteConfig gconfig = null;
            InfluxConfig iconfig = null;

            switch (type.ToLowerInvariant())
            {
                case "graphite":
                    gconfig = GraphiteConfig.ParseAppSettings();
                    break;
                case "influxdb":
                case "influx":
                    iconfig = InfluxConfig.ParseAppSettings();
                    break;
            }

            return new MetricConfig(interval, gconfig, iconfig);
        }
    }
}