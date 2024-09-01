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
public class Mapper<TInput, TOutput> : ChainablePipe<TOutput>, IProcessablePipe<TInput>
{
    public Func<TInput?, TOutput?> Function { get; init; }

    public Mapper(IChainablePort<TInput> upstream, Func<TInput?, TOutput?> function)
    : base(upstream.Pipe.GetObservabilityProvider())
    {
        upstream.RegisterDownstream(Emit);
        upstream.Pipe.RegisterOnCompleted(Complete);
        Function = function;
    }

    [Meter]
    public virtual void Emit(TInput? obj)
        => PushDownstream(Invoke(obj));

    [Trace]
    protected virtual TOutput? Invoke(TInput? obj)
        => Function.Invoke(obj);
}
