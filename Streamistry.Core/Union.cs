using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Observability;

namespace Streamistry;

/// <summary>
/// Represents a pipeline element that merges two or more upstream streams into a single downstream stream by emetting each values from each upstream.
/// </summary>
/// <typeparam name="TInput">The type of the elements in any input stream.</typeparam>
public class Union<TInput> : ChainablePipe<TInput>, IProcessablePipe<TInput>
{
    public Union(IChainablePipe<TInput>[] upstreams)
    : base(upstreams[0]?.GetObservabilityProvider())
    {
        foreach (var upstream in upstreams)
            upstream.RegisterDownstream(Emit, Complete);
    }

    [Meter]
    public void Emit(TInput? obj)
        => PushDownstream(Invoke(obj));

    [Trace]
    protected TInput? Invoke(TInput? obj)
        => obj;
}
