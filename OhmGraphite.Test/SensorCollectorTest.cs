using System.Linq;
using System.Runtime.InteropServices;
using LibreHardwareMonitor.Hardware;
using Xunit;

namespace OhmGraphite.Test
{
    public class SensorCollectorTest
    {
        [Fact]
        public void SensorsAddedWhenHardwareAdded()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return;
            }

            var computer = new Computer();
            using var collector = new SensorCollector(computer, MetricConfig.ParseAppSettings(new BlankConfig()));

            collector.Open();
            var unused = collector.ReadAllSensors().Count();

            computer.IsCpuEnabled = true;
            computer.IsMotherboardEnabled = true;
            computer.IsStorageEnabled = true;
            computer.IsMemoryEnabled = true;

            var addedCount = collector.ReadAllSensors().Count();

            // On CI platforms there may be no detected hardware
            if (addedCount <= 0)
            {
                return;
            }

            computer.IsCpuEnabled = false;
            computer.IsMotherboardEnabled = false;
            computer.IsStorageEnabled = false;
            computer.IsMemoryEnabled = false;

            var removedCount = collector.ReadAllSensors().Count();
            Assert.True(addedCount > removedCount, "addedCount > removedCount");
        }

        [Theory]
        [InlineData("/amdcpu/0", "0")]
        [InlineData("/nvme/1", "1")]
        [InlineData("/ram", "ram")]
        [InlineData("/nct6792d/0", "0")]
        [InlineData("/nic/%7BDBC40827-A257-41FA-84F5-ACBB6A148017%7D", "DBC40827-A257-41FA-84F5-ACBB6A148017")]
        public void ExtractHardwareInstance_ReturnsCorrectValue(string input, string expected)
        {
            var actual = SensorCollector.ExtractHardwareInstance(input);
            Assert.Equal(expected, actual);
        }
    }
}
