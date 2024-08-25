using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry.Observability;
public abstract class BaseTracer : ITracer
{
    public abstract IDisposable? StartActiveSpan(string spanName);

    protected class TracerScope : IDisposable
    {
        private string SpanName { get; }
        public Stopwatch StopWatch { get; }

        public TracerScope(string spanName)
        {
            SpanName = spanName;
            StopWatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            Console.WriteLine($"Ending span '{SpanName}' in {StopWatch.ElapsedTicks} ticks");
        }
    }
}
