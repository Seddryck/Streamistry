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
using System.Globalization;

namespace Streamistry.Json.Testing.Fluent;
public class ValueMapperTests
{
    [Test]
    public void AsJsonValue_ValidEntry_Successful()
    {
        var pipeline = new PipelineBuilder()
            .Source([new DateOnly(1879, 3, 14), new DateOnly(1856, 7, 10), new DateOnly(1903, 12, 28)])
            .AsJsonValue()
            .Checkpoint(out var mapper)
            .Build();

        var outputs = mapper.GetOutputs(pipeline.Start);
        Assert.Multiple(() => {
            Assert.That(outputs, Has.Length.EqualTo(3));
            Assert.That(outputs, Has.All.Not.Null);
            Assert.That(outputs, Has.All.AssignableTo<JsonValue>());
        });
        Assert.That(outputs.Select(x => x!.ToJsonString()), Has.One.EqualTo("\"1879-03-14\""));
    }

    [Test]
    public void AsJsonValue_ValidEntryWithCustomFunction_Successful()
    {
        var pipeline = new PipelineBuilder()
            .Source([new DateOnly(1879, 3, 14), new DateOnly(1856, 7, 10), new DateOnly(1903, 12, 28)])
            .AsJsonValue(x => $"{x.ToString("MMMM", CultureInfo.InvariantCulture)} {x.Year}")
            .Checkpoint(out var mapper)
            .Build();

        var outputs = mapper.GetOutputs(pipeline.Start);
        Assert.Multiple(() => {
            Assert.That(outputs, Has.Length.EqualTo(3));
            Assert.That(outputs, Has.All.Not.Null);
            Assert.That(outputs, Has.All.AssignableTo<JsonValue>());
        });
        Assert.That(outputs.Select(x => x!.ToJsonString()), Has.One.EqualTo("\"March 1879\""));
    }
}
