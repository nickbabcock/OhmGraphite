using OpenHardwareMonitor.Hardware;

namespace OhmGraphite
{
    public class ReportedValue
    {
        public ReportedValue(string identifier,
            string sensor,
            float value,
            SensorType sensorType,
            string hardware,
            HardwareType hardwareType,
            int sensorIndex)
        {
            Identifier = identifier;
            Sensor = sensor;
            Value = value;
            SensorType = sensorType;
            Hardware = hardware;
            HardwareType = hardwareType;
            SensorIndex = sensorIndex;
        }

        public string Identifier { get; }
        public string Sensor { get; }
        public float Value { get; }
        public SensorType SensorType { get; }
        public string Hardware { get; }
        public HardwareType HardwareType { get; }
        public int SensorIndex { get; }
    }
}