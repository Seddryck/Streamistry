using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry.Observability;
public class NullTracer : ITracer
{
    public IDisposable? StartActiveSpan(string spanName)
        => null;
}
