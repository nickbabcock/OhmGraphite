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
        public static SensorType ToOwnSensor(this LibreHardwareMonitor.Hardware.SensorType s)
        {
            switch (s)
            {
                case LibreHardwareMonitor.Hardware.SensorType.Voltage:
                    return SensorType.Voltage;
                case LibreHardwareMonitor.Hardware.SensorType.Clock:
                    return SensorType.Clock;
                case LibreHardwareMonitor.Hardware.SensorType.Temperature:
                    return SensorType.Temperature;
                case LibreHardwareMonitor.Hardware.SensorType.Load:
                    return SensorType.Load; 
                case LibreHardwareMonitor.Hardware.SensorType.Frequency:
                    return SensorType.Frequency;
                case LibreHardwareMonitor.Hardware.SensorType.Fan:
                    return SensorType.Fan;  
                case LibreHardwareMonitor.Hardware.SensorType.Flow:
                    return SensorType.Flow;
                case LibreHardwareMonitor.Hardware.SensorType.Control:
                    return SensorType.Control;
                case LibreHardwareMonitor.Hardware.SensorType.Level:
                    return SensorType.Level;
                case LibreHardwareMonitor.Hardware.SensorType.Factor:
                    return SensorType.Factor;
                case LibreHardwareMonitor.Hardware.SensorType.Power:
                    return SensorType.Power;
                case LibreHardwareMonitor.Hardware.SensorType.Data:
                    return SensorType.Data;
                case LibreHardwareMonitor.Hardware.SensorType.SmallData:
                    return SensorType.SmallData;
                case LibreHardwareMonitor.Hardware.SensorType.Throughput:
                    return SensorType.Throughput;
                default:
                    throw new ArgumentOutOfRangeException(nameof(s), s, "unexpected hardware monitor sensor translation");
            }
        }

        public static HardwareType ToOwnHardware(this LibreHardwareMonitor.Hardware.HardwareType s)
        {
            switch (s)
            {
                case LibreHardwareMonitor.Hardware.HardwareType.Motherboard:
                    return HardwareType.Mainboard;
                case LibreHardwareMonitor.Hardware.HardwareType.SuperIO:
                    return HardwareType.SuperIO;
                case LibreHardwareMonitor.Hardware.HardwareType.AquaComputer:
                    return HardwareType.Aquacomputer;
                case LibreHardwareMonitor.Hardware.HardwareType.Cpu:
                    return HardwareType.CPU;
                case LibreHardwareMonitor.Hardware.HardwareType.Memory:
                    return HardwareType.RAM;
                case LibreHardwareMonitor.Hardware.HardwareType.GpuNvidia:
                    return HardwareType.GpuNvidia;
                case LibreHardwareMonitor.Hardware.HardwareType.GpuAmd:
                    return HardwareType.GpuAti;
                case LibreHardwareMonitor.Hardware.HardwareType.TBalancer:
                    return HardwareType.TBalancer;
                case LibreHardwareMonitor.Hardware.HardwareType.Heatmaster:
                    return HardwareType.Heatmaster;
                case LibreHardwareMonitor.Hardware.HardwareType.Storage:
                    return HardwareType.HDD;
                case LibreHardwareMonitor.Hardware.HardwareType.Network:
                    return HardwareType.NIC;
                default:
                    throw new ArgumentOutOfRangeException(nameof(s), s, "unexpected hardware monitor hardware translation");
            }
        }
    }
}
