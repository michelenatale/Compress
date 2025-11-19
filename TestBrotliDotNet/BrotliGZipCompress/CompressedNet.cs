


//Synchron: Span<T> / ReadOnlySpan<T> → maximale Performance, keine Allokationen.
//Asynchron: byte[] / Memory<T> → await-sicher, stabil über Task-Grenzen hinweg.



using System.IO.Compression;

namespace michele.natale.Compressors;



/// <summary>
/// Provides a comprehensive set of utilities for compression and decompression
/// using GZip and Brotli algorithms in .NET.
/// </summary>
/// <remarks>
/// <para>
/// The <c>CompressedNet</c> class offers synchronous and asynchronous methods for working with
/// both raw byte arrays and streams. It supports cancellation tokens for responsive operations,
/// buffer size configuration for performance tuning, and "Try" methods with safe buffer growth
/// strategies to handle edge cases where compressed output may exceed expectations.
/// </para>
/// <para>
/// Key features include:
/// <list type="bullet">
/// <item><description>Compression and decompression with GZip and Brotli.</description></item>
/// <item><description>Synchronous methods optimized for small payloads using <see cref="ReadOnlySpan{T}"/>.</description></item>
/// <item><description>Asynchronous methods for larger data sets, supporting <see cref="CancellationToken"/>.</description></item>
/// <item><description>File-based and stream-based overloads for flexible integration.</description></item>
/// <item><description>Robust "TryCompress" and "TryDecompress" methods with exponential buffer growth and safety limits.</description></item>
/// </list>
/// </para>
/// <para>
/// This class is designed for audit-friendly, production-ready use cases where correctness,
/// efficiency, and explicit resource control are required. It is suitable for applications
/// that need reliable compression services with clear exception handling and cancellation support.
/// </para>
/// </remarks>
/// <example>
/// Example usage:
/// <code>
/// // Compress a file asynchronously with Brotli
/// await CompressedNet.CompressBrotliAsync("input.txt", "output.br");
///
/// // Decompress a GZip stream synchronously
/// using var input = File.OpenRead("data.gz");
/// using var output = File.Create("data.txt");
/// CompressedNet.DecompressGZip(input, output);
/// </code>
/// </example>
public class CompressedNet
{

  /// <summary>
  /// The minimum Brotli window size (logarithmic, base 2).
  /// </summary>
  /// <remarks>
  /// Brotli requires a window size between 10 and 24. Smaller windows reduce memory usage
  /// but may lower compression efficiency.
  /// </remarks>
  public const int BROTLI_MIN_WINDOW = 10;

  /// <summary>
  /// The maximum Brotli window size (logarithmic, base 2).
  /// </summary>
  /// <remarks>
  /// Brotli requires a window size between 10 and 24. Larger windows improve compression
  /// ratio for large inputs but increase memory usage.
  /// </remarks>
  public const int BROTLI_MAX_WINDOW = 24;

  /// <summary>
  /// The minimum Brotli compression quality.
  /// </summary>
  /// <remarks>
  /// Brotli quality ranges from 0 (fastest, lowest compression) to 11 (slowest, highest compression).
  /// </remarks>
  public const int BROTLI_MIN_QUALITY = 0;

  /// <summary>
  /// The maximum Brotli compression quality.
  /// </summary>
  /// <remarks>
  /// Brotli quality ranges from 0 (fastest, lowest compression) to 11 (slowest, highest compression).
  /// </remarks>
  public const int BROTLI_MAX_QUALITY = 11;

  /// <summary>
  /// The maximum allowed input length in bytes for compression and decompression operations.
  /// </summary>
  /// <remarks>
  /// This limit is set to <c>2^23</c> (8 MB) to prevent uncontrolled memory growth during buffer
  /// allocation in "TryCompress" and "TryDecompress" methods. Inputs larger than this threshold
  /// will throw an <see cref="ArgumentOutOfRangeException"/>.
  /// </remarks>
  public const int MAX_BYTES_LENGTH = 1 << 23;

  #region GZip

  #region GZip Bytes

  #region GZip Compress

  /// <summary>
  /// Compresses the given input using GZip with the specified compression level.
  /// </summary>
  /// <param name="bytes">
  /// The input data to be compressed. Must not be empty and must not exceed <see cref="MAX_BYTES_LENGTH"/>.
  /// </param>
  /// <param name="compresslevel">
  /// The compression level to use. Defaults to <see cref="CompressionLevel.Optimal"/>.
  /// </param>
  /// <returns>
  /// A byte array containing the compressed data.
  /// </returns>
  /// <remarks>
  /// <para>
  /// This synchronous method is optimized for small payloads (≤ 8 MB). It uses <see cref="ReadOnlySpan{T}"/> to avoid
  /// unnecessary allocations and achieve maximum performance.
  /// </para>
  /// <para>
  /// GZip compression may in rare cases produce output larger than the original input, especially for small or
  /// incompressible data. This is expected behavior and not an error.
  /// </para>
  /// <para>
  /// For larger data sets or scenarios where responsiveness is important, consider using the asynchronous
  /// stream-based overloads (<c>CompressGZipAsync</c>) to avoid blocking the calling thread.
  /// </para>
  /// </remarks>
  /// <exception cref="ArgumentException">
  /// Thrown if <paramref name="bytes"/> is empty.
  /// </exception>
  /// <exception cref="ArgumentOutOfRangeException">
  /// Thrown if <paramref name="bytes"/> exceeds <see cref="MAX_BYTES_LENGTH"/>.
  /// </exception>
  public static byte[] CompressGZip(
    ReadOnlySpan<byte> bytes, CompressionLevel compresslevel =
      CompressionLevel.Optimal)
  {
    AssertCompress(bytes);

    using var ms = new MemoryStream();
    using (var gzip = new GZipStream(ms, compresslevel))
      gzip.Write(bytes);

    return ms.ToArray();
  }

  /// <summary>
  /// Asynchronously compresses the given input using GZip with the specified compression level.
  /// </summary>
  /// <param name="bytes">
  /// The input data to be compressed. Must not be empty and must not exceed <see cref="MAX_BYTES_LENGTH"/>.
  /// </param>
  /// <param name="ct">
  /// A cancellation token that can be used to cancel the compression operation.
  /// </param>
  /// <param name="compresslevel">
  /// The compression level to use. Defaults to <see cref="CompressionLevel.Optimal"/>.
  /// </param>
  /// <returns>
  /// A task that represents the asynchronous compression operation. The task result contains the compressed data.
  /// </returns>
  /// <remarks>
  /// <para>
  /// This method is await-safe and supports cancellation. If <paramref name="ct"/> is triggered during the operation,
  /// an <see cref="OperationCanceledException"/> will be thrown.
  /// </para>
  /// <para>
  /// For small payloads (≤ 8 MB), the entire input is written in one operation. For larger data, use the stream-based
  /// overloads to avoid excessive memory usage.
  /// </para>
  /// </remarks>
  /// <exception cref="ArgumentException">Thrown if <paramref name="bytes"/> is empty.</exception>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="bytes"/> exceeds <see cref="MAX_BYTES_LENGTH"/>.</exception>
  public static byte[] CompressGZip(
  ReadOnlySpan<byte> bytes, CancellationToken ct,
    CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    AssertCompress(bytes);

    ct.ThrowIfCancellationRequested();

    using var ms = new MemoryStream();
    using (var gzip = new GZipStream(ms, compresslevel))
      gzip.Write(bytes);

    return ms.ToArray();
  }

  /// <summary>
  /// Decompresses GZip-compressed input into its original form.
  /// </summary>
  /// <param name="bytes">
  /// The compressed input data. Must not be empty.
  /// </param>
  /// <returns>
  /// A byte array containing the decompressed data.
  /// </returns>
  /// <remarks>
  /// <para>
  /// This synchronous method is optimized for small payloads (≤ 8 MB). It uses <see cref="ReadOnlySpan{T}"/> to avoid
  /// unnecessary allocations and achieve maximum performance.
  /// </para>
  /// <para>
  /// The method creates a <see cref="MemoryStream"/> from the compressed input and uses a <see cref="GZipStream"/>
  /// in <see cref="CompressionMode.Decompress"/> to copy the decompressed data into an output stream.
  /// </para>
  /// <para>
  /// If the input data is invalid or corrupted, an <see cref="InvalidDataException"/> may be thrown by
  /// <see cref="GZipStream"/>.
  /// </para>
  /// <para>
  /// For larger data sets or scenarios where responsiveness is important, consider using the asynchronous
  /// stream-based overloads (<c>DecompressGZipAsync</c>) to avoid blocking the calling thread.
  /// </para>
  /// </remarks>
  /// <exception cref="ArgumentException">
  /// Thrown if <paramref name="bytes"/> is empty.
  /// </exception>
  /// <exception cref="InvalidDataException">
  /// Thrown if <paramref name="bytes"/> does not represent valid GZip-compressed data.
  /// </exception>
  public static byte[] DecompressGZip(ReadOnlySpan<byte> bytes)
  {
    using var msout = new MemoryStream();
    using var ms = new MemoryStream(bytes.ToArray());
    using (var gzip = new GZipStream(ms, CompressionMode.Decompress))
      gzip.CopyTo(msout);

    return msout.ToArray();
  }

  /// <summary>
  /// Asynchronously decompresses GZip-compressed input into its original form.
  /// </summary>
  /// <param name="bytes">
  /// The compressed input data. Must not be empty.
  /// </param>
  /// <param name="ct">
  /// A cancellation token that can be used to cancel the decompression operation.
  /// </param>
  /// <returns>
  /// A task that represents the asynchronous decompression operation. The task result contains the decompressed data.
  /// </returns>
  /// <remarks>
  /// <para>
  /// This method is await-safe and supports cancellation. If <paramref name="ct"/> is triggered during the operation,
  /// an <see cref="OperationCanceledException"/> will be thrown.
  /// </para>
  /// <para>
  /// For large data sets, consider using the stream-based overloads to avoid excessive memory usage.
  /// </para>
  /// </remarks>
  /// <exception cref="ArgumentException">Thrown if <paramref name="bytes"/> is empty.</exception>
  /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via <paramref name="ct"/>.</exception>
  public static byte[] DecompressGZip(
    ReadOnlySpan<byte> bytes, CancellationToken ct)
  {
    ct.ThrowIfCancellationRequested();

    using var msout = new MemoryStream();
    using var ms = new MemoryStream(bytes.ToArray());
    using (var gzip = new GZipStream(ms, CompressionMode.Decompress))
      gzip.CopyTo(msout);

    return msout.ToArray();
  }

