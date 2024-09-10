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
        Current, // A
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
        TimeSpan, // Seconds
        Energy, // milliwatt-hour (mWh)
        Noise, // dBA
        Humidity, // %
        Conductivity, // µS/cm
    }

    /// <summary>
    /// Insulate ourselves from hardware monitor's API changes so that our metric names / values
    /// don't unexpectedly change when a user updates OhmGraphite and their dashboard breaks
    /// </summary>
    public enum HardwareType
    {
        Mainboard,
        SuperIO,
        CPU,
        RAM,
        GpuNvidia,
        GpuAti,
        GpuIntel,
        Cooler,
        HDD,
        NIC,
        PSU,
        EmbeddedController,
        Battery,
    }

    public static class TranslationExtension
    {
        public static SensorType ToOwnSensor(this LibreHardwareMonitor.Hardware.SensorType s)
        {
            switch (s)
            {
                case LibreHardwareMonitor.Hardware.SensorType.Voltage:
                    return SensorType.Voltage;
                case LibreHardwareMonitor.Hardware.SensorType.Current:
                    return SensorType.Current;
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
                case LibreHardwareMonitor.Hardware.SensorType.TimeSpan:
                    return SensorType.TimeSpan;
                case LibreHardwareMonitor.Hardware.SensorType.Energy:
                    return SensorType.Energy;
                case LibreHardwareMonitor.Hardware.SensorType.Noise:
                    return SensorType.Noise;
                case LibreHardwareMonitor.Hardware.SensorType.Humidity:
                    return SensorType.Humidity;
                case LibreHardwareMonitor.Hardware.SensorType.Conductivity:
                    return SensorType.Conductivity;
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
                case LibreHardwareMonitor.Hardware.HardwareType.Cpu:
                    return HardwareType.CPU;
                case LibreHardwareMonitor.Hardware.HardwareType.Memory:
                    return HardwareType.RAM;
                case LibreHardwareMonitor.Hardware.HardwareType.GpuNvidia:
                    return HardwareType.GpuNvidia;
                case LibreHardwareMonitor.Hardware.HardwareType.GpuAmd:
                    return HardwareType.GpuAti;
                case LibreHardwareMonitor.Hardware.HardwareType.GpuIntel:
                    return HardwareType.GpuIntel;
                case LibreHardwareMonitor.Hardware.HardwareType.Cooler:
                    return HardwareType.Cooler;
                case LibreHardwareMonitor.Hardware.HardwareType.Storage:
                    return HardwareType.HDD;
                case LibreHardwareMonitor.Hardware.HardwareType.Network:
                    return HardwareType.NIC;
                case LibreHardwareMonitor.Hardware.HardwareType.Psu:
                    return HardwareType.PSU;
                case LibreHardwareMonitor.Hardware.HardwareType.EmbeddedController:
                    return HardwareType.EmbeddedController;
                case LibreHardwareMonitor.Hardware.HardwareType.Battery:
                    return HardwareType.Battery;
                default:
                    throw new ArgumentOutOfRangeException(nameof(s), s, "unexpected hardware monitor hardware translation");
            }
        }
    }
}
