using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Observability;

namespace Streamistry;
public class Sink<T> : IProcessablePipe<T>, IObservablePipe
{
    public Action<T?> Function { get; }
    protected ObservabilityProvider? Observability { get; private set; }

    public Sink(IChainablePipe<T> upstream, Action<T?> function)
    {
        upstream.RegisterDownstream(Emit, null);
        RegisterObservability(upstream.GetObservabilityProvider());
        Function = function;
    }

    [Meter]
    public void Emit(T? obj)
        => Invoke(obj);

    [Trace]
    protected void Invoke(T? obj)
        => Function.Invoke(obj);

    public void RegisterObservability(ObservabilityProvider? observability)
        => Observability = observability;
    public void CascadeObservability(IObservablePipe pipe)
        => pipe.RegisterObservability(Observability);

    public ObservabilityProvider? GetObservabilityProvider()
        => Observability;
}
