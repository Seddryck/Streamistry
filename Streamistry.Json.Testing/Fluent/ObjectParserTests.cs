using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using NUnit.Framework;
using Streamistry.Fluent;
using Streamistry.Testability;
using Streamistry.Json.Fluent;

namespace Streamistry.Json.Testing.Fluent;
public class ObjectParserTests
{
    [Test]
    public void ParseAsJsonObject_ValidEntries_Successful()
    {
        var pipeline = new PipelineBuilder()
            .Source([JsonTests.JsonFirst, JsonTests.JsonSecond, JsonTests.JsonThird])
            .Parse()
                .AsJsonObject()
            .Checkpoint(out var parser)
            .Build();

        var outputs = parser.GetOutputs(pipeline.Start);
        Assert.That(outputs, Has.Length.EqualTo(3));
        Assert.That(outputs, Has.All.Not.Null);
        Assert.That(outputs, Has.All.TypeOf<JsonObject>());
    }
}
