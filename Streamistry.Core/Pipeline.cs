using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Observability;

namespace Streamistry;

public class Pipeline : ObservablePipe
{
    private Action? Starting { get; set; }

    public Pipeline(ObservabilityProvider? observability = null)
        : this([], observability)
    { }

    public Pipeline(ISource source, ObservabilityProvider? observability = null)
        : this([source], observability)
    { }

    public Pipeline(ISource[] sources, ObservabilityProvider? observability = null)
        : base(observability)
    {
        foreach (var source in sources)
            AddSource(source);
    }

    internal void AddSource(ISource source)
    {
        Starting += source.Start;
        source.RegisterObservability(GetObservabilityProvider());
    }

    public void Start()
        => Starting?.Invoke();
}

