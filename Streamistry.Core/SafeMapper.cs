using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Observability;

namespace Streamistry;
public class SafeMapper<TInput, TOutput> : ExceptionRouterPipe<TInput, TOutput>
{
    public Func<TInput, TOutput> Function { get; init; }

    protected SafeMapper(Func<TInput, TOutput> function, IChainablePort<TInput> ? upstream)
        : base(upstream)
    {
        Function = function;
    }

    public SafeMapper(IChainablePort<TInput> upstream, Func<TInput, TOutput> function)
        : this(function, upstream)
    { }

    public SafeMapper(Func<TInput, TOutput> function)
        : this(function, null)
    { }

    protected override TOutput Invoke(TInput obj)
        => Function.Invoke(obj);
}
