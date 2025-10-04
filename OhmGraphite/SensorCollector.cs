using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NLog;
using LibreHardwareMonitor.Hardware;
using LibreHardwareMonitor.Hardware.Storage;
using System;
using LibreHardwareMonitor.PawnIo;

namespace OhmGraphite
{
    public class SensorCollector : IGiveSensors
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly Computer _computer;
        private readonly MetricConfig _config;
        private readonly IVisitor _updateVisitor = new UpdateVisitor();

        private readonly ConcurrentDictionary<Identifier, object> _ids =
            new ConcurrentDictionary<Identifier, object>();

        private readonly ConcurrentDictionary<Identifier, OhmNvme> _nvmes =
            new ConcurrentDictionary<Identifier, OhmNvme>();

        public SensorCollector(Computer computer, MetricConfig config)
        {
            _computer = computer;
            _config = config;
        }

        public void Open()
        {
            foreach (var hardware in _computer.Hardware)
            {
                HardwareAdded(hardware);
            }

            _computer.HardwareAdded += HardwareAdded;
            _computer.HardwareRemoved += HardwareRemoved;
            _computer.Open();

            if (PawnIo.IsInstalled)
            {
                Logger.Info("Detected PawnIo version ({0}) installed", PawnIo.Version());
            }
            else
            {
                Logger.Warn("PawnIo is not installed. Limited sensors detecting capabilities.");
            }
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

        public void Start() => Open();
        public void Dispose() => Close();

        private void HardwareAdded(IHardware hardware)
        {
            if (!_ids.TryAdd(hardware.Identifier, hardware))
            {
                Logger.Debug("Hardware previously added: {0}", hardware.Identifier);
                return;
            }

            Logger.Debug(
                "Hardware added: {0} (Type: {1}, Class: {2})",
                hardware.Identifier,
                hardware.HardwareType,
                hardware.GetType().Name
            );

            hardware.SensorAdded += SensorAdded;
            hardware.SensorRemoved += SensorRemoved;
            foreach (var sensor in hardware.Sensors)
            {
                SensorAdded(sensor);
            }

            if (hardware is NVMeGeneric nvme)
            {
                var ohmNvme = new OhmNvme(nvme);
                _nvmes.TryAdd(hardware.Identifier, ohmNvme);
                SensorAdded(ohmNvme.MediaErrors);
                SensorAdded(ohmNvme.PowerCycles);
                SensorAdded(ohmNvme.ErrorInfoLogEntryCount);
                SensorAdded(ohmNvme.UnsafeShutdowns);
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

            if (_nvmes.TryRemove(hardware.Identifier, out OhmNvme ohmNvme))
            {
                SensorRemoved(ohmNvme.MediaErrors);
                SensorRemoved(ohmNvme.PowerCycles);
                SensorRemoved(ohmNvme.ErrorInfoLogEntryCount);
                SensorRemoved(ohmNvme.UnsafeShutdowns);
            }

            foreach (var sub in hardware.SubHardware)
            {
                HardwareRemoved(sub);
            }
        }

        private void SensorAdded(ISensor sensor)
        {
            sensor.ValuesTimeWindow = TimeSpan.Zero;
            var added = _ids.TryAdd(sensor.Identifier, sensor);
            var msg = added ? "Sensor added: {0} \"{1}\"" : "Sensor previously added: {0} \"{1}\"";
            Logger.Info(msg, sensor.Identifier, SensorName(sensor));
        }

        private void SensorRemoved(ISensor sensor)
        {
            Logger.Debug("Sensor removed: {0}", sensor.Identifier);
            _ids.TryRemove(sensor.Identifier, out _);
        }

        public IEnumerable<ReportedValue> ReadAllSensors()
        {
            _computer.Accept(_updateVisitor);
            foreach (var nvme in _nvmes.Values)
            {
                nvme.Update();
            }

            return _ids.Values.OfType<ISensor>().SelectMany(ReportedValues);
        }

        private string SensorName(ISensor sensor)
        {
            // Remove hardware index indentifier for motherboards to preserve
            // librehardwaremonitor pre-0.9.3 behavior
            // https://github.com/nickbabcock/OhmGraphite/pull/433
            return sensor.Hardware.HardwareType == LibreHardwareMonitor.Hardware.HardwareType.Motherboard
                ? string.Join("/", sensor.Name.Split('/').Where((x, i) => i != 2))
                : sensor.Name;
        }

        private IEnumerable<ReportedValue> ReportedValues(ISensor sensor)
        {
            string sensorName = SensorName(sensor);
            string id = sensor.Identifier.ToString();

            // Only report a value if the sensor was able to get a value
            // as 0 is different than "didn't read". For example, are the
            // fans really spinning at 0 RPM or was the value not read.
            if (!sensor.Value.HasValue)
            {
                Logger.Debug($"{id} did not have a value");
            }
            else if (float.IsNaN(sensor.Value.Value))
            {
                Logger.Debug($"{id} had a NaN value");
            }
            else if (float.IsInfinity(sensor.Value.Value))
            {
                Logger.Debug($"{id} had an infinite value");
            }
            else if (!_config.IsHidden(sensor.Identifier.ToString()) && !_config.IsHidden(sensorName))
            {
                var hwInstance = ExtractHardwareInstance(sensor.Hardware.Identifier.ToString());
                var name = _config.TryGetAlias(sensor.Identifier.ToString(), out string alias) ? alias : sensorName;
                var result = new ReportedValue(id,
                    name,
                    sensor.Value.Value,
                    sensor.SensorType.ToOwnSensor(),
                    sensor.Hardware.Name,
                    sensor.Hardware.HardwareType.ToOwnHardware(),
                    hwInstance,
                    sensor.Index);

                Logger.Trace(
                    "Value: ID {id}, sensor: {sensor}, value: {value}, sensor type: {sensorType}, hardware: {hardware}, hardware type: {hardwareType}, sensor index: {sensorIndex}, hardware instance: {hardwareInstance}",
                    result.Identifier,
                    result.Sensor,
                    result.Value,
                    result.SensorType,
                    result.Hardware,
                    result.HardwareType,
                    result.SensorIndex,
                    result.HardwareInstance
                );

                yield return result;
            }
        }

        public static string ExtractHardwareInstance(string hwInstance)
        {
            var ind = hwInstance.LastIndexOf('/');
            hwInstance = hwInstance.Substring(ind + 1);
            hwInstance = hwInstance.Replace("%7B", "").Replace("%7D", "");
            return hwInstance;
        }
    }
}