using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry.Observability;
public sealed class ObservabilityProvider
{
    private static ITracer Tracer { get; set; } = new NullTracer();

    public ObservabilityProvider(ITracer tracer)
        => Tracer = tracer;

    public static ITracer GetTracer() => Tracer;
}
