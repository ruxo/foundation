namespace RZ.Foundation.Helpers;

/// <summary>
/// A stream wrapper that takes ownership of an <see cref="IDisposable"/> or <see cref="IAsyncDisposable"/> resource,
/// disposing it when the stream itself is disposed. Useful when a stream's lifetime is tied to an external resource
/// (e.g., an HTTP response that must stay alive while the content stream is read).
/// </summary>
[PublicAPI]
public sealed class OwnedStream(Stream inner, IDisposable? owner = null, IAsyncDisposable? asyncOwner = null) : Stream
{
    public static OwnedStream Of(Stream inner, IDisposable owner) => new(inner, owner);
    public static OwnedStream Of(Stream inner, IAsyncDisposable owner) => new(inner, asyncOwner: owner);

    public override bool CanRead => inner.CanRead;
    public override bool CanSeek => inner.CanSeek;
    public override bool CanWrite => inner.CanWrite;
    public override long Length => inner.Length;

    public override long Position {
        get => inner.Position;
        set => inner.Position = value;
    }

    public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);
    public override int Read(Span<byte> buffer) => inner.Read(buffer);
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => inner.ReadAsync(buffer, offset, count, cancellationToken);
    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => inner.ReadAsync(buffer, cancellationToken);
    public override int ReadByte() => inner.ReadByte();

    public override void Write(byte[] buffer, int offset, int count) => inner.Write(buffer, offset, count);
    public override void Write(ReadOnlySpan<byte> buffer) => inner.Write(buffer);
    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => inner.WriteAsync(buffer, offset, count, cancellationToken);
    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) => inner.WriteAsync(buffer, cancellationToken);
    public override void WriteByte(byte value) => inner.WriteByte(value);

    public override long Seek(long offset, SeekOrigin origin) => inner.Seek(offset, origin);
    public override void SetLength(long value) => inner.SetLength(value);
    public override void Flush() => inner.Flush();
    public override Task FlushAsync(CancellationToken cancellationToken) => inner.FlushAsync(cancellationToken);

    public override void CopyTo(Stream destination, int bufferSize) => inner.CopyTo(destination, bufferSize);
    public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken) => inner.CopyToAsync(destination, bufferSize, cancellationToken);

    public override bool CanTimeout => inner.CanTimeout;
    public override int ReadTimeout { get => inner.ReadTimeout; set => inner.ReadTimeout = value; }
    public override int WriteTimeout { get => inner.WriteTimeout; set => inner.WriteTimeout = value; }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            inner.Dispose();
            owner?.Dispose();
            (asyncOwner as IDisposable)?.Dispose();
        }
        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync() {
        await inner.DisposeAsync();
        owner?.Dispose();
        if (asyncOwner is not null)
            await asyncOwner.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}