using System;
using System.Collections.Generic;
using System.Linq;

namespace OhmGraphite
{
    public class MetricConfig
    {
        private readonly INameResolution _nameLookup;

        public MetricConfig(TimeSpan interval, INameResolution nameLookup, GraphiteConfig graphite, InfluxConfig influx,
            PrometheusConfig prometheus, TimescaleConfig timescale, Dictionary<String, String> aliases)
        {
            _nameLookup = nameLookup;
            Interval = interval;
            Graphite = graphite;
            Influx = influx;
            Prometheus = prometheus;
            Timescale = timescale;
            Aliases = aliases;
        }

        public string LookupName() => _nameLookup.LookupName();
        public TimeSpan Interval { get; }
        public GraphiteConfig Graphite { get; }
        public InfluxConfig Influx { get; }
        public PrometheusConfig Prometheus { get; }
        public TimescaleConfig Timescale { get; }
        public Dictionary<string, string> Aliases { get; }

        public static MetricConfig ParseAppSettings(IAppConfig config)
        {
            if (!int.TryParse(config["interval"], out int seconds))
            {
                seconds = 5;
            }

            var interval = TimeSpan.FromSeconds(seconds);

            INameResolution nameLookup = NameLookup(config["name_lookup"] ?? "netbios");

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

            // Trim off the LibreHardwareMonitor "/name" suffix so that it is just the
            // the sensor ID.
            var aliases = config.GetKeys().Where(x => x.EndsWith("/name"))
                .ToDictionary(
                    x => x.Remove(x.LastIndexOf("/name", StringComparison.Ordinal)),
                    x => config[x]
                );

            return new MetricConfig(interval, nameLookup, gconfig, iconfig, pconfig, timescale, aliases);
        }

        private static INameResolution NameLookup(string lookup)
        {
            switch (lookup.ToLowerInvariant())
            {
                case "dns":
                    return new DnsResolution();
                case "netbios":
                    return new NetBiosResolution();
                default:
                    return new StaticResolution(lookup);
            }
        }

        public bool TryGetAlias(string v, out string alias) => Aliases.TryGetValue(v, out alias);
    }
}