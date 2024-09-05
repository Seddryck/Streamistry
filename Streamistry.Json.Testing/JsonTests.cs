using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Streamistry.Pipes.Sinks;
using Streamistry.Pipes.Sources;
using System.Text.Json.Nodes;

namespace Streamistry.Json.Testing;
public class JsonTests
{
    public const string JsonFirst = @"
    {
        ""user"": {
            ""name"": ""Albert Einstein"",
            ""age"": 30,
            ""contact"": {
                ""email"": ""albert.einstein@gmail.com"",
                ""phone"": ""123-456-7890""
            }
        }
    }";

    public const string JsonSecond = @"
    {
        ""user"": {
            ""name"": ""Nikola Tesla"",
            ""age"": 35,
            ""contact"": {
                ""email"": ""nikola.tesla@blueorigin.com"",
                ""phone"": ""321-654-0987""
            }
        }
    }";

    public const string JsonThird = @"
    {
        ""user"": {
            ""name"": ""John von Neumann"",
            ""age"": 72,
            ""contact"": {
                ""phone"": ""456-123-0987""
            }
        }
    }";

    [Test]
    [TestCase(JsonFirst, "albert.einstein@gmail.com")]
    [TestCase(JsonThird, null)]
    public void JsonPathPlucker_ValidPath_ExistingValue(string jsonString, string? email)
    {
        var pipeline = new Pipeline<JsonObject>();
        var plucker = new PathPlucker<string>(pipeline, "$.user.contact.email");
        var sink = new MemorySink<string>(plucker);
        plucker.Emit((JsonObject)JsonNode.Parse(jsonString)!);

        Assert.That(sink.State, Has.Count.EqualTo(1));
        Assert.That(sink.State.First(), Is.EqualTo(email));
    }

    [Test]
    public void JsonObjectParser_ValidString_ExistingValue()
    {
        var source = new EnumerableSource<string>([JsonFirst, JsonSecond, JsonThird]);
        var pipeline = new Pipeline(source);
        var parser = new ObjectParser(source);
        var plucker = new PathPlucker<string>(parser, "$.user.contact.email");
        var sink = new MemorySink<string>(plucker);
        pipeline.Start();

        Assert.That(sink.State, Has.Count.EqualTo(3));
        Assert.That(sink.State.First(), Is.EqualTo("albert.einstein@gmail.com"));
        Assert.That(sink.State.Last(), Is.Null);
    }

    [Test]
    public void JsonArrayParser_ValidString_ExistingValue()
    {
        var array = $"[{JsonFirst}, {JsonSecond}, {JsonThird}]";
        var source = new EnumerableSource<string>([array]);
        var pipeline = new Pipeline(source);
        var parser = new ArrayParser(source);
        var splitter = new ArraySplitter(parser);
        var plucker = new PathPlucker<string>(splitter, "$.user.contact.email");
        var sink = new MemorySink<string>(plucker);
        pipeline.Start();

        Assert.That(sink.State, Has.Count.EqualTo(3));
        Assert.That(sink.State.First(), Is.EqualTo("albert.einstein@gmail.com"));
        Assert.That(sink.State.Last(), Is.Null);
    }
}
