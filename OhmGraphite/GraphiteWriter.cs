using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using static System.FormattableString;

namespace OhmGraphite
{
    public class GraphiteWriter
    {
        private readonly string _localHost;
        private readonly string _remoteHost;
        private readonly int _remotePort;

        public GraphiteWriter(string remoteHost, int remotePort)
        {
            _remoteHost = remoteHost;
            _remotePort = remotePort;
            _localHost = Environment.MachineName;
        }

        public void ReportMetrics(DateTime reportTime, IEnumerable<Sensor> sensors)
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
                    var data = Normalize(sensor);

                    // Graphite API wants <metric> <value> <timestamp>. We prefix the metric
                    // with `ohm` as to not overwrite potentially existing metrics
                    writer.WriteLine(FormatGraphiteData(_localHost, epoch, data));
                }
            }
        }

        private static Sensor Normalize(Sensor sensor)
        {
            // Take the sensor's identifier (eg. /nvidiagpu/0/load/0)
            // and tranform into nvidiagpu.0.load.<name> where <name>
            // is the name of the sensor lowercased with spaces removed.
            // A name like "GPU Core" is turned into "gpucore". Also
            // since some names are like "cpucore#2", turn them into
            // separate metrics by replacing "#" with "."
            string identifier = sensor.Identifier.Replace('/', '.').Substring(1);
            identifier = identifier.Remove(identifier.LastIndexOf('.'));
            string name = sensor.Name.ToLower().Replace(" ", null).Replace('#', '.');
            return new Sensor(identifier, name, sensor.Value);
        }

        public static string FormatGraphiteData(string host, long epoch, Sensor data)
        {
            return Invariant($"ohm.{host}.{data.Identifier}.{data.Name} {data.Value} {epoch:d}");
        }
    }
}