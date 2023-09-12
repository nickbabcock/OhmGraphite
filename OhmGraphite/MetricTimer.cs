using System;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using NLog;

namespace OhmGraphite
{
    public class MetricTimer : IManage
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IGiveSensors _collector;

        private readonly Timer _timer;
        private readonly IWriteMetrics _writer;

        public MetricTimer(TimeSpan interval, IGiveSensors collector, IWriteMetrics writer)
        {
            _timer = new Timer(interval.TotalMilliseconds) { AutoReset = true };
            _timer.Elapsed += ReportMetrics;
            _collector = collector;
            _writer = writer;
        }

        public void Start()
        {
            Logger.LogAction("starting metric timer", () =>
            {
                _collector.Start();
                _timer.Start();
            });
        }

        private async void ReportMetrics(object sender, ElapsedEventArgs e)
        {
            Logger.Debug("Starting to report metrics");
            try
            {
                // Read all the sensors into a list so that they are only polled once.
                // Polling sensors can be relatively expensive so the intermediate
                // list cuts down on the number of potential updates.
                var stopwatch = Stopwatch.StartNew();
                var sensors = _collector.ReadAllSensors().ToList();
                await _writer.ReportMetrics(e.SignalTime, sensors);
                Logger.Info($"Sent {sensors.Count} metrics in {stopwatch.Elapsed.TotalMilliseconds}ms");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to send metrics");
            }
        }

        public void Dispose()
        {
            Logger.LogAction("stopping metric timer", () =>
            {
                _writer?.Dispose();
                _collector?.Dispose();
                _timer?.Dispose();
            });
        }
    }
}