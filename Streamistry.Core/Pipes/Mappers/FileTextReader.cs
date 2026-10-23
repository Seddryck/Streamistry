using System.Text;

namespace Streamistry.Pipes.Mappers;

/// <summary>Reads each input path as text, closing each file before emitting its content.</summary>
public sealed class FileTextReader(IChainablePort<string> upstream, Encoding? encoding = null)
    : Mapper<string, string>(upstream, path => File.ReadAllText(path, encoding ?? Encoding.UTF8));
