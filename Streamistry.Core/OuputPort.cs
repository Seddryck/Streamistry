using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry;
public class OutputPort<T> : IChainablePort<T>
{
    private Action<T?>? Downstream { get; set; }

    public void RegisterDownstream(Action<T?> downstream)
        => Downstream += downstream;

    public string Name { get; init; }

    public IChainablePipe Pipe { get; init; }


    public OutputPort(IChainablePipe pipe, string name)
        => (Name, Pipe) = (name, pipe);

    public void PushDownstream(T? obj)
        => Downstream?.Invoke(obj);
}

public class MainOutputPort<T> : OutputPort<T>
{
    public MainOutputPort(IChainablePipe pipe)
        : base(pipe, "Main")
    { }
}
