using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Observability;

namespace Streamistry;
public class Sink<T> : ObservablePipe, IProcessablePipe<T>
{
    public Action<T?> Action { get; }
    public Pipeline? Pipeline { get; }

    public Sink(IChainablePort<T> upstream, Action<T?> function)
        :this(function, upstream)
    { }

    protected Sink(Action<T?> action, IChainablePort<T>? upstream)
        : base(upstream?.Pipe.GetObservabilityProvider())
    {
        upstream?.RegisterDownstream(Emit);
        Action = action;
        Pipeline = upstream?.Pipe.Pipeline;
    }

    [Meter]
    public void Emit(T? obj)
        => Invoke(obj);

    [Trace]
    protected void Invoke(T? obj)
        => Action.Invoke(obj);
}
