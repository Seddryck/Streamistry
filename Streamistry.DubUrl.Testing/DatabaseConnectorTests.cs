using System.Data.Common;
using Microsoft.Data.Sqlite;
using NUnit.Framework;
using Streamistry.Pipes.Sources;

namespace Streamistry.DubUrl.Testing;

public class DatabaseConnectorTests
{
    private string path = null!;
    private string Url => "sqlite://" + Path.GetFileName(path);
    [SetUp]
    public void Setup()
    {
        path = Path.Combine(Environment.CurrentDirectory, $"direct-{Guid.NewGuid():N}.db");
        DbProviderFactories.RegisterFactory("Microsoft.Data.Sqlite", SqliteFactory.Instance);
        using var connection = Open(); using var command = connection.CreateCommand();
        command.CommandText = "CREATE TABLE people (name TEXT UNIQUE, note TEXT)"; command.ExecuteNonQuery();
    }
    private SqliteConnection Open()
    {
        var connection = new SqliteConnection($"Data Source={path}"); connection.Open(); return connection;
    }
    private int Count()
    {
        using var connection = Open(); using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM people"; return Convert.ToInt32(command.ExecuteScalar());
    }
    [TearDown]
    public void Cleanup() { SqliteConnection.ClearAllPools(); if (File.Exists(path)) File.Delete(path); }

    [Test]
    public async Task ParameterizedSourceAndSinkPreserveLiteralValuesAndNull()
    {
        const string name = "O'Reilly'); DROP TABLE people; --";
        var input = new EnumerableSource<string>([name, "Bob"]);
        using var sink = new DatabaseCommandSink<string>(input, Url,
            "INSERT INTO people (name,note) VALUES (@name,@note)",
            value => new Dictionary<string, object?> { ["@name"] = value, ["@note"] = null }, TransactionPolicy.CommitOnCompletion);
        await sink.ExecuteAsync(new Pipeline(input));
        var source = new DatabaseSource(Url, "SELECT name,note FROM people WHERE name=@name",
            new Dictionary<string, object?> { ["@name"] = name });
        var rows = new List<IReadOnlyDictionary<string, object>>(); source.RegisterDownstream(rows.Add);
        await source.StartAsync();
        Assert.That(rows.Single()["name"], Is.EqualTo(name));
        Assert.That(rows.Single()["note"], Is.Null);
        Assert.That(Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task CommandFailureRollsBackEntireTransaction()
    {
        var input = new EnumerableSource<string>(["same", "same"]);
        using var sink = new DatabaseCommandSink<string>(input, Url, "INSERT INTO people(name) VALUES (@name)",
            value => new Dictionary<string, object?> { ["@name"] = value }, TransactionPolicy.CommitOnCompletion);
        var error = await Assert.ThrowsAsync<DatabaseOperationException>(() => sink.ExecuteAsync(new Pipeline(input)));
        Assert.That(error!.InnerException, Is.Null);
        Assert.That(Count(), Is.Zero);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ManualTransactionRequiresExplicitCommit(bool commit)
    {
        var input = new EnumerableSource<string>(["Alice"]);
        using var sink = new DatabaseCommandSink<string>(input, Url, "INSERT INTO people(name) VALUES (@name)",
            value => new Dictionary<string, object?> { ["@name"] = value }, TransactionPolicy.Manual);
        sink.Execute(new Pipeline(input));
        if (commit) sink.Commit(); else sink.Rollback();
        Assert.That(Count(), Is.EqualTo(commit ? 1 : 0));
    }

    [Test]
    public async Task CancellationAfterDeliveryRollsBack()
    {
        using var cancellation = new CancellationTokenSource();
        var input = new EnumerableSource<string>(["Alice", "Bob"]);
        using var sink = new DatabaseCommandSink<string>(input, Url, "INSERT INTO people(name) VALUES (@name)",
            value => new Dictionary<string, object?> { ["@name"] = value }, TransactionPolicy.CommitOnCompletion);
        input.RegisterDownstream(_ => cancellation.Cancel());
        await Assert.ThrowsAsync<OperationCanceledException>(() => sink.ExecuteAsync(new Pipeline(input), cancellation.Token));
        Assert.That(Count(), Is.Zero);
    }

    [Test]
    public async Task AutocommitPolicyPreservesEarlierWritesOnFailure()
    {
        var input = new EnumerableSource<string>(["same", "same"]);
        using var sink = new DatabaseCommandSink<string>(input, Url, "INSERT INTO people(name) VALUES (@name)",
            value => new Dictionary<string, object?> { ["@name"] = value }, TransactionPolicy.None);
        await Assert.ThrowsAsync<DatabaseOperationException>(() => sink.ExecuteAsync(new Pipeline(input)));
        Assert.That(Count(), Is.EqualTo(1));
    }

    [Test]
    public void DisposalRollsBackUncommittedManualTransaction()
    {
        var input = new EnumerableSource<string>(["Alice"]);
        using (var sink = new DatabaseCommandSink<string>(input, Url, "INSERT INTO people(name) VALUES (@name)",
            value => new Dictionary<string, object?> { ["@name"] = value }, TransactionPolicy.Manual))
            sink.Execute(new Pipeline(input));
        Assert.That(Count(), Is.Zero);
    }

    [Test]
    public async Task ExecutionWrapperRollsBackFailureElsewhereUpstream()
    {
        var input = new EnumerableSource<string>(["Alice", "Bob"]);
        var mapper = new Mapper<string, string>(input, value => value == "Bob" ? throw new InvalidOperationException() : value);
        using var sink = new DatabaseCommandSink<string>(mapper, Url, "INSERT INTO people(name) VALUES (@name)",
            value => new Dictionary<string, object?> { ["@name"] = value }, TransactionPolicy.CommitOnCompletion);
        await Assert.ThrowsAsync<InvalidOperationException>(() => sink.ExecuteAsync(new Pipeline(input)));
        Assert.That(Count(), Is.Zero);
    }

    [Test]
    public async Task SetupErrorsExposeNoUrlProviderMessageOrInnerException()
    {
        var source = new DatabaseSource("unknown://user:synthetic-marker@host/database", "SELECT 1");
        var error = await Assert.ThrowsAsync<DatabaseOperationException>(() => source.StartAsync());
        Assert.That(error!.ToString(), Does.Not.Contain("synthetic-marker"));
        Assert.That(error.InnerException, Is.Null);
        var query = new DatabaseSource(Url, "SELECT * FROM nonexistent");
        var queryError = await Assert.ThrowsAsync<DatabaseOperationException>(() => query.StartAsync());
        Assert.That(queryError!.Message, Does.Not.Contain(path));
    }
}