  #endregion GZip Compress

  #region Async GZip Compress

  /// <summary>
  /// Asynchronously compresses the given input using GZip with the specified compression level.
  /// </summary>
  /// <param name="bytes">
  /// The input data to be compressed. Must not be empty and must not exceed <see cref="MAX_BYTES_LENGTH"/>.
  /// </param>
  /// <param name="compresslevel">
  /// The compression level to use. Defaults to <see cref="CompressionLevel.Optimal"/>.
  /// </param>
  /// <returns>
  /// A task that represents the asynchronous compression operation. The task result contains the compressed data.
  /// </returns>
  /// <remarks>
  /// <para>
  /// This overload does not support cancellation. Use the variant with <see cref="CancellationToken"/> if cancellation
  /// is required.
  /// </para>
  /// <para>
  /// For small payloads (≤ 8 MB), the entire input is written in one operation. For larger data sets, consider using
  /// the stream-based overloads to avoid excessive memory usage.
  /// </para>
  /// </remarks>
  /// <exception cref="ArgumentException">Thrown if <paramref name="bytes"/> is empty.</exception>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="bytes"/> exceeds <see cref="MAX_BYTES_LENGTH"/>.</exception>
  public async static Task<byte[]> CompressGZipAsync(
    byte[] bytes, CompressionLevel compresslevel =
      CompressionLevel.Optimal)
  {
    AssertCompress(bytes);

    await using var ms = new MemoryStream();
    await using (var gzip = new GZipStream(ms, compresslevel))
      await gzip.WriteAsync(bytes).ConfigureAwait(false);

    return ms.ToArray();
  }

  /// <summary>
  /// Asynchronously compresses the given input using GZip with the specified compression level, supporting cancellation.
  /// </summary>
  /// <param name="bytes">
  /// The input data to be compressed. Must not be empty and must not exceed <see cref="MAX_BYTES_LENGTH"/>.
  /// </param>
  /// <param name="ct">
  /// A cancellation token that can be used to cancel the compression operation.
  /// </param>
  /// <param name="compresslevel">
  /// The compression level to use. Defaults to <see cref="CompressionLevel.Optimal"/>.
  /// </param>
  /// <returns>
  /// A task that represents the asynchronous compression operation. The task result contains the compressed data.
  /// </returns>
  /// <remarks>
  /// <para>
  /// If <paramref name="ct"/> is triggered during the operation, an <see cref="OperationCanceledException"/> will be thrown.
  /// </para>
  /// <para>
  /// For larger data sets, consider using the stream-based overloads to avoid blocking and excessive memory usage.
  /// </para>
  /// </remarks>
  /// <exception cref="ArgumentException">Thrown if <paramref name="bytes"/> is empty.</exception>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="bytes"/> exceeds <see cref="MAX_BYTES_LENGTH"/>.</exception>
  /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via <paramref name="ct"/>.</exception>
  public async static Task<byte[]> CompressGZipAsync(
    byte[] bytes, CancellationToken ct,
    CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    AssertCompress(bytes);

    await using var ms = new MemoryStream();
    await using (var gzip = new GZipStream(ms, compresslevel))
      await gzip.WriteAsync(bytes, ct).ConfigureAwait(false);

    return ms.ToArray();
  }

  /// <summary>
  /// Asynchronously decompresses GZip-compressed input into its original form.
  /// </summary>
  /// <param name="bytes">
  /// The compressed input data. Must not be empty.
  /// </param>
  /// <returns>
  /// A task that represents the asynchronous decompression operation. The task result contains the decompressed data.
  /// </returns>
  /// <remarks>
  /// <para>
  /// This overload does not support cancellation. Use the variant with <see cref="CancellationToken"/> if cancellation
  /// is required.
  /// </para>
  /// <para>
  /// If the input data is invalid or corrupted, an <see cref="InvalidDataException"/> may be thrown by
  /// <see cref="GZipStream"/>.
  /// </para>
  /// </remarks>
  /// <exception cref="ArgumentException">Thrown if <paramref name="bytes"/> is empty.</exception>
  /// <exception cref="InvalidDataException">Thrown if <paramref name="bytes"/> does not represent valid GZip-compressed data.</exception>
  public async static Task<byte[]> DecompressGZipAsync(byte[] bytes)
  {
    await using var msout = new MemoryStream();
    await using var ms = new MemoryStream(bytes);
    await using (var gzip = new GZipStream(ms, CompressionMode.Decompress))
      await gzip.CopyToAsync(msout).ConfigureAwait(false);

    return msout.ToArray();
  }

  /// <summary>
  /// Asynchronously decompresses GZip-compressed input into its original form, supporting cancellation.
  /// </summary>
  /// <param name="bytes">
  /// The compressed input data. Must not be empty.
  /// </param>
  /// <param name="ct">
  /// A cancellation token that can be used to cancel the decompression operation.
  /// </param>
  /// <returns>
  /// A task that represents the asynchronous decompression operation. The task result contains the decompressed data.
  /// </returns>
  /// <remarks>
  /// <para>
  /// If <paramref name="ct"/> is triggered during the operation, an <see cref="OperationCanceledException"/> will be thrown.
  /// </para>
  /// <para>
  /// If the input data is invalid or corrupted, an <see cref="InvalidDataException"/> may be thrown by
  /// <see cref="GZipStream"/>.
  /// </para>
  /// </remarks>
  /// <exception cref="ArgumentException">Thrown if <paramref name="bytes"/> is empty.</exception>
  /// <exception cref="InvalidDataException">Thrown if <paramref name="bytes"/> does not represent valid GZip-compressed data.</exception>
  /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via <paramref name="ct"/>.</exception>
  public async static Task<byte[]> DecompressGZipAsync(
   byte[] bytes, CancellationToken ct)
  {
    await using var msout = new MemoryStream();
    await using var ms = new MemoryStream(bytes);
    await using (var gzip = new GZipStream(ms, CompressionMode.Decompress))
      await gzip.CopyToAsync(msout, ct).ConfigureAwait(false);

    return msout.ToArray();
  }

  #endregion Async GZip Compress

  #endregion GZip Bytes

  #region GZip FileStream

  #region FileStream Compress

  /// <summary>
  /// Compresses the contents of a source file into a destination file using GZip.
  /// </summary>
  /// <param name="src">The path to the source file to be compressed.</param>
  /// <param name="dest">The path to the destination file where compressed data will be written.</param>
  /// <param name="buffersize">
  /// The size of the buffer used for reading and writing. Defaults to 81920 bytes (the .NET default).
  /// </param>
  /// <param name="compresslevel">
  /// The compression level to use. Defaults to <see cref="CompressionLevel.Optimal"/>.
  /// </param>
  /// <remarks>
  /// <para>
  /// This synchronous method is suitable for batch jobs or scenarios where blocking is acceptable.
  /// </para>
  /// <para>
  /// For large files or responsive applications, consider using the asynchronous overloads to avoid blocking.
  /// </para>
  /// </remarks>
  /// <exception cref="FileNotFoundException">Thrown if the source file does not exist.</exception>
  /// <exception cref="IOException">Thrown if an I/O error occurs during compression.</exception>
  public static void CompressGZip(
    string src, string dest, int buffersize = 81920,
    CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
    using var fsout = new FileStream(dest, FileMode.Create, FileAccess.Write);
    CompressGZip(fsin, fsout, buffersize, compresslevel);
  }

  /// <summary>
  /// Compresses the contents of a source file into a destination file using GZip, supporting cancellation.
  /// </summary>
  /// <param name="src">The path to the source file to be compressed.</param>
  /// <param name="dest">The path to the destination file where compressed data will be written.</param>
  /// <param name="ct">A cancellation token that can be used to cancel the compression operation.</param>
  /// <param name="buffersize">The size of the buffer used for reading and writing. Defaults to 81920 bytes.</param>
  /// <param name="compresslevel">The compression level to use. Defaults to <see cref="CompressionLevel.Optimal"/>.</param>
  /// <remarks>
  /// If <paramref name="ct"/> is triggered during the operation, an <see cref="OperationCanceledException"/> will be thrown.
  /// </remarks>
  /// <exception cref="FileNotFoundException">Thrown if the source file does not exist.</exception>
  /// <exception cref="IOException">Thrown if an I/O error occurs during compression.</exception>
  /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via <paramref name="ct"/>.</exception>
  public static void CompressGZip(
    string src, string dest, CancellationToken ct, int buffersize = 81920,
    CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
    using var fsout = new FileStream(dest, FileMode.Create, FileAccess.Write);
    CompressGZip(fsin, fsout, ct, buffersize, compresslevel);
  }

  /// <summary>
  /// Compresses data from an input stream into an output stream using GZip.
  /// </summary>
  /// <param name="input">The input stream containing data to be compressed.</param>
  /// <param name="output">The output stream where compressed data will be written.</param>
  /// <param name="buffersize">The size of the buffer used for reading and writing. Defaults to 81920 bytes.</param>
  /// <param name="compresslevel">The compression level to use. Defaults to <see cref="CompressionLevel.Optimal"/>.</param>
  /// <remarks>
  /// The caller is responsible for managing the lifetime of <paramref name="input"/> and <paramref name="output"/>.
  /// </remarks>
  /// <exception cref="IOException">Thrown if an I/O error occurs during compression.</exception>
  public static void CompressGZip(
    Stream input, Stream output, int buffersize = 81920,
    CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    int readbytes;
    var buffer = new byte[buffersize];

    using var gzip = new GZipStream(output, compresslevel, false);
    while ((readbytes = input.Read(buffer)) > 0)
      gzip.Write(buffer, 0, readbytes);
  }

  /// <summary>
  /// Compresses data from an input stream into an output stream using GZip, supporting cancellation.
  /// </summary>
  /// <param name="input">The input stream containing data to be compressed.</param>
  /// <param name="output">The output stream where compressed data will be written.</param>
  /// <param name="ct">A cancellation token that can be used to cancel the compression operation.</param>
  /// <param name="buffersize">The size of the buffer used for reading and writing. Defaults to 81920 bytes.</param>
  /// <param name="compresslevel">The compression level to use. Defaults to <see cref="CompressionLevel.Optimal"/>.</param>
  /// <remarks>
  /// If <paramref name="ct"/> is triggered during the operation, an <see cref="OperationCanceledException"/> will be thrown.
  /// </remarks>
  /// <exception cref="IOException">Thrown if an I/O error occurs during compression.</exception>
  /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via <paramref name="ct"/>.</exception>
  public static void CompressGZip(
    Stream input, Stream output, CancellationToken ct, int buffersize = 81920,
    CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    int readbytes;
    var buffer = new byte[buffersize];

    using var gzip = new GZipStream(output, compresslevel, false);
    while ((readbytes = input.Read(buffer)) > 0)
    {
      ct.ThrowIfCancellationRequested();
      gzip.Write(buffer, 0, readbytes);
    }
  }

