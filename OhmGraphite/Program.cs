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
                    var config = Logger.LogFunction("parse config", MetricConfig.ParseAppSettings);
                    var seconds = config.Interval.TotalSeconds;
                    Logger.Info($"Host: {config.Host} port: {config.Port} interval: {seconds}");
                    s.ConstructUsing(name => Logger.LogFunction("creating timer", () => new MetricTimer(config)));
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.UseNLog();
                x.RunAsLocalSystem();
                x.SetDescription("Extract hardware sensor data and exports it to a given host and port in a graphite compatible format");
                x.SetDisplayName("Ohm Graphite");
                x.SetServiceName("OhmGraphite");
                x.OnException(ex => Logger.Error(ex, "OhmGraphite TopShelf encountered an error"));
            });
        }
    }
}
