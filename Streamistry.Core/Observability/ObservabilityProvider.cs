using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Observability.Meters;

namespace Streamistry.Observability;
public sealed class ObservabilityProvider
{
    private ITracer Tracer { get; set; } = new NullTracer();
    private Dictionary<IObservablePipe, IMeter[]> Meters { get; } = [];

    public ObservabilityProvider(ITracer tracer)
        => Tracer = tracer;

    public void AttachMeters(IObservablePipe observable, IMeter[] meters)
    {
        if (Meters.TryGetValue(observable, out var value))
            Meters[observable] = [.. value, .. meters];
        else
            Meters.Add(observable, meters);
    }

    public void DetachMeters(IObservablePipe observable)
        => Meters.Remove(observable);

    public ITracer GetTracer() => Tracer;
    public IMeter[] GetMeters(IObservablePipe observable)
        => Meters.TryGetValue(observable, out var meters) ? meters : [];
}
