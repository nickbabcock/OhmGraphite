using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using OpenHardwareMonitor.Hardware;
using static System.FormattableString;

namespace OhmGraphite
{
    public class GraphiteWriter : IWriteMetrics
    {
        private readonly string _localHost;
        private readonly string _remoteHost;
        private readonly int _remotePort;
        private readonly bool _tags;

        public GraphiteWriter(string remoteHost, int remotePort, string localHost, bool tags)
        {
            _remoteHost = remoteHost;
            _remotePort = remotePort;
            _tags = tags;
            _localHost = localHost;
        }

        public void ReportMetrics(DateTime reportTime, IEnumerable<ReportedValue> sensors)
        {
            // We don't want to transmit metrics across multiple seconds as they
            // are being retrieved so calculate the timestamp of the signaled event
            // only once.
            long epoch = new DateTimeOffset(reportTime).ToUnixTimeSeconds();
            using (var client = new TcpClient(_remoteHost, _remotePort))
            using (var networkStream = client.GetStream())
            using (var writer = new StreamWriter(networkStream))
            {
                foreach (var sensor in sensors)
                {
                    writer.WriteLine(FormatGraphiteData(epoch, sensor));
                }
            }
        }

        private static string NormalizedIdentifier(string host, ReportedValue sensor)
        {
            // Take the sensor's identifier (eg. /nvidiagpu/0/load/0)
            // and tranform into nvidiagpu.0.load.<name> where <name>
            // is the name of the sensor lowercased with spaces removed.
            // A name like "GPU Core" is turned into "gpucore". Also
            // since some names are like "cpucore#2", turn them into
            // separate metrics by replacing "#" with "."
            string identifier = sensor.Identifier.Replace('/', '.').Substring(1);
            identifier = identifier.Remove(identifier.LastIndexOf('.'));
            string name = sensor.Sensor.ToLower().Replace(" ", null).Replace('#', '.');
            return $"ohm.{host}.{identifier}.{name}";
        }

        public static string GraphiteEscape(string src)
        {
            // Formula for escaping graphite data is taken from
            // collectd's utils_format_graphite.c
            var builder = new StringBuilder(src.Length);
            foreach (char c in src)
            {
                if (c == '.' || char.IsWhiteSpace(c) || char.IsControl(c))
                {
                    builder.Append('-');
                }
                else
                {
                    builder.Append(c);
                }
            }

            return builder.ToString();
        }

        public string FormatGraphiteData(long epoch, ReportedValue data)
        {
            // Graphite API wants <metric> <value> <timestamp>. We prefix the metric
            // with `ohm` as to not overwrite potentially existing metrics
            string id = NormalizedIdentifier(_localHost, data);

            if (!_tags)
            {
                return Invariant($"{id} {data.Value} {epoch:d}");
            }

            return $"{id};" +
                   $"host={_localHost};" +
                   "app=ohm;" +
                   $"hardware={GraphiteEscape(data.Hardware)};" +
                   $"hardware_type={Enum.GetName(typeof(HardwareType), data.HardwareType)};" +
                   $"sensor_type={Enum.GetName(typeof(SensorType), data.SensorType)};" +
                   $"sensor_index={data.SensorIndex};" +
                   $"raw_name={GraphiteEscape(data.Sensor)} " +
                   $"{data.Value} {epoch:d}";
        }
    }
}