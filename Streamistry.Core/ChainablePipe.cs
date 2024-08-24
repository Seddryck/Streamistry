using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Streamistry.Observability;

namespace Streamistry;
public abstract class ChainablePipe<T> : IChainablePipe<T>, IObservablePipe
{
    private Action<T?>? Downstream { get; set; }
    protected ObservabilityProvider? Observability { get; private set; }

    public void RegisterDownstream(Action<T?> action)
        => Downstream += action;

    protected ChainablePipe(ObservabilityProvider? observability)
        => RegisterObservability(observability);

    public void RegisterObservability(ObservabilityProvider? observability)
        => Observability = observability;

    public ObservabilityProvider? GetObservabilityProvider()
        => Observability;

    protected virtual void PushDownstream(T? obj)
        => Downstream?.Invoke(obj);
}
