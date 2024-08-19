using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry;

public class Pipeline<T> : ChainablePipe<T>, IProcessablePipe<T>
{
    public void Emit(T? obj)
        => PushDownstream(obj);
}
