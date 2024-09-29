using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Streamistry.Fluent;

namespace Streamistry.Json.Fluent;
public class PathPluckerBuilder<TOutput> : PipeElementBuilder<JsonObject, TOutput>
{
    protected string Path { get; set; }

    public PathPluckerBuilder(IPipeBuilder<JsonObject> upstream, string path)
        : base(upstream)
        => (Path) = (path);

    public override IChainablePort<TOutput> OnBuildPipeElement()
        => new PathPlucker<TOutput>(
                Upstream.BuildPipeElement()
                , Path ?? throw new InvalidOperationException()
            );
}
