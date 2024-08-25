using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry.Observability;

public class ConsoleTracer : BaseTracer
{
    public override IDisposable? StartActiveSpan(string spanName)
    {
        spanName = $"{spanName} [{Guid.NewGuid()}]"; 
        Console.WriteLine($"Starting span '{spanName}'");
        return new TracerScope(spanName);
    }
}
