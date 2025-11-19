
using System.IO.Compression;

namespace michele.natale.Compressors;


//Synchron: Span<T> / ReadOnlySpan<T> → maximale Performance, keine Allokationen.
//Asynchron: byte[] / Memory<T> → await-sicher, stabil über Task-Grenzen hinweg.


public class CompressedNet
{

  public const int BROTLI_MIN_WINDOW = 10;
  public const int BROTLI_MAX_WINDOW = 24;

  public const int BROTLI_MIN_QUALITY = 0;
  public const int BROTLI_MAX_QUALITY = 11;

  public const int MAX_BYTES_LENGTH = 1 << 23;

  #region GZip

  #region Bytes

  #region GZip Compress

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

  public static byte[] DecompressGZip(ReadOnlySpan<byte> bytes)
  {
    using var msout = new MemoryStream();
    using var ms = new MemoryStream(bytes.ToArray());
    using (var gzip = new GZipStream(ms, CompressionMode.Decompress))
      gzip.CopyTo(msout);

    return msout.ToArray();
  }

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

  public async static Task<byte[]> DecompressGZipAsync(byte[] bytes)
  {
    await using var msout = new MemoryStream();
    await using var ms = new MemoryStream(bytes);
    await using (var gzip = new GZipStream(ms, CompressionMode.Decompress))
      await gzip.CopyToAsync(msout).ConfigureAwait(false);

    return msout.ToArray();
  }

  public async static Task<byte[]> DecompressGZipAsync(
   byte[] bytes, CancellationToken ct)
  {
    using var msout = new MemoryStream();
    using var ms = new MemoryStream(bytes);
    using (var gzip = new GZipStream(ms, CompressionMode.Decompress))
      await gzip.CopyToAsync(msout, ct).ConfigureAwait(false);

    return msout.ToArray();
  }

  #endregion Async GZip Compress

  #endregion Bytes

  #region FileStream

  #region FileStream Compress

  public static void CompressGZip(
    string src, string dest, int buffersize = 81920,
    CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
    using var fsout = new FileStream(dest, FileMode.Create, FileAccess.Write);
    CompressGZip(fsin, fsout, buffersize, compresslevel);
  }

  public static void CompressGZip(
    string src, string dest, CancellationToken ct, int buffersize = 81920,
    CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
    using var fsout = new FileStream(dest, FileMode.Create, FileAccess.Write);
    CompressGZip(fsin, fsout, ct, buffersize, compresslevel);
  }


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


  public static void DecompressGZip(
    string src, string dest, int buffersize = 81920)
  {
    using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
    using var fsout = new FileStream(dest, FileMode.Create, FileAccess.Write);
    DecompressGZip(fsin, fsout, buffersize);
  }

  public static void DecompressGZip(
    string src, string dest, CancellationToken ct, int buffersize = 81920)
  {
    using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
    using var fsout = new FileStream(dest, FileMode.Create, FileAccess.Write);
    DecompressGZip(fsin, fsout, ct, buffersize);
  }

  public static void DecompressGZip(
    Stream input, Stream output, int buffersize = 81920)
  {
    int readbytes;
    var buffer = new byte[buffersize];

    using var gzip = new GZipStream(input, CompressionMode.Decompress);
    while ((readbytes = gzip.Read(buffer)) > 0)
      output.Write(buffer, 0, readbytes);
  }

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

  public async static Task CompressGZipAsync(
      string src, string dest, int buffersize = 81920,
      CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    await using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
    await using var fsout = new FileStream(dest, FileMode.Create, FileAccess.Write);
    await CompressGZipAsync(fsin, fsout, buffersize, compresslevel).ConfigureAwait(false);
  }

  public async static Task CompressGZipAsync(
    string src, string dest, CancellationToken ct, int buffersize = 81920,
    CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    await using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
    await using var fsout = new FileStream(dest, FileMode.Create, FileAccess.Write);
    await CompressGZipAsync(fsin, fsout, ct, buffersize, compresslevel).ConfigureAwait(false);
  }


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



  public async static Task DecompressGZipAsync(
    string src, string dest, int buffersize = 81920)
  {
    await using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
    await using var fsout = new FileStream(dest, FileMode.Create, FileAccess.Write);
    await DecompressGZipAsync(fsin, fsout, buffersize).ConfigureAwait(false);
  }

  public async static Task DecompressGZipAsync(
    string src, string dest, CancellationToken ct, int buffersize = 81920)
  {
    await using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
    await using var fsout = new FileStream(dest, FileMode.Create, FileAccess.Write);
    await DecompressGZipAsync(fsin, fsout, ct, buffersize).ConfigureAwait(false);
  }

  public async static Task DecompressGZipAsync(
    Stream input, Stream output, int buffersize = 81920)
  {
    int readbytes;
    var buffer = new byte[buffersize];

    using var gzip = new GZipStream(input, CompressionMode.Decompress);
    while ((readbytes = await gzip.ReadAsync(
      buffer.AsMemory(0, buffer.Length)).ConfigureAwait(false)) > 0)
      await output.WriteAsync(buffer.AsMemory(0, readbytes)).ConfigureAwait(false);
  }

  public async static Task DecompressGZipAsync(
    Stream input, Stream output, CancellationToken ct, int buffersize = 81920)
  {
    int readbytes;
    var buffer = new byte[buffersize];

    using var gzip = new GZipStream(input, CompressionMode.Decompress);
    while ((readbytes = await gzip.ReadAsync(
      buffer.AsMemory(0, buffer.Length), ct).ConfigureAwait(false)) > 0)
      await output.WriteAsync(buffer.AsMemory(0, readbytes), ct).ConfigureAwait(false);
  }


  #endregion Async FileStream Compress

