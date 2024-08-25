using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry;
public interface IChainablePipe<T> : IObservablePipe
{
    void RegisterDownstream(Action<T?> action, Action? complete);
}
