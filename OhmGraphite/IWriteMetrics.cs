using System;
using System.Collections.Generic;

namespace OhmGraphite
{
    public interface IWriteMetrics
    {
        void ReportMetrics(DateTime reportTime, IEnumerable<ReportedValue> sensors);
    }
}