  #endregion FileStream

  #endregion GZip

  #region Brotli

  #region Bytes

  #region Brotli Compress
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

  public static byte[] DecompressBrotli(ReadOnlySpan<byte> bytes)
  {
    using var msout = new MemoryStream();
    using var ms = new MemoryStream(bytes.ToArray());
    using (var Brotli = new BrotliStream(ms, CompressionMode.Decompress))
      Brotli.CopyTo(msout);

    return msout.ToArray();
  }

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
      if (decompressed.Length > 1 << 23)
        throw new ArgumentOutOfRangeException(nameof(decompressed),
          $"{nameof(decompressed)}.Length has failed!");

      result = BrotliDecoder.TryDecompress(
        bytes, decompressed, out writtenbytes);

      if (!result) continue;
      Array.Resize(ref decompressed, writtenbytes);
    }

    return result;
  }


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

      if (decompressed.Length > 1 << 23)
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

  public async static Task<byte[]> DecompressBrotliAsync(byte[] bytes)
  {
    await using var msout = new MemoryStream();
    await using var ms = new MemoryStream(bytes);
    await using (var Brotli = new BrotliStream(ms, CompressionMode.Decompress))
      await Brotli.CopyToAsync(msout).ConfigureAwait(false);

    return msout.ToArray();
  }

  public async static Task<byte[]> DecompressBrotliAsync(
   byte[] bytes, CancellationToken ct)
  {
    using var msout = new MemoryStream();
    using var ms = new MemoryStream(bytes);
    using (var Brotli = new BrotliStream(ms, CompressionMode.Decompress))
      await Brotli.CopyToAsync(msout, ct).ConfigureAwait(false);

    return msout.ToArray();
  }

  #endregion Async Brotli Compress

  #endregion Bytes

  #region FileStream

  #region FileStream Compress

  public static void CompressBrotli(
    string src, string dest, int buffersize = 81920,
    CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
    using var fsout = new FileStream(dest, FileMode.Create, FileAccess.Write);
    CompressBrotli(fsin, fsout, buffersize, compresslevel);
  }

  public static void CompressBrotli(
    string src, string dest, CancellationToken ct, int buffersize = 81920,
    CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
    using var fsout = new FileStream(dest, FileMode.Create, FileAccess.Write);
    CompressBrotli(fsin, fsout, ct, buffersize, compresslevel);
  }


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


  public static void DecompressBrotli(
    string src, string dest, int buffersize = 81920)
  {
    using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
    using var fsout = new FileStream(dest, FileMode.Create, FileAccess.Write);
    DecompressBrotli(fsin, fsout, buffersize);
  }

  public static void DecompressBrotli(
    string src, string dest, CancellationToken ct, int buffersize = 81920)
  {
    using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
    using var fsout = new FileStream(dest, FileMode.Create, FileAccess.Write);
    DecompressBrotli(fsin, fsout, ct, buffersize);
  }

  public static void DecompressBrotli(
    Stream input, Stream output, int buffersize = 81920)
  {
    int readbytes;
    var buffer = new byte[buffersize];

    using var Brotli = new BrotliStream(input, CompressionMode.Decompress);
    while ((readbytes = Brotli.Read(buffer)) > 0)
      output.Write(buffer, 0, readbytes); 
  }

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

  public async static Task CompressBrotliAsync(
      string src, string dest, int buffersize = 81920,
      CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    await using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
    await using var fsout = new FileStream(dest, FileMode.Create, FileAccess.Write);
    await CompressBrotliAsync(fsin, fsout, buffersize, compresslevel).ConfigureAwait(false);
  }

  public async static Task CompressBrotliAsync(
    string src, string dest, CancellationToken ct, int buffersize = 81920,
    CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    await using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
    await using var fsout = new FileStream(dest, FileMode.Create, FileAccess.Write);
    await CompressBrotliAsync(fsin, fsout, ct, buffersize, compresslevel).ConfigureAwait(false);
  }


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


  public async static Task DecompressBrotliAsync(
    string src, string dest, int buffersize = 81920)
  {
    await using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
    await using var fsout = new FileStream(dest, FileMode.Create, FileAccess.Write);
    await DecompressBrotliAsync(fsin, fsout, buffersize).ConfigureAwait(false);
  }

  public async static Task DecompressBrotliAsync(
    string src, string dest, CancellationToken ct, int buffersize = 81920)
  {
    await using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
    await using var fsout = new FileStream(dest, FileMode.Create, FileAccess.Write);
    await DecompressBrotliAsync(fsin, fsout, ct, buffersize).ConfigureAwait(false);
  }

  public async static Task DecompressBrotliAsync(
    Stream input, Stream output, int buffersize = 81920)
  {
    int readbytes;
    var buffer = new byte[buffersize];

    using var Brotli = new BrotliStream(input, CompressionMode.Decompress);
    while ((readbytes = await Brotli.ReadAsync(
      buffer.AsMemory(0, buffer.Length)).ConfigureAwait(false)) > 0)
      await output.WriteAsync(buffer.AsMemory(0, readbytes)).ConfigureAwait(false);
  }

  public async static Task DecompressBrotliAsync(
    Stream input, Stream output, CancellationToken ct, int buffersize = 81920)
  {
    int readbytes;
    var buffer = new byte[buffersize];

    using var Brotli = new BrotliStream(input, CompressionMode.Decompress);
    while ((readbytes = await Brotli.ReadAsync(
      buffer.AsMemory(0, buffer.Length), ct).ConfigureAwait(false)) > 0)
      await output.WriteAsync(buffer.AsMemory(0, readbytes), ct).ConfigureAwait(false);
  }


  #endregion Async FileStream Compress

  #endregion FileStream

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
