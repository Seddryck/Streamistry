using System.Data;
using System.Data.Common;
using global::DubUrl;
using global::DubUrl.Mapping;
using global::DubUrl.Registering;

namespace Streamistry.DubUrl;

/// <summary>A sanitized adapter error. Provider messages and credential-bearing URLs are not retained.</summary>
public sealed class DatabaseOperationException(string operation) : Exception($"Database {operation} failed.");

internal static class DatabaseAccess
{
    public static IDbConnection Connect(string url, ConnectionUrlFactory? resolver)
    {
        new ProviderFactoriesRegistrator().Register();
        resolver ??= new ConnectionUrlFactory(new SchemeRegistryBuilder()
            .WithAssemblies(typeof(SchemeRegistryBuilder).Assembly).WithAutoDiscoveredMappings().Build());
        return resolver.Instantiate(url).Connect();
    }

    public static async ValueTask<IDbConnection> OpenAsync(string url, ConnectionUrlFactory? resolver, CancellationToken token)
    {
        IDbConnection? connection = null;
        try
        {
            token.ThrowIfCancellationRequested();
            connection = Connect(url, resolver);
            if (connection is DbConnection db) await db.OpenAsync(token).ConfigureAwait(false);
            else { token.ThrowIfCancellationRequested(); connection.Open(); }
            token.ThrowIfCancellationRequested();
            return connection;
        }
        catch (OperationCanceledException) { DisposeQuietly(connection); throw new OperationCanceledException(token); }
        catch { DisposeQuietly(connection); throw new DatabaseOperationException("connection setup"); }
    }

    public static void Bind(IDbCommand command, IReadOnlyDictionary<string, object?> values)
    {
        command.Parameters.Clear();
        foreach (var pair in values)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = pair.Key;
            parameter.Value = pair.Value ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }
    }

    public static void DisposeQuietly(IDisposable? resource)
    {
        try { resource?.Dispose(); }
        catch { /* Provider errors may expose the connection URL. */ }
    }
}
