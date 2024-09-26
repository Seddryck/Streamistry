using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Observability;

namespace Streamistry;
public abstract class BaseSingleRouterPipe<TInput, TOutput> : ChainablePipe<TOutput>, IProcessablePipe<TInput>, IBindablePipe<TInput>
{
    protected BaseSingleRouterPipe(IChainablePort<TInput>? upstream)
        : base(upstream?.Pipe)
    {
        upstream?.RegisterDownstream(Emit);
        upstream?.Pipe.RegisterOnCompleted(Complete);
    }

    [Meter]
    public abstract void Emit(TInput obj);

    public void Bind(IChainablePort<TInput> input)
    {
        input.RegisterDownstream(Emit);
        Pipeline = input.Pipe.Pipeline;
    }

    public void Unbind(IChainablePort<TInput> input)
    {
        input.UnregisterDownstream(Emit);
    }
}

public abstract class SingleRouterPipe<TInput, TOutput> : BaseSingleRouterPipe<TInput, TOutput>
{
    protected SingleRouterPipe(IChainablePort<TInput>? upstream)
        : base(upstream)
    { }

    [Meter]
    public override void Emit(TInput obj)
    {
        var value = Invoke(obj);
        PushDownstream(value);
    }

    [Trace]
    protected abstract TOutput Invoke(TInput obj);
}
