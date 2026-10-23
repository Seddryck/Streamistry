using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Observability;

namespace Streamistry;
public abstract class ChainablePipe<T> : ObservablePipe, IChainablePipe<T>
{
    private Guid? runId;
    private bool completed;
    private Action<Guid>? starting;
    public void RegisterOnStarting(Action<Guid> action) => starting += action;
    protected internal void BeginRun(Guid id)
    {
        if (runId == id) return;
        runId = id;
        completed = false;
        OnRunStarting();
        starting?.Invoke(id);
    }
    protected virtual void OnRunStarting() { }
    protected bool IsCompleted => completed;
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
        upstream?.RegisterOnStarting(BeginRun);
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
    {
        if (completed) return;
        completed = true;
        Completion?.Invoke();
    }
}
