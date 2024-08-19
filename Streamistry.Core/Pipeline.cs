using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry;

public class Pipeline<T> : IChainablePipe<T>, IProcessablePipe<T>
{
    private ICollection<IProcessablePipe<T>> Downstreams { get; } = [];

    public void RegisterDownstream(IProcessablePipe<T> element)
        => Downstreams.Add(element);

    public void Emit(T? obj)
    {
        foreach (var downstream in Downstreams)
            downstream.Emit(obj);
    }
}
