using System;
using NLog;
using OpenHardwareMonitor.Hardware;
using Prometheus;
using Topshelf;

namespace OhmGraphite
{
    internal class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<IManage>(s =>
                {
                    // We'll want to capture all available hardware metrics
                    // to send to graphite
                    var computer = new Computer
                    {
                        GPUEnabled = true,
                        MainboardEnabled = true,
                        CPUEnabled = true,
                        RAMEnabled = true,
                        FanControllerEnabled = true,
                        HDDEnabled = true,
                        NICEnabled = true
                    };

                    var collector = new SensorCollector(computer);

                    // We need to know where the graphite server lives and how often
                    // to poll the hardware
                    var config = Logger.LogFunction("parse config", () => MetricConfig.ParseAppSettings(new AppConfigManager()));
                    var metricsManager = CreateManager(config, collector);

                    s.ConstructUsing(name => metricsManager);
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.UseNLog();
                x.RunAsLocalSystem();
                x.SetDescription(
                    "Extract hardware sensor data and exports it to a given host and port in a graphite compatible format");
                x.SetDisplayName("Ohm Graphite");
                x.SetServiceName("OhmGraphite");
                x.OnException(ex => Logger.Error(ex, "OhmGraphite TopShelf encountered an error"));
            });
        }

        private static IManage CreateManager(MetricConfig config, SensorCollector collector)
        {
            double seconds = config.Interval.TotalSeconds;
            if (config.Graphite != null)
            {
                Logger.Info(
                    $"Graphite host: {config.Graphite.Host} port: {config.Graphite.Port} interval: {seconds} tags: {config.Graphite.Tags}");
                var writer = new GraphiteWriter(config.Graphite.Host,
                    config.Graphite.Port,
                    Environment.MachineName,
                    config.Graphite.Tags);
                return new MetricTimer(config.Interval, collector, writer);
            }
            else if (config.Prometheus != null)
            {
                Logger.Info($"Prometheus port: {config.Prometheus.Port}");
                var prometheusCollection = new PrometheusCollection(collector, Environment.MachineName);
                var server = new MetricServer(config.Prometheus.Host, config.Prometheus.Port);
                return new PrometheusServer(server, collector, prometheusCollection);
            }
            else if (config.Timescale != null)
            {
                var writer = new TimescaleWriter(config.Timescale, Environment.MachineName);
                return new MetricTimer(config.Interval, collector, writer);
            }
            else
            {
                Logger.Info($"Influxdb address: {config.Influx.Address} db: {config.Influx.Db}");
                var writer = new InfluxWriter(config.Influx, Environment.MachineName);
                return new MetricTimer(config.Interval, collector, writer);
            }
        }
    }
}