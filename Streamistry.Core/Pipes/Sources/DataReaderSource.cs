using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Streamistry.Observability;

namespace Streamistry.Pipes.Sources;

public abstract class DataReaderSource<TOutput> : Source<TOutput>
{
    private Func<ReaderLease>? readerFactory;
    private Func<CancellationToken, ValueTask<ReaderLease>>? asyncFactory;
    private ReaderLease? lease;
    private IDataReader? suppliedReader;
    protected IDataReader DataReader
    {
        get => lease?.Reader ?? suppliedReader
            ?? throw new InvalidOperationException("The factory reader is available only during execution.");
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (ReferenceEquals(lease?.Reader, value)) return;
            var previous = lease;
            lease = null;
            previous?.Dispose();
            suppliedReader = value;
            readerFactory = () => ReaderLease.Borrow(value);
            asyncFactory = null;
            lease = ReaderLease.Borrow(value);
        }
    }

    public DataReaderSource(IDataReader dataReader, ObservabilityProvider? provider = null)
        : this(() => ReaderLease.Borrow(dataReader), provider) => suppliedReader = dataReader;

    public DataReaderSource(Pipeline upstream, IDataReader dataReader)
        : this(() => ReaderLease.Borrow(dataReader), upstream.GetObservabilityProvider())
    {
        Pipeline = upstream;
        suppliedReader = dataReader;
    }

    public DataReaderSource(Func<ReaderLease> readerFactory, ObservabilityProvider? provider = null)
        : base(provider)
        => this.readerFactory = readerFactory ?? throw new ArgumentNullException(nameof(readerFactory));

    public DataReaderSource(Func<CancellationToken, ValueTask<ReaderLease>> factory, ObservabilityProvider? provider = null)
        : base(provider) => asyncFactory = factory ?? throw new ArgumentNullException(nameof(factory));

    public override async ValueTask SetupAsync(CancellationToken cancellationToken)
    {
        if (asyncFactory is null) { Setup(); return; }
        lease = await asyncFactory(cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("The reader factory returned null.");
    }

    protected override async ValueTask<(bool HasValue, TOutput? Item)> ReadNextAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var hasValue = DataReader is DbDataReader db
            ? await db.ReadAsync(cancellationToken).ConfigureAwait(false)
            : DataReader.Read();
        return (hasValue, hasValue ? OnRead() : default);
    }

    public override void Setup()
        => lease = (readerFactory ?? throw new InvalidOperationException("Use StartAsync for an asynchronous reader factory."))() ?? throw new InvalidOperationException("The reader factory returned null.");

    public override void Cleanup()
    {
        var current = lease;
        lease = null;
        current?.Dispose();
    }

    protected override bool TryReadNext(out TOutput? item)
    {
        if (DataReader.Read())
        {
            item = OnRead();
            return true;
        }
        item = default;
        return false;
    }

    protected abstract TOutput? OnRead();
}

public class DataReaderAsDictionarySource : DataReaderSource<IReadOnlyDictionary<string, object>>
{
    public DataReaderAsDictionarySource(Func<CancellationToken, ValueTask<ReaderLease>> factory, ObservabilityProvider? provider = null)
        : base(factory, provider) { }

    public DataReaderAsDictionarySource(Func<ReaderLease> factory, ObservabilityProvider? provider = null)
        : base(factory, provider) { }

    public DataReaderAsDictionarySource(IDataReader dataReader, ObservabilityProvider? provider = null)
        : base(dataReader, provider)
    { }

    public DataReaderAsDictionarySource(Pipeline upstream, IDataReader dataReader)
        : base(upstream, dataReader)
    { }

    protected override IReadOnlyDictionary<string, object>? OnRead()
    {
        var dictionary = new Dictionary<string, object>();
        foreach (var i in Enumerable.Range(0, DataReader.FieldCount))
            dictionary.Add(DataReader.GetName(i), DataReader.IsDBNull(i) ? null! : DataReader.GetValue(i));
        return dictionary.AsReadOnly();
    }
}

public class DataReaderAsArraySource : DataReaderSource<ImmutableArray<object>>
{
    public DataReaderAsArraySource(Func<CancellationToken, ValueTask<ReaderLease>> factory, ObservabilityProvider? provider = null)
        : base(factory, provider) { }

    public DataReaderAsArraySource(Func<ReaderLease> factory, ObservabilityProvider? provider = null)
        : base(factory, provider) { }

    public DataReaderAsArraySource(IDataReader dataReader, ObservabilityProvider? provider = null)
        : base(dataReader, provider)
    { }

    public DataReaderAsArraySource(Pipeline upstream, IDataReader dataReader)
        : base(upstream, dataReader)
    { }

    protected override ImmutableArray<object> OnRead()
    {
        var list = new List<object>();
        foreach (var i in Enumerable.Range(0, DataReader.FieldCount))
            list.Add(DataReader.IsDBNull(i) ? null! : DataReader.GetValue(i));
        return list.ToImmutableArray();
    }
}

public class DataReaderAsValueSource<TOutput> : DataReaderSource<TOutput>
{
    private static readonly Type OutputType = Nullable.GetUnderlyingType(typeof(TOutput)) ?? typeof(TOutput);
    public DataReaderAsValueSource(Func<CancellationToken, ValueTask<ReaderLease>> factory, ObservabilityProvider? provider = null)
        : base(factory, provider) { }

    public DataReaderAsValueSource(Func<ReaderLease> factory, ObservabilityProvider? provider = null)
        : base(factory, provider) { }

    public DataReaderAsValueSource(IDataReader dataReader, ObservabilityProvider? provider = null)
        : base(dataReader, provider)
    { }

    public DataReaderAsValueSource(Pipeline upstream, IDataReader dataReader)
        : base(upstream, dataReader)
    { }

    protected override TOutput? OnRead()
    {
        if (DataReader.IsDBNull(0))
            return default;

        if (OutputType == typeof(object))
            return (TOutput)DataReader.GetValue(0);
        else if (OutputType == typeof(bool))
            return (TOutput)(object)DataReader.GetBoolean(0);
        else if (OutputType == typeof(byte))
            return (TOutput)(object)DataReader.GetByte(0);
        else if (OutputType == typeof(short))
            return (TOutput)(object)DataReader.GetInt16(0);
        else if (OutputType == typeof(int))
            return (TOutput)(object)DataReader.GetInt32(0);
        else if (OutputType == typeof(long))
            return (TOutput)(object)DataReader.GetInt64(0);
        else if (OutputType == typeof(float))
            return (TOutput)(object)DataReader.GetFloat(0);
        else if (OutputType == typeof(double))
            return (TOutput)(object)DataReader.GetDouble(0);
        else if (OutputType == typeof(decimal))
            return (TOutput)(object)DataReader.GetDecimal(0);
        else if (OutputType == typeof(Guid))
            return (TOutput)(object)DataReader.GetGuid(0);
        else if (OutputType == typeof(char))
            return (TOutput)(object)DataReader.GetChar(0);
        else if (OutputType == typeof(string))
            return (TOutput)(object)DataReader.GetString(0);
        else if (OutputType == typeof(DateTime))
            return (TOutput)(object)DataReader.GetDateTime(0);
        else
            throw new NotSupportedException($"Type {typeof(TOutput)} is not supported.");
    }
}
