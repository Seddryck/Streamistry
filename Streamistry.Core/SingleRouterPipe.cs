using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Observability;

namespace Streamistry;
public abstract class SingleRouterPipe<TInput, TOutput> : ChainablePipe<TOutput>, IProcessablePipe<TInput>
{
    protected SingleRouterPipe(IChainablePort<TInput> upstream)
        : base(upstream.Pipe.GetObservabilityProvider())
    {
        upstream.RegisterDownstream(Emit);
        upstream.Pipe.RegisterOnCompleted(Complete);
    }

    [Meter]
    public void Emit(TInput? obj)
    {
        var value = Invoke(obj);
        PushDownstream(value);
    }

    [Trace]
    protected abstract TOutput? Invoke(TInput? obj);
}
