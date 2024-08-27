using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry;

public interface IChainablePipe : IObservablePipe
{
    void RegisterOnCompleted(Action? completion);
}

public interface IChainablePipe<T> : IChainablePipe
{
    void RegisterDownstream(Action<T?> action, Action? complete);
}
