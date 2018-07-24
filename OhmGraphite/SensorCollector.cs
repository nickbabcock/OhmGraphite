using System.Collections.Concurrent;
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
        private readonly IVisitor _updateVisitor = new UpdateVisitor();

        private readonly ConcurrentDictionary<Identifier, object> _ids =
            new ConcurrentDictionary<Identifier, object>();

        public SensorCollector(Computer computer) => _computer = computer;
        public void Open()
        {
            foreach (var hardware in _computer.Hardware)
            {
                HardwareAdded(hardware);
            }

            _computer.HardwareAdded += HardwareAdded;
            _computer.HardwareRemoved += HardwareRemoved;
            _computer.Open();
        }

        public void Close()
        {
            _computer.HardwareRemoved -= HardwareRemoved;
            _computer.HardwareAdded -= HardwareAdded;
            foreach (var hardware in _computer.Hardware)
            {
                HardwareRemoved(hardware);
            }
            _computer.Close();
        }

        private void HardwareAdded(IHardware hardware)
        {
            if (!_ids.TryAdd(hardware.Identifier, hardware))
            {
                Logger.Debug("Hardware previously added: {0}", hardware.Identifier);
                return;
            }

            Logger.Debug("Hardware added: {0}", hardware.Identifier);
            hardware.SensorAdded += SensorAdded;
            hardware.SensorRemoved += SensorRemoved;
            foreach (var sensor in hardware.Sensors)
            {
                SensorAdded(sensor);
            }

            foreach (var sub in hardware.SubHardware)
            {
                HardwareAdded(sub);
            }
        }

        private void HardwareRemoved(IHardware hardware)
        {
            _ids.TryRemove(hardware.Identifier, out _);
            Logger.Debug("Hardware removed: {0}", hardware.Identifier);
            hardware.SensorAdded -= SensorAdded;
            hardware.SensorRemoved -= SensorRemoved;
            foreach (var sensor in hardware.Sensors)
            {
                SensorRemoved(sensor);
            }

            foreach (var sub in hardware.SubHardware)
            {
                HardwareRemoved(sub);
            }
        }

        private void SensorAdded(ISensor sensor)
        {
            Logger.Debug(!_ids.TryAdd(sensor.Identifier, sensor) ?
                    "Sensor previously added: {0}" : "Sensor added: {0}", sensor.Identifier);
        }

        private void SensorRemoved(ISensor sensor)
        {
            Logger.Debug("Sensor removed: {0}", sensor.Identifier);
            _ids.TryRemove(sensor.Identifier, out _);
        }

        public IEnumerable<ReportedValue> ReadAllSensors()
        {
            _computer.Accept(_updateVisitor);
            return _ids.Values.OfType<ISensor>().SelectMany(ReportedValues);
        }

        private static IEnumerable<ReportedValue> ReportedValues(ISensor sensor)
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