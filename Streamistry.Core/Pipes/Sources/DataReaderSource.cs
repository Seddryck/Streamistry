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
    private readonly Func<ReaderLease> readerFactory;
    private ReaderLease? lease;
    protected IDataReader DataReader => lease?.Reader
        ?? throw new InvalidOperationException("The reader is available only during execution.");

    public DataReaderSource(IDataReader dataReader, ObservabilityProvider? provider = null)
        : this(() => ReaderLease.Borrow(dataReader), provider) { }

    public DataReaderSource(Pipeline upstream, IDataReader dataReader)
        : this(() => ReaderLease.Borrow(dataReader), upstream.GetObservabilityProvider())
        => Pipeline = upstream;

    public DataReaderSource(Func<ReaderLease> readerFactory, ObservabilityProvider? provider = null)
        : base(provider)
        => this.readerFactory = readerFactory ?? throw new ArgumentNullException(nameof(readerFactory));

    public override void Setup()
        => lease = readerFactory() ?? throw new InvalidOperationException("The reader factory returned null.");

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

        if (typeof(TOutput) == typeof(object))
            return (TOutput)DataReader.GetValue(0);
        else if (typeof(TOutput) == typeof(bool))
            return (TOutput)(object)DataReader.GetBoolean(0);
        else if (typeof(TOutput) == typeof(byte))
            return (TOutput)(object)DataReader.GetByte(0);
        else if (typeof(TOutput) == typeof(short))
            return (TOutput)(object)DataReader.GetInt16(0);
        else if (typeof(TOutput) == typeof(int))
            return (TOutput)(object)DataReader.GetInt32(0);
        else if (typeof(TOutput) == typeof(long))
            return (TOutput)(object)DataReader.GetInt64(0);
        else if (typeof(TOutput) == typeof(float))
            return (TOutput)(object)DataReader.GetFloat(0);
        else if (typeof(TOutput) == typeof(double))
            return (TOutput)(object)DataReader.GetDouble(0);
        else if (typeof(TOutput) == typeof(decimal))
            return (TOutput)(object)DataReader.GetDecimal(0);
        else if (typeof(TOutput) == typeof(Guid))
            return (TOutput)(object)DataReader.GetGuid(0);
        else if (typeof(TOutput) == typeof(char))
            return (TOutput)(object)DataReader.GetChar(0);
        else if (typeof(TOutput) == typeof(string))
            return (TOutput)(object)DataReader.GetString(0);
        else if (typeof(TOutput) == typeof(DateTime))
            return (TOutput)(object)DataReader.GetDateTime(0);
        else
            throw new NotSupportedException($"Type {typeof(TOutput)} is not supported.");
    }
}
