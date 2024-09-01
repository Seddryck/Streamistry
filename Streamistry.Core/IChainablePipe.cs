using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry;
public interface IChainablePipe<T> : IChainablePort<T>, IChainablePipe
{
    void RegisterDownstream(Action<T?> downstream, Action? complete);
}

public interface IChainablePipe : IObservablePipe
{
    void RegisterCompletion(Action? complete);
}

public interface IChainablePort<T>
{
    void RegisterDownstream(Action<T?> action);
    IChainablePipe Pipe { get; }
}
