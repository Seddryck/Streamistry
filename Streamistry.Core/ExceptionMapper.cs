using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Observability;

namespace Streamistry;
public class ExceptionMapper<TInput, TOutput> : ExceptionRouterPipe<TInput, TOutput>
{
    public Func<TInput?, TOutput?> Function { get; init; }

    public ExceptionMapper(IChainablePort<TInput> upstream, Func<TInput?, TOutput?> function)
        : base(upstream)
    {
        Function = function;
    }

    protected override TOutput? Invoke(TInput? obj)
        => Function.Invoke(obj);
}
