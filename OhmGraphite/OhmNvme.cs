using System.Linq;
using DiskInfoToolkit;
using DiskInfoToolkit.Interop.Enums;
using LibreHardwareMonitor.Hardware;
using LibreHardwareMonitor.Hardware.Storage;

namespace OhmGraphite
{
    // LibreHardwareMonitor doesn't expose all the SMART attributes we want on NVMe storage
    // so we create our own pseudo-hardware wrapper that exposes some important factors.
    internal class OhmNvme
    {
        private readonly StorageDevice _nvme;

        public OhmNvme(StorageDevice nvme)
        {
            _nvme = nvme;
            var factor = LibreHardwareMonitor.Hardware.SensorType.Factor.ToString().ToLowerInvariant();
            ErrorInfoLogEntryCount = new OhmSensor
            {
                Identifier = new Identifier(nvme.Identifier, factor, "error_info_log_entries"),
                Name = "Error Info Log Entries",
                Hardware = nvme,
                SensorType = LibreHardwareMonitor.Hardware.SensorType.Factor,
            };

            MediaErrors = new OhmSensor
            {
                Identifier = new Identifier(nvme.Identifier, factor, "media_errors"),
                Name = "Media Errors",
                Hardware = nvme,
                SensorType = LibreHardwareMonitor.Hardware.SensorType.Factor,
            };

            PowerCycles = new OhmSensor
            {
                Identifier = new Identifier(nvme.Identifier, factor, "power_cycles"),
                Name = "Power Cycles",
                Hardware = nvme,
                SensorType = LibreHardwareMonitor.Hardware.SensorType.Factor,
            };

            UnsafeShutdowns = new OhmSensor
            {
                Identifier = new Identifier(nvme.Identifier, factor, "unsafe_shutdowns"),
                Name = "Unsafe Shutdowns",
                Hardware = nvme,
                SensorType = LibreHardwareMonitor.Hardware.SensorType.Factor,
            };
        }

        public OhmSensor UnsafeShutdowns { get; }

        public OhmSensor PowerCycles { get; }

        public OhmSensor MediaErrors { get; }

        public OhmSensor ErrorInfoLogEntryCount { get; }

        public void Update()
        {
            var smart = _nvme?.Storage?.Smart;
            ErrorInfoLogEntryCount.Value = SmartValue(smart, SmartAttributeType.NumberOfErrorInformationLogEntries);
            MediaErrors.Value = SmartValue(smart, SmartAttributeType.MediaAndDataIntegrityErrors);
            PowerCycles.Value = SmartValue(smart, SmartAttributeType.PowerCycleCount);
            UnsafeShutdowns.Value = SmartValue(smart, SmartAttributeType.UnsafeShutdownCount);
        }

        private static float? SmartValue(SmartInfo smart, SmartAttributeType type)
        {
            return smart?.SmartAttributes
                ?.FirstOrDefault(attribute => attribute.Info.Type == type)
                ?.Attribute.RawValueULong;
        }
    }
}
