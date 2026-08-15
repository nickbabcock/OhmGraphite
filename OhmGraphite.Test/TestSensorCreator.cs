using System;
using System.Collections.Generic;
using System.Linq;

namespace OhmGraphite.Test
{
    class TestSensorCreator : IGiveSensors
    {
        public static IEnumerable<ReportedValue> Values()
        {
            yield return new ReportedValue("/intelcpu/0/temperature/0", "CPU Core #1", 20, SensorType.Temperature, "Intel Core i7-6700K", HardwareType.CPU, "0", 0);
            yield return new ReportedValue("/intelcpu/0/temperature/1", "CPU Core #2", 15, SensorType.Temperature, "Intel Core i7-6700K", HardwareType.CPU, "0", 1);
            yield return new ReportedValue("/intelcpu/0/temperature/2", "CPU Core #3", 10, SensorType.Temperature, "Intel Core i7-6700K", HardwareType.CPU, "0", 2);
        }

        public static IEnumerable<MetricReport> Reports(params DateTime[] reportTimes)
        {
            foreach (var reportTime in reportTimes)
            {
                yield return new MetricReport(reportTime, Values().ToList());
            }
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
