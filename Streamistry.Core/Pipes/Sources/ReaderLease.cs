using System.Data;

namespace Streamistry.Pipes.Sources;

/// <summary>A reader and the resources whose lifetime belongs to that reader execution.</summary>
public sealed class ReaderLease : IDisposable
{
    private readonly IDisposable[] owned;
    private bool disposed;
    public IDataReader Reader { get; }

    private ReaderLease(IDataReader reader, IDisposable[] owned)
        => (Reader, this.owned) = (reader ?? throw new ArgumentNullException(nameof(reader)), owned);

    /// <summary>Leaves the reader and its resources open after execution.</summary>
    public static ReaderLease Borrow(IDataReader reader) => new(reader, []);

    /// <summary>Disposes the reader first, then dependencies in supplied order (command, connection, stream).</summary>
    public static ReaderLease Own(IDataReader reader, params IDisposable[] dependencies)
        => new(reader, [reader, .. dependencies]);

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        List<Exception>? errors = null;
        foreach (var resource in owned.Distinct<IDisposable>(ReferenceEqualityComparer.Instance))
        {
            try { resource.Dispose(); }
            catch (Exception ex) { (errors ??= []).Add(ex); }
        }
        if (errors is not null) throw new AggregateException("Reader resource cleanup failed.", errors);
    }
}
