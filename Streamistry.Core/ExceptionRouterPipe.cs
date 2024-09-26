using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Observability;

namespace Streamistry;
public abstract class ExceptionRouterPipe<TInput, TOutput> : DualRouterPipe<TInput, TOutput>
{
    public ExceptionRouterPipe(IChainablePort<TInput>? upstream)
        : base(upstream)
    { }

    [Trace]
    public override void Emit(TInput obj)
    {
        if (TryCatchInvoke(obj, out var value, out var exception))
            PushDownstream(value);
        else
            Alternate.PushDownstream(obj);
    }

    [Trace]
    protected virtual bool TryCatchInvoke(TInput obj, out TOutput value, out Exception? ex)
    {
        value = default!;
        ex = null;
        try
        {
            value = Invoke(obj);
            return true;
        }
        catch (Exception e)
        {
            ex = e;
            return false;
        }
    }

    [Trace]
    protected abstract TOutput Invoke(TInput obj);
}
