using global::Packata.Core.Contracts;
using global::Packata.Core.Reading;
using Streamistry.Pipes.Sources;

namespace Streamistry.Packata;

/// <summary>Opens canonical endpoints through Packata without adding storage/format dispatch.</summary>
public sealed class EndpointSource : DataReaderAsDictionarySource
{
    public EndpointSource(IDataEndpointReaderFactory factory, DataEndpoint endpoint, DataSchema? schema = null)
        : base(async token => ReaderLease.Own(await factory.OpenAsync(endpoint, schema, token).ConfigureAwait(false)))
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(endpoint);
    }

    public EndpointSource(IDataEndpointReaderFactory factory, DataEndpoint endpoint, DataAsset asset)
        : this(factory, endpoint, asset.Schema) { }
}
