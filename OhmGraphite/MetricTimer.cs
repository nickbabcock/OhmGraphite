using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Timers;
using NLog;
using OpenHardwareMonitor.Hardware;

namespace OhmGraphite
{
    public class MetricTimer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly Computer _computer;
        private readonly int _graphitePort;
        private readonly string _graphiteHost;
        private readonly Timer _timer;

        public MetricTimer(MetricConfig config)
        {
            _computer = config.Computer;
            _graphitePort = config.Port;
            _graphiteHost = config.Host;
            _timer = new Timer(config.Interval.TotalMilliseconds) { AutoReset = true };
            _timer.Elapsed += ReportMetrics;
        }

        public void Start()
        {
            Logger.LogAction("starting metric timer", () =>
            {
                _computer.Open();
                _timer.Start();
            });
        }

        public void Stop()
        {
            Logger.LogAction("stopping metric timer", () =>
            {
                _computer.Close();
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
                foreach (var hardware in ReadHardware(_computer))
                {
                    foreach (var sensor in ReadSensors(hardware))
                    {
                        var data = Normalize(sensor);

                        // Graphite API wants <metric> <value> <timestamp>. We prefix the metric
                        // with `ohm` as to not overwrite potentially existing metrics
                        writer.WriteLine($"ohm.{host}.{data.Identifier}.{data.Name} {data.Value} {epoch:d}");

                        sensorCount++;
                    }
                }
            }

            Logger.Info($"Sent {sensorCount} metrics in {stopwatch.Elapsed.TotalMilliseconds}ms");
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

        private static IEnumerable<IHardware> ReadHardware(IComputer computer)
        {
            foreach (var hardware in computer.Hardware)
            {
                yield return hardware;

                foreach (var subHardware in hardware.SubHardware)
                {
                    yield return subHardware;
                }
            }
        }

        private static IEnumerable<Sensor> ReadSensors(IHardware hardware)
        {
            hardware.Update();
            foreach (var sensor in hardware.Sensors)
            {
                var id = sensor.Identifier.ToString();

                // Only report a value if the sensor was able to get a value
                // as 0 is different than "didn't read". For example, are the
                // fans really spinning at 0 RPM or was the value not read.
                if (sensor.Value.HasValue)
                {
                    yield return new Sensor(id, sensor.Name, sensor.Value.Value);
                }
                else
                {
                    Logger.Warn($"{id} did not have a value");
                }
            }
        }
    }
}