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
public class ArraySplitterTests
{
    [Test]
    public void Split_ValidEntry_Successful()
    {
        var pipeline = new PipelineBuilder()
            .Source([
                        $"[{JsonTests.JsonFirst}, {JsonTests.JsonSecond}, {JsonTests.JsonThird}]",
                        $"[{JsonTests.JsonFirst}, {JsonTests.JsonThird}]",
                    ])
            .Parse()
                .AsJsonArray()
            .Split()
            .Checkpoint(out var splitter)
            .Build();

        var outputs = splitter.GetOutputs(pipeline.Start);
        Assert.That(outputs, Has.Length.EqualTo(5));
        Assert.That(outputs, Has.All.Not.Null);
        Assert.That(outputs, Has.All.TypeOf<JsonObject>());
    }
}
