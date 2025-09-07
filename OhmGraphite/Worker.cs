using NLog;
using LibreHardwareMonitor.Hardware;
using Prometheus;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Hosting;

namespace OhmGraphite
{
    class Worker : BackgroundService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly MetricConfig config;
        public Worker(MetricConfig config)
        {
            this.config = config;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() =>
            {
                using var app = CreateOhmGraphite(config);
                app.Start();
                stoppingToken.WaitHandle.WaitOne();
            }, stoppingToken);
        }

        private static IManage CreateOhmGraphite(MetricConfig config)
        {
            var computer = new Computer
            {
                IsGpuEnabled = config.EnabledHardware.Gpu,
                IsMotherboardEnabled = config.EnabledHardware.Motherboard,
                IsCpuEnabled = config.EnabledHardware.Cpu,
                IsMemoryEnabled = config.EnabledHardware.Ram,
                IsNetworkEnabled = config.EnabledHardware.Network,
                IsStorageEnabled = config.EnabledHardware.Storage,
                IsControllerEnabled = config.EnabledHardware.Controller,
                IsPsuEnabled = config.EnabledHardware.Psu,
                IsBatteryEnabled = config.EnabledHardware.Battery,
                IsRing0Enabled = false,
            };

            var collector = new SensorCollector(computer, config);
            return CreateManager(config, collector);
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
                var server = new MetricServer(config.Prometheus.Host, config.Prometheus.Port, url: config.Prometheus.Path, registry: registry, useHttps: config.Prometheus.UseHttps);
                return new PrometheusServer(server, collector);
            }
            else if (config.Timescale != null)
            {
                var writer = new TimescaleWriter(config.Timescale.Connection, config.Timescale.SetupTable, hostname);
                return new MetricTimer(config.Interval, collector, writer);
            }
            else if (config.Influx != null)
            {
                Logger.Info($"Influxdb address: {config.Influx.Address} db: {config.Influx.Db}");
                var writer = new InfluxWriter(config.Influx, hostname);
                return new MetricTimer(config.Interval, collector, writer);
            }
            else
            {
                Logger.Info($"Influx2 address: {config.Influx2.Options.Url}");
                var writer = new Influx2Writer(config.Influx2, hostname);
                return new MetricTimer(config.Interval, collector, writer);
            }
        }
    }
}