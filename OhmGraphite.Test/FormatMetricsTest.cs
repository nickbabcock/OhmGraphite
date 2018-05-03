using System;
using System.Globalization;
using System.Threading;
using Xunit;

namespace OhmGraphite.Test
{
    public class FormatMetricsTest
    {
        [Fact]
        public void FormatGraphiteIdentifier()
        {
            var epoch = new DateTimeOffset(new DateTime(2001, 1, 13), TimeSpan.Zero).ToUnixTimeSeconds();
            var sensor = new Sensor("my.cpu.identifier", "voltage", 1.06f);
            string actual = MetricTimer.FormatGraphiteData("MY-PC", epoch, sensor);
            Assert.Equal("ohm.MY-PC.my.cpu.identifier.voltage 1.06 979344000", actual);
        }

        [Fact]
        public void FormatCultureInvariant()
        {
            CultureInfo original = Thread.CurrentThread.CurrentCulture;
            try
            {
                // de-DE culture will format 1.06 as 1,06 which graphite doesn't like
                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");

                var epoch = new DateTimeOffset(new DateTime(2001, 1, 13), TimeSpan.Zero).ToUnixTimeSeconds();
                var sensor = new Sensor("my.cpu.identifier", "voltage", 1.06f);
                string actual = MetricTimer.FormatGraphiteData("MY-PC", epoch, sensor);
                Assert.Equal("ohm.MY-PC.my.cpu.identifier.voltage 1.06 979344000", actual);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = original;
            }
        }
    }
}
