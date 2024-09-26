using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry;

public interface IBindablePipe<T>
{
    void Bind(IChainablePort<T> input);
    void Unbind(IChainablePort<T> input);
}

public interface IProcessablePipe<T>
{
    void Emit(T obj);
}
