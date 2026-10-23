using System.Net;
using System.Text.Json.Nodes;
using NUnit.Framework;
using Streamistry.Json.Expressif;
using Streamistry.Pipes.Sources;
using Streamistry.Testability;

namespace Streamistry.Json.Testing;

public class CompositionTests
{
    [TestCase("not json")]
    [TestCase("[]")]
    [TestCase("null")]
    [TestCase("42")]
    public void ObjectParserRejectsMalformedAndWrongRoot(string text)
    {
        var source = new EnumerableSource<string>([text]);
        var parser = new ObjectParser(source);
        var rejected = new List<string>(); parser.Alternate.RegisterDownstream(rejected.Add);
        Assert.That(parser.GetOutputs(source.Start), Is.Empty);
        Assert.That(rejected, Is.EqualTo(new[] { text }));
    }

    [Test]
    public void NestedExpressionCreatesFreshJsonAndMissingFieldBecomesNull()
    {
        var original = JsonNode.Parse("{\"customer\":{\"name\":\"Alice\"}}")!;
        var source = new EnumerableSource<JsonNode?>([original]);
        var mapper = new JsonExpressionMapper(source, ".customer | put(name := .name | upper)");
        var output = mapper.GetOutputs(source.Start)[0];
        Assert.That(output!["name"]!.GetValue<string>(), Is.EqualTo("ALICE"));
        Assert.That(original["customer"]!["name"]!.GetValue<string>(), Is.EqualTo("Alice"));
        var missing = new JsonExpressionMapper(source, ".missing");
        Assert.That(missing.GetOutputs(source.Start), Is.EqualTo(new JsonNode?[] { null }));
    }

    private sealed class Handler(HttpStatusCode status) : HttpMessageHandler
    {
        public TrackingContent Content { get; } = new();
        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
            => new(status) { Content = Content };
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(Send(request, cancellationToken));
    }
    private sealed class TrackingContent : StringContent
    {
        public bool WasDisposed { get; private set; }
        public TrackingContent() : base("{\"ok\":true}") { }
        protected override void Dispose(bool disposing) { WasDisposed = true; base.Dispose(disposing); }
    }

    [TestCase(HttpStatusCode.OK, 1)]
    [TestCase(HttpStatusCode.BadRequest, 0)]
    public void HttpTextStageDisposesResponseOnSuccessAndRejection(HttpStatusCode status, int count)
    {
        var handler = new Handler(status);
        using var client = new HttpClient(handler);
        var source = new EnumerableSource<Uri>([new("https://example.test/data")]);
        var responder = new HttpTextResponder(source, client);
        Assert.That(responder.GetOutputs(source.Start), Has.Length.EqualTo(count));
        Assert.That(handler.Content.WasDisposed, Is.True);
    }
}
