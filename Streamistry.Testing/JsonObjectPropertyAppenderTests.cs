using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using NUnit.Framework;
using Streamistry.Pipes.Parsers;
using Streamistry.Pipes.Sinks;
using Streamistry.Pipes.Sources;

namespace Streamistry.Testing;
public class JsonObjectPropertyAppenderTests
{
    [Test]
    public void X()
    {
        var persons = new EnumerableSource<string>([
            JsonTests.JsonFirst, JsonTests.JsonSecond, JsonTests.JsonThird]);
        var birthdates = new EnumerableSource<DateOnly>([new DateOnly(1879, 3, 14), new DateOnly(1856, 7, 10), new DateOnly(1903, 12, 28)]);
        var pipeline = new Pipeline([persons, birthdates]);
        var personObject = new JsonObjectParser(persons);
        var birthdateValue = new JsonValueMapper<DateOnly>(birthdates, date => date.ToString("yyyy-MM-dd"));
        var appender = new JsonObjectPropertyAppender<JsonObject, JsonValue>(personObject, birthdateValue, "$.user.birthdate");
        var sink = new MemorySink<JsonObject>(appender);
        pipeline.Start();

        Assert.That(sink.State.Count, Is.EqualTo(3));
        Assert.That(sink.State.First()!["user"]!["birthdate"], Is.Not.Null);
        Assert.That(sink.State.First()!["user"]!["birthdate"]!.GetValue<string>(), Is.EqualTo("1879-03-14"));
    }
}
