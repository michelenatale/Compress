

namespace michele.natale.Compresses;

/// <summary>
/// A stream wrapper that counts the number of bytes written to the underlying stream.
/// </summary>
/// <remarks>
/// <para>
/// <c>CountingStream</c> delegates all operations to an inner <see cref="Stream"/> instance,
/// while incrementing the <see cref="BytesWritten"/> property whenever data is written.
/// </para>
/// <para>
/// This is useful in scenarios where the exact number of compressed or serialized bytes
/// must be known without relying on <see cref="Stream.Position"/> or <see cref="Stream.Length"/>,
/// especially when working with non-seekable streams.
/// </para>
/// <para>
/// Reading operations are passed through without affecting the counter.
/// </para>
/// </remarks>
public class CountingStream : Stream
{


  /// <summary>
  /// The underlying stream to which all operations are delegated.
  /// </summary>
  private readonly Stream InnerStream = null!;

  /// <summary>
  /// Gets the total number of bytes written to the stream.
  /// Initialized to -1 until the first write occurs.
  /// </summary>
  public long BytesWritten { get; private set; } = -1;

  /// <inheritdoc/>
  public override bool CanRead => this.InnerStream.CanRead;

  /// <inheritdoc/>
  public override bool CanSeek => this.InnerStream.CanSeek;

  /// <inheritdoc/>
  public override bool CanWrite => this.InnerStream.CanWrite;

  /// <inheritdoc/>
  public override long Length => this.InnerStream.Length;

  /// <inheritdoc/>
  public override long Position
  {
    get => this.InnerStream.Position;
    set => this.InnerStream.Position = value;
  }

  /// <inheritdoc/>
  public override void Flush() => this.InnerStream.Flush();

  /// <inheritdoc/>
  public override Task FlushAsync(CancellationToken cancellationToken) =>
      this.InnerStream.FlushAsync(cancellationToken);

  /// <inheritdoc/>
  public override int Read(byte[] buffer, int offset, int count) =>
      this.InnerStream.Read(buffer, offset, count);

  /// <inheritdoc/>
  public override long Seek(long offset, SeekOrigin origin) =>
      this.InnerStream.Seek(offset, origin);

  /// <inheritdoc/>
  public override void SetLength(long value) =>
      this.InnerStream.SetLength(value);

  /// <summary>
  /// Initializes a new instance of the <see cref="CountingStream"/> class
  /// that wraps the specified inner stream.
  /// </summary>
  /// <param name="inner_stream">The underlying stream to wrap.</param>
  public CountingStream(Stream inner_stream) =>
      this.InnerStream = inner_stream;

  /// <inheritdoc/>
  public override void Write(byte[] buffer, int offset, int count)
  {
    this.InnerStream.Write(buffer, offset, count);
    this.BytesWritten += count;
  }

  /// <inheritdoc/>
  public override async Task WriteAsync(
      byte[] buffer, int offset, int count,
      CancellationToken cancellationToken)
  {
    await this.InnerStream.WriteAsync(
        buffer.AsMemory(offset, count), cancellationToken);
    this.BytesWritten += count;
  }

  /// <inheritdoc/>
  public override void Write(ReadOnlySpan<byte> buffer)
  {
    this.InnerStream.Write(buffer);
    this.BytesWritten += buffer.Length;
  }

  /// <inheritdoc/>
  public override ValueTask WriteAsync(
      ReadOnlyMemory<byte> buffer,
      CancellationToken cancellationToken = default)
  {
    this.BytesWritten += buffer.Length;
    return this.InnerStream.WriteAsync(buffer, cancellationToken);
  }
}
