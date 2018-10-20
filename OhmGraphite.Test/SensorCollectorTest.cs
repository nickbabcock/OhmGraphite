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
            var computer = new Computer()
            {
                CPUEnabled = true,
                FanControllerEnabled = false,
                GPUEnabled = false,
                HDDEnabled = false,
                MainboardEnabled = false,
                RAMEnabled = false
            };

            var collector = new SensorCollector(computer);
            try
            {
                collector.Open();
                var firstCount = collector.ReadAllSensors().Count();
                Assert.True(firstCount > 0, "firstCount > 0");

                computer.MainboardEnabled = true;
                computer.FanControllerEnabled = true;
                computer.HDDEnabled = true;
                computer.RAMEnabled = true;

                var secondCount = collector.ReadAllSensors().Count();
                Assert.True(secondCount > firstCount, "secondCount > firstCount");

                computer.MainboardEnabled = false;
                computer.FanControllerEnabled = false;
                computer.HDDEnabled = false;
                computer.RAMEnabled = false;

                var thirdCount = collector.ReadAllSensors().Count();
                Assert.True(thirdCount < secondCount, "thirdCount < secondCount");
                Assert.True(thirdCount > 0, "thirdCount > 0");
            }
            finally
            {
                collector.Close();
            }
        }
    }
}
