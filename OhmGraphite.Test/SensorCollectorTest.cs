using System.Linq;
using LibreHardwareMonitor.Hardware;
using Xunit;

namespace OhmGraphite.Test
{
    public class SensorCollectorTest
    {
        [Fact]
        public void SensorsAddedWhenHardwareAdded()
        {
            var computer = new Computer();
            var collector = new SensorCollector(computer);

            try
            {
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
            finally
            {
                collector.Close();
            }
        }
    }
}
