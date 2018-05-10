using System;
using System.Configuration;

namespace OhmGraphite
{
    public class MetricConfig
    {
        public MetricConfig(string host, int port, TimeSpan interval, bool tags)
        {
            Host = host;
            Port = port;
            Interval = interval;
            Tags = tags;
        }

        public string Host { get; }
        public int Port { get; }
        public TimeSpan Interval { get; }
        public bool Tags { get; }

        public static MetricConfig ParseAppSettings()
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

            if (!int.TryParse(ConfigurationManager.AppSettings["interval"], out int seconds))
            {
                seconds = 5;
            }

            var interval = TimeSpan.FromSeconds(seconds);
            return new MetricConfig(host, port, interval, tags);
        }
    }
}