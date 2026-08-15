using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace OhmGraphite
{
    public class MetricTimer : IManage
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly TimeSpan ShutdownTimeout = TimeSpan.FromSeconds(5);
        private readonly int _batchSize;
        private readonly IGiveSensors _collector;
        private readonly TimeSpan _interval;
        private readonly List<MetricReport> _reports = new List<MetricReport>();
        private readonly IWriteMetrics _writer;
        private CancellationTokenSource _cancellation;
        private bool _disposed;
        private Task _reportTask;

        public MetricTimer(TimeSpan interval, int batchSize, IGiveSensors collector, IWriteMetrics writer)
        {
            if (interval <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(interval));
            }

            if (batchSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(batchSize));
            }

            _interval = interval;
            _batchSize = batchSize;
            _collector = collector;
            _writer = writer;
        }

        public void Start()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(MetricTimer));
            }

            if (_reportTask != null)
            {
                return;
            }

            Logger.LogAction("starting metric timer", () =>
            {
                _collector.Start();
                _cancellation = new CancellationTokenSource();
                _reportTask = ReportOnInterval(_cancellation.Token);
            });
        }

        private async Task ReportOnInterval(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(_interval);
            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await CollectMetrics(DateTime.Now);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
            finally
            {
                await FlushMetrics();
            }
        }

        internal async Task CollectMetrics(DateTime reportTime)
        {
            Logger.Debug("Starting to collect metrics");
            try
            {
                // Read all sensors into a list so that each sensor is polled one time.
                var sensors = _collector.ReadAllSensors().ToList();
                _reports.Add(new MetricReport(reportTime, sensors));
                if (_reports.Count >= _batchSize)
                {
                    await FlushMetrics();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to collect metrics");
            }
        }

        private async Task FlushMetrics()
        {
            if (_reports.Count == 0)
            {
                return;
            }

            var reports = _reports.ToList();
            _reports.Clear();
            var metricCount = reports.Sum(x => x.Sensors.Count);
            var stopwatch = Stopwatch.StartNew();
            try
            {
                await _writer.ReportMetrics(reports);
                Logger.Info($"Sent {metricCount} metrics from {reports.Count} reports in {stopwatch.Elapsed.TotalMilliseconds}ms");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Unable to send {metricCount} metrics from {reports.Count} reports");
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            Logger.LogAction("stopping metric timer", () =>
            {
                _cancellation?.Cancel();

                if (_reportTask?.Wait(ShutdownTimeout) ?? true)
                {
                    FlushMetrics().GetAwaiter().GetResult();
                }
                else
                {
                    Logger.Warn($"Metric timer did not stop within {ShutdownTimeout.TotalSeconds}s");
                }

                _cancellation?.Dispose();
                _writer?.Dispose();
                _collector?.Dispose();
            });
        }
    }
}
