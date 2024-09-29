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
public class ArrayPathPluckerTests
{
    [Test]
    public void Pluck_ValidSinglePath_ExistingValue()
    {
        var pipeline = new PipelineBuilder()
            .Source([
                        (JsonArray)JsonNode.Parse(
                            $"[{JsonTests.JsonFirst}, {JsonTests.JsonSecond}, {JsonTests.JsonThird}]"
                        )!
                    ])
            .Pluck<string>("$[1].user.contact.email")
            .Checkpoint(out var pluck)
            .Build();

        var outputs = pluck.GetOutputs(pipeline.Start);
        Assert.That(outputs, Has.Length.EqualTo(1));
        Assert.That(outputs[0], Does.Contain("nikola.tesla@blueorigin.com"));
    }
}
