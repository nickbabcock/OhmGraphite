using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OhmGraphite
{
    public interface IGiveSensors : IManage
    {
        IEnumerable<ReportedValue> ReadAllSensors();
    }
}
