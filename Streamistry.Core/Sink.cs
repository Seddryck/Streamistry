using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Observability;

namespace Streamistry;
public class Sink<T> : ObservablePipe, IProcessablePipe<T>
{
    public Action<T?> Function { get; }

    public Sink(IChainablePort<T> upstream, Action<T?> function)
        :base(upstream.Pipe.GetObservabilityProvider())
    {
        upstream.RegisterDownstream(Emit);
        Function = function;
    }

    [Meter]
    public void Emit(T? obj)
        => Invoke(obj);

    [Trace]
    protected void Invoke(T? obj)
        => Function.Invoke(obj);
}
