using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry.Pipes.Pipelines;
public class SourcePipeline<TOutput> : Pipeline<TOutput>
{
    private Source<TOutput> Source { get; }
    public SourcePipeline(Source<TOutput> source)
        => Source = source;

    public void Start()
        => Source.Start();

    public void Stop()
        => Source.Stop();
}
