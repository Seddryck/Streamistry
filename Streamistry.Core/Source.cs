using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Observability;

namespace Streamistry;

/// <summary>
/// Represents a pipeline element that applies a predicate function to each element within a batch of this stream.
/// The output stream is composed of elements that satisfy the predicate; elements that do not satisfy the predicate are excluded from the downstream stream.
/// </summary>
/// <typeparam name="TOutput">The type of the elements in both the input and output streams.</typeparam>
public abstract class Source<TOutput> : ChainablePipe<TOutput>, ISource
{
    protected Source(ObservabilityProvider? provider)
        : base(provider)
    { }

    private bool IsStarted { get; set; }

    public void Start()
    {
        if (IsStarted)
            return;
        IsStarted = true;
        Read();
    }

    public void Stop()
    {
        IsStarted = false;
    }

    protected virtual void Read()
    {
        while (IsStarted && TryReadNext(out var item))
            PushDownstream(item);
        PushComplete();
    }

    protected abstract bool TryReadNext(out TOutput? item);

    public void WaitOnPrepared(IPreparablePipe pipe)
        => pipe.RegisterOnPrepared(Start);

    public void WaitOnCompleted(IChainablePipe pipe)
        => pipe.RegisterOnCompleted(Start);
}
