using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Timers;
using NLog;
using OpenHardwareMonitor.Hardware;
using static System.FormattableString;

namespace OhmGraphite
{
    public class MetricTimer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly int _graphitePort;
        private readonly string _graphiteHost;
        private readonly Timer _timer;
        private readonly SensorCollector _collector;

        public MetricTimer(MetricConfig config, SensorCollector collector)
        {
            _graphitePort = config.Port;
            _graphiteHost = config.Host;
            _timer = new Timer(config.Interval.TotalMilliseconds) { AutoReset = true };
            _timer.Elapsed += ReportMetrics;
            _collector = collector;
        }

        public void Start()
        {
            Logger.LogAction("starting metric timer", () =>
            {
                _collector.Open();
                _timer.Start();
            });
        }

        public void Stop()
        {
            Logger.LogAction("stopping metric timer", () =>
            {
                _collector.Close();
                _timer.Stop();
            });
        }

        private void ReportMetrics(object sender, ElapsedEventArgs e)
        {
            Logger.Debug("Starting to report metrics");
            try
            {
                // We don't want to transmit metrics across multiple seconds as they
                // are being retrieved so calculate the timestamp of the signaled event
                // only once.
                long epoch = new DateTimeOffset(e.SignalTime).ToUnixTimeSeconds();
                string host = Environment.MachineName;
                SendMetrics(host, epoch);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to send metrics");
            }
        }

        private void SendMetrics(string host, long epoch)
        {
            int sensorCount = 0;

            // Every 5 seconds (or superceding interval) we connect to graphite
            // and poll the hardware. It may be inefficient to open a new connection
            // every 5 seconds, and there are ways to optimize this, but opening a
            // new connection is the easiest way to ensure that previous failures
            // don't affect future results
            var stopwatch = Stopwatch.StartNew();
            using (var client = new TcpClient(_graphiteHost, _graphitePort))
            using (var networkStream = client.GetStream())
            using (var writer = new StreamWriter(networkStream))
            {
                foreach (var sensor in _collector.ReadAllSensors())
                {
                    var data = Normalize(sensor);

                    // Graphite API wants <metric> <value> <timestamp>. We prefix the metric
                    // with `ohm` as to not overwrite potentially existing metrics
                    writer.WriteLine(FormatGraphiteData(host, epoch, data));

                    sensorCount++;
                }
            }

            Logger.Info($"Sent {sensorCount} metrics in {stopwatch.Elapsed.TotalMilliseconds}ms");
        }

        public static string FormatGraphiteData(string host, long epoch, Sensor data)
        {
            return Invariant($"ohm.{host}.{data.Identifier}.{data.Name} {data.Value} {epoch:d}");
        }

        private static Sensor Normalize(Sensor sensor)
        {
            // Take the sensor's identifier (eg. /nvidiagpu/0/load/0)
            // and tranform into nvidiagpu.0.load.<name> where <name>
            // is the name of the sensor lowercased with spaces removed.
            // A name like "GPU Core" is turned into "gpucore". Also
            // since some names are like "cpucore#2", turn them into
            // separate metrics by replacing "#" with "."
            var identifier = sensor.Identifier.Replace('/', '.').Substring(1);
            identifier = identifier.Remove(identifier.LastIndexOf('.'));
            var name = sensor.Name.ToLower().Replace(" ", null).Replace('#', '.');
            return new Sensor(identifier, name, sensor.Value);
        }
    }
}