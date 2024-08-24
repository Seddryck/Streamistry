using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Observability;

namespace Streamistry;

public class Pipeline<T> : ChainablePipe<T>, IProcessablePipe<T>
{
    public void Emit(T? obj)
        => PushDownstream(obj);

    public Pipeline(ObservabilityProvider? provider = null)
        : base(provider)
        => RegisterObservability(provider);
}
