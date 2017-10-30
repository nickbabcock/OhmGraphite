using System;
using OpenHardwareMonitor.Hardware;
using System.Configuration;
using NLog;
using Topshelf;

namespace OhmGraphite
{
    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<MetricTimer>(s =>
                {
                    // We need to know where the graphite server lives and how often
                    // to poll the hardware
                    ParseConfig(out string host, out int port, out int seconds);

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

                    s.ConstructUsing(name => new MetricTimer(computer, TimeSpan.FromSeconds(seconds), host, port));
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.RunAsLocalSystem();

                x.SetDescription("Extract hardware sensor data and exports it to a given host and port in a graphite compatible format");
                x.SetDisplayName("Ohm Graphite");
                x.SetServiceName("OhmGraphite");
                x.OnException(ex =>
                {
                    Logger.Error(ex, "OhmGraphite TopShelf encountered an error");
                });
            });
        }

        private static void ParseConfig(out string host, out int port, out int seconds)
        {
            host = ConfigurationManager.AppSettings["host"] ?? "localhost";
            if (!int.TryParse(ConfigurationManager.AppSettings["port"], out port))
            {
                port = 2003;
            }

            if (!int.TryParse(ConfigurationManager.AppSettings["interval"], out seconds))
            {
                seconds = 5;
            }

            Logger.Info($"Host: {host} port: {port} interval: {seconds}");
        }
    }
}
