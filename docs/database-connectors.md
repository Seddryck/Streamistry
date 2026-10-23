# Direct databases

Streamistry.DubUrl pins DubUrl 0.29.34 (the version verified by Packata 0.63.4) and
Scriban 7.5.1. DubUrl owns URL/provider resolution; Streamistry owns execution,
parameters, local transactions and resource lifetime. Core references neither.
Configure provider factories in the application and obtain URLs from a secure
configuration source; examples never embed credential-bearing URLs.

```csharp
var source = new DatabaseSource(connectionUrl,
    "SELECT name FROM people WHERE active=@active",
    new Dictionary<string, object?> { ["@active"] = 1 });
await new Pipeline(source).StartAsync(cancellationToken);
```

Source opens a fresh owned connection per execution through DubUrl, binds parameters
without SQL interpolation, then owns reader, command and connection via ReaderLease.
DbConnection.OpenAsync, DbCommand.ExecuteReaderAsync and DbDataReader.ReadAsync receive
the token; plain IDb providers remain synchronous and only observe boundary checks.
DatabaseSource requires StartAsync because its resource factory is asynchronous.
For an already open/borrowed reader, use Core reader sources instead.

DatabaseCommandSink<T> requires explicit TransactionPolicy: CommitOnCompletion
commits when its upstream completes, Manual requires Commit/Rollback, and None
allows provider autocommit (prior writes cannot be rolled back). It executes one
parameterized command per record; there is no batching or bulk-copy support.
All local resources belong to the sink. No external transaction is implicitly
enlisted. Keep the sink in a using scope, and run through sink.Execute/ExecuteAsync
so failures or cancellation elsewhere upstream roll back and close pending resources.
Calling Pipeline.Start directly requires the caller to roll back/dispose on failure.
Stop is successful partial completion and can commit; cancel the token to abort.

```csharp
using var sink = new DatabaseCommandSink<Row>(input, connectionUrl,
    "INSERT INTO people(name) VALUES (@name)",
    row => new Dictionary<string, object?> { ["@name"] = row.Name },
    TransactionPolicy.CommitOnCompletion);
await sink.ExecuteAsync(pipeline, cancellationToken);
```

Sink delivery and nonquery execution are synchronous because ports use Action<T>.
ExecuteAsync awaits source setup/reads; it does not claim asynchronous writes.
Cancellation cannot interrupt a blocking synchronous sink provider; it is checked
before/after each command and rolls back through the execution wrapper. Concurrent
execution wrappers reject. Sequential runs open fresh connections/transactions.

Command/setup/read/cleanup errors expose sanitized operation-only exceptions with
no provider inner exception, SQL, parameter values or URL. Adapter tracing never
receives the connection URL as an emitted item. Provider logging configured outside
the adapter remains the application's responsibility. A rollback failure reports
an unknown outcome while still closing resources; the adapter cannot guarantee an
unavailable server rolled back. Dispose failures during best-effort cleanup are
suppressed to avoid leaking provider diagnostics.

Validation uses isolated local SQLite files and exercises parameter literals,
DBNull/null, commits, rollback on command failure/cancellation and sanitized setup
errors. Other database providers are not claimed as tested. Packata endpoint
resolution and upstream bulk-copy APIs are not duplicated here.
