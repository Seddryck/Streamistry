using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Pipes.Sinks;

namespace Streamistry.Fluent;
public class SinkBuilder<TInput>
{
    protected IPipeBuilder<TInput> Upstream { get; }
    private Type? SinkType { get; set; }

    public SinkBuilder(IPipeBuilder<TInput> upstream)
        => (Upstream) = (upstream);

    public SinkBuilder<TInput> InMemory()
    {
        SinkType = typeof(MemorySink<TInput>);
        return this;
    }

    public Pipeline Build()
    {
        var sink = (Sink<TInput>)Activator.CreateInstance(SinkType ?? throw new InvalidOperationException(), [Upstream.BuildPipeElement()])!;
        return sink.Pipeline!;
    }
}
