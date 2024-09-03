using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Observability;

namespace Streamistry;

/// <summary>
/// Represents a pipeline element that applies a specified function to each element within a batch of this stream to create multiple new elements.
/// The output stream is determined by the result of the function applied to the input elements.
/// </summary>
/// <typeparam name="TInput">The type of the elements in the input stream.</typeparam>
/// <typeparam name="TOutput">The type of the elements in the output stream after the function is applied.</typeparam>
public class Splitter<TInput, TOutput> : BaseSingleRouterPipe<TInput, TOutput>
{
    public Func<TInput?, TOutput[]?> Function { get; init; }

    public Splitter(IChainablePort<TInput> upstream, Func<TInput?, TOutput[]?> function)
        : base(upstream)
    {
        Function = function;
    }

    [Meter]
    public override void Emit(TInput? obj)
    {
        var results = Invoke(obj);
        if (results is null)
            PushDownstream(default);
        else
            foreach (var result in results)
                PushDownstream(result);
    }

    [Trace]
    protected TOutput[]? Invoke(TInput? obj)
        => Function.Invoke(obj);
}
