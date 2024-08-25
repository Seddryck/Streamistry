using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry.Observability.Thresholds;
internal readonly struct MaxCardinality(int Value) : IThreshold
{
    public bool Check(int count, DateTime timestamp)
        => count >= Value;
}
