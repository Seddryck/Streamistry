using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Streamistry.Fluent;

namespace Streamistry.Json.Fluent;
public class ArraySplitterBuilder : PipeElementBuilder<JsonArray, JsonObject>
{
    public ArraySplitterBuilder(IPipeBuilder<JsonArray> upstream)
        : base(upstream)
    { }

    public override IChainablePort<JsonObject> OnBuildPipeElement()
        => new ArraySplitter(
                Upstream.BuildPipeElement()
            );
}
