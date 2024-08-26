using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Streamistry.Observability;

namespace Streamistry;

public class Pipeline : ObservablePipe
{
    private Action? Starting { get; }

    public Pipeline(ISource source, ObservabilityProvider? observability = null)
        : this([source])
    { }

    public Pipeline(ISource[] sources, ObservabilityProvider? observability = null)
        : base(observability)
    {
        foreach (var source in sources)
        {
            Starting += source.Start;
            source.RegisterObservability(observability);
        }
    }

    public void Start()
        => Starting?.Invoke();
}

public class Pipeline<T> : ChainablePipe<T>, IProcessablePipe<T>
{
    public void Emit(T? obj)
        => PushDownstream(obj);

    public Pipeline(ObservabilityProvider? provider = null)
        : base(provider)
    { }
}
