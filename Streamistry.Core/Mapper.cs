using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Observability;

namespace Streamistry;

/// <summary>
/// Represents a pipeline element that applies a specified function to each element within a batch of this stream.
/// The output stream is determined by the result of the function applied to the input elements.
/// </summary>
/// <typeparam name="TInput">The type of the elements in the input stream.</typeparam>
/// <typeparam name="TOutput">The type of the elements in the output stream after the function is applied.</typeparam>
public class Mapper<TInput, TOutput> : SingleRouterPipe<TInput, TOutput>
{
    public Func<TInput?, TOutput?> Function { get; init; }

    protected Mapper(Func<TInput?, TOutput?> function, IChainablePort<TInput>? upstream)
    : base(upstream)
    {
        Function = function;
    }

    public Mapper(IChainablePort<TInput> upstream, Func<TInput?, TOutput?> function)
    : this(function, upstream)
    { }

    public Mapper(Func<TInput?, TOutput?> function)
    : this(function, null)
    { }

    [Trace]
    protected override TOutput? Invoke(TInput? obj)
        => Function.Invoke(obj);
}
