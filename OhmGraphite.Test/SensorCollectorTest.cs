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
                computer.FanControllerEnabled = true;
                computer.MainboardEnabled = true;
                computer.FanControllerEnabled = true;
                computer.HDDEnabled = true;
                computer.RAMEnabled = true;

                var addedCount = collector.ReadAllSensors().Count();
                Assert.True(addedCount > 0, "addedCount > 0");

                computer.CPUEnabled = false;
                computer.FanControllerEnabled = false;
                computer.MainboardEnabled = false;
                computer.FanControllerEnabled = false;
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
