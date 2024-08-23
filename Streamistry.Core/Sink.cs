using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Telemetry;

namespace Streamistry;
public class Sink<T> : IProcessablePipe<T>
{
    public Action<T?> Function { get; }

    public Sink(IChainablePipe<T> upstream, Action<T?> function)
    {
        upstream.RegisterDownstream(Emit);
        Function = function;
    }

    public void Emit(T? obj)
        => Invoke(obj);

    [Telemetry]
    protected void Invoke(T? obj)
        => Function.Invoke(obj);
}
