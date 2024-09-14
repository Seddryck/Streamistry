using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Observability;

namespace Streamistry;
public class ExceptionMapper<TInput, TOutput> : ExceptionRouterPipe<TInput, TOutput>
{
    public Func<TInput?, TOutput?> Function { get; init; }

    protected ExceptionMapper(Func<TInput?, TOutput?> function, IChainablePort<TInput> ? upstream)
        : base(upstream)
    {
        Function = function;
    }

    public ExceptionMapper(IChainablePort<TInput> upstream, Func<TInput?, TOutput?> function)
        : this(function, upstream)
    { }

    public ExceptionMapper(Func<TInput?, TOutput?> function)
        : this(function, null)
    { }

    protected override TOutput? Invoke(TInput? obj)
        => Function.Invoke(obj);
}
