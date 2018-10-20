using System.Linq;
using OpenHardwareMonitor.Hardware;
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

                computer.CPUEnabled = true;
                computer.MainboardEnabled = true;
                computer.HDDEnabled = true;
                computer.RAMEnabled = true;

                var addedCount = collector.ReadAllSensors().Count();

                // On CI platforms there may be no detected hardware
                if (addedCount <= 0)
                {
                    return;
                }

                computer.CPUEnabled = false;
                computer.MainboardEnabled = false;
                computer.HDDEnabled = false;
                computer.RAMEnabled = false;

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
