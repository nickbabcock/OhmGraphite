using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OhmGraphite
{
    public interface IWriteMetrics
    {
        Task ReportMetrics(DateTime reportTime, IEnumerable<ReportedValue> sensors);
    }
}
