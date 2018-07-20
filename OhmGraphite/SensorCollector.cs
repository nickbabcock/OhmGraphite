using System.Collections.Generic;
using System.Linq;
using NLog;
using OpenHardwareMonitor.Hardware;

namespace OhmGraphite
{
    public class SensorCollector
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly Computer _computer;

        public SensorCollector(Computer computer) => _computer = computer;
        public void Open() => _computer.Open();
        public void Close() => _computer.Close();
        public IEnumerable<ReportedValue> ReadAllSensors() =>
            _computer
                .Hardware
                .SelectMany(ReadHardware)
                .SelectMany(ReadSensors);

        private static IEnumerable<IHardware> ReadHardware(IHardware hardware)
        {
            yield return hardware;
            foreach (var subHardware in hardware.SubHardware)
            {
                foreach (var ware in ReadHardware(subHardware))
                {
                    yield return ware;
                }
            }
        }

        private static IEnumerable<ReportedValue> ReadSensors(IHardware hardware)
        {
            hardware.Update();
            foreach (var sensor in hardware.Sensors)
            {
                string id = sensor.Identifier.ToString();

                // Only report a value if the sensor was able to get a value
                // as 0 is different than "didn't read". For example, are the
                // fans really spinning at 0 RPM or was the value not read.
                if (sensor.Value.HasValue)
                {
                    yield return new ReportedValue(id,
                        sensor.Name,
                        sensor.Value.Value,
                        sensor.SensorType,
                        sensor.Hardware.Name,
                        sensor.Hardware.HardwareType,
                        sensor.Index);
                }
                else
                {
                    Logger.Debug($"{id} did not have a value");
                }
            }
        }
    }
}