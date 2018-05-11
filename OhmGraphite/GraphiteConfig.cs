using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OhmGraphite
{
    class GraphiteConfig
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

        public static GraphiteConfig ParseAppSettings()
        {
            string host = ConfigurationManager.AppSettings["host"] ?? "localhost";
            if (!int.TryParse(ConfigurationManager.AppSettings["port"], out int port))
            {
                port = 2003;
            }

            if (!bool.TryParse(ConfigurationManager.AppSettings["tags"], out bool tags))
            {
                tags = false;
            }

            return new GraphiteConfig(host, port, tags);
        }
    }
}
