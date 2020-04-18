using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibreHardwareMonitor.Hardware;

namespace OhmGraphite
{
    class OhmSensor : ISensor
    {
        public void Accept(IVisitor visitor)
        {
        }

        public void Traverse(IVisitor visitor)
        {
        }

        public IControl Control => null;
        public IHardware Hardware { get; set; }
        public Identifier Identifier { get; set; }
        public int Index => 0;
        public bool IsDefaultHidden => false;
        public float? Max => null;
        public float? Min => null;
        public string Name { get; set; }
        public IReadOnlyList<IParameter> Parameters => new List<IParameter>();
        public LibreHardwareMonitor.Hardware.SensorType SensorType { get; set; }
        public float? Value { get; set; }
        public IEnumerable<SensorValue> Values => new List<SensorValue>();
        public TimeSpan ValuesTimeWindow { get; set; }

        public void ResetMin()
        {
        }

        public void ResetMax()
        {
        }
    }
}
