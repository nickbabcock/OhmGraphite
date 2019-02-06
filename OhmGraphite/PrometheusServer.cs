using NLog;
using Prometheus;

namespace OhmGraphite
{
    public class PrometheusServer : IManage
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly MetricServer _server;
        private readonly IGiveSensors _collector;

        public PrometheusServer(MetricServer server, IGiveSensors collector)
        {
            _server = server;
            _collector = collector;
        }

        public void Start()
        {
            Logger.LogAction("starting prometheus server", () =>
            {
                _collector.Start();
                _server.Start();
            });
        }

        public void Stop()
        {
            Logger.LogAction("stopping prometheus server", () =>
            {
                _collector.Stop();
                _server.Stop();
            });
        }
    }
}
