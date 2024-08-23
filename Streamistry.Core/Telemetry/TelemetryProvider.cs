using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry.Telemetry;
public sealed class TelemetryProvider
{
    private static ITracer Tracer { get; set; } = new NullTracer();

    public TelemetryProvider(ITracer tracer)
        => Tracer = tracer;

    public static ITracer GetTracer() => Tracer;

}
