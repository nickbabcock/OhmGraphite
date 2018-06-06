namespace OhmGraphite
{
    public class GraphiteConfig
    {
        public GraphiteConfig(string host, int port, bool tags)
        {
            Host = host;
            Port = port;
            Tags = tags;
        }

        public string Host { get; }
        public int Port { get; }
        public bool Tags { get; }

        public static GraphiteConfig ParseAppSettings(IAppConfig config)
        {
            string host = config["host"] ?? "localhost";
            if (!int.TryParse(config["port"], out int port))
            {
                port = 2003;
            }

            if (!bool.TryParse(config["tags"], out bool tags))
            {
                tags = false;
            }

            return new GraphiteConfig(host, port, tags);
        }
    }
}
