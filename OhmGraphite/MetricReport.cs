using System;
using System.Collections.Generic;

namespace OhmGraphite
{
    public record MetricReport(DateTime ReportTime, IReadOnlyList<ReportedValue> Sensors);
}
