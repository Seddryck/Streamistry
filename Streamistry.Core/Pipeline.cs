using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Observability;

namespace Streamistry;

public class Pipeline : ObservablePipe
{
    internal static readonly AsyncLocal<Guid?> CurrentRun = new();
    private readonly List<ISource> sources = [];

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
        sources.Add(source);
        source.RegisterObservability(GetObservabilityProvider());
    }

    public void Start()
    {
        var previous = CurrentRun.Value;
        var runId = Guid.NewGuid();
        CurrentRun.Value = runId;
        try
        {
            foreach (var source in sources) source.PrepareRun(runId);
            foreach (var source in sources) source.Start();
        }
        finally { CurrentRun.Value = previous; }
    }
}