  // ********** ********** ********** ********** ********** ********** ********** **********
  // ********** ********** ********** ********** ********** ********** ********** **********

  /// <summary>
  /// Decompresses a GZip-compressed source file into a destination file.
  /// </summary>
  /// <param name="src">The path to the source file containing GZip-compressed data.</param>
  /// <param name="dest">The path to the destination file where decompressed data will be written.</param>
  /// <param name="buffersize">The size of the buffer used for reading and writing. Defaults to 81920 bytes.</param>
  /// <exception cref="FileNotFoundException">Thrown if the source file does not exist.</exception>
  /// <exception cref="InvalidDataException">Thrown if the source file does not contain valid GZip data.</exception>
  /// <exception cref="IOException">Thrown if an I/O error occurs during decompression.</exception>
  public static void DecompressGZip(
    string src, string dest, int buffersize = 81920)
  {
    using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
    using var fsout = new FileStream(dest, FileMode.Create, FileAccess.Write);
    DecompressGZip(fsin, fsout, buffersize);
  }

  /// <summary>
  /// Decompresses a GZip-compressed source file into a destination file, supporting cancellation.
  /// </summary>
  /// <param name="src">The path to the source file containing GZip-compressed data.</param>
  /// <param name="dest">The path to the destination file where decompressed data will be written.</param>
  /// <param name="ct">A cancellation token that can be used to cancel the decompression operation.</param>
  /// <param name="buffersize">The size of the buffer used for reading and writing. Defaults to 81920 bytes.</param>
  /// <exception cref="FileNotFoundException">Thrown if the source file does not exist.</exception>
  /// <exception cref="InvalidDataException">Thrown if the source file does not contain valid GZip data.</exception>
  /// <exception cref="IOException">Thrown if an I/O error occurs during decompression.</exception>
  /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via <paramref name="ct"/>.</exception>
  public static void DecompressGZip(
    string src, string dest, CancellationToken ct, int buffersize = 81920)
  {
    using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
    using var fsout = new FileStream(dest, FileMode.Create, FileAccess.Write);
    DecompressGZip(fsin, fsout, ct, buffersize);
  }

  /// <summary>
  /// Decompresses GZip-compressed data from an input stream into an output stream.
  /// </summary>
  /// <param name="input">The input stream containing GZip-compressed data.</param>
  /// <param name="output">The output stream where decompressed data will be written.</param>
  /// <param name="buffersize">The size of the buffer used for reading and writing. Defaults to 81920 bytes.</param>
  /// <remarks>
  /// The caller is responsible for managing the lifetime of <paramref name="input"/> and <paramref name="output"/>.
  /// </remarks>
  /// <exception cref="InvalidDataException">Thrown if <paramref name="input"/> does not contain valid GZip data.</exception>
  /// <exception cref="IOException">Thrown if an I/O error occurs during decompression.</exception>
  public static void DecompressGZip(
    Stream input, Stream output, int buffersize = 81920)
  {
    int readbytes;
    var buffer = new byte[buffersize];

    using var gzip = new GZipStream(input, CompressionMode.Decompress);
    while ((readbytes = gzip.Read(buffer)) > 0)
      output.Write(buffer, 0, readbytes);
  }

  /// <summary>
  /// Decompresses GZip-compressed data from an input stream into an output stream, supporting cancellation.
  /// </summary>
  /// <param name="input">The input stream containing GZip-compressed data.</param>
  /// <param name="output">The output stream where decompressed data will be written.</param>
  /// <param name="ct">A cancellation token that can be used to cancel the decompression operation.</param>
  /// <param name="buffersize">The size of the buffer used for reading and writing. Defaults to 81920 bytes.</param>
  /// <remarks>
  /// If <paramref name="ct"/> is triggered during the operation, an <see cref="OperationCanceledException"/> will be thrown.
  /// </remarks>
  /// <exception cref="InvalidDataException">Thrown if <paramref name="input"/> does not contain valid GZip data.</exception>
  /// <exception cref="IOException">Thrown if an I/O error occurs during decompression.</exception>
  /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via <paramref name="ct"/>.</exception>
  public static void DecompressGZip(
    Stream input, Stream output, CancellationToken ct, int buffersize = 81920)
  {
    int readbytes;
    var buffer = new byte[buffersize];

    using var gzip = new GZipStream(input, CompressionMode.Decompress);
    while ((readbytes = gzip.Read(buffer)) > 0)
    {
      ct.ThrowIfCancellationRequested();
      output.Write(buffer, 0, readbytes);
    }
  }

  #endregion FileStream Compress

  #region Async FileStream Compress

  /// <summary>
  /// Asynchronously compresses the contents of a source file into a destination file using GZip.
  /// </summary>
  /// <param name="src">The path to the source file to be compressed.</param>
  /// <param name="dest">The path to the destination file where compressed data will be written.</param>
  /// <param name="buffersize">
  /// The size of the buffer used for reading and writing. Defaults to 81920 bytes (the .NET default).
  /// </param>
  /// <param name="compresslevel">
  /// The compression level to use. Defaults to <see cref="CompressionLevel.Optimal"/>.
  /// </param>
  /// <returns>A task representing the asynchronous compression operation.</returns>
  /// <remarks>
  /// <para>
  /// This overload does not support cancellation. Use the variant with <see cref="CancellationToken"/> if cancellation
  /// is required.
  /// </para>
  /// <para>
  /// For large files or responsive applications, asynchronous compression avoids blocking the calling thread.
  /// </para>
  /// </remarks>
  /// <exception cref="FileNotFoundException">Thrown if the source file does not exist.</exception>
  /// <exception cref="IOException">Thrown if an I/O error occurs during compression.</exception>
  public async static Task CompressGZipAsync(
      string src, string dest, int buffersize = 81920,
      CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    await using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
    await using var fsout = new FileStream(dest, FileMode.Create, FileAccess.Write);
    await CompressGZipAsync(fsin, fsout, buffersize, compresslevel).ConfigureAwait(false);
  }

  /// <summary>
  /// Asynchronously compresses the contents of a source file into a destination file using GZip, supporting cancellation.
  /// </summary>
  /// <param name="src">The path to the source file to be compressed.</param>
  /// <param name="dest">The path to the destination file where compressed data will be written.</param>
  /// <param name="ct">A cancellation token that can be used to cancel the compression operation.</param>
  /// <param name="buffersize">The size of the buffer used for reading and writing. Defaults to 81920 bytes.</param>
  /// <param name="compresslevel">The compression level to use. Defaults to <see cref="CompressionLevel.Optimal"/>.</param>
  /// <returns>A task representing the asynchronous compression operation.</returns>
  /// <remarks>
  /// If <paramref name="ct"/> is triggered during the operation, an <see cref="OperationCanceledException"/> will be thrown.
  /// </remarks>
  /// <exception cref="FileNotFoundException">Thrown if the source file does not exist.</exception>
  /// <exception cref="IOException">Thrown if an I/O error occurs during compression.</exception>
  /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via <paramref name="ct"/>.</exception>
  public async static Task CompressGZipAsync(
    string src, string dest, CancellationToken ct, int buffersize = 81920,
    CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    await using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
    await using var fsout = new FileStream(dest, FileMode.Create, FileAccess.Write);
    await CompressGZipAsync(fsin, fsout, ct, buffersize, compresslevel).ConfigureAwait(false);
  }

  /// <summary>
  /// Asynchronously compresses data from an input stream into an output stream using GZip.
  /// </summary>
  /// <param name="input">The input stream containing data to be compressed.</param>
  /// <param name="output">The output stream where compressed data will be written.</param>
  /// <param name="buffersize">The size of the buffer used for reading and writing. Defaults to 81920 bytes.</param>
  /// <param name="compresslevel">The compression level to use. Defaults to <see cref="CompressionLevel.Optimal"/>.</param>
  /// <returns>A task representing the asynchronous compression operation.</returns>
  /// <remarks>
  /// The caller is responsible for managing the lifetime of <paramref name="input"/> and <paramref name="output"/>.
  /// </remarks>
  /// <exception cref="IOException">Thrown if an I/O error occurs during compression.</exception>
  public async static Task CompressGZipAsync(
    Stream input, Stream output, int buffersize = 81920,
    CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    int readbytes;
    var buffer = new byte[buffersize];

    await using var gzip = new GZipStream(output, compresslevel);
    while ((readbytes = await input.ReadAsync(
      buffer.AsMemory(0, buffer.Length)).ConfigureAwait(false)) > 0)
      await gzip.WriteAsync(buffer.AsMemory(0, readbytes)).ConfigureAwait(false);
  }

  /// <summary>
  /// Asynchronously compresses data from an input stream into an output stream using GZip, supporting cancellation.
  /// </summary>
  /// <param name="input">The input stream containing data to be compressed.</param>
  /// <param name="output">The output stream where compressed data will be written.</param>
  /// <param name="ct">A cancellation token that can be used to cancel the compression operation.</param>
  /// <param name="buffersize">The size of the buffer used for reading and writing. Defaults to 81920 bytes.</param>
  /// <param name="compresslevel">The compression level to use. Defaults to <see cref="CompressionLevel.Optimal"/>.</param>
  /// <returns>A task representing the asynchronous compression operation.</returns>
  /// <remarks>
  /// If <paramref name="ct"/> is triggered during the operation, an <see cref="OperationCanceledException"/> will be thrown.
  /// </remarks>
  /// <exception cref="IOException">Thrown if an I/O error occurs during compression.</exception>
  /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via <paramref name="ct"/>.</exception>
  public async static Task CompressGZipAsync(
    Stream input, Stream output, CancellationToken ct, int buffersize = 81920,
    CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    int readbytes;
    var buffer = new byte[buffersize];

    await using var gzip = new GZipStream(output, compresslevel);
    while ((readbytes = await input.ReadAsync(
      buffer.AsMemory(0, buffer.Length), ct).ConfigureAwait(false)) > 0)
      await gzip.WriteAsync(buffer.AsMemory(0, readbytes), ct).ConfigureAwait(false);
  }


