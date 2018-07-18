
namespace OhmGraphite
{
    public class PrometheusConfig
    {
        public int Port { get; }
        public string Host { get; }

        public PrometheusConfig(int port, string host)
        {
            Port = port;
            Host = host;
        }

        internal static PrometheusConfig ParseAppSettings(IAppConfig config)
        {
            string host = config["prometheus_host"] ?? "*";
            if (!int.TryParse(config["prometheus_port"], out int port))
            {
                port = 4445;
            }

            return new PrometheusConfig(port, host);
        }
    }
}
