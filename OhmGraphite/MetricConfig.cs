using System;
using System.Configuration;
using OpenHardwareMonitor.Hardware;

namespace OhmGraphite
{
    public class MetricConfig
    {
        public string Host { get; }
        public int Port { get; }
        public Computer Computer { get; }
        public TimeSpan Interval { get; }

        public MetricConfig(string host, int port, Computer computer, TimeSpan interval)
        {
            Host = host;
            Port = port;
            Computer = computer;
            Interval = interval;
        }

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

            // We'll want to capture all available hardware metrics
            // to send to graphite
            var computer = new Computer
            {
                GPUEnabled = true,
                MainboardEnabled = true,
                CPUEnabled = true,
                RAMEnabled = true,
                FanControllerEnabled = true,
                HDDEnabled = true
            };

            return new MetricConfig(host, port, computer, interval);
        }
    }
}
