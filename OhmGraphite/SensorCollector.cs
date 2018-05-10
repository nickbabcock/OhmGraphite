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

        public SensorCollector(Computer computer)
        {
            _computer = computer;
        }

        public void Open()
        {
            _computer.Open();
        }

        public void Close()
        {
            _computer.Close();
        }

        public IEnumerable<Sensor> ReadAllSensors()
        {
            return ReadHardware().SelectMany(ReadSensors);
        }

        private IEnumerable<IHardware> ReadHardware()
        {
            foreach (var hardware in _computer.Hardware)
            {
                yield return hardware;

                foreach (var subHardware in hardware.SubHardware) yield return subHardware;
            }
        }

        private static IEnumerable<Sensor> ReadSensors(IHardware hardware)
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
                    yield return new Sensor(id, sensor.Name, sensor.Value.Value);
                }
                else
                {
                    Logger.Debug($"{id} did not have a value");
                }
            }
        }
    }
}