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
    private int active;

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

    public void Start() => ExecuteAsync(false, default).GetAwaiter().GetResult();
    public Task StartAsync(CancellationToken cancellationToken = default) => ExecuteAsync(true, cancellationToken);

    private async Task ExecuteAsync(bool asynchronous, CancellationToken cancellationToken)
    {
        if (Interlocked.CompareExchange(ref active, 1, 0) != 0)
            throw new InvalidOperationException("The pipeline is already running.");
        var previous = CurrentRun.Value;
        var runId = Guid.NewGuid();
        CurrentRun.Value = runId;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var source in sources) source.PrepareRun(runId);
            foreach (var source in sources)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (asynchronous) await source.StartAsync(cancellationToken).ConfigureAwait(false);
                else source.Start();
            }
        }
        finally
        {
            CurrentRun.Value = previous;
            Volatile.Write(ref active, 0);
        }
    }
}
