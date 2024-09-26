using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry;

public class OutputPort<T> : IChainablePort<T>
{
    private Action<T?>? Downstream { get; set; }

    public string Name { get; init; }

    public IChainablePipe Pipe { get; init; }

    public OutputPort(IChainablePipe pipe, string name)
        => (Name, Pipe) = (name, pipe);

    public void RegisterDownstream(Action<T> downstream)
        => Downstream += downstream;

    public void UnregisterDownstream(Action<T> downstream)
        => Downstream -= downstream;

    public void PushDownstream(T? obj)
        => Downstream?.Invoke(obj);

    public Action<T>[] GetDownstreamInvocations()
        => Downstream?.GetInvocationList().Cast<Action<T>>().ToArray() ?? Array.Empty<Action<T>>();
}

public class MainOutputPort<T> : OutputPort<T>
{
    public MainOutputPort(IChainablePipe pipe)
        : base(pipe, "Main")
    { }
}
