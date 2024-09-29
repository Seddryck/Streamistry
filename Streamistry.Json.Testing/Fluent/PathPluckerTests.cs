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
public class PathPluckerTests
{
    [Test]
    [TestCase(JsonTests.JsonFirst, "albert.einstein@gmail.com")]
    [TestCase(JsonTests.JsonThird, null)]
    public void Pluck_ValidPath_ExistingValue(string jsonString, string? email)
    {
        var pipeline = new PipelineBuilder()
            .Source([(JsonObject)JsonNode.Parse(jsonString)!])
            .Pluck<string>("$.user.contact.email")
            .Checkpoint(out var pluck)
            .Build();

        var outputs = pluck.GetOutputs(pipeline.Start);
        Assert.That(outputs, Has.Length.EqualTo(1));
        Assert.That(outputs, Does.Contain(email));
    }

    [Test]
    [TestCase(JsonTests.JsonThird, null)]
    public void Pluck_NonExistingPath_Null(string jsonString, string? email)
    {
        var pipeline = new PipelineBuilder()
            .Source([(JsonObject)JsonNode.Parse(jsonString)!])
            .Pluck<string>("$.user.contact.email")
            .Checkpoint(out var pluck)
            .Build();

        var outputs = pluck.GetOutputs(pipeline.Start);
        Assert.That(outputs, Has.Length.EqualTo(1));
        Assert.That(outputs, Does.Contain(email));
    }
}
