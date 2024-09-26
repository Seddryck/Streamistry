using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Observability;

namespace Streamistry;
public abstract class DualRouterPipe<TInput, TOutput> : ChainablePipe<TOutput>, IDualRoute<TOutput, TInput>, IBindablePipe<TInput>
{
    public OutputPort<TInput> Alternate { get; }
    public new OutputPort<TOutput> Main { get => base.Main; }

    public DualRouterPipe(IChainablePort<TInput>? upstream)
    : base(upstream?.Pipe)
    {
        Alternate = new(this, "Alternate");
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
