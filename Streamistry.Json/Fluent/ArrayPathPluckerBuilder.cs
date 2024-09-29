using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Streamistry.Fluent;

namespace Streamistry.Json.Fluent;
public class ArrayPathPluckerBuilder<TOutput> : PipeElementBuilder<JsonArray, TOutput>
{
    protected string Path { get; set; }

    public ArrayPathPluckerBuilder(IPipeBuilder<JsonArray> upstream, string path)
        : base(upstream)
        => (Path) = (path);

    public override IChainablePort<TOutput> OnBuildPipeElement()
        => new PathArrayPlucker<TOutput>(
                Upstream.BuildPipeElement()
                , Path ?? throw new InvalidOperationException()
            );
}
