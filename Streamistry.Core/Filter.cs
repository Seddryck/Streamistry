using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Observability;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Streamistry;

/// <summary>
/// Represents a pipeline element that applies a predicate function to each element within a batch of this stream.
/// The output stream is composed of elements that satisfy the predicate; elements that do not satisfy the predicate are excluded from the downstream stream.
/// </summary>
/// <typeparam name="TInput">The type of the elements in both the input and output streams.</typeparam>
public class Filter<TInput> : BaseSingleRouterPipe<TInput, TInput>
{
    public Func<TInput?, bool> Predicate { get; init; }

    public Filter(Func<TInput?, bool> predicate)
        : this(predicate, null)
    { }

    public Filter(IChainablePort<TInput> upstream, Func<TInput?, bool> predicate)
        : this(predicate, upstream)
    { }

    public Filter(Func<TInput?, bool> predicate, IChainablePort<TInput>? upstream = null)
    : base(upstream)
    {
        Predicate = predicate;
    }

    public override void Emit(TInput? obj)
    {
        if (Invoke(obj))
            PushDownstream(obj);
    }

    [Trace]
    protected bool Invoke(TInput? input)
        => Predicate.Invoke(input);
}
