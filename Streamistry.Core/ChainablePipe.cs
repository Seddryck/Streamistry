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
    private Action? Completion { get; set; }

    protected ObservabilityProvider? Observability { get; private set; }

    public void RegisterDownstream(Action<T?> downstream, Action? completion)
    {
        Downstream += downstream;
        Completion += completion;
    }

    protected ChainablePipe(ObservabilityProvider? observability)
        => RegisterObservability(observability);

    public void RegisterObservability(ObservabilityProvider? observability)
        => Observability = observability;

    public ObservabilityProvider? GetObservabilityProvider()
        => Observability;

    protected void PushDownstream(T? obj)
        => Downstream?.Invoke(obj);

    public virtual void Complete()
        => PushComplete();

    protected void PushComplete()
        => Completion?.Invoke();
}
