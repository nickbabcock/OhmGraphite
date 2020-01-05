using System;
using Xunit;

namespace OhmGraphite.Test
{
    public class TranslationTest
    {
        [Fact]
        public void TranslateAllSensorTypes()
        {
            var values = Enum.GetValues(typeof(OpenHardwareMonitor.Hardware.SensorType));
            foreach (var value in values)
            {
                Assert.IsType<SensorType>(((OpenHardwareMonitor.Hardware.SensorType)value).ToOwnSensor());
            }
        }

        [Fact]
        public void TranslateAllHardwareTypes()
        {
            var values = Enum.GetValues(typeof(OpenHardwareMonitor.Hardware.HardwareType));
            foreach (var value in values)
            {
                Assert.IsType<HardwareType>(((OpenHardwareMonitor.Hardware.HardwareType)value).ToOwnHardware());
            }
        }
    }
}
