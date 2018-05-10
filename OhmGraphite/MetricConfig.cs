using System;
using System.Configuration;

namespace OhmGraphite
{
    public class MetricConfig
    {
        public MetricConfig(string host, int port, TimeSpan interval)
        {
            Host = host;
            Port = port;
            Interval = interval;
        }

        public string Host { get; }
        public int Port { get; }
        public TimeSpan Interval { get; }

        public static MetricConfig ParseAppSettings()
        {
            string host = ConfigurationManager.AppSettings["host"] ?? "localhost";
            if (!int.TryParse(ConfigurationManager.AppSettings["port"], out int port))
            {
                port = 2003;
            }

            if (!int.TryParse(ConfigurationManager.AppSettings["interval"], out int seconds))
            {
                seconds = 5;
            }

            var interval = TimeSpan.FromSeconds(seconds);
            return new MetricConfig(host, port, interval);
        }
    }
}