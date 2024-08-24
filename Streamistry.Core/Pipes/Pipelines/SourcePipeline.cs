using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Observability;

namespace Streamistry.Pipes.Pipelines;
public class SourcePipeline<TOutput> : Pipeline<TOutput>
{
    private Source<TOutput> Source { get; }
    public SourcePipeline(Source<TOutput> source, ObservabilityProvider? provider = null)
        : base(provider)
    {
        Source = source;
        Source.RegisterObservability(provider);
    }

    public void Start()
        => Source.Start();

    public void Stop()
        => Source.Stop();
}
