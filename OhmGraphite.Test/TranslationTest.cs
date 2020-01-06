using System;
using Xunit;

namespace OhmGraphite.Test
{
    public class TranslationTest
    {
        [Fact]
        public void TranslateAllSensorTypes()
        {
            var values = Enum.GetValues(typeof(LibreHardwareMonitor.Hardware.SensorType));
            foreach (var value in values)
            {
                Assert.IsType<SensorType>(((LibreHardwareMonitor.Hardware.SensorType)value).ToOwnSensor());
            }
        }

        [Fact]
        public void TranslateAllHardwareTypes()
        {
            var values = Enum.GetValues(typeof(LibreHardwareMonitor.Hardware.HardwareType));
            foreach (var value in values)
            {
                Assert.IsType<HardwareType>(((LibreHardwareMonitor.Hardware.HardwareType)value).ToOwnHardware());
            }
        }
    }
}
