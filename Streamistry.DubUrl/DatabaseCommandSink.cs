using System.Data;
using global::DubUrl;

namespace Streamistry.DubUrl;

public enum TransactionPolicy { CommitOnCompletion, Manual, None }

/// <summary>One parameterized command per input. Owns its connection, commands and local transaction.</summary>
public sealed class DatabaseCommandSink<T> : BaseSink<T>, IDisposable
{
    private readonly string url;
    private readonly string sql;
    private readonly Func<T, IReadOnlyDictionary<string, object?>> parameters;
    private readonly TransactionPolicy policy;
    private readonly ConnectionUrlFactory? resolver;
    private IDbConnection? connection;
    private IDbTransaction? transaction;
    private CancellationToken executionToken;
    private bool completed;
    private bool disposed;
    private int executing;

    public DatabaseCommandSink(IChainablePort<T> upstream, string connectionUrl, string commandText,
        Func<T, IReadOnlyDictionary<string, object?>> parameters, TransactionPolicy policy,
        ConnectionUrlFactory? resolver = null) : base(upstream)
    {
        if (!Enum.IsDefined(policy)) throw new ArgumentOutOfRangeException(nameof(policy));
        (url, sql, this.parameters, this.policy, this.resolver) = (connectionUrl, commandText, parameters, policy, resolver);
        upstream.Pipe.RegisterOnStarting(_ => { Rollback(); completed = false; });
        upstream.Pipe.RegisterOnCompleted(OnCompleted);
    }

    protected override void Invoke(T input)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (completed) throw new InvalidOperationException("Start a new run before writing more records.");
        try
        {
            executionToken.ThrowIfCancellationRequested();
            if (connection is null)
            {
                connection = DatabaseAccess.Connect(url, resolver); connection.Open();
                if (policy != TransactionPolicy.None) transaction = connection.BeginTransaction();
            }
            using var command = connection.CreateCommand();
            command.CommandText = sql; command.Transaction = transaction;
            DatabaseAccess.Bind(command, parameters(input));
            command.ExecuteNonQuery();
            executionToken.ThrowIfCancellationRequested();
        }
        catch (OperationCanceledException) { Rollback(); throw new OperationCanceledException(executionToken); }
        catch { Rollback(); throw new DatabaseOperationException("command"); }
    }

    private void OnCompleted()
    {
        completed = true;
        if (policy == TransactionPolicy.CommitOnCompletion) Commit();
        else if (policy == TransactionPolicy.None) Close();
    }

    public void Commit()
    {
        try { transaction?.Commit(); }
        catch { Rollback(); throw new DatabaseOperationException("commit"); }
        finally { Close(); }
    }
    public void Rollback()
    {
        var failed = false;
        try { transaction?.Rollback(); }
        catch { failed = true; }
        finally { Close(); }
        if (failed) throw new DatabaseOperationException("rollback (outcome unknown)");
    }
    private void Close()
    {
        DatabaseAccess.DisposeQuietly(transaction); transaction = null;
        DatabaseAccess.DisposeQuietly(connection); connection = null;
    }

    /// <summary>Ensures upstream failure/cancellation also rolls back a pending transaction.</summary>
    public async Task ExecuteAsync(Pipeline pipeline, CancellationToken cancellationToken = default)
    {
        if (Interlocked.CompareExchange(ref executing, 1, 0) != 0)
            throw new InvalidOperationException("The sink is already executing.");
        executionToken = cancellationToken;
        try { await pipeline.StartAsync(cancellationToken).ConfigureAwait(false); }
        catch { Rollback(); throw; }
        finally { executionToken = default; Volatile.Write(ref executing, 0); }
    }
    public void Execute(Pipeline pipeline)
    {
        if (Interlocked.CompareExchange(ref executing, 1, 0) != 0)
            throw new InvalidOperationException("The sink is already executing.");
        try { pipeline.Start(); }
        catch { Rollback(); throw; }
        finally { Volatile.Write(ref executing, 0); }
    }
    public void Dispose()
    {
        if (disposed) return;
        disposed = true; Rollback();
    }
}
