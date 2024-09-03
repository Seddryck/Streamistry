using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Moq;
using RichardSzalay.MockHttp;
using NUnit.Framework;
using Streamistry.Pipes;
using Streamistry.Pipes.Mappers;
using Streamistry.Pipes.Sinks;

namespace Streamistry.Testing;
public class RestResponderTests
{
    [Test]
    public void Emit_ValidData_CallHttpClient()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("/customer/1").Respond("application/json", JsonTests.JsonFirst);
        mockHttp.When("/customer/2").Respond("application/json", JsonTests.JsonSecond);
        mockHttp.When("/customer/3").Respond("application/json", JsonTests.JsonThird);
        var client = mockHttp.ToHttpClient();
        client.BaseAddress = new Uri("http://localhost");

        var pipeline = new Pipeline<int>();
        var mapper = new RestResponder<int, JsonObject>(pipeline, client, x => $"/customer/{x}");
        var plunker = new JsonPathPlucker<string>(mapper, "$.user.name");
        var sink = new MemorySink<string>(plunker);

        pipeline.Emit(1);
        pipeline.Emit(2);
        pipeline.Emit(3);

        Assert.That(sink.State.Count, Is.EqualTo(3));
        Assert.Multiple(() =>
        {
            Assert.That(sink.State.First(), Is.EqualTo("Albert Einstein"));
            Assert.That(sink.State.Last(), Is.EqualTo("John von Neumann"));
        });
    }
}
