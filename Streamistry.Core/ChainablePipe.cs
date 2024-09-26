using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Observability;

namespace Streamistry;
public abstract class ChainablePipe<T> : ObservablePipe, IChainablePipe<T>
{
    public MainOutputPort<T> Main { get; }
    protected Action? Completion { get; set; }
    public IChainablePipe Pipe { get => this; }
    public Pipeline? Pipeline { get; protected set; }

    protected ChainablePipe(ObservabilityProvider? observability)
        : base(observability)
    {
        Main = new(this);
    }

    protected ChainablePipe(IChainablePipe? upstream)
        : base(upstream?.GetObservabilityProvider())
    {
        Main = new(this);
        if (upstream is not null)
            if (upstream is Pipeline pipeline)
                Pipeline = pipeline;
            else
                Pipeline = upstream.Pipeline;
    }

    public void RegisterDownstream(Action<T> downstream, Action? completion)
    {
        RegisterDownstream(downstream);
        RegisterOnCompleted(completion);
    }

    public void RegisterOnCompleted(Action? action)
        => Completion += action;

    public void RegisterDownstream(Action<T> action)
        => Main.RegisterDownstream(action);

    public void UnregisterDownstream(Action<T> downstream)
        => Main.UnregisterDownstream(downstream);

    protected void PushDownstream(T? obj)
        => Main.PushDownstream(obj);

    public virtual void Complete()
        => PushComplete();

    protected void PushComplete()
        => Completion?.Invoke();
}