  // ********** ********** ********** ********** ********** ********** ********** **********
  // ********** ********** ********** ********** ********** ********** ********** **********


  /// <summary>
  /// Asynchronously decompresses a GZip-compressed source file into a destination file.
  /// </summary>
  /// <param name="src">The path to the source file containing GZip-compressed data.</param>
  /// <param name="dest">The path to the destination file where decompressed data will be written.</param>
  /// <param name="buffersize">The size of the buffer used for reading and writing. Defaults to 81920 bytes.</param>
  /// <returns>A task representing the asynchronous decompression operation.</returns>
  /// <exception cref="FileNotFoundException">Thrown if the source file does not exist.</exception>
  /// <exception cref="InvalidDataException">Thrown if the source file does not contain valid GZip data.</exception>
  /// <exception cref="IOException">Thrown if an I/O error occurs during decompression.</exception>
  public async static Task DecompressGZipAsync(
    string src, string dest, int buffersize = 81920)
  {
    await using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
    await using var fsout = new FileStream(dest, FileMode.Create, FileAccess.Write);
    await DecompressGZipAsync(fsin, fsout, buffersize).ConfigureAwait(false);
  }

  /// <summary>
  /// Asynchronously decompresses a GZip-compressed source file into a destination file, supporting cancellation.
  /// </summary>
  /// <param name="src">The path to the source file containing GZip-compressed data.</param>
  /// <param name="dest">The path to the destination file where decompressed data will be written.</param>
  /// <param name="ct">A cancellation token that can be used to cancel the decompression operation.</param>
  /// <param name="buffersize">The size of the buffer used for reading and writing. Defaults to 81920 bytes.</param>
  /// <returns>A task representing the asynchronous decompression operation.</returns>
  /// <exception cref="FileNotFoundException">Thrown if the source file does not exist.</exception>
  /// <exception cref="InvalidDataException">Thrown if the source file does not contain valid GZip data.</exception>
  /// <exception cref="IOException">Thrown if an I/O error occurs during decompression.</exception>
  /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via <paramref name="ct"/>.</exception>
  public async static Task DecompressGZipAsync(
    string src, string dest, CancellationToken ct, int buffersize = 81920)
  {
    await using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
    await using var fsout = new FileStream(dest, FileMode.Create, FileAccess.Write);
    await DecompressGZipAsync(fsin, fsout, ct, buffersize).ConfigureAwait(false);
  }

  /// <summary>
  /// Asynchronously decompresses GZip-compressed data from an input stream into an output stream.
  /// </summary>
  /// <param name="input">The input stream containing GZip-compressed data.</param>
  /// <param name="output">The output stream where decompressed data will be written.</param>
  /// <param name="buffersize">The size of the buffer used for reading and writing. Defaults to 81920 bytes.</param>
  /// <returns>A task representing the asynchronous decompression operation.</returns>
  /// <remarks>
  /// The caller is responsible for managing the lifetime of <paramref name="input"/> and <paramref name="output"/>.
  /// </remarks>
  /// <exception cref="InvalidDataException">Thrown if <paramref name="input"/> does not contain valid GZip data.</exception>
  /// <exception cref="IOException">Thrown if an I/O error occurs during decompression.</exception>
  public async static Task DecompressGZipAsync(
    Stream input, Stream output, int buffersize = 81920)
  {
    int readbytes;
    var buffer = new byte[buffersize];

    await using var gzip = new GZipStream(input, CompressionMode.Decompress);
    while ((readbytes = await gzip.ReadAsync(
      buffer.AsMemory(0, buffer.Length)).ConfigureAwait(false)) > 0)
      await output.WriteAsync(buffer.AsMemory(0, readbytes)).ConfigureAwait(false);
  }

  /// <summary>
  /// Asynchronously decompresses GZip-compressed data from an input stream into an output stream, supporting cancellation.
  /// </summary>
  /// <param name="input">
  /// The input stream containing GZip-compressed data. The stream must be readable and positioned at the start of the compressed content.
  /// </param>
  /// <param name="output">
  /// The output stream where decompressed data will be written. The stream must be writable.
  /// </param>
  /// <param name="ct">
  /// A cancellation token that can be used to cancel the decompression operation.
  /// </param>
  /// <param name="buffersize">
  /// The size of the buffer used for reading and writing. Defaults to 81920 bytes (the .NET recommended default).
  /// </param>
  /// <returns>
  /// A task representing the asynchronous decompression operation.
  /// </returns>
  /// <remarks>
  /// <para>
  /// The caller is responsible for the lifetime of <paramref name="input"/> and <paramref name="output"/>. This method
  /// does not close or dispose the provided streams.
  /// </para>
  /// <para>
  /// If <paramref name="ct"/> is triggered during the operation, an <see cref="OperationCanceledException"/> will be thrown.
  /// </para>
  /// <para>
  /// If the input data is invalid or corrupted, <see cref="GZipStream"/> may throw an <see cref="InvalidDataException"/>.
  /// </para>
  /// </remarks>
  /// <exception cref="InvalidDataException">
  /// Thrown if <paramref name="input"/> does not contain valid GZip-compressed data.
  /// </exception>
  /// <exception cref="IOException">
  /// Thrown if an I/O error occurs while reading from <paramref name="input"/> or writing to <paramref name="output"/>.
  /// </exception>
  /// <exception cref="OperationCanceledException">
  /// Thrown if the operation is canceled via <paramref name="ct"/>.
  /// </exception>
  public async static Task DecompressGZipAsync(
    Stream input, Stream output, CancellationToken ct, int buffersize = 81920)
  {
    int readbytes;
    var buffer = new byte[buffersize];

    await using var gzip = new GZipStream(input, CompressionMode.Decompress);
    while ((readbytes = await gzip.ReadAsync(
      buffer.AsMemory(0, buffer.Length), ct).ConfigureAwait(false)) > 0)
      await output.WriteAsync(buffer.AsMemory(0, readbytes), ct).ConfigureAwait(false);
  }


  #endregion Async FileStream Compress

  #endregion GZip FileStream

  #endregion GZip

  #region Brotli

  #region Brotli Bytes

  #region Brotli Compress

  /// <summary>
  /// Compresses the given input using Brotli with the specified compression level.
  /// </summary>
  /// <param name="bytes">
  /// The input data to be compressed. Must not be empty and must not exceed <see cref="MAX_BYTES_LENGTH"/>.
  /// </param>
  /// <param name="compresslevel">
  /// The compression level to use. Defaults to <see cref="CompressionLevel.Optimal"/>.
  /// </param>
  /// <returns>
  /// A byte array containing the compressed data.
  /// </returns>
  /// <remarks>
  /// <para>
  /// This synchronous method is optimized for small payloads (≤ 8 MB). It uses <see cref="ReadOnlySpan{T}"/> to avoid
  /// unnecessary allocations and achieve maximum performance.
  /// </para>
  /// <para>
  /// Brotli compression may in rare cases produce output larger than the original input, especially for small or
  /// incompressible data. This is expected behavior and not an error.
  /// </para>
  /// <para>
  /// For larger data sets or scenarios where responsiveness is important, consider using the asynchronous
  /// stream-based overloads (<c>CompressBrotliAsync</c>) to avoid blocking the calling thread.
  /// </para>
  /// </remarks>
  /// <exception cref="ArgumentException">Thrown if <paramref name="bytes"/> is empty.</exception>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="bytes"/> exceeds <see cref="MAX_BYTES_LENGTH"/>.</exception>
  public static byte[] CompressBrotli(
   ReadOnlySpan<byte> bytes, CompressionLevel compresslevel =
     CompressionLevel.Optimal)
  {
    AssertCompress(bytes);

    using var ms = new MemoryStream();
    using (var Brotli = new BrotliStream(ms, compresslevel))
      Brotli.Write(bytes);
    return ms.ToArray();
  }

  /// <summary>
  /// Compresses the given input using Brotli with the specified compression level, supporting cancellation.
  /// </summary>
  /// <param name="bytes">
  /// The input data to be compressed. Must not be empty and must not exceed <see cref="MAX_BYTES_LENGTH"/>.
  /// </param>
  /// <param name="ct">
  /// A cancellation token that can be used to cancel the compression operation.
  /// </param>
  /// <param name="compresslevel">
  /// The compression level to use. Defaults to <see cref="CompressionLevel.Optimal"/>.
  /// </param>
  /// <returns>
  /// A byte array containing the compressed data.
  /// </returns>
  /// <remarks>
  /// <para>
  /// If <paramref name="ct"/> is triggered before or during the operation, an <see cref="OperationCanceledException"/> will be thrown.
  /// </para>
  /// <para>
  /// Brotli compression may in rare cases produce output larger than the original input, especially for small or
  /// incompressible data. This is expected behavior and not an error.
  /// </para>
  /// <para>
  /// For larger data sets or scenarios where responsiveness is important, consider using the asynchronous
  /// stream-based overloads (<c>CompressBrotliAsync</c>) to avoid blocking the calling thread.
  /// </para>
  /// </remarks>
  /// <exception cref="ArgumentException">Thrown if <paramref name="bytes"/> is empty.</exception>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="bytes"/> exceeds <see cref="MAX_BYTES_LENGTH"/>.</exception>
  /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via <paramref name="ct"/>.</exception>
  public static byte[] CompressBrotli(
  ReadOnlySpan<byte> bytes, CancellationToken ct,
    CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    AssertCompress(bytes);

    ct.ThrowIfCancellationRequested();

    using var ms = new MemoryStream();
    using (var Brotli = new BrotliStream(ms, compresslevel))
      Brotli.Write(bytes);
    return ms.ToArray();
  }

