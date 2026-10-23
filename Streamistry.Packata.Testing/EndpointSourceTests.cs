using System.Data;
using System.Text;
using Microsoft.Data.Sqlite;
using Moq;
using NUnit.Framework;
using Packata.Core.Contracts;
using Packata.Core.Reading;
using Packata.DataPackage;
using Packata.DataPackage.Mapping;
using Packata.ResourceReaders;
using Streamistry.PocketCsvReader;
using Streamistry.Testability;

namespace Streamistry.Packata.Testing;

public class EndpointSourceTests
{
    private static DataEndpoint Endpoint(string path, DataFormat? format = null)
        => new("input", null, EndpointKind.File, null, new PathLocation([path]), format);

    private sealed class TextResolver(string text) : IEndpointStreamResolver
    {
        public Stream? Last { get; private set; }
        public ValueTask<Stream> OpenAsync(string path, CancellationToken cancellationToken = default)
            => ValueTask.FromResult(Last = new MemoryStream(Encoding.UTF8.GetBytes(text.Replace("\n", "\r\n"))));
    }

    [Test]
    public async Task DelimitedEndpointsShareTheDirectCsvRowContract()
    {
        var resolver = new TextResolver("id,name\n1,Alice\n2,Bob\n");
        var endpoint = Endpoint("data.csv", new("csv", Options: new Dictionary<string, object?> { ["header"] = true }));
        var source = new EndpointSource(new ResourceReaderFactory(resolver), endpoint);
        var rows = new List<IReadOnlyDictionary<string, object>>(); source.RegisterDownstream(rows.Add);
        await source.StartAsync();
        Assert.That(resolver.Last!.CanRead, Is.False);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("id,name\n1,Alice\n2,Bob\n"));
        var csv = new CsvSource(stream, () => new global::PocketCsvReader.CsvProfile(',', '"', "\n", true));
        Assert.That(rows, Is.EqualTo(csv.GetOutputs(csv.Start)));
    }

    [Test]
    public async Task FactoryReceivesSchemaAndCancellationAndReaderIsOwned()
    {
        using var cancellation = new CancellationTokenSource();
        var reader = new Mock<IDataReader>();
        var factory = new Mock<IDataEndpointReaderFactory>();
        var schema = new DataSchema([new("name", "string", Required: true)]);
        var endpoint = Endpoint("data.csv");
        factory.Setup(x => x.OpenAsync(endpoint, schema, cancellation.Token)).ReturnsAsync(reader.Object);
        var source = new EndpointSource(factory.Object, endpoint, schema);
        await source.StartAsync(cancellation.Token);
        factory.VerifyAll(); reader.Verify(x => x.Dispose(), Times.Once());
    }

    [TestCase("my-book.xlsx", "xlsx")]
    [TestCase("iris.parquet", "parquet")]
    public async Task PublishedFactorySupportsSpreadsheetAndParquet(string fixture, string format)
    {
        var options = new Dictionary<string, object?>();
        if (format == "xlsx") { options["sheetName"] = "Country"; options["header"] = true; }
        var source = new EndpointSource(new ResourceReaderFactory(),
            Endpoint(Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures", fixture), new(format, Options: options)));
        var rows = new List<IReadOnlyDictionary<string, object>>(); source.RegisterDownstream(rows.Add);
        await source.StartAsync();
        Assert.That(rows, Is.Not.Empty);
    }

    [Test]
    public async Task SchemaMetadataDoesNotEnforceRequiredRowValidation()
    {
        var resolver = new TextResolver("name\n\"\"\n");
        var schema = new DataSchema([new("name", "string", Required: true)]);
        var source = new EndpointSource(new ResourceReaderFactory(resolver), Endpoint("data.csv",
            new("csv", Options: new Dictionary<string, object?> { ["header"] = true })), schema);
        var count = 0; source.RegisterDownstream(_ => count++);
        await source.StartAsync();
        Assert.That(count, Is.EqualTo(1));
    }

    [Test]
    public async Task DataPackageMapsToCanonicalBeforePipelineConstruction()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("""{"name":"example","resources":[{"name":"people","path":"people.csv","format":"csv"}]}"""));
        var contract = new DataPackageFactory().LoadFromStream(stream).ToCanonicalContract().RequireValue();
        var source = new EndpointSource(new ResourceReaderFactory(new TextResolver("name\nAlice\n")), contract.Endpoints.Single());
        var rows = 0; source.RegisterDownstream(_ => rows++);
        await source.StartAsync();
        Assert.That(contract.Assets, Has.Count.EqualTo(1));
        Assert.That(rows, Is.EqualTo(1));
    }

    [Test]
    public async Task DatabaseEndpointUsesPackataDubUrlResolution()
    {
        var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"endpoint-{Guid.NewGuid():N}.db");
        try
        {
            using (var connection = new SqliteConnection($"Data Source={path}"))
            {
                connection.Open(); using var command = connection.CreateCommand();
                command.CommandText = "CREATE TABLE people (name TEXT); INSERT INTO people VALUES ('Alice')";
                command.ExecuteNonQuery();
            }
            System.Data.Common.DbProviderFactories.RegisterFactory("Microsoft.Data.Sqlite", SqliteFactory.Instance);
            var endpoint = new DataEndpoint("db", null, EndpointKind.Database, null,
                new ConnectionLocation("sqlite", ConnectionUrl: "sqlite:///" + path.Replace('\\', '/')),
                new DataFormat("database", Options: new Dictionary<string, object?> { ["table"] = "people" }));
            var source = new EndpointSource(new ResourceReaderFactory(), endpoint);
            var rows = new List<IReadOnlyDictionary<string, object>>(); source.RegisterDownstream(rows.Add);
            await source.StartAsync();
            Assert.That(rows.Single()["name"], Is.EqualTo("Alice"));
        }
        finally { SqliteConnection.ClearAllPools(); if (File.Exists(path)) File.Delete(path); }
    }
}
