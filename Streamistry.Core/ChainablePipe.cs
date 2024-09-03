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
    public MainOutputPort<T> Main { get; }
    protected Action? Completion { get; set; }
    public IChainablePipe Pipe { get => this; }


    protected ChainablePipe(ObservabilityProvider? observability)
        : base(observability)
    {
        Main = new(this);
    }

    public void RegisterDownstream(Action<T?> downstream, Action? completion)
    {
        RegisterDownstream(downstream);
        RegisterOnCompleted(completion);
    }

    public void RegisterOnCompleted(Action? action)
        => Completion += action;

    public void RegisterDownstream(Action<T?> action)
        => Main.RegisterDownstream(action);

    protected void PushDownstream(T? obj)
        => Main.PushDownstream(obj);

    public virtual void Complete()
        => PushComplete();

    protected void PushComplete()
        => Completion?.Invoke();
}
