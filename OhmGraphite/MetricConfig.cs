using System;

namespace OhmGraphite
{
    public class MetricConfig
    {
        private readonly INameResolution _nameLookup;

        public MetricConfig(TimeSpan interval, INameResolution nameLookup, GraphiteConfig graphite, InfluxConfig influx,
            PrometheusConfig prometheus, TimescaleConfig timescale)
        {
            _nameLookup = nameLookup;
            Interval = interval;
            Graphite = graphite;
            Influx = influx;
            Prometheus = prometheus;
            Timescale = timescale;
        }

        public string LookupName() => _nameLookup.LookupName();
        public TimeSpan Interval { get; }
        public GraphiteConfig Graphite { get; }
        public InfluxConfig Influx { get; }
        public PrometheusConfig Prometheus { get; }
        public TimescaleConfig Timescale { get; }

        public static MetricConfig ParseAppSettings(IAppConfig config)
        {
            if (!int.TryParse(config["interval"], out int seconds))
            {
                seconds = 5;
            }

            var interval = TimeSpan.FromSeconds(seconds);

            var lookup = config["name_lookup"] ?? "netbios";
            INameResolution nameLookup;
            switch (lookup.ToLowerInvariant())
            {
                case "dns":
                    nameLookup = new DnsResolution();
                    break;
                default:
                    nameLookup = new NetBiosResolution();
                    break;
            }

            var type = config["type"] ?? "graphite";
            GraphiteConfig gconfig = null;
            InfluxConfig iconfig = null;
            PrometheusConfig pconfig = null;
            TimescaleConfig timescale = null;

            switch (type.ToLowerInvariant())
            {
                case "graphite":
                    gconfig = GraphiteConfig.ParseAppSettings(config);
                    break;
                case "influxdb":
                case "influx":
                    iconfig = InfluxConfig.ParseAppSettings(config);
                    break;
                case "prometheus":
                    pconfig = PrometheusConfig.ParseAppSettings(config);
                    break;
                case "timescale":
                case "timescaledb":
                    timescale = TimescaleConfig.ParseAppSettings(config);
                    break;
            }

            return new MetricConfig(interval, nameLookup, gconfig, iconfig, pconfig, timescale);
        }
    }
}