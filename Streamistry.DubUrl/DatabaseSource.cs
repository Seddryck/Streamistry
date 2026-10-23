using System.Data;
using System.Data.Common;
using global::DubUrl;
using Streamistry.Pipes.Sources;

namespace Streamistry.DubUrl;

public sealed class DatabaseSource : DataReaderAsDictionarySource
{
    public DatabaseSource(string connectionUrl, string query, IReadOnlyDictionary<string, object?>? parameters = null,
        ConnectionUrlFactory? resolver = null)
        : base(token => Open(connectionUrl, query, parameters, resolver, token)) { }

    private static async ValueTask<ReaderLease> Open(string url, string query,
        IReadOnlyDictionary<string, object?>? parameters, ConnectionUrlFactory? resolver, CancellationToken token)
    {
        var connection = await DatabaseAccess.OpenAsync(url, resolver, token).ConfigureAwait(false);
        IDbCommand? command = null;
        try
        {
            command = connection.CreateCommand(); command.CommandText = query;
            DatabaseAccess.Bind(command, parameters ?? new Dictionary<string, object?>());
            var reader = command is DbCommand db
                ? await db.ExecuteReaderAsync(token).ConfigureAwait(false)
                : command.ExecuteReader();
            return ReaderLease.Own(reader, command, connection);
        }
        catch (OperationCanceledException)
        {
            DatabaseAccess.DisposeQuietly(command); DatabaseAccess.DisposeQuietly(connection);
            throw new OperationCanceledException(token);
        }
        catch
        {
            DatabaseAccess.DisposeQuietly(command); DatabaseAccess.DisposeQuietly(connection);
            throw new DatabaseOperationException("query setup");
        }
    }

    protected override bool TryReadNext(out IReadOnlyDictionary<string, object>? item)
    {
        try { return base.TryReadNext(out item); }
        catch { throw new DatabaseOperationException("read"); }
    }
    protected override async ValueTask<(bool HasValue, IReadOnlyDictionary<string, object>? Item)> ReadNextAsync(CancellationToken cancellationToken)
    {
        try { return await base.ReadNextAsync(cancellationToken).ConfigureAwait(false); }
        catch (OperationCanceledException) { throw new OperationCanceledException(cancellationToken); }
        catch { throw new DatabaseOperationException("read"); }
    }
    public override void Cleanup()
    {
        try { base.Cleanup(); }
        catch { throw new DatabaseOperationException("cleanup"); }
    }
}
