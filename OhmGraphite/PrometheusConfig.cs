namespace OhmGraphite
{
    public class PrometheusConfig
    {
        public int Port { get; }
        public string Host { get; }
        public bool UseHttps { get; }
        public string Path { get; }

        public PrometheusConfig(int port, string host, bool useHttps, string path)
        {
            Port = port;
            Host = host;
            UseHttps = useHttps;
            Path = path;
        }

        internal static PrometheusConfig ParseAppSettings(IAppConfig config)
        {
            string path = config["prometheus_path"] ?? "metrics/";
            string host = config["prometheus_host"] ?? "*";
            if (!bool.TryParse(config["prometheus_https"], out bool useHttps))
            {
                useHttps = false;
            }
            if (!int.TryParse(config["prometheus_port"], out int port))
            {
                port = 4445;
            }

            return new PrometheusConfig(port, host, useHttps, path);
        }
    }
}
