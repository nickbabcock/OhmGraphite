using System;

namespace OhmGraphite
{
    /// <summary>
    /// Insulate ourselves from hardware monitor's API changes so that our metric names / values
    /// don't unexpectedly change when a user updates OhmGraphite and their dashboard breaks
    /// </summary>
    public enum SensorType
    {
        Voltage, // V
        Clock, // MHz
        Temperature, // °C
        Load, // %
        Frequency, // Hz
        Fan, // RPM
        Flow, // L/h
        Control, // %
        Level, // %
        Factor, // 1
        Power, // W
        Data, // GB = 2^30 Bytes    
        SmallData, // MB = 2^20 Bytes
        Throughput, // B/s
    }

    /// <summary>
    /// Insulate ourselves from hardware monitor's API changes so that our metric names / values
    /// don't unexpectedly change when a user updates OhmGraphite and their dashboard breaks
    /// </summary>
    public enum HardwareType
    {
        Mainboard,
        SuperIO,
        Aquacomputer,
        CPU,
        RAM,
        GpuNvidia,
        GpuAti,
        TBalancer,
        Heatmaster,
        HDD,
        NIC
    }

    public static class TranslationExtension {
        public static SensorType ToOwnSensor(this OpenHardwareMonitor.Hardware.SensorType s)
        {
            switch (s)
            {
                case OpenHardwareMonitor.Hardware.SensorType.Voltage:
                    return SensorType.Voltage;
                case OpenHardwareMonitor.Hardware.SensorType.Clock:
                    return SensorType.Clock;
                case OpenHardwareMonitor.Hardware.SensorType.Temperature:
                    return SensorType.Temperature;
                case OpenHardwareMonitor.Hardware.SensorType.Load:
                    return SensorType.Load; 
                case OpenHardwareMonitor.Hardware.SensorType.Frequency:
                    return SensorType.Frequency;
                case OpenHardwareMonitor.Hardware.SensorType.Fan:
                    return SensorType.Fan;  
                case OpenHardwareMonitor.Hardware.SensorType.Flow:
                    return SensorType.Flow;
                case OpenHardwareMonitor.Hardware.SensorType.Control:
                    return SensorType.Control;
                case OpenHardwareMonitor.Hardware.SensorType.Level:
                    return SensorType.Level;
                case OpenHardwareMonitor.Hardware.SensorType.Factor:
                    return SensorType.Factor;
                case OpenHardwareMonitor.Hardware.SensorType.Power:
                    return SensorType.Power;
                case OpenHardwareMonitor.Hardware.SensorType.Data:
                    return SensorType.Data;
                case OpenHardwareMonitor.Hardware.SensorType.SmallData:
                    return SensorType.SmallData;
                case OpenHardwareMonitor.Hardware.SensorType.Throughput:
                    return SensorType.Throughput;
                default:
                    throw new ArgumentOutOfRangeException(nameof(s), s, "unexpected hardware monitor sensor translation");
            }
        }

        public static HardwareType ToOwnHardware(this OpenHardwareMonitor.Hardware.HardwareType s)
        {
            switch (s)
            {
                case OpenHardwareMonitor.Hardware.HardwareType.Mainboard:
                    return HardwareType.Mainboard;
                case OpenHardwareMonitor.Hardware.HardwareType.SuperIO:
                    return HardwareType.SuperIO;
                case OpenHardwareMonitor.Hardware.HardwareType.Aquacomputer:
                    return HardwareType.Aquacomputer;
                case OpenHardwareMonitor.Hardware.HardwareType.CPU:
                    return HardwareType.CPU;
                case OpenHardwareMonitor.Hardware.HardwareType.RAM:
                    return HardwareType.RAM;
                case OpenHardwareMonitor.Hardware.HardwareType.GpuNvidia:
                    return HardwareType.GpuNvidia;
                case OpenHardwareMonitor.Hardware.HardwareType.GpuAti:
                    return HardwareType.GpuAti;
                case OpenHardwareMonitor.Hardware.HardwareType.TBalancer:
                    return HardwareType.TBalancer;
                case OpenHardwareMonitor.Hardware.HardwareType.Heatmaster:
                    return HardwareType.Heatmaster;
                case OpenHardwareMonitor.Hardware.HardwareType.HDD:
                    return HardwareType.HDD;
                case OpenHardwareMonitor.Hardware.HardwareType.NIC:
                    return HardwareType.NIC;
                default:
                    throw new ArgumentOutOfRangeException(nameof(s), s, "unexpected hardware monitor hardware translation");
            }
        }
    }
}
