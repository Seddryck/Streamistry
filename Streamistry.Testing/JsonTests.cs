using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Streamistry.Pipes.Sinks;
using Streamistry.Pipes.Sources;
using Streamistry.Pipes.Mappers;
using System.Text.Json.Nodes;
using Streamistry.Pipes.Parsers;
using Streamistry.Pipes.Splitters;

namespace Streamistry.Testing;
public class JsonTests
{
    private const string JsonFirst = @"
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

    private const string JsonSecond = @"
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

    private const string JsonThird = @"
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
        var plucker = new JsonPathPlucker<string>(pipeline, "$.user.contact.email");
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
        var parser = new JsonObjectParser(source);
        var plucker = new JsonPathPlucker<string>(parser, "$.user.contact.email");
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
        var parser = new JsonArrayParser(source);
        var splitter = new JsonArraySplitter(parser);
        var plucker = new JsonPathPlucker<string>(splitter, "$.user.contact.email");
        var sink = new MemorySink<string>(plucker);
        pipeline.Start();

        Assert.That(sink.State, Has.Count.EqualTo(3));
        Assert.That(sink.State.First(), Is.EqualTo("albert.einstein@gmail.com"));
        Assert.That(sink.State.Last(), Is.Null);
    }
}
