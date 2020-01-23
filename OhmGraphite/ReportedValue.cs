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
            string hwInstance,
            int sensorIndex)
        {
            Identifier = identifier;
            Sensor = sensor;
            Value = value;
            SensorType = sensorType;
            Hardware = hardware;
            HardwareType = hardwareType;
            SensorIndex = sensorIndex;
            HardwareInstance = hwInstance;
        }

        /// <summary>
        /// A globally unique identifier for each metric. Eg: /amdcpu/0/power/5. This
        /// identifier can be read as "The 6th power sensor on the 1st amd cpu"
        /// </summary>
        public string Identifier { get; }

        /// <summary>
        /// Descriptive name for sensor. Eg: CPU Core #10
        /// </summary>
        public string Sensor { get; }

        /// <summary>
        /// The reported sensor reading
        /// </summary>
        public float Value { get; }

        /// <summary>
        /// The type of sensor
        /// </summary>
        public SensorType SensorType { get; }

        /// <summary>
        /// Descriptive name for the hardware. Eg: AMD Ryzen 7 2700. Note
        /// that this name does not need to be unique among hardware types such
        /// as in multi-gpu or multi-hdd setups
        /// </summary>
        public string Hardware { get; }

        /// <summary>
        /// The type of hardware the sensor is monitoring
        /// </summary>
        public HardwareType HardwareType { get; }

        /// <summary>
        /// The index. The "5" in /amdcpu/0/power/5. There typically isn't
        /// ambiguity for sensors as they have differing names
        /// (else wouldn't they be measuring the same thing?)
        /// </summary>
        public int SensorIndex { get; }

        /// <summary>
        /// The disambiguation factor for same hardware (multi-gpu and multi-hdd).
        /// This is typically the index of the hardware found in the identifier
        /// (eg: the "0" in /amdcpu/0/power/5). It's not always the index, for
        /// NIC sensors, the NIC's GUID is used.
        /// </summary>
        public string HardwareInstance { get; }
    }
}
