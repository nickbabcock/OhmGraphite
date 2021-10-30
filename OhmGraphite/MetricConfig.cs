using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace OhmGraphite
{
    public record EnabledHardware(bool Cpu, bool Gpu, bool Motherboard, bool Ram, bool Network, bool Storage,
        bool Controller);

    public class MetricConfig
    {
        private readonly INameResolution _nameLookup;

        public MetricConfig(TimeSpan interval, INameResolution nameLookup, GraphiteConfig graphite, InfluxConfig influx,
            PrometheusConfig prometheus, TimescaleConfig timescale, Dictionary<string, string> aliases, List<Regex> hiddenSensors, Influx2Config influx2, EnabledHardware enabledHardware)
        {
            _nameLookup = nameLookup;
            Interval = interval;
            Graphite = graphite;
            Influx = influx;
            Prometheus = prometheus;
            Timescale = timescale;
            Aliases = aliases;
            HiddenSensors = hiddenSensors;
            Influx2 = influx2;
            EnabledHardware = enabledHardware;
        }

        public string LookupName() => _nameLookup.LookupName();
        public TimeSpan Interval { get; }
        public GraphiteConfig Graphite { get; }
        public InfluxConfig Influx { get; }
        public Influx2Config Influx2 { get; }
        public EnabledHardware EnabledHardware { get; }
        public PrometheusConfig Prometheus { get; }
        public TimescaleConfig Timescale { get; }
        public Dictionary<string, string> Aliases { get; }
        public List<Regex> HiddenSensors { get; }

        public static MetricConfig ParseAppSettings(IAppConfig config)
        {
            if (!int.TryParse(config["interval"], out int seconds))
            {
                seconds = 5;
            }

            var interval = TimeSpan.FromSeconds(seconds);

            INameResolution nameLookup = NameLookup(config["name_lookup"] ?? "netbios");
            InstallCertificateVerification(config["certificate_verification"] ?? "True");
            var enabledHardware = ParseEnabledHardware(config);

            var type = config["type"] ?? "graphite";
            GraphiteConfig gconfig = null;
            InfluxConfig iconfig = null;
            PrometheusConfig pconfig = null;
            TimescaleConfig timescale = null;
            Influx2Config influx2 = null;

            switch (type.ToLowerInvariant())
            {
                case "graphite":
                    gconfig = GraphiteConfig.ParseAppSettings(config);
                    break;
                case "influxdb":
                case "influx":
                    iconfig = InfluxConfig.ParseAppSettings(config);
                    break;
                case "influx2":
                    influx2 = Influx2Config.ParseAppSettings(config);
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

            // Do a similar trim with "/hidden" suffix except treat the prefix as a glob to hide
            // a group of sensors like if one wants to hide all power sensors they can do:
            // /*/power/*/hidden
            // Code to detect interpret strings as globs is shamelessly taken from stackoverflow:
            // https://stackoverflow.com/a/4146349/433785
            var hiddenSensors = config.GetKeys().Where(x => x.EndsWith("/hidden"))
                .Select(x => x.Remove(x.LastIndexOf("/hidden", StringComparison.Ordinal)))
                .Select(pattern => new Regex(
                    "^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$",
                    RegexOptions.IgnoreCase | RegexOptions.Singleline
                )).ToList();

            return new MetricConfig(interval, nameLookup, gconfig, iconfig, pconfig, timescale, aliases, hiddenSensors, influx2, enabledHardware);
        }

        private static EnabledHardware ParseEnabledHardware(IAppConfig config)
        {
            return new (
                config["/cpu/enabled"]?.ToLowerInvariant() != "false",
                config["/gpu/enabled"]?.ToLowerInvariant() != "false",
                config["/motherboard/enabled"]?.ToLowerInvariant() != "false",
                config["/ram/enabled"]?.ToLowerInvariant() != "false",
                config["/network/enabled"]?.ToLowerInvariant() != "false",
                config["/storage/enabled"]?.ToLowerInvariant() != "false",
                config["/controller/enabled"]?.ToLowerInvariant() != "false"
            );
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

        public static void InstallCertificateVerification(string type)
        {
            switch (type.ToLowerInvariant())
            {
                // Do not change default .net behavior when given True
                case "true":
                    break;

                // Do not verify certificate
                case "false":
                    ServicePointManager.ServerCertificateValidationCallback =
                        (sender, certificate, chain, errors) => true;
                    break;

                // Else assume that it points to a file path of a self signed
                // certificate that we will check against
                default:
                    var cert = new X509Certificate2(type);
                    ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) =>
                        errors == SslPolicyErrors.None ||
                        string.Equals(cert.Thumbprint, certificate.GetCertHashString(), StringComparison.InvariantCultureIgnoreCase);
                    break;
            }
        }

        public bool TryGetAlias(string v, out string alias) => Aliases.TryGetValue(v, out alias);
        public bool IsHidden(string id) => HiddenSensors.Any(x => x.IsMatch(id));
    }
}