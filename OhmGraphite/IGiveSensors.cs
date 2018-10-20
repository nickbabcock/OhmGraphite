using System.Collections.Generic;

namespace OhmGraphite
{
    public interface IGiveSensors : IManage
    {
        IEnumerable<ReportedValue> ReadAllSensors();
    }
}
