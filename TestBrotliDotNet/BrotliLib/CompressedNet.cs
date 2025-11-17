using System.Buffers;
using System.IO.Compression;


namespace michele.natale.Compressors;


//Synchron: Span<T> / ReadOnlySpan<T> → maximale Performance, keine Allokationen.
//Asynchron: byte[] / Memory<T> → await-sicher, stabil über Task-Grenzen hinweg.


public class CompressedNet
{

  #region GZip

  #region Bytes
  public static byte[] CompressGZip(
    ReadOnlySpan<byte> bytes, CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    AssertCompress(bytes);

    using var ms = new MemoryStream();
    using (var gzip = new GZipStream(ms, compresslevel))
      gzip.Write(bytes);
    return ms.ToArray();
  }

  public async static Task<byte[]> CompressGZipAsync(
    byte[] bytes, CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    AssertCompress(bytes);

    using var ms = new MemoryStream();
    using (var gzip = new GZipStream(ms, compresslevel))
      await gzip.WriteAsync(bytes).ConfigureAwait(false);
    return ms.ToArray();
  }

  public static byte[] CompressGZip(
      ReadOnlySpan<byte> bytes, CancellationToken ct,
      CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    var input = bytes.ToArray();
    using var result = Task.Run(() => CompressGZip(input, compresslevel));
    result.Wait(ct);

    if (result.IsCanceled) return [];

    return result.Result;
  }

  public static byte[] CompressGZipSpec(
      ReadOnlySpan<byte> bytes, CancellationToken ct,
      CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    using var result = CompressGZipSpecAsync(bytes.ToArray(), ct, compresslevel);
    return result.Result;
  }

  public async static Task<byte[]> CompressGZipAsync(
    byte[] bytes, CancellationToken ct,
    CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    using var result = CompressGZipAsync(bytes, compresslevel);
    result.Wait(ct);

    if (result.IsCanceled) return [];

    return await result;
  }

  public async static Task<byte[]> CompressGZipSpecAsync(
    byte[] bytes, CancellationToken ct,
    CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    AssertCompress(bytes);

    using var ms = new MemoryStream();
    using (var gzip = new GZipStream(ms, compresslevel))
      await gzip.WriteAsync(bytes, ct).ConfigureAwait(false);
    return ms.ToArray();
  }


  public static byte[] DecompressGZip(ReadOnlySpan<byte> bytes)
  {
    using var ms = new MemoryStream(bytes.ToArray());
    using var msout = new MemoryStream();
    using (var gzip = new GZipStream(ms, CompressionMode.Decompress))
      gzip.CopyTo(msout);
    return msout.ToArray();
  }

  public async static Task<byte[]> DecompressGZipAsync(byte[] bytes)
  {
    using var ms = new MemoryStream(bytes);
    using var msout = new MemoryStream();
    using (var gzip = new GZipStream(ms, CompressionMode.Decompress))
      await gzip.CopyToAsync(msout).ConfigureAwait(false);
    return msout.ToArray();
  }


  public static byte[] DecompressGZip(
    ReadOnlySpan<byte> bytes, CancellationToken ct)
  {
    var input = bytes.ToArray();
    using var result = Task.Run(() => DecompressGZip(input));
    result.Wait(ct);

    if (result.IsCanceled) return [];
    return result.Result;
  }


  public static byte[] DecompressGZipSpec(
    ReadOnlySpan<byte> bytes, CancellationToken ct)
  {
    using var result = DecompressGZipSpecAsync(bytes.ToArray(), ct);
    return result.Result;
  }

  public async static Task<byte[]> DecompressGZipAsync(
    byte[] bytes, CancellationToken ct)
  {
    var input = bytes.ToArray();
    using var result = DecompressGZipAsync(input);
    result.Wait(ct);

    if (result.IsCanceled) return [];
    return await result;
  }

