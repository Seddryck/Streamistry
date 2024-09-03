using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Observability;

namespace Streamistry;
public abstract class EscapeRouterPipe<TInput, TOutput> : DualRouterPipe<TInput, TOutput>
{
    public EscapeRouterPipe(IChainablePort<TInput> upstream)
    : base(upstream)
    { }

    [Meter]
    public override void Emit(TInput? obj)
    {
        if (TryInvoke(obj, out var value))
            PushDownstream(value);
        else
            Alternate.PushDownstream(obj);
    }

    [Trace]
    protected abstract bool TryInvoke(TInput? obj, [NotNullWhen(true)] out TOutput? value);
}