  /// <summary>
  /// Attempts to compress the given input using Brotli with the specified quality and window size.
  /// </summary>
  /// <param name="bytes">
  /// The input data to be compressed. Must not be empty and must not exceed <see cref="MAX_BYTES_LENGTH"/>.
  /// </param>
  /// <param name="compressed">
  /// The resulting compressed data. The buffer is resized to the actual number of bytes written.
  /// </param>
  /// <param name="writtenbytes">
  /// The number of bytes written to the compressed output.
  /// </param>
  /// <param name="quality">
  /// Brotli compression quality (0–11). Higher values improve compression ratio but increase CPU cost.
  /// </param>
  /// <param name="window">
  /// Brotli window size (10–24). Larger windows improve compression ratio for large inputs but require more memory.
  /// </param>
  /// <returns>
  /// <c>true</c> if compression succeeded; otherwise an exception is thrown.
  /// </returns>
  /// <remarks>
  /// <para>
  /// Brotli compression may in rare cases produce output larger than the original input, especially for small or
  /// incompressible data. For this reason, the method uses an exponential buffer growth strategy to ensure that
  /// sufficient space is allocated for the compressed output.
  /// </para>
  /// <para>
  /// A hard limit of <c>2^23</c> bytes (8 MB) is enforced to prevent uncontrolled memory growth. If the compressed
  /// output would exceed this limit, an <see cref="ArgumentOutOfRangeException"/> is thrown.
  /// </para>
  /// <para>
  /// This method is designed for robustness: it guarantees either a valid compressed result or a clear exception.
  /// The internal growth strategy is not relevant for most consumers, but ensures correctness in edge cases.
  /// </para>
  /// </remarks>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="bytes"/> is empty.</exception>
  /// <exception cref="ArgumentOutOfRangeException">
  /// Thrown if <paramref name="bytes"/> exceeds <see cref="MAX_BYTES_LENGTH"/> or if the output buffer grows beyond 8 MB.
  /// </exception>
  /// <exception cref="InvalidOperationException">Thrown if compression fails after multiple attempts.</exception>
  public static bool TryCompressBrotli(
    ReadOnlySpan<byte> bytes,
    out byte[] compressed, out int writtenbytes,
    int quality = 4, int window = 22)
  {
    AssertCompress(bytes, quality, window);

    var cnt = 1;

    while (true)
    {
      writtenbytes = -1;
      var maxlength = BrotliEncoder.GetMaxCompressedLength(cnt++ * bytes.Length);
      compressed = new byte[maxlength];

      if (BrotliEncoder.TryCompress(
        bytes, compressed, out writtenbytes,
        quality, window))
      {
        Array.Resize(ref compressed, writtenbytes);
        return true;
      }

      if (cnt > 3) break;
    }

    throw new InvalidOperationException("Compression failed.");
  }

  /// <summary>
  /// Attempts to compress the given input using Brotli with the specified quality and window size, supporting cancellation.
  /// </summary>
  /// <param name="bytes">
  /// The input data to be compressed. Must not be empty and must not exceed <see cref="MAX_BYTES_LENGTH"/>.
  /// </param>
  /// <param name="ct">
  /// A cancellation token that can be used to cancel the compression operation.
  /// </param>
  /// <param name="compressed">
  /// The resulting compressed data. The buffer is resized to the actual number of bytes written.
  /// </param>
  /// <param name="writtenbytes">
  /// The number of bytes written to the compressed output.
  /// </param>
  /// <param name="quality">
  /// Brotli compression quality (0–11). Higher values improve compression ratio but increase CPU cost.
  /// </param>
  /// <param name="window">
  /// Brotli window size (10–24). Larger windows improve compression ratio for large inputs but require more memory.
  /// </param>
  /// <returns>
  /// <c>true</c> if compression succeeded; otherwise an exception is thrown.
  /// </returns>
  /// <remarks>
  /// <para>
  /// Brotli compression may in rare cases produce output larger than the original input, especially for small or
  /// incompressible data. For this reason, the method uses an exponential buffer growth strategy to ensure that
  /// sufficient space is allocated for the compressed output.
  /// </para>
  /// <para>
  /// A hard limit of <c>2^23</c> bytes (8 MB) is enforced to prevent uncontrolled memory growth. If the compressed
  /// output would exceed this limit, an <see cref="ArgumentOutOfRangeException"/> is thrown.
  /// </para>
  /// <para>
  /// If <paramref name="ct"/> is triggered during the operation, an <see cref="OperationCanceledException"/> will be thrown.
  /// </para>
  /// </remarks>
  /// <exception cref="ArgumentException">Thrown if <paramref name="bytes"/> is empty.</exception>
  /// <exception cref="ArgumentOutOfRangeException">
  /// Thrown if <paramref name="bytes"/> exceeds <see cref="MAX_BYTES_LENGTH"/> or if the output buffer grows beyond 8 MB.
  /// </exception>
  /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via <paramref name="ct"/>.</exception>
  /// <exception cref="InvalidOperationException">Thrown if compression fails after multiple attempts.</exception>
  public static bool TryCompressBrotli(
    ReadOnlySpan<byte> bytes, CancellationToken ct,
    out byte[] compressed, out int writtenbytes,
    int quality = 4, int window = 22)
  {
    AssertCompress(bytes, quality, window);

    var cnt = 1;

    while (true)
    {
      writtenbytes = -1;
      ct.ThrowIfCancellationRequested();
      var maxlength = BrotliEncoder.GetMaxCompressedLength(cnt++ * bytes.Length);
      compressed = new byte[maxlength];

      if (BrotliEncoder.TryCompress(
        bytes, compressed, out writtenbytes,
        quality, window))
      {
        ct.ThrowIfCancellationRequested();
        Array.Resize(ref compressed, writtenbytes);
        return true;
      }

      if (cnt > 3) break;
    }

    throw new InvalidOperationException("Compression failed.");
  }

  /// <summary>
  /// Decompresses Brotli-compressed input into its original form.
  /// </summary>
  /// <param name="bytes">
  /// The compressed input data. Must not be empty.
  /// </param>
  /// <returns>
  /// A byte array containing the decompressed data.
  /// </returns>
  /// <remarks>
  /// <para>
  /// This synchronous method is optimized for small payloads (≤ 8 MB). It creates a <see cref="MemoryStream"/> from
  /// the compressed input and uses a <see cref="BrotliStream"/> in <see cref="CompressionMode.Decompress"/> to copy
  /// the decompressed data into an output stream.
  /// </para>
  /// <para>
  /// If the input data is invalid or corrupted, an <see cref="InvalidDataException"/> may be thrown by
  /// <see cref="BrotliStream"/>.
  /// </para>
  /// </remarks>
  /// <exception cref="ArgumentException">Thrown if <paramref name="bytes"/> is empty.</exception>
  /// <exception cref="InvalidDataException">Thrown if <paramref name="bytes"/> does not represent valid Brotli-compressed data.</exception>
  public static byte[] DecompressBrotli(ReadOnlySpan<byte> bytes)
  {
    using var msout = new MemoryStream();
    using var ms = new MemoryStream(bytes.ToArray());
    using (var Brotli = new BrotliStream(ms, CompressionMode.Decompress))
      Brotli.CopyTo(msout);

    return msout.ToArray();
  }

  /// <summary>
  /// Decompresses Brotli-compressed input into its original form, supporting cancellation.
  /// </summary>
  /// <param name="bytes">
  /// The compressed input data. Must not be empty.
  /// </param>
  /// <param name="ct">
  /// A cancellation token that can be used to cancel the decompression operation.
  /// </param>
  /// <returns>
  /// A byte array containing the decompressed data.
  /// </returns>
  /// <remarks>
  /// <para>
  /// If <paramref name="ct"/> is triggered before or during the operation, an <see cref="OperationCanceledException"/> will be thrown.
  /// </para>
  /// <para>
  /// If the input data is invalid or corrupted, an <see cref="InvalidDataException"/> may be thrown by
  /// <see cref="BrotliStream"/>.
  /// </para>
  /// </remarks>
  /// <exception cref="ArgumentException">Thrown if <paramref name="bytes"/> is empty.</exception>
  /// <exception cref="InvalidDataException">Thrown if <paramref name="bytes"/> does not represent valid Brotli-compressed data.</exception>
  /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via <paramref name="ct"/>.</exception>
  public static byte[] DecompressBrotli(
    ReadOnlySpan<byte> bytes, CancellationToken ct)
  {
    ct.ThrowIfCancellationRequested();

    using var msout = new MemoryStream();
    using var ms = new MemoryStream(bytes.ToArray());
    using (var Brotli = new BrotliStream(ms, CompressionMode.Decompress))
      Brotli.CopyTo(msout);

    return msout.ToArray();
  }

  /// <summary>
  /// Attempts to decompress Brotli-compressed input into its original form.
  /// </summary>
  /// <param name="bytes">
  /// The compressed input data. Must not be empty.
  /// </param>
  /// <param name="decompressed">
  /// The resulting decompressed data. The buffer is resized to the actual number of bytes written.
  /// </param>
  /// <param name="writtenbytes">
  /// The number of bytes written to the decompressed output.
  /// </param>
  /// <returns>
  /// <c>true</c> if decompression succeeded; otherwise an exception is thrown.
  /// </returns>
  /// <remarks>
  /// <para>
  /// Brotli decompression requires a buffer large enough to hold the original data. Since the decompressed size
  /// cannot be known in advance, this method uses an exponential buffer growth strategy until decompression succeeds.
  /// </para>
  /// <para>
  /// A hard limit of <c>2^23</c> bytes (8 MB) is enforced to prevent uncontrolled memory growth. If the decompressed
  /// output would exceed this limit, an <see cref="ArgumentOutOfRangeException"/> is thrown.
  /// </para>
  /// <para>
  /// This method guarantees either a valid decompressed result or a clear exception. The internal growth strategy
  /// ensures robustness in edge cases where Brotli output is unexpectedly large.
  /// </para>
  /// </remarks>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="bytes"/> is empty.</exception>
  /// <exception cref="ArgumentOutOfRangeException">
  /// Thrown if the output buffer grows beyond 8 MB.
  /// </exception>
  /// <exception cref="InvalidOperationException">Thrown if decompression fails after multiple attempts.</exception>
  public static bool TryDecompressBrotli(
    ReadOnlySpan<byte> bytes, out byte[] decompressed,
    out int writtenbytes)
  {
    var cnt = 2;
    decompressed = [];
    writtenbytes = -1;
    var result = false;
    while (!result)
    {
      writtenbytes = -1;
      decompressed = new byte[(1 << cnt++) * bytes.Length];
      if (decompressed.Length > MAX_BYTES_LENGTH)
        throw new ArgumentOutOfRangeException(nameof(decompressed),
          $"{nameof(decompressed)}.Length has failed!");

      result = BrotliDecoder.TryDecompress(
        bytes, decompressed, out writtenbytes);

      if (!result) continue;
      Array.Resize(ref decompressed, writtenbytes);
    }

    return result;
  }

  /// <summary>
  /// Attempts to decompress Brotli-compressed input into its original form, supporting cancellation.
  /// </summary>
  /// <param name="bytes">
  /// The compressed input data. Must not be empty.
  /// </param>
  /// <param name="ct">
  /// A cancellation token that can be used to cancel the decompression operation.
  /// </param>
  /// <param name="decompressed">
  /// The resulting decompressed data. The buffer is resized to the actual number of bytes written.
  /// </param>
  /// <param name="writtenbytes">
  /// The number of bytes written to the decompressed output.
  /// </param>
  /// <returns>
  /// <c>true</c> if decompression succeeded; otherwise an exception is thrown.
  /// </returns>
  /// <remarks>
  /// <para>
  /// Brotli decompression requires a buffer large enough to hold the original data. Since the decompressed size
  /// cannot be known in advance, this method uses an exponential buffer growth strategy until decompression succeeds.
  /// </para>
  /// <para>
  /// A hard limit of <c>2^23</c> bytes (8 MB) is enforced to prevent uncontrolled memory growth. If the decompressed
  /// output would exceed this limit, an <see cref="ArgumentOutOfRangeException"/> is thrown.
  /// </para>
  /// <para>
  /// If <paramref name="ct"/> is triggered during the operation, an <see cref="OperationCanceledException"/> will be thrown.
  /// </para>
  /// </remarks>
  /// <exception cref="ArgumentException">Thrown if <paramref name="bytes"/> is empty.</exception>
  /// <exception cref="ArgumentOutOfRangeException">
  /// Thrown if the output buffer grows beyond 8 MB.
  /// </exception>
  /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via <paramref name="ct"/>.</exception>
  /// <exception cref="InvalidOperationException">Thrown if decompression fails after multiple attempts.</exception>
  public static bool TryDecompressBrotli(
    ReadOnlySpan<byte> bytes, CancellationToken ct,
    out byte[] decompressed, out int writtenbytes)
  {
    var cnt = 2;
    decompressed = [];
    writtenbytes = -1;
    var result = false;
    while (!result)
    {
      writtenbytes = -1;
      ct.ThrowIfCancellationRequested();
      decompressed = new byte[(1 << cnt++) * bytes.Length];

      if (decompressed.Length > MAX_BYTES_LENGTH)
        throw new ArgumentOutOfRangeException(nameof(decompressed),
            $"{nameof(decompressed)}.Length has failed!");

      result = BrotliDecoder.TryDecompress(
        bytes, decompressed, out writtenbytes);

      if (!result) continue;
      Array.Resize(ref decompressed, writtenbytes);
    }

    return result;
  }

  #endregion Brotli Compress

  #region Async Brotli Compress

  /// <summary>
  /// Asynchronously compresses the given input using Brotli with the specified compression level.
  /// </summary>
  /// <param name="bytes">
  /// The input data to be compressed. Must not be empty and must not exceed <see cref="MAX_BYTES_LENGTH"/>.
  /// </param>
  /// <param name="compresslevel">
  /// The compression level to use. Defaults to <see cref="CompressionLevel.Optimal"/>.
  /// </param>
  /// <returns>
  /// A task that represents the asynchronous compression operation. The task result contains the compressed data.
  /// </returns>
  /// <remarks>
  /// <para>
  /// This method is await-safe and optimized for small payloads (≤ 8 MB). It writes the entire input into a
  /// <see cref="BrotliStream"/> and returns the compressed result.
  /// </para>
  /// <para>
  /// Brotli compression may in rare cases produce output larger than the original input, especially for small or
  /// incompressible data. This is expected behavior and not an error.
  /// </para>
  /// <para>
  /// For larger data sets or scenarios where responsiveness is important, consider using the stream-based overloads
  /// (<c>CompressBrotliAsync(Stream, Stream, …)</c>) to avoid blocking and excessive memory usage.
  /// </para>
  /// </remarks>
  /// <exception cref="ArgumentException">Thrown if <paramref name="bytes"/> is empty.</exception>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="bytes"/> exceeds <see cref="MAX_BYTES_LENGTH"/>.</exception>
  public async static Task<byte[]> CompressBrotliAsync(
    byte[] bytes, CompressionLevel compresslevel =
      CompressionLevel.Optimal)
  {
    AssertCompress(bytes);

    await using var ms = new MemoryStream();
    await using (var Brotli = new BrotliStream(ms, compresslevel))
      await Brotli.WriteAsync(bytes).ConfigureAwait(false);

    return ms.ToArray();
  }

  /// <summary>
  /// Asynchronously compresses the given input using Brotli with the specified compression level.
  /// </summary>
  /// <param name="bytes">
  /// The input data to be compressed. Must not be empty and must not exceed <see cref="MAX_BYTES_LENGTH"/>.
  /// </param>
  /// <param name="ct">
  /// A cancellation token that can be used to cancel the compression operation.
  /// </param>
  /// <param name="compresslevel">
  /// The compression level to use. Defaults to <see cref="CompressionLevel.Optimal"/>.
  /// </param>
  /// <returns>
  /// A task that represents the asynchronous compression operation. The task result contains the compressed data.
  /// </returns>
  /// <remarks>
  /// <para>
  /// Brotli compression may in rare cases produce output larger than the original input. This method ensures that
  /// sufficient buffer space is allocated and supports cancellation.
  /// </para>
  /// <para>
  /// For small payloads (≤ 8 MB), the entire input is written in one operation. For larger data, use the stream-based
  /// overloads to avoid excessive memory usage.
  /// </para>
  /// </remarks>
  /// <exception cref="ArgumentException">Thrown if <paramref name="bytes"/> is empty.</exception>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="bytes"/> exceeds <see cref="MAX_BYTES_LENGTH"/>.</exception>
  /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via <paramref name="ct"/>.</exception>
  public async static Task<byte[]> CompressBrotliAsync(
    byte[] bytes, CancellationToken ct,
    CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    AssertCompress(bytes);

    await using var ms = new MemoryStream();
    await using (var Brotli = new BrotliStream(ms, compresslevel))
      await Brotli.WriteAsync(bytes, ct).ConfigureAwait(false);

    return ms.ToArray();
  }

  /// <summary>
  /// Asynchronously decompresses Brotli-compressed input into its original form.
  /// </summary>
  /// <param name="bytes">
  /// The compressed input data. Must not be empty.
  /// </param>
  /// <returns>
  /// A task that represents the asynchronous decompression operation. The task result contains the decompressed data.
  /// </returns>
  /// <remarks>
  /// <para>
  /// This method is await-safe and optimized for small payloads (≤ 8 MB). It creates a <see cref="MemoryStream"/> from
  /// the compressed input and uses a <see cref="BrotliStream"/> in <see cref="CompressionMode.Decompress"/> to copy
  /// the decompressed data into an output stream.
  /// </para>
  /// <para>
  /// If the input data is invalid or corrupted, an <see cref="InvalidDataException"/> may be thrown by
  /// <see cref="BrotliStream"/>.
  /// </para>
  /// <para>
  /// For larger data sets or scenarios where responsiveness is important, consider using the stream-based overloads
  /// (<c>DecompressBrotliAsync(Stream, Stream, …)</c>) to avoid blocking and excessive memory usage.
  /// </para>
  /// </remarks>
  /// <exception cref="ArgumentException">Thrown if <paramref name="bytes"/> is empty.</exception>
  /// <exception cref="InvalidDataException">Thrown if <paramref name="bytes"/> does not represent valid Brotli-compressed data.</exception>
  /// <exception cref="IOException">Thrown if an I/O error occurs while reading or writing streams.</exception>
  public async static Task<byte[]> DecompressBrotliAsync(byte[] bytes)
  {
    await using var msout = new MemoryStream();
    await using var ms = new MemoryStream(bytes);
    await using (var Brotli = new BrotliStream(ms, CompressionMode.Decompress))
      await Brotli.CopyToAsync(msout).ConfigureAwait(false);

    return msout.ToArray();
  }

  /// <summary>
  /// Asynchronously decompresses Brotli-compressed input into its original form.
  /// </summary>
  /// <param name="bytes">
  /// The compressed input data. Must not be empty.
  /// </param>
  /// <param name="ct">
  /// A cancellation token that can be used to cancel the decompression operation.
  /// </param>
  /// <returns>
  /// A task that represents the asynchronous decompression operation. The task result contains the decompressed data.
  /// </returns>
  /// <remarks>
  /// <para>
  /// Brotli decompression requires a buffer large enough to hold the original data. Since the decompressed size
  /// cannot be known in advance, this method ensures robustness by supporting cancellation and safe buffer allocation.
  /// </para>
  /// </remarks>
  /// <exception cref="ArgumentException">Thrown if <paramref name="bytes"/> is empty.</exception>
  /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via <paramref name="ct"/>.</exception>
  public async static Task<byte[]> DecompressBrotliAsync(
   byte[] bytes, CancellationToken ct)
  {
    await using var msout = new MemoryStream();
    await using var ms = new MemoryStream(bytes);
    await using (var Brotli = new BrotliStream(ms, CompressionMode.Decompress))
      await Brotli.CopyToAsync(msout, ct).ConfigureAwait(false);

    return msout.ToArray();
  }

  #endregion Async Brotli Compress

  #endregion Brotli Bytes

  #region Brotli FileStream

  #region FileStream Compress

  /// <summary>
  /// Compresses the contents of a source file into a destination file using Brotli.
  /// </summary>
  /// <param name="src">The path to the source file to be compressed.</param>
  /// <param name="dest">The path to the destination file where compressed data will be written.</param>
  /// <param name="buffersize">
  /// The size of the buffer used for reading and writing. Defaults to 81920 bytes (the .NET default).
  /// </param>
  /// <param name="compresslevel">
  /// The compression level to use. Defaults to <see cref="CompressionLevel.Optimal"/>.
  /// </param>
  /// <remarks>
  /// This synchronous method is suitable for batch jobs or scenarios where blocking is acceptable.
  /// For large files or responsive applications, consider using the asynchronous overloads.
  /// </remarks>
  /// <exception cref="FileNotFoundException">Thrown if the source file does not exist.</exception>
  /// <exception cref="IOException">Thrown if an I/O error occurs during compression.</exception>
  public static void CompressBrotli(
    string src, string dest, int buffersize = 81920,
    CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
    using var fsout = new FileStream(dest, FileMode.Create, FileAccess.Write);
    CompressBrotli(fsin, fsout, buffersize, compresslevel);
  }

