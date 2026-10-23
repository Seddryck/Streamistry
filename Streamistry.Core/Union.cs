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
    private readonly HashSet<int> completed = [];
    private readonly int count;
    protected override void OnRunStarting() => completed.Clear();
    public Union(IChainablePipe<TInput>[] upstreams)
    : base(upstreams.FirstOrDefault() ?? throw new ArgumentException("At least one upstream is required.", nameof(upstreams)))
    {
        count = upstreams.Length;
        for (var i = 0; i < upstreams.Length; i++)
        {
            var index = i;
            upstreams[i].RegisterOnStarting(BeginRun);
            upstreams[i].RegisterDownstream(Emit, () =>
            {
                completed.Add(index);
                if (completed.Count == count) Complete();
            });
        }
    }

    [Meter]
    public void Emit(TInput? obj)
    {
        if (!IsCompleted) PushDownstream(Invoke(obj));
    }

    [Trace]
    protected TInput? Invoke(TInput? obj)
        => obj;
}
