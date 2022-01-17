using LibreHardwareMonitor.Hardware;
using LibreHardwareMonitor.Hardware.Storage;

namespace OhmGraphite
{
    // LibreHardwareMonitor doesn't expose all the SMART attributes on a generic NVMe drive
    // so we create our own pseudo-hardware class that exposes some important factors
    internal class OhmNvme
    {
        private readonly NVMeGeneric _nvme;

        public OhmNvme(NVMeGeneric nvme)
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
            var health = _nvme?.Smart?.GetHealthInfo();
            ErrorInfoLogEntryCount.Value = health?.ErrorInfoLogEntryCount;
            MediaErrors.Value = health?.MediaErrors;
            PowerCycles.Value = health?.PowerCycle;
            UnsafeShutdowns.Value = health?.UnsafeShutdowns;
        }
    }
}
