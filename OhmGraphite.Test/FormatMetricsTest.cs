using System;
using System.Globalization;
using System.Threading;
using OpenHardwareMonitor.Hardware;
using Xunit;

namespace OhmGraphite.Test
{
    public class FormatMetricsTest
    {
        [Fact]
        public void FormatGraphiteIdentifier()
        {
            var writer = new GraphiteWriter("localhost", 2003, "MY-PC", false);
            var epoch = new DateTimeOffset(new DateTime(2001, 1, 13), TimeSpan.Zero).ToUnixTimeSeconds();
            var sensor = new ReportedValue("/my/cpu/identifier/1", "voltage", 1.06f, SensorType.Voltage, "cpu", HardwareType.CPU, "identifier", 1);
            string actual = writer.FormatGraphiteData(epoch, sensor);
            Assert.Equal("ohm.MY-PC.my.cpu.identifier.voltage 1.06 979344000", actual);
        }

        [Fact]
        public void FormatGraphiteWithSpecialCharacters()
        {
            var writer = new GraphiteWriter("localhost", 2003, "MY-PC", false);
            var epoch = new DateTimeOffset(new DateTime(2001, 1, 13), TimeSpan.Zero).ToUnixTimeSeconds();
            var sensor = new ReportedValue("/nic/{my-guid}/throughput/7", "Bluetooth Network Connection 2", 1.06f, SensorType.Throughput, "cpu", HardwareType.NIC, "{my-guid}", 7);
            string actual = writer.FormatGraphiteData(epoch, sensor);
            Assert.Equal("ohm.MY-PC.nic.my-guid.throughput.bluetoothnetworkconnection2 1.06 979344000", actual);
        }

        [Fact]
        public void FormatCultureInvariant()
        {
            var writer = new GraphiteWriter("localhost", 2003, "MY-PC", false);
            CultureInfo original = Thread.CurrentThread.CurrentCulture;
            try
            {
                // de-DE culture will format 1.06 as 1,06 which graphite doesn't like
                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");

                var epoch = new DateTimeOffset(new DateTime(2001, 1, 13), TimeSpan.Zero).ToUnixTimeSeconds();
                var sensor = new ReportedValue("/my/cpu/identifier/1", "voltage", 1.06f, SensorType.Voltage, "cpu", HardwareType.CPU, "identifier", 1);
                string actual = writer.FormatGraphiteData(epoch, sensor);
                Assert.Equal("ohm.MY-PC.my.cpu.identifier.voltage 1.06 979344000", actual);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = original;
            }
        }

        [Fact]
        public void FormatGraphiteTags()
        {
            var writer = new GraphiteWriter("localhost", 2003, "MY-PC", true);
            var epoch = new DateTimeOffset(new DateTime(2001, 1, 13), TimeSpan.Zero).ToUnixTimeSeconds();
            var sensor = new ReportedValue("/my/cpu/identifier/1", "voltage", 1.06f, SensorType.Voltage, "cpu", HardwareType.CPU, "identifier", 1);
            string actual = writer.FormatGraphiteData(epoch, sensor);
            Assert.Equal("ohm.MY-PC.my.cpu.identifier.voltage;host=MY-PC;app=ohm;hardware=cpu;hardware_type=CPU;sensor_type=Voltage;sensor_index=1;raw_name=voltage 1.06 979344000", actual);
        }

        [Fact]
        public void FormatTagsCultureInvariant()
        {
            var writer = new GraphiteWriter("localhost", 2003, "MY-PC", true);
            CultureInfo original = Thread.CurrentThread.CurrentCulture;
            try
            {
                // de-DE culture will format 1.06 as 1,06 which graphite doesn't like
                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");

                var epoch = new DateTimeOffset(new DateTime(2001, 1, 13), TimeSpan.Zero).ToUnixTimeSeconds();
                var sensor = new ReportedValue("/my/cpu/identifier/1", "voltage", 1.06f, SensorType.Voltage, "cpu", HardwareType.CPU, "identifier", 1);
                string actual = writer.FormatGraphiteData(epoch, sensor);
                Assert.Contains("1.06", actual);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = original;
            }
        }
    }
}
