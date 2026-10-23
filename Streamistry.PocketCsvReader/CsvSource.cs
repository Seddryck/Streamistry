using global::PocketCsvReader;
using Streamistry.Pipes.Sources;

namespace Streamistry.PocketCsvReader;

/// <summary>Incremental CSV rows using the common reader boundary and PocketCsvReader configuration.</summary>
public sealed class CsvSource : DataReaderAsDictionarySource
{
    public CsvSource(Stream stream, Func<CsvProfile> profileFactory, bool leaveOpen = true)
        : base(() => Open(stream, profileFactory, leaveOpen)) { }

    public CsvSource(string path, Func<CsvProfile> profileFactory)
        : base(() => Open(File.OpenRead(path), profileFactory, false)) { }

    public CsvSource(Func<CancellationToken, ValueTask<Stream>> streamFactory, Func<CsvProfile> profileFactory)
        : base(async token => Open(await streamFactory(token).ConfigureAwait(false), profileFactory, false)) { }

    private static ReaderLease Open(Stream stream, Func<CsvProfile> profileFactory, bool leaveOpen)
    {
        ArgumentNullException.ThrowIfNull(stream);
        try
        {
            var input = leaveOpen ? new BorrowedStream(stream) : stream;
            var reader = new CsvReader(profileFactory()).ToDataReader(input);
            return ReaderLease.Own(reader, input);
        }
        catch
        {
            if (!leaveOpen) stream.Dispose();
            throw;
        }
    }

    // PocketCsvReader owns its supplied stream; this wrapper protects borrowed inputs.
    private sealed class BorrowedStream(Stream stream) : Stream
    {
        public override bool CanRead => stream.CanRead;
        public override bool CanSeek => stream.CanSeek;
        public override bool CanWrite => false;
        public override long Length => stream.Length;
        public override long Position { get => stream.Position; set => stream.Position = value; }
        public override int Read(byte[] buffer, int offset, int count) => stream.Read(buffer, offset, count);
        public override int Read(Span<byte> buffer) => stream.Read(buffer);
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            => stream.ReadAsync(buffer, cancellationToken);
        public override long Seek(long offset, SeekOrigin origin) => stream.Seek(offset, origin);
        public override void Flush() { }
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        protected override void Dispose(bool disposing) { }
    }
}
