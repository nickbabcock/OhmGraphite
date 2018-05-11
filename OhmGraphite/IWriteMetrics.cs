using System;
using System.Collections.Generic;

namespace OhmGraphite
{
    interface IWriteMetrics
    {
        void ReportMetrics(DateTime reportTime, IEnumerable<ReportedValue> sensors);
    }
}
