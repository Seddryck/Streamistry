using System.Text;
using PocketCsvReader;
using NUnit.Framework;
using Streamistry.Expressif;
using Streamistry.Pipes.Sinks;
using Streamistry.Testability;

namespace Streamistry.PocketCsvReader.Testing;

public class CsvSourceTests
{
    private static CsvProfile Profile() => new(',', '"', "\n", true);

    [Test]
    public void ConfiguredNullSequenceBecomesNullInCommonRow()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("name\nNULL\n"));
        var source = new CsvSource(stream, () => new CsvProfile(new global::PocketCsvReader.Configuration.DialectDescriptorBuilder()
            .WithDelimiter(',').WithLineTerminator("\n").WithHeader(true).WithNullSequence("NULL").Build()));
        Assert.That(source.GetOutputs(source.Start)[0]!["name"], Is.Null);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void QuotesEmbeddedNewlinesSnapshotsAndOwnership(bool leaveOpen)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("id,name\n1,\"Alice\nSmith\"\n2,\"Bob, Jr\"\n"));
        var source = new CsvSource(stream, Profile, leaveOpen);
        var rows = source.GetOutputs(source.Start);
        Assert.That(rows, Has.Length.EqualTo(2));
        Assert.That(rows[0]!["name"], Is.EqualTo("Alice\nSmith"));
        Assert.That(rows[1]!["name"], Is.EqualTo("Bob, Jr"));
        Assert.That(rows[0]!.Keys, Is.EqualTo(new[] { "id", "name" }));
        Assert.That(stream.CanRead, Is.EqualTo(leaveOpen));
    }

    [Test]
    public void TsvWithoutHeadersAndEmptyFields()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("1\t\n2\tx\n"));
        var source = new CsvSource(stream, () => new CsvProfile('\t', '"', "\n", false));
        var rows = source.GetOutputs(source.Start);
        Assert.That(rows, Has.Length.EqualTo(2));
        Assert.That(rows[0]!.Values.Last(), Is.EqualTo(string.Empty));
    }

    [Test]
    public async Task AsyncFactoryCancellationDisposesStream()
    {
        using var cancellation = new CancellationTokenSource();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("id,name\n1,Alice\n"));
        var source = new CsvSource(token =>
        {
            Assert.That(token, Is.EqualTo(cancellation.Token));
            cancellation.Cancel();
            return ValueTask.FromResult<Stream>(stream);
        }, Profile);
        await Assert.ThrowsAsync<OperationCanceledException>(() => source.StartAsync(cancellation.Token));
        Assert.That(stream.CanRead, Is.False);
    }

    [Test]
    public void CsvToExpressifToMemoryDoesNotRequireDataTable()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("name\nAlice\nBob\n"));
        var source = new CsvSource(stream, Profile);
        var expression = global::Expressif.Expression.Create(".name | upper");
        var map = new Mapper<IReadOnlyDictionary<string, object>, string>(source,
            row => ExpressionAdapters.Adapt<string>(expression.Evaluate(StructuredValues.FromClr(row))));
        var sink = new MemorySink<string>(map);
        Assert.That(map.GetOutputs(source.Start), Is.EqualTo(new[] { "ALICE", "BOB" }));
    }

    [Test]
    public void OwnedFactoryStreamClosesWhenProfileSetupFails()
    {
        var stream = new MemoryStream();
        var source = new CsvSource(_ => ValueTask.FromResult<Stream>(stream), () => throw new InvalidOperationException());
        Assert.That(async () => await source.StartAsync(), Throws.TypeOf<InvalidOperationException>());
        Assert.That(stream.CanRead, Is.False);
    }
}
