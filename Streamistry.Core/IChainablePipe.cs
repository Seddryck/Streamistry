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
    void RegisterOnCompleted(Action? complete);
    Pipeline? Pipeline { get; }
}

public interface IChainablePort<T> : IChainablePort
{
    void RegisterDownstream(Action<T?> action);
    void UnregisterDownstream(Action<T?> downstream);
}

public interface IChainablePort
{
    IChainablePipe Pipe { get; }
}