  public async static Task<byte[]> DecompressGZipSpecAsync(
    byte[] bytes, CancellationToken ct)
  {
    using var ms = new MemoryStream(bytes);
    using var msout = new MemoryStream();
    using (var gzip = new GZipStream(ms, CompressionMode.Decompress))
      await gzip.CopyToAsync(msout, ct).ConfigureAwait(false);
    return msout.ToArray();
  }

  #endregion Bytes

  #region Streams

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

  public static void CompressGZipSpec(
    string src, string dest, CancellationToken ct, int buffersize = 81920,
    CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
    using var fsout = new FileStream(dest, FileMode.Create, FileAccess.Write);
    CompressGZipSpec(fsin, fsout, ct, buffersize, compresslevel);
  }

  public async static Task CompressGZipAsync(
      string src, string dest, int buffersize = 81920,
      CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
    using var fsout = new FileStream(dest, FileMode.Create, FileAccess.Write);
    await CompressGZipAsync(fsin, fsout, buffersize, compresslevel).ConfigureAwait(false);
  }

  public async static Task CompressGZipAsync(
    string src, string dest, CancellationToken ct, int buffersize = 81920,
    CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
    using var fsout = new FileStream(dest, FileMode.Create, FileAccess.Write);
    await CompressGZipAsync(fsin, fsout, ct, buffersize, compresslevel).ConfigureAwait(false);
  }

