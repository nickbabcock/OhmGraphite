using System;
using NLog;
using OpenHardwareMonitor.Hardware;
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
                x.Service<MetricTimer>(s =>
                {
                    // We need to know where the graphite server lives and how often
                    // to poll the hardware
                    var config = Logger.LogFunction("parse config", MetricConfig.ParseAppSettings);
                    double seconds = config.Interval.TotalSeconds;
                    IWriteMetrics writer = null;
                    if (config.Graphite != null)
                    {
                        Logger.Info($"Graphite host: {config.Graphite.Host} port: {config.Graphite.Port} interval: {seconds} tags: {config.Graphite.Tags}");
                        writer = new GraphiteWriter(config.Graphite.Host,
                            config.Graphite.Port,
                            Environment.MachineName,
                            config.Graphite.Tags);
                    }
                    else
                    {
                        Logger.Info($"Influxdb address: {config.Influx.Address} db: {config.Influx.Db}");
                        writer = new InfluxWriter(config.Influx, Environment.MachineName);
                    }

                    // We'll want to capture all available hardware metrics
                    // to send to graphite
                    var computer = new Computer
                    {
                        GPUEnabled = true,
                        MainboardEnabled = true,
                        CPUEnabled = true,
                        RAMEnabled = true,
                        FanControllerEnabled = true,
                        HDDEnabled = true
                    };

                    var collector = new SensorCollector(computer);

                    s.ConstructUsing(name =>
                        Logger.LogFunction("creating timer",
                            () => new MetricTimer(config.Interval, collector, writer)));
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
    }
}