  /// <summary>
  /// Compresses the contents of a source file into a destination file using Brotli, supporting cancellation.
  /// </summary>
  /// <param name="src">The path to the source file to be compressed.</param>
  /// <param name="dest">The path to the destination file where compressed data will be written.</param>
  /// <param name="ct">A cancellation token that can be used to cancel the compression operation.</param>
  /// <param name="buffersize">The size of the buffer used for reading and writing. Defaults to 81920 bytes.</param>
  /// <param name="compresslevel">The compression level to use. Defaults to <see cref="CompressionLevel.Optimal"/>.</param>
  /// <remarks>
  /// If <paramref name="ct"/> is triggered during the operation, an <see cref="OperationCanceledException"/> will be thrown.
  /// </remarks>
  /// <exception cref="FileNotFoundException">Thrown if the source file does not exist.</exception>
  /// <exception cref="IOException">Thrown if an I/O error occurs during compression.</exception>
  /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via <paramref name="ct"/>.</exception>
  public static void CompressBrotli(
    string src, string dest, CancellationToken ct, int buffersize = 81920,
    CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
    using var fsout = new FileStream(dest, FileMode.Create, FileAccess.Write);
    CompressBrotli(fsin, fsout, ct, buffersize, compresslevel);
  }

  /// <summary>
  /// Compresses data from an input stream into an output stream using Brotli.
  /// </summary>
  /// <param name="input">The input stream containing data to be compressed.</param>
  /// <param name="output">The output stream where compressed data will be written.</param>
  /// <param name="buffersize">The size of the buffer used for reading and writing. Defaults to 81920 bytes.</param>
  /// <param name="compresslevel">The compression level to use. Defaults to <see cref="CompressionLevel.Optimal"/>.</param>
  /// <remarks>
  /// The caller is responsible for managing the lifetime of <paramref name="input"/> and <paramref name="output"/>.
  /// </remarks>
  /// <exception cref="IOException">Thrown if an I/O error occurs during compression.</exception>
  public static void CompressBrotli(
    Stream input, Stream output, int buffersize = 81920,
    CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    int readbytes;
    var buffer = new byte[buffersize];

    using var Brotli = new BrotliStream(output, compresslevel, false);
    while ((readbytes = input.Read(buffer)) > 0)
      Brotli.Write(buffer, 0, readbytes);
  }

  /// <summary>
  /// Compresses data from an input stream into an output stream using Brotli, supporting cancellation.
  /// </summary>
  /// <param name="input">The input stream containing data to be compressed.</param>
  /// <param name="output">The output stream where compressed data will be written.</param>
  /// <param name="ct">A cancellation token that can be used to cancel the compression operation.</param>
  /// <param name="buffersize">The size of the buffer used for reading and writing. Defaults to 81920 bytes.</param>
  /// <param name="compresslevel">The compression level to use. Defaults to <see cref="CompressionLevel.Optimal"/>.</param>
  /// <remarks>
  /// If <paramref name="ct"/> is triggered during the operation, an <see cref="OperationCanceledException"/> will be thrown.
  /// </remarks>
  /// <exception cref="IOException">Thrown if an I/O error occurs during compression.</exception>
  /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via <paramref name="ct"/>.</exception>
  public static void CompressBrotli(
    Stream input, Stream output, CancellationToken ct, int buffersize = 81920,
    CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    int readbytes;
    var buffer = new byte[buffersize];

    using var Brotli = new BrotliStream(output, compresslevel, false);
    while ((readbytes = input.Read(buffer)) > 0)
    {
      ct.ThrowIfCancellationRequested();
      Brotli.Write(buffer, 0, readbytes);
    }
  }

  // ********** ********** ********** ********** ********** ********** ********** **********
  // ********** ********** ********** ********** ********** ********** ********** **********

  /// <summary>
  /// Decompresses a Brotli-compressed source file into a destination file.
  /// </summary>
  /// <param name="src">The path to the source file containing Brotli-compressed data.</param>
  /// <param name="dest">The path to the destination file where decompressed data will be written.</param>
  /// <param name="buffersize">The size of the buffer used for reading and writing. Defaults to 81920 bytes.</param>
  /// <exception cref="FileNotFoundException">Thrown if the source file does not exist.</exception>
  /// <exception cref="InvalidDataException">Thrown if the source file does not contain valid Brotli data.</exception>
  /// <exception cref="IOException">Thrown if an I/O error occurs during decompression.</exception>
  public static void DecompressBrotli(
    string src, string dest, int buffersize = 81920)
  {
    using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
    using var fsout = new FileStream(dest, FileMode.Create, FileAccess.Write);
    DecompressBrotli(fsin, fsout, buffersize);
  }

  /// <summary>
  /// Decompresses a Brotli-compressed source file into a destination file, supporting cancellation.
  /// </summary>
  /// <param name="src">The path to the source file containing Brotli-compressed data.</param>
  /// <param name="dest">The path to the destination file where decompressed data will be written.</param>
  /// <param name="ct">A cancellation token that can be used to cancel the decompression operation.</param>
  /// <param name="buffersize">The size of the buffer used for reading and writing. Defaults to 81920 bytes.</param>
  /// <exception cref="FileNotFoundException">Thrown if the source file does not exist.</exception>
  /// <exception cref="InvalidDataException">Thrown if the source file does not contain valid Brotli data.</exception>
  /// <exception cref="IOException">Thrown if an I/O error occurs during decompression.</exception>
  /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via <paramref name="ct"/>.</exception>
  public static void DecompressBrotli(
    string src, string dest, CancellationToken ct, int buffersize = 81920)
  {
    using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
    using var fsout = new FileStream(dest, FileMode.Create, FileAccess.Write);
    DecompressBrotli(fsin, fsout, ct, buffersize);
  }

  /// <summary>
  /// Decompresses Brotli-compressed data from an input stream into an output stream.
  /// </summary>
  /// <param name="input">The input stream containing Brotli-compressed data.</param>
  /// <param name="output">The output stream where decompressed data will be written.</param>
  /// <param name="buffersize">The size of the buffer used for reading and writing. Defaults to 81920 bytes.</param>
  /// <remarks>
  /// The caller is responsible for managing the lifetime of <paramref name="input"/> and <paramref name="output"/>.
  /// </remarks>
  /// <exception cref="InvalidDataException">Thrown if <paramref name="input"/> does not contain valid Brotli data.</exception>
  /// <exception cref="IOException">Thrown if an I/O error occurs during decompression.</exception>
  public static void DecompressBrotli(
    Stream input, Stream output, int buffersize = 81920)
  {
    int readbytes;
    var buffer = new byte[buffersize];

    using var Brotli = new BrotliStream(input, CompressionMode.Decompress);
    while ((readbytes = Brotli.Read(buffer)) > 0)
      output.Write(buffer, 0, readbytes);
  }

  /// <summary>
  /// Decompresses Brotli-compressed data from an input stream into an output stream, supporting cancellation.
  /// </summary>
  /// <param name="input">
  /// The input stream containing Brotli-compressed data. The stream must be readable and positioned at the start of the compressed content.
  /// </param>
  /// <param name="output">
  /// The output stream where decompressed data will be written. The stream must be writable.
  /// </param>
  /// <param name="ct">
  /// A cancellation token that can be used to cancel the decompression operation.
  /// </param>
  /// <param name="buffersize">
  /// The size of the buffer used for reading and writing. Defaults to 81920 bytes (the .NET recommended default).
  /// </param>
  /// <remarks>
  /// <para>
  /// The caller is responsible for the lifetime of <paramref name="input"/> and <paramref name="output"/>. This method
  /// does not close or dispose the provided streams.
  /// </para>
  /// <para>
  /// If <paramref name="ct"/> is triggered during the operation, an <see cref="OperationCanceledException"/> will be thrown.
  /// </para>
  /// <para>
  /// If the input data is invalid or corrupted, <see cref="BrotliStream"/> may throw an <see cref="InvalidDataException"/>.
  /// </para>
  /// </remarks>
  /// <exception cref="InvalidDataException">
  /// Thrown if <paramref name="input"/> does not contain valid Brotli-compressed data.
  /// </exception>
  /// <exception cref="IOException">
  /// Thrown if an I/O error occurs while reading from <paramref name="input"/> or writing to <paramref name="output"/>.
  /// </exception>
  /// <exception cref="OperationCanceledException">
  /// Thrown if the operation is canceled via <paramref name="ct"/>.
  /// </exception>
  public static void DecompressBrotli(
    Stream input, Stream output, CancellationToken ct, int buffersize = 81920)
  {
    int readbytes;
    var buffer = new byte[buffersize];

    using var Brotli = new BrotliStream(input, CompressionMode.Decompress);
    while ((readbytes = Brotli.Read(buffer)) > 0)
    {
      ct.ThrowIfCancellationRequested();
      output.Write(buffer, 0, readbytes);
    }
  }

  #endregion FileStream Compress

  #region Async FileStream Compress

  /// <summary>
  /// Asynchronously compresses the contents of a source file into a destination file using Brotli.
  /// </summary>
  /// <param name="src">The path to the source file to be compressed.</param>
  /// <param name="dest">The path to the destination file where compressed data will be written.</param>
  /// <param name="buffersize">The size of the buffer used for reading and writing. Defaults to 81920 bytes.</param>
  /// <param name="compresslevel">The compression level to use. Defaults to <see cref="CompressionLevel.Optimal"/>.</param>
  /// <returns>A task representing the asynchronous compression operation.</returns>
  /// <remarks>
  /// This overload does not support cancellation. Use the variant with <see cref="CancellationToken"/> if cancellation is required.
  /// </remarks>
  /// <exception cref="FileNotFoundException">Thrown if the source file does not exist.</exception>
  /// <exception cref="IOException">Thrown if an I/O error occurs during compression.</exception>
  public async static Task CompressBrotliAsync(
      string src, string dest, int buffersize = 81920,
      CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    await using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
    await using var fsout = new FileStream(dest, FileMode.Create, FileAccess.Write);
    await CompressBrotliAsync(fsin, fsout, buffersize, compresslevel).ConfigureAwait(false);
  }

  /// <summary>
  /// Asynchronously compresses the contents of a source file into a destination file using Brotli, supporting cancellation.
  /// </summary>
  /// <param name="src">The path to the source file to be compressed.</param>
  /// <param name="dest">The path to the destination file where compressed data will be written.</param>
  /// <param name="ct">A cancellation token that can be used to cancel the compression operation.</param>
  /// <param name="buffersize">The size of the buffer used for reading and writing. Defaults to 81920 bytes.</param>
  /// <param name="compresslevel">The compression level to use. Defaults to <see cref="CompressionLevel.Optimal"/>.</param>
  /// <returns>A task representing the asynchronous compression operation.</returns>
  /// <remarks>
  /// If <paramref name="ct"/> is triggered during the operation, an <see cref="OperationCanceledException"/> will be thrown.
  /// </remarks>
  /// <exception cref="FileNotFoundException">Thrown if the source file does not exist.</exception>
  /// <exception cref="IOException">Thrown if an I/O error occurs during compression.</exception>
  /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via <paramref name="ct"/>.</exception>
  public async static Task CompressBrotliAsync(
    string src, string dest, CancellationToken ct, int buffersize = 81920,
    CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    await using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
    await using var fsout = new FileStream(dest, FileMode.Create, FileAccess.Write);
    await CompressBrotliAsync(fsin, fsout, ct, buffersize, compresslevel).ConfigureAwait(false);
  }

