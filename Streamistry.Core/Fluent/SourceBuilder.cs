using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Pipes.Sources;

namespace Streamistry.Fluent;
public class SourceBuilder<TOutput> : BasePipeBuilder<TOutput>
{
    protected IBuilder<Pipeline> Upstream { get; }

    protected IEnumerable<TOutput> Enumeration { get; }

    public SourceBuilder(IBuilder<Pipeline> upstream, IEnumerable<TOutput> enumeration)
        => (Enumeration, Upstream) = (enumeration, upstream);

    public override IChainablePort<TOutput> OnBuildPipeElement()
    {
        var pipeline = Upstream.BuildPipeElement();
        var source = new EnumerableSource<TOutput>(pipeline, Enumeration);
        pipeline.AddSource(source);
        return source;
    }
}
