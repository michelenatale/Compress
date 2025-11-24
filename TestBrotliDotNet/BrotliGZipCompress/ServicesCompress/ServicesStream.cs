



namespace michele.natale.Services;

partial class ServicesCompress
{
  /// <summary>
  /// Asynchronously copies a specified range of bytes from an input stream to an output stream.
  /// </summary>
  /// <param name="input">
  /// The source <see cref="Stream"/> to read from. 
  /// Must support seeking in order to position at <paramref name="start"/>.
  /// </param>
  /// <param name="output">
  /// The destination <see cref="Stream"/> to write to. 
  /// Must be writable. The stream remains open after the operation.
  /// </param>
  /// <param name="start">
  /// The byte offset in the input stream where copying begins.
  /// </param>
  /// <param name="length">
  /// The number of bytes to copy from the input stream.
  /// </param>
  /// <param name="bufferSize">
  /// The size of the buffer used for reading and writing. 
  /// Default is 81,920 bytes (80 KB).
  /// </param>
  /// <returns>
  /// A task that represents the asynchronous copy operation. 
  /// The task result contains the total number of bytes actually copied.
  /// </returns>
  /// <remarks>
  /// <para>
  /// The method seeks to the specified <paramref name="start"/> position in the input stream
  /// and copies exactly <paramref name="length"/> bytes (or fewer if the end of the stream is reached).
  /// </para>
  /// <para>
  /// Both streams remain open after the operation completes. 
  /// The output stream is flushed asynchronously at the end.
  /// </para>
  /// </remarks>
  public static async Task<long> CopyChunkAsync(
    Stream input, Stream output, long start, long length, int bufferSize = 81920)
  {
    if (!input.CanSeek)
      throw new InvalidOperationException("Input stream must support seeking.");

    var result = 0;
    var remaining = length;
    var buffer = new byte[bufferSize];
    input.Seek(start, SeekOrigin.Begin);
    while (remaining > 0)
    {
      var size = (int)Math.Min(bufferSize, remaining);
      var readbytes = await input.ReadAsync(buffer.AsMemory(0, size)).ConfigureAwait(false);
      if (readbytes == 0) break;
      result += readbytes;

      await output.WriteAsync(buffer.AsMemory(0, readbytes)).ConfigureAwait(false);
      remaining -= readbytes;
    }

    await output.FlushAsync().ConfigureAwait(false);
    return result;
  }
}