  /// <summary>
  /// Asynchronously compresses data from an input stream into an output stream using Brotli.
  /// </summary>
  /// <param name="input">The input stream containing data to be compressed.</param>
  /// <param name="output">The output stream where compressed data will be written.</param>
  /// <param name="buffersize">The size of the buffer used for reading and writing. Defaults to 81920 bytes.</param>
  /// <param name="compresslevel">The compression level to use. Defaults to <see cref="CompressionLevel.Optimal"/>.</param>
  /// <returns>A task representing the asynchronous compression operation.</returns>
  /// <remarks>
  /// The caller is responsible for managing the lifetime of <paramref name="input"/> and <paramref name="output"/>.
  /// </remarks>
  /// <exception cref="IOException">Thrown if an I/O error occurs during compression.</exception>
  public async static Task CompressBrotliAsync(
    Stream input, Stream output, int buffersize = 81920,
    CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    int readbytes;
    var buffer = new byte[buffersize];

    await using var Brotli = new BrotliStream(output, compresslevel);
    while ((readbytes = await input.ReadAsync(
      buffer.AsMemory(0, buffer.Length)).ConfigureAwait(false)) > 0)
      await Brotli.WriteAsync(buffer.AsMemory(0, readbytes)).ConfigureAwait(false);
  }

  /// <summary>
  /// Asynchronously compresses data from an input stream into an output stream using Brotli, supporting cancellation.
  /// </summary>
  /// <param name="input">The input stream containing data to be compressed.</param>
  /// <param name="output">The output stream where compressed data will be written.</param>
  /// <param name="ct">A cancellation token that can be used to cancel the compression operation.</param>
  /// <param name="buffersize">The size of the buffer used for reading and writing. Defaults to 81920 bytes.</param>
  /// <param name="compresslevel">The compression level to use. Defaults to <see cref="CompressionLevel.Optimal"/>.</param>
  /// <returns>A task representing the asynchronous compression operation.</returns>
  /// <remarks>
  /// If <paramref name="ct"/> is triggered during the operation, an <see cref="OperationCanceledException"/> will be thrown.
  /// </remarks>
  /// <exception cref="IOException">Thrown if an I/O error occurs during compression.</exception>
  /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via <paramref name="ct"/>.</exception>
  public async static Task CompressBrotliAsync(
    Stream input, Stream output, CancellationToken ct, int buffersize = 81920,
    CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    int readbytes;
    var buffer = new byte[buffersize];

    await using var Brotli = new BrotliStream(output, compresslevel);
    while ((readbytes = await input.ReadAsync(
      buffer.AsMemory(0, buffer.Length), ct).ConfigureAwait(false)) > 0)
      await Brotli.WriteAsync(buffer.AsMemory(0, readbytes), ct).ConfigureAwait(false);
  }


  // ********** ********** ********** ********** ********** ********** ********** **********
  // ********** ********** ********** ********** ********** ********** ********** **********

  /// <summary>
  /// Asynchronously decompresses a Brotli-compressed source file into a destination file.
  /// </summary>
  /// <param name="src">The path to the source file containing Brotli-compressed data.</param>
  /// <param name="dest">The path to the destination file where decompressed data will be written.</param>
  /// <param name="buffersize">The size of the buffer used for reading and writing. Defaults to 81920 bytes.</param>
  /// <returns>A task representing the asynchronous decompression operation.</returns>
  /// <exception cref="FileNotFoundException">Thrown if the source file does not exist.</exception>
  /// <exception cref="InvalidDataException">Thrown if the source file does not contain valid Brotli data.</exception>
  /// <exception cref="IOException">Thrown if an I/O error occurs during decompression.</exception>
  public async static Task DecompressBrotliAsync(
    string src, string dest, int buffersize = 81920)
  {
    await using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
    await using var fsout = new FileStream(dest, FileMode.Create, FileAccess.Write);
    await DecompressBrotliAsync(fsin, fsout, buffersize).ConfigureAwait(false);
  }

  /// <summary>
  /// Asynchronously decompresses a Brotli-compressed source file into a destination file, supporting cancellation.
  /// </summary>
  /// <param name="src">The path to the source file containing Brotli-compressed data.</param>
  /// <param name="dest">The path to the destination file where decompressed data will be written.</param>
  /// <param name="ct">A cancellation token that can be used to cancel the decompression operation.</param>
  /// <param name="buffersize">The size of the buffer used for reading and writing. Defaults to 81920 bytes.</param>
  /// <returns>A task representing the asynchronous decompression operation.</returns>
  /// <remarks>
  /// <para>
  /// If <paramref name="ct"/> is triggered during the operation, an <see cref="OperationCanceledException"/> will be thrown.
  /// </para>
  /// <para>
  /// If the input data is invalid or corrupted, an <see cref="InvalidDataException"/> may be thrown by <see cref="BrotliStream"/>.
  /// </para>
  /// </remarks>
  /// <exception cref="FileNotFoundException">Thrown if the source file does not exist.</exception>
  /// <exception cref="InvalidDataException">Thrown if the source file does not contain valid Brotli data.</exception>
  /// <exception cref="IOException">Thrown if an I/O error occurs during decompression.</exception>
  /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via <paramref name="ct"/>.</exception>
  public async static Task DecompressBrotliAsync(
    string src, string dest, CancellationToken ct, int buffersize = 81920)
  {
    await using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
    await using var fsout = new FileStream(dest, FileMode.Create, FileAccess.Write);
    await DecompressBrotliAsync(fsin, fsout, ct, buffersize).ConfigureAwait(false);
  }

  /// <summary>
  /// Asynchronously decompresses Brotli-compressed data from an input stream into an output stream.
  /// </summary>
  /// <param name="input">The input stream containing Brotli-compressed data.</param>
  /// <param name="output">The output stream where decompressed data will be written.</param>
  /// <param name="buffersize">The size of the buffer used for reading and writing. Defaults to 81920 bytes.</param>
  /// <returns>A task representing the asynchronous decompression operation.</returns>
  /// <remarks>
  /// The caller is responsible for managing the lifetime of <paramref name="input"/> and <paramref name="output"/>.
  /// </remarks>
  /// <exception cref="InvalidDataException">Thrown if <paramref name="input"/> does not contain valid Brotli data.</exception>
  /// <exception cref="IOException">Thrown if an I/O error occurs during decompression.</exception>
  public async static Task DecompressBrotliAsync(
    Stream input, Stream output, int buffersize = 81920)
  {
    int readbytes;
    var buffer = new byte[buffersize];

    await using var Brotli = new BrotliStream(input, CompressionMode.Decompress);
    while ((readbytes = await Brotli.ReadAsync(
      buffer.AsMemory(0, buffer.Length)).ConfigureAwait(false)) > 0)
      await output.WriteAsync(buffer.AsMemory(0, readbytes)).ConfigureAwait(false);
  }

  /// <summary>
  /// Asynchronously decompresses Brotli-compressed data from an input stream into an output stream, supporting cancellation.
  /// </summary>
  /// <param name="input">The input stream containing Brotli-compressed data.</param>
  /// <param name="output">The output stream where decompressed data will be written.</param>
  /// <param name="ct">A cancellation token that can be used to cancel the decompression operation.</param>
  /// <param name="buffersize">The size of the buffer used for reading and writing. Defaults to 81920 bytes.</param>
  /// <returns>A task representing the asynchronous decompression operation.</returns>
  /// <remarks>
  /// <para>
  /// If <paramref name="ct"/> is triggered during the operation, an <see cref="OperationCanceledException"/> will be thrown.
  /// </para>
  /// <para>
  /// If the input data is invalid or corrupted, an <see cref="InvalidDataException"/> may be thrown by <see cref="BrotliStream"/>.
  /// </para>
  /// </remarks>
  /// <exception cref="InvalidDataException">Thrown if <paramref name="input"/> does not contain valid Brotli data.</exception>
  /// <exception cref="IOException">Thrown if an I/O error occurs during decompression.</exception>
  /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via <paramref name="ct"/>.</exception>
  public async static Task DecompressBrotliAsync(
    Stream input, Stream output, CancellationToken ct, int buffersize = 81920)
  {
    int readbytes;
    var buffer = new byte[buffersize];

    await using var Brotli = new BrotliStream(input, CompressionMode.Decompress);
    while ((readbytes = await Brotli.ReadAsync(
      buffer.AsMemory(0, buffer.Length), ct).ConfigureAwait(false)) > 0)
      await output.WriteAsync(buffer.AsMemory(0, readbytes), ct).ConfigureAwait(false);
  }


  #endregion Async FileStream Compress

  #endregion Brotli FileStream

  #endregion Brotli

  #region Assert Compress  
  private static void AssertCompress(ReadOnlySpan<byte> bytes)
  {
    if (bytes.IsEmpty || bytes.Length == 0)
      throw new ArgumentNullException(nameof(bytes),
        $"{nameof(bytes)} has failed! NULL or Zero-Length");

    if (bytes.Length > MAX_BYTES_LENGTH)
      throw new ArgumentOutOfRangeException(nameof(bytes),
        $"{nameof(bytes)}.Length has failed! " +
        $"Length exceeds maximum allowed size (8 MB).");
  }

  private static void AssertCompress(
    ReadOnlySpan<byte> bytes, int quality, int window)
  {
    AssertCompress(bytes);

    if (quality < BROTLI_MIN_QUALITY || quality > BROTLI_MAX_QUALITY)
      throw new ArgumentOutOfRangeException(nameof(quality),
        $"{nameof(quality)} has failed: must be between {BROTLI_MIN_QUALITY} and {BROTLI_MAX_QUALITY}.");

    if (window < BROTLI_MIN_WINDOW || window > BROTLI_MAX_WINDOW)
      throw new ArgumentOutOfRangeException(nameof(window),
        $"{nameof(window)} has failed: Size must be between {BROTLI_MIN_WINDOW} and {BROTLI_MAX_WINDOW}.");
  }

  #endregion Assert Compress
}
