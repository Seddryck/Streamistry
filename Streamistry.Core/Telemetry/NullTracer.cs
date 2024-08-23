using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry.Telemetry;
public class NullTracer : ITracer
{
    public IDisposable? StartActiveSpan(string spanName)
        => null;

    private class TracerScope : IDisposable
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
