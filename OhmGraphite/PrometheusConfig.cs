namespace OhmGraphite
{
    public class PrometheusConfig
    {
        public int Port { get; }
        public string Host { get; }
        public bool UseHttps { get; }

        public PrometheusConfig(int port, string host, bool useHttps)
        {
            Port = port;
            Host = host;
            UseHttps = useHttps;
        }

        internal static PrometheusConfig ParseAppSettings(IAppConfig config)
        {
            string host = config["prometheus_host"] ?? "*";
            if (!bool.TryParse(config["prometheus_https"], out bool useHttps))
            {
                useHttps = false;
            }
            if (!int.TryParse(config["prometheus_port"], out int port))
            {
                port = 4445;
            }

            return new PrometheusConfig(port, host, useHttps);
        }
    }
}
