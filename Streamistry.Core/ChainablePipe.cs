using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Streamistry.Observability;

namespace Streamistry;
public abstract class ChainablePipe<T> : ObservablePipe, IChainablePipe<T>
{
    private Action<T?>? Downstream { get; set; }
    private Action? Completion { get; set; }

    protected ChainablePipe(ObservabilityProvider? observability)
        : base(observability)
    { }

    public void RegisterDownstream(Action<T?> downstream, Action? completion)
    {
        Downstream += downstream;
        Completion += completion;
    }
    
    protected void PushDownstream(T? obj)
        => Downstream?.Invoke(obj);

    public virtual void Complete()
        => PushComplete();

    protected void PushComplete()
        => Completion?.Invoke();
}
