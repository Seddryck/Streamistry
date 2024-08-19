using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry;
public class Sink<T> : IProcessablePipe<T>
{
    public Action<T?> Function { get; }

    public Sink(IChainablePipe<T> upstream, Action<T?> function)
    {
        upstream.RegisterDownstream(this);
        Function = function;
    }

    public void Emit(T? obj)
        => Function.Invoke(obj);
}
