using NLog;
using Prometheus;
using Prometheus.Advanced;

namespace OhmGraphite
{
    public class PrometheusServer : IManage
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly MetricServer _server;
        private readonly IGiveSensors _collector;
        private readonly PrometheusCollection _prometheusCollection;

        public PrometheusServer(MetricServer server, IGiveSensors collector, PrometheusCollection prometheusCollection)
        {
            _server = server;
            _collector = collector;
            _prometheusCollection = prometheusCollection;
        }

        public void Start()
        {
            Logger.LogAction("starting prometheus server", () =>
            {
                DefaultCollectorRegistry.Instance.RegisterOnDemandCollectors(_prometheusCollection);
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
