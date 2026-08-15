using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace OhmGraphite.Test
{
    public class MetricTimerTest
    {
        [Fact]
        public async Task SendsReportsWhenBatchIsFull()
        {
            var collector = new RecordingCollector();
            var writer = new RecordingWriter();
            using var timer = new MetricTimer(TimeSpan.FromHours(1), 3, collector, writer);
            var times = new[]
            {
                new DateTime(2026, 1, 1, 0, 0, 1, DateTimeKind.Utc),
                new DateTime(2026, 1, 1, 0, 0, 2, DateTimeKind.Utc),
                new DateTime(2026, 1, 1, 0, 0, 3, DateTimeKind.Utc),
            };

            await timer.CollectMetrics(times[0]);
            await timer.CollectMetrics(times[1]);

            Assert.Empty(writer.Batches);
            Assert.Equal(2, collector.ReadCount);

            await timer.CollectMetrics(times[2]);

            var batch = Assert.Single(writer.Batches);
            Assert.Equal(times, batch.Select(x => x.ReportTime));
            Assert.All(batch, report => Assert.Equal(3, report.Sensors.Count));
            Assert.Equal(3, collector.ReadCount);
        }

        [Fact]
        public async Task BatchSizeOneSendsImmediately()
        {
            var writer = new RecordingWriter();
            using var timer = new MetricTimer(TimeSpan.FromHours(1), 1, new RecordingCollector(), writer);

            await timer.CollectMetrics(DateTime.UtcNow);

            Assert.Single(writer.Batches);
        }

        [Fact]
        public async Task FailedBatchIsDropped()
        {
            var writer = new RecordingWriter(failures: 1);
            using var timer = new MetricTimer(TimeSpan.FromHours(1), 2, new RecordingCollector(), writer);
            var first = new DateTime(2026, 1, 1, 0, 0, 1, DateTimeKind.Utc);
            var second = first.AddSeconds(1);
            var third = second.AddSeconds(1);
            var fourth = third.AddSeconds(1);

            await timer.CollectMetrics(first);
            await timer.CollectMetrics(second);
            await timer.CollectMetrics(third);

            Assert.Equal(1, writer.Attempts);
            Assert.Empty(writer.Batches);

            await timer.CollectMetrics(fourth);

            Assert.Equal(2, writer.Attempts);
            var batch = Assert.Single(writer.Batches);
            Assert.Equal(new[] { third, fourth }, batch.Select(x => x.ReportTime));
        }

        [Fact]
        public async Task DisposeFlushesPartialBatch()
        {
            var writer = new RecordingWriter();
            var timer = new MetricTimer(TimeSpan.FromHours(1), 3, new RecordingCollector(), writer);
            var reportTime = new DateTime(2026, 1, 1, 0, 0, 1, DateTimeKind.Utc);
            await timer.CollectMetrics(reportTime);

            timer.Dispose();

            var batch = Assert.Single(writer.Batches);
            Assert.Equal(reportTime, Assert.Single(batch).ReportTime);
            Assert.True(writer.IsDisposed);
        }

        private sealed class RecordingCollector : IGiveSensors
        {
            public int ReadCount { get; private set; }

            public IEnumerable<ReportedValue> ReadAllSensors()
            {
                ReadCount++;
                return TestSensorCreator.Values();
            }

            public void Start()
            {
            }

            public void Dispose()
            {
            }
        }

        private sealed class RecordingWriter : IWriteMetrics
        {
            private int _failures;

            public RecordingWriter(int failures = 0)
            {
                _failures = failures;
            }

            public int Attempts { get; private set; }
            public List<IReadOnlyList<MetricReport>> Batches { get; } = new List<IReadOnlyList<MetricReport>>();
            public bool IsDisposed { get; private set; }

            public Task ReportMetrics(IEnumerable<MetricReport> reports)
            {
                Attempts++;
                if (_failures > 0)
                {
                    _failures--;
                    throw new InvalidOperationException("Test failure");
                }

                Batches.Add(reports.ToList());
                return Task.CompletedTask;
            }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }
    }
}
