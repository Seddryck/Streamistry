using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Observability;

namespace Streamistry;
public abstract class BaseSink<T> : ObservablePipe, IProcessablePipe<T>, IBindablePipe<T>
{
    public Pipeline? Pipeline { get; private set; }

   protected BaseSink(IChainablePort<T>? upstream)
        : base(upstream?.Pipe.GetObservabilityProvider())
    {
        upstream?.RegisterDownstream(Emit);
        Pipeline = upstream?.Pipe.Pipeline;
    }

    [Meter]
    public void Emit(T obj)
        => Invoke(obj);

    [Trace]
    protected abstract void Invoke(T obj);
    public void Bind(IChainablePort<T> input)
    {
        input.RegisterDownstream(Emit);
        Pipeline = input.Pipe.Pipeline;
    }
    public void Bind(IChainablePort input)
        => Bind(input as IChainablePort<T> ?? throw new InvalidCastException());

    public void Unbind(IChainablePort<T> input)
        => input.UnregisterDownstream(Emit);

    public void Unbind(IChainablePort input)
        => Unbind(input as IChainablePort<T> ?? throw new InvalidCastException());
}

public class Sink<T> : BaseSink<T>
{
    public Action<T> Action { get; }

    public Sink(Action<T> function)
        : this(function, null)
    { }

    public Sink(IChainablePort<T> upstream, Action<T> function)
        : this(function, upstream)
    { }

    protected Sink(Action<T> action, IChainablePort<T>? upstream)
        : base(upstream)
    {
        Action = action;
    }

    [Trace]
    protected override void Invoke(T obj)
        => Action.Invoke(obj);
}
