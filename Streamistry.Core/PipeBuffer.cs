using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Observability;

namespace Streamistry;

/// <summary>
/// Represents a pipeline element that temporarily stores elements in a buffer as they pass through the stream.
/// The buffer can accumulate a specified number of elements or a batch of elements over time, allowing for more efficient processing or delayed downstream transmission.
/// Once the buffer reaches a certain threshold, such as a maximum size or time limit, its contents are released downstream for further processing.
/// </summary>
/// <typeparam name="T">The type of the elements stored in the buffer.</typeparam>
public class PipeBuffer<T> : BaseSingleRouterPipe<T, T>
{
    protected List<T?> Store { get; } = [];
    protected int? MaxCapacity { get; }

    public PipeBuffer(IChainablePort<T> upstream, int? maxCapacity = null)
    : base(upstream)
    {
        MaxCapacity = maxCapacity;
    }

    [Meter]
    public override void Emit(T? obj)
    {
        Invoke(obj);
        if (MaxCapacity.HasValue && Store.Count >= MaxCapacity.Value)
            Complete();
    }
            
    [Trace]
    protected void Invoke(T? obj)
        => Store.Add(obj);

    [Trace]
    public override void Complete()
    {
        foreach (var item in Store)
            PushDownstream(item);
        Store.Clear();
        PushComplete();
    }
}