  // ********** ********** ********** ********** ********** ********** ********** **********
  // ********** ********** ********** ********** ********** ********** ********** **********

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
    using var result = CompressGZipAsync(input, output, buffersize, compresslevel);
    result.Wait(ct);
  }

  public static void CompressGZipSpec(
    Stream input, Stream output, CancellationToken ct, int buffersize = 81920,
    CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    using var result = CompressGZipAsync(input, output, ct, buffersize, compresslevel);
    result.Wait();
  }

  public async static Task CompressGZipAsync(
    Stream input, Stream output, int buffersize = 81920,
    CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    int readbytes;
    var buffer = new byte[buffersize];

    using var gzip = new GZipStream(output, compresslevel);
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

    using var gzip = new GZipStream(output, compresslevel);
    while ((readbytes = await input.ReadAsync(
      buffer.AsMemory(0, buffer.Length), ct).ConfigureAwait(false)) > 0)
      await gzip.WriteAsync(buffer.AsMemory(0, readbytes), ct).ConfigureAwait(false);
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

  public static void DecompressGZipSpec(
    string src, string dest, CancellationToken ct, int buffersize = 81920)
  {
    using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
    using var fsout = new FileStream(dest, FileMode.Create, FileAccess.Write);
    DecompressGZipSpec(fsin, fsout, ct, buffersize);
  }

  public async static Task DecompressGZipAsync(
    string src, string dest, int buffersize = 81920)
  {
    using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
    using var fsout = new FileStream(dest, FileMode.Create, FileAccess.Write);
    await DecompressGZipAsync(fsin, fsout, buffersize).ConfigureAwait(false);
  }

  public async static Task DecompressGZipAsync(
    string src, string dest, CancellationToken ct, int buffersize = 81920)
  {
    using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
    using var fsout = new FileStream(dest, FileMode.Create, FileAccess.Write);
    await DecompressGZipAsync(fsin, fsout, ct, buffersize).ConfigureAwait(false);
  }

  // ********** ********** ********** ********** ********** ********** ********** **********
  // ********** ********** ********** ********** ********** ********** ********** **********

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
    using var result = DecompressGZipAsync(input, output, buffersize);
    result.Wait(ct);
  }

  public static void DecompressGZipSpec(
    Stream input, Stream output, CancellationToken ct, int buffersize = 81920)
  {
    using var result = DecompressGZipAsync(input, output, ct, buffersize);
    result.Wait();
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

  #endregion Streams

  #endregion GZip


  #region Brotli

  #region Bytes
  public static byte[] CompressBrotli(
    ReadOnlySpan<byte> bytes,
    CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    AssertCompress(bytes);

    using var ms = new MemoryStream();
    using (var brotli = new BrotliStream(ms, compresslevel))
      brotli.Write(bytes);
    return ms.ToArray();

  }

  public static byte[] CompressBrotli(
    ReadOnlySpan<byte> bytes, CancellationToken ct,
    CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    var input = bytes.ToArray();
    using var result = Task.Run(() => CompressBrotli(input, compresslevel));
    result.Wait(ct);

    if (result.IsCanceled) return [];
    return result.Result;
  }

  public static bool TryCompressBrotli(
    ReadOnlySpan<byte> bytes,
    out byte[] compressed, out int compressed_bytes,
    int quality = 4, int window = 22)
  {
    AssertCompress(bytes);

    compressed = new byte[bytes.Length];
    if (BrotliEncoder.TryCompress(
    bytes, compressed, out compressed_bytes,
    quality, window))
    {
      Array.Resize(ref compressed, compressed_bytes);
      return true;
    }

    return false;
  }

  public static bool TryCompressBrotli(
    ReadOnlySpan<byte> bytes, CancellationToken ct,
    out byte[] compressed, out int compressed_bytes,
    int quality = 4, int window = 22)
  {
    var counts = 0;
    var input = bytes.ToArray();
    byte[] output = new byte[bytes.Length];
    using var result = Task.Run(() => TryCompressBrotli(
      input, out output, out counts,
      quality, window));
    result.Wait(ct);

    compressed = output;
    compressed_bytes = counts;

    if (result.IsCanceled) return false;

    return result.Result;
  }

  public static async Task<byte[]> CompressBrotliAsync(
    byte[] bytes, CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    AssertCompress(bytes);

    using var ms = new MemoryStream();
    using (var brotli = new BrotliStream(ms, compresslevel))
      await brotli.WriteAsync(bytes);
    return ms.ToArray();
  }

  public static byte[] CompressBrotliSpec(
    ReadOnlySpan<byte> bytes, CancellationToken ct,
    CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    using var result = CompressBrotliAsync(bytes.ToArray(), ct, compresslevel);
    return result.Result;
  }

  public async static Task<byte[]> CompressBrotliAsync(
    byte[] bytes, CancellationToken ct,
    CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    using var result = CompressBrotliAsync(bytes, compresslevel);
    result.Wait(ct);

    if (result.IsCanceled) return [];
    return await result;
  }

  public static async Task<byte[]> CompressBrotliAsyncSpec(
    byte[] bytes, CancellationToken ct,
    CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    AssertCompress(bytes);

    using var ms = new MemoryStream();
    using (var brotli = new BrotliStream(ms, compresslevel))
      await brotli.WriteAsync(bytes, ct);
    return ms.ToArray();
  }


  public async static Task<byte[]> TryCompressBrotliAsync(
    byte[] bytes, int quality = 4, int window = 22)
  {
    AssertCompress(bytes, quality, window);

    var cnt = 1;
    var result = false;
    var memorybytes = bytes.AsMemory();
    await using var output = new MemoryStream();

    while (!result)
    {
      var buffer = new byte[cnt++ * bytes.Length].AsMemory();
      if (BrotliEncoder.TryCompress(memorybytes.Span,
        buffer.Span, out var written, quality, window))
        if (written > 0)
        {
          await output.WriteAsync(buffer[..written]).ConfigureAwait(false);
          result = true;
        }
    }

    return output.ToArray();
  }


  public async static Task<byte[]> TryCompressBrotliAsync(
    byte[] bytes, CancellationToken ct,
    int quality = 4, int window = 22)
  {
    using var result = Task.Run(() => TryCompressBrotliAsync(bytes, quality, window));
    result.Wait(ct);

    if (result.IsCanceled) return [];
    return await result;
  }


  public static byte[] DecompressBrotli(ReadOnlySpan<byte> bytes)
  {
    using var msout = new MemoryStream();
    using var ms = new MemoryStream(bytes.ToArray());
    using (var brotli = new BrotliStream(ms, CompressionMode.Decompress))
      brotli.CopyTo(msout);
    return msout.ToArray();
  }

  public static async Task<byte[]> DecompressBrotliAsync(byte[] bytes)
  {
    using var msout = new MemoryStream();
    using var ms = new MemoryStream(bytes);
    using (var brotli = new BrotliStream(
      ms, CompressionMode.Decompress))
      await brotli.CopyToAsync(msout);
    return msout.ToArray();
  }

  public static byte[] DecompressBrotli(
    ReadOnlySpan<byte> bytes, CancellationToken ct)
  {
    var input = bytes.ToArray();
    using var result = Task.Run(() => DecompressBrotli(input));
    result.Wait(ct);

    if (result.IsCanceled) return [];
    return result.Result;
  }

  public static byte[] DecompressBrotliSpec(
    ReadOnlySpan<byte> bytes, CancellationToken ct)
  {
    using var result = DecompressBrotliAsync(bytes.ToArray(), ct);
    return result.Result;
  }


  public static bool TryDecompressBrotli(
    ReadOnlySpan<byte> bytes, out byte[] decompressed,
    out int decompressed_bytes)
  {
    var cnt = 2;
    decompressed = [];
    var result = false;
    decompressed_bytes = -1;
    while (!result)
    {
      decompressed = new byte[(1 << cnt++) * bytes.Length];
      result = BrotliDecoder.TryDecompress(
        bytes, decompressed, out decompressed_bytes);
      if (!result) continue;
      Array.Resize(ref decompressed, decompressed_bytes);
    }

    return result;
  }


  public static bool TryDecompressBrotli(
    ReadOnlySpan<byte> bytes, CancellationToken ct,
    out byte[] decompressed, out int decompressed_bytes)
  {
    var counts = 0;
    var input = bytes.ToArray();
    byte[] output = new byte[bytes.Length];
    using var result = Task.Run(() => TryDecompressBrotli(
      input, out output, out counts));
    result.Wait(ct);

    decompressed = output;
    decompressed_bytes = counts;

    if (result.IsCanceled) return false;

    return result.Result;
  }

  public static async Task<byte[]> DecompressBrotliAsync(
    byte[] bytes, CancellationToken ct)
  {
    using var result = DecompressBrotliAsync(bytes);
    result.Wait(ct);

    if (result.IsCanceled) return [];
    return await result;
  }

  public static async Task<byte[]> DecompressBrotliAsyncSpec(
    byte[] bytes, CancellationToken ct)
  {
    using var msout = new MemoryStream();
    using var ms = new MemoryStream(bytes);
    using (var brotli = new BrotliStream(
      ms, CompressionMode.Decompress))
      await brotli.CopyToAsync(msout, ct);
    return msout.ToArray();
  }

  public async static Task<byte[]> TryDecompressBrotliAsync(
      byte[] bytes)
  {
    AssertCompress(bytes);

    var cnt = 1;
    var result = false;
    var memorybytes = bytes.AsMemory();
    await using var output = new MemoryStream();

    while (!result)
    {
      var buffer = new byte[(1 << cnt++) * bytes.Length].AsMemory();
      if (BrotliDecoder.TryDecompress(memorybytes.Span,
        buffer.Span, out var written))
        if (written > 0)
        {
          await output.WriteAsync(buffer[..written]).ConfigureAwait(false);
          result = true;
        }
    }

    return output.ToArray();
  }

  public async static Task<byte[]> TryDecompressBrotliAsync(
    byte[] bytes, CancellationToken ct)
  {
    using var result = Task.Run(() => TryDecompressBrotliAsync(bytes));
    result.Wait(ct);

    if (result.IsCanceled) return [];
    return await result;
  }


  // ********** ********** ********** ********** ********** ********** ********** **********
  // ********** ********** ********** ********** ********** ********** ********** **********


  private static void AssertCompress(ReadOnlySpan<byte> bytes)
  {
    if (bytes.IsEmpty || bytes.Length == 0)
      throw new ArgumentNullException(nameof(bytes));
  }

  private static void AssertCompress(
    ReadOnlySpan<byte> bytes, int quality, int window)
  {
    AssertCompress(bytes);

    if (quality < 0 || quality > 11)
      throw new ArgumentOutOfRangeException(nameof(quality));

    if (window < 10 || window > 24)
      throw new ArgumentOutOfRangeException(nameof(window));
  }

  #endregion Bytes

  #region Streams

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

  public static void CompressBrotliSpec(
    string src, string dest, CancellationToken ct, int buffersize = 81920,
    CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
    using var fsout = new FileStream(dest, FileMode.Create, FileAccess.Write);
    CompressBrotliSpec(fsin, fsout, ct, buffersize, compresslevel);
  }

  public async static Task CompressBrotliAsync(
      string src, string dest, int buffersize = 81920,
      CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
    using var fsout = new FileStream(dest, FileMode.Create, FileAccess.Write);
    await CompressBrotliAsync(fsin, fsout, buffersize, compresslevel).ConfigureAwait(false);
  }

  public async static Task CompressBrotliAsync(
    string src, string dest, CancellationToken ct, int buffersize = 81920,
    CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
    using var fsout = new FileStream(dest, FileMode.Create, FileAccess.Write);
    await CompressBrotliAsync(fsin, fsout, ct, buffersize, compresslevel).ConfigureAwait(false);
  }

  // ********** ********** ********** ********** ********** ********** ********** **********
  // ********** ********** ********** ********** ********** ********** ********** **********

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
    using var result = CompressBrotliAsync(input, output, buffersize, compresslevel);
    result.Wait(ct);
  }

  public static void CompressBrotliSpec(
    Stream input, Stream output, CancellationToken ct, int buffersize = 81920,
    CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    using var result = CompressBrotliAsync(input, output, ct, buffersize, compresslevel);
    result.Wait();
  }

  public async static Task CompressBrotliAsync(
    Stream input, Stream output, int buffersize = 81920,
    CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    int readbytes;
    var buffer = new byte[buffersize];

    using var Brotli = new BrotliStream(output, compresslevel);
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

    using var Brotli = new BrotliStream(output, compresslevel);
    while ((readbytes = await input.ReadAsync(
      buffer.AsMemory(0, buffer.Length), ct).ConfigureAwait(false)) > 0)
      await Brotli.WriteAsync(buffer.AsMemory(0, readbytes), ct).ConfigureAwait(false);
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

  public static void DecompressBrotliSpec(
    string src, string dest, CancellationToken ct, int buffersize = 81920)
  {
    using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
    using var fsout = new FileStream(dest, FileMode.Create, FileAccess.Write);
    DecompressBrotliSpec(fsin, fsout, ct, buffersize);
  }

  public async static Task DecompressBrotliAsync(
    string src, string dest, int buffersize = 81920)
  {
    using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
    using var fsout = new FileStream(dest, FileMode.Create, FileAccess.Write);
    await DecompressBrotliAsync(fsin, fsout, buffersize).ConfigureAwait(false);
  }

  public async static Task DecompressBrotliAsync(
    string src, string dest, CancellationToken ct, int buffersize = 81920)
  {
    using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
    using var fsout = new FileStream(dest, FileMode.Create, FileAccess.Write);
    await DecompressBrotliAsync(fsin, fsout, ct, buffersize).ConfigureAwait(false);
  }

  // ********** ********** ********** ********** ********** ********** ********** **********
  // ********** ********** ********** ********** ********** ********** ********** **********

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
    using var result = DecompressBrotliAsync(input, output, buffersize);
    result.Wait(ct);
  }

  public static void DecompressBrotliSpec(
    Stream input, Stream output, CancellationToken ct, int buffersize = 81920)
  {
    using var result = DecompressBrotliAsync(input, output, ct, buffersize);
    result.Wait();
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


  #endregion Streams

  #endregion Brotli


}

