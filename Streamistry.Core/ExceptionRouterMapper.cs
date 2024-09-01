using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Observability;

namespace Streamistry;
public class ExceptionRouterMapper<TInput, TOutput> : Mapper<TInput, TOutput>, IDualRoute<TOutput, TInput>
{
    public OutputPort<TInput> Alternate { get; }
    public new OutputPort<TOutput> Main { get => base.Main; }

    public ExceptionRouterMapper(IChainablePort<TInput> upstream, Func<TInput?, TOutput?> function)
        : base(upstream, function)
    {
        Alternate = new(this, "Alternate");
    }

    [Trace]
    public override void Emit(TInput? obj)
    {
        if (TryInvokeCatch(obj, out var value, out var exception))
            PushDownstream(value);
        else
            Alternate.PushDownstream(obj);
    }

    [Meter]
    protected virtual bool TryInvokeCatch(TInput? obj, out TOutput? value, out Exception? ex)
    {
        value = default;
        ex = null;
        try
        {
            value = Function.Invoke(obj);
            return true;
        }
        catch (Exception e)
        {
            ex = e;
            return false;
        }
    }
}
