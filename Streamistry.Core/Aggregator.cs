using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Observability;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Streamistry;

/// <summary>
/// Represents a pipeline element that accumulates a stateful function across the elements of a batch within this stream.
/// The output stream type is determined by the result of the accumulation applied to the input elements.
/// </summary>
/// <typeparam name="TSource">The type of the elements in the input stream.</typeparam>
/// <typeparam name="TAccumulate">The type of the intermediate accumulated state during processing.</typeparam>
/// <typeparam name="TResult">The type of the elements in the output stream, determined by applying a selection function to the accumulated state.</typeparam>
public class Aggregator<TSource, TAccumulate, TResult> : ChainablePipe<TResult>, IProcessablePipe<TSource>
{
    public Func<TAccumulate?, TSource?, TAccumulate?> Accumulator { get; }
    public Func<TAccumulate?, TResult?> Selector { get; }
    private TAccumulate? State { get; set; }

    public Aggregator(IChainablePort<TSource> upstream, Func<TAccumulate?, TSource?, TAccumulate?> accumulator, Func<TAccumulate?, TResult?> selector, TAccumulate? seed = default)
        : base(upstream.Pipe.GetObservabilityProvider())
    {
        upstream.RegisterDownstream(Emit);
        upstream.Pipe.RegisterCompletion(PushComplete);
        (Accumulator, Selector, State) = (accumulator, selector, seed);
    }

    [Meter]
    public virtual void Emit(TSource? obj)
        => PushDownstream(Invoke(obj));

    [Trace]
    protected virtual TResult? Invoke(TSource? obj)
    {
        State = Accumulator.Invoke(State, obj);
        return Selector.Invoke(State);
    }
}
