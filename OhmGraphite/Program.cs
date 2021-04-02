using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using NLog;
using LibreHardwareMonitor.Hardware;
using OhmGraphite.Test;
using Prometheus;
using Topshelf;

namespace OhmGraphite
{
    internal class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static void Main()
        {
            string configPath = string.Empty;
            HostFactory.Run(x =>
            {
                x.Service<IManage>(s =>
                {
                    // We'll want to capture all available hardware metrics
                    // to send to graphite
                    var computer = new Computer
                    {
                        IsGpuEnabled = true,
                        IsMotherboardEnabled = true,
                        IsCpuEnabled = true,
                        IsMemoryEnabled = true,
                        IsNetworkEnabled = true,
                        IsStorageEnabled = true,
                        IsControllerEnabled = true
                    };

                    s.ConstructUsing(name =>
                    {
                        var configDisplay = string.IsNullOrEmpty(configPath) ? "default" : configPath;
                        var config = Logger.LogFunction($"parse config {configDisplay}", () => MetricConfig.ParseAppSettings(CreateConfiguration(configPath)));
                        var collector = new SensorCollector(computer, config);
                        return CreateManager(config, collector);
                    });
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Dispose());
                });

                // Allow one to specify a command line argument when running interactively
                x.AddCommandLineDefinition("config", v => configPath = v);
                x.UseNLog();
                x.RunAsLocalSystem();
                x.SetDescription(
                    "Extract hardware sensor data and exports it to a given host and port in a graphite compatible format");
                x.SetDisplayName("Ohm Graphite");
                x.SetServiceName("OhmGraphite");
                x.OnException(ex => Logger.Error(ex, "OhmGraphite TopShelf encountered an error"));
            });
        }

        private static IAppConfig CreateConfiguration(string configPath)
        {
            if (string.IsNullOrEmpty(configPath))
            {
                // https://github.com/dotnet/runtime/issues/13051#issuecomment-510267727
                var processModule = Process.GetCurrentProcess().MainModule;
                if (processModule != null)
                {
                    var pt = processModule.FileName;
                    var fn = Path.Join(Path.GetDirectoryName(pt), "OhmGraphite.exe.config");
                    var configMap1 = new ExeConfigurationFileMap { ExeConfigFilename = fn };
                    var config1 = ConfigurationManager.OpenMappedExeConfiguration(configMap1, ConfigurationUserLevel.None);
                    return new CustomConfig(config1);
                }
            }

            if (!File.Exists(configPath))
            {
                throw new ApplicationException($"unable to detect config: ${configPath}");
            }

            var configMap = new ExeConfigurationFileMap { ExeConfigFilename = configPath };
            var config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
            return new CustomConfig(config);
        }

        private static IManage CreateManager(MetricConfig config, SensorCollector collector)
        {
            var hostname = config.LookupName();
            double seconds = config.Interval.TotalSeconds;
            if (config.Graphite != null)
            {
                Logger.Info(
                    $"Graphite host: {config.Graphite.Host} port: {config.Graphite.Port} interval: {seconds} tags: {config.Graphite.Tags}");
                var writer = new GraphiteWriter(config.Graphite.Host,
                    config.Graphite.Port,
                    hostname,
                    config.Graphite.Tags);
                return new MetricTimer(config.Interval, collector, writer);
            }
            else if (config.Prometheus != null)
            {
                Logger.Info($"Prometheus port: {config.Prometheus.Port}");
                var registry = PrometheusCollection.SetupDefault(collector);
                var server = new MetricServer(config.Prometheus.Host, config.Prometheus.Port, registry: registry);
                return new PrometheusServer(server, collector);
            }
            else if (config.Timescale != null)
            {
                var writer = new TimescaleWriter(config.Timescale.Connection, config.Timescale.SetupTable, hostname);
                return new MetricTimer(config.Interval, collector, writer);
            }
            else
            {
                Logger.Info($"Influxdb address: {config.Influx.Address} db: {config.Influx.Db}");
                var writer = new InfluxWriter(config.Influx, hostname);
                return new MetricTimer(config.Interval, collector, writer);
            }
        }
    }
}