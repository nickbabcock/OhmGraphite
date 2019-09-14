using System.Collections.Generic;
using OpenHardwareMonitor.Hardware;

namespace OhmGraphite.Test
{
    class TestSensorCreator : IGiveSensors
    {
        public static IEnumerable<ReportedValue> Values()
        {
            yield return new ReportedValue("/intelcpu/0/temperature/0", "CPU Core #1", 20, SensorType.Temperature, "Intel Core i7-6700K", HardwareType.CPU, 0);
            yield return new ReportedValue("/intelcpu/0/temperature/1", "CPU Core #2", 15, SensorType.Temperature, "Intel Core i7-6700K", HardwareType.CPU, 1);
            yield return new ReportedValue("/intelcpu/0/temperature/2", "CPU Core #3", 10, SensorType.Temperature, "Intel Core i7-6700K", HardwareType.CPU, 2);
        }

        public IEnumerable<ReportedValue> ReadAllSensors() => Values();

        public void Start()
        {
        }

        public void Dispose()
        {
        }
    }
}
