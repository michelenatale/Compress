



namespace michele.natale.Compresses;

/// <summary>
/// A stream wrapper that exposes only a limited segment of an underlying stream.
/// </summary>
/// <remarks>
/// <para>
/// <c>SubStream</c> allows reading a defined number of bytes (<see cref="Length"/>) from an inner stream,
/// starting at its current position. It ensures that no more than the specified length is consumed.
/// </para>
/// <para>
/// This is useful for archive formats where each entry has a known compressed length,
/// and you want to restrict decompression to exactly that segment.
/// </para>
/// <para>
/// The stream supports reading only; seeking and writing are not supported.
/// </para>
/// </remarks>
public class SubStream : Stream
{
  private long MPosition;
  private readonly long MLength;
  private readonly bool MLeaveOpen;
  private readonly Stream MInnerStream;

  /// <inheritdoc/>
  public override bool CanRead => true;

  /// <inheritdoc/>
  public override bool CanSeek => false;

  /// <inheritdoc/>
  public override bool CanWrite => false;

  /// <summary>
  /// Gets the maximum length of the segment exposed by this stream.
  /// </summary>
  public override long Length => this.MLength;

  /// <summary>
  /// Gets the current read position within the segment.
  /// </summary>
  /// <exception cref="NotSupportedException">
  /// Setting the position is not supported.
  /// </exception>
  public override long Position
  {
    get => this.MPosition;
    set => throw new NotSupportedException();
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="SubStream"/> class.
  /// </summary>
  /// <param name="inner_stream">The underlying stream to wrap.</param>
  /// <param name="length">The maximum number of bytes to expose from the inner stream.</param>
  /// <param name="leave_open">
  /// If <c>true</c>, the inner stream remains open when this <see cref="SubStream"/> is disposed.
  /// </param>
  public SubStream(Stream inner_stream, long length, bool leave_open = false)
  {
    this.MLength = length;
    this.MLeaveOpen = leave_open;
    this.MInnerStream = inner_stream;
  }

  /// <summary>
  /// Reads a block of bytes from the stream and advances the position within the segment.
  /// </summary>
  /// <param name="buffer">The buffer to write the data into.</param>
  /// <param name="offset">The byte offset in <paramref name="buffer"/> at which to begin writing.</param>
  /// <param name="count">The maximum number of bytes to read.</param>
  /// <returns>
  /// The total number of bytes read into the buffer. Returns 0 if the end of the segment is reached.
  /// </returns>
  public override int Read(byte[] buffer, int offset, int count)
  {
    var remaining = this.MLength - this.MPosition;
    if (remaining < 1) return 0;

    if (count > remaining) count = (int)remaining;
    var readbytes = this.MInnerStream.Read(buffer, offset, count);
    this.MPosition += readbytes;
    return readbytes;
  }

  /// <summary>
  /// Asynchronously reads a block of bytes from the stream and advances the position within the segment.
  /// </summary>
  /// <param name="buffer">The memory buffer to write the data into.</param>
  /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
  /// <returns>
  /// A task that represents the asynchronous read operation. The result contains the number of bytes read.
  /// Returns 0 if the end of the segment is reached.
  /// </returns>
  public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
  {
    var remaining = this.MLength - this.MPosition;
    if (remaining <= 0) return 0;

    if (buffer.Length > remaining) buffer = buffer[..(int)remaining];
    var readbytes = await this.MInnerStream.ReadAsync(buffer, cancellationToken);
    this.MPosition += readbytes;
    return readbytes;
  }

  /// <summary>
  /// Releases the resources used by the <see cref="SubStream"/>.
  /// </summary>
  /// <param name="disposing">
  /// <c>true</c> to release managed resources; <c>false</c> otherwise.
  /// </param>
  /// <remarks>
  /// If <c>leave_open</c> was set to <c>true</c>, the inner stream remains open.
  /// </remarks>
  protected override void Dispose(bool disposing)
  {
    if (!this.MLeaveOpen)
      this.MInnerStream.Dispose();
    base.Dispose(disposing);
  }

  // Not Supported:

  /// <inheritdoc/>
  public override void Flush() => throw new NotSupportedException();

  /// <inheritdoc/>
  public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

  /// <inheritdoc/>
  public override void SetLength(long value) => throw new NotSupportedException();

  /// <inheritdoc/>
  public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}
