

using BrotliLib;
using System.Text;
using BrotliLib.Services;


namespace MyTestBrotli;


public class CompressedNetTest
{

  private static string OriginalString =>
    File.ReadAllText("data.txt"); //5.294 MB


  public static void Start()
  {
    TestGzip();
    TestGzipAsync();

    TestGzipStream();
    TestGzipStreamAsync();


    TestBrotli();
    TestBrotliAsync();

    TestBrotliStream();
    TestBrotliStreamAsync();

    Console.WriteLine();
  }


  private static void TestGzip()
  {

    var message = Encoding.UTF8.GetBytes(OriginalString);

    //GZip Classic
    Console.WriteLine("GZip Classic");
    var compress = CompressedNet.CompressGZip(message);
    Console.WriteLine("Length of compressed bytes: " + compress.Length);

    var decompress = CompressedNet.DecompressGZip(compress);
    Console.WriteLine("Length of decompressed bytes: " + decompress.Length);
    Console.WriteLine();
    if (!message.SequenceEqual(decompress)) throw new Exception();


    //GZip with CancellationToken I
    Console.WriteLine("GZip with CancellationToken I");
    var cts = new CancellationTokenSource();
    compress = CompressedNet.CompressGZip(message, cts.Token);
    Console.WriteLine("Length of compressed bytes: " + compress.Length);

    cts = new CancellationTokenSource();
    decompress = CompressedNet.DecompressGZip(compress, cts.Token);
    Console.WriteLine("Length of decompressed bytes: " + decompress.Length);
    Console.WriteLine();
    if (!message.SequenceEqual(decompress)) throw new Exception();


    //GZip with CancellationToken II
    Console.WriteLine("GZip with CancellationToken II");
    cts = new CancellationTokenSource();
    compress = CompressedNet.CompressGZipSpec(message, cts.Token);
    Console.WriteLine("Length of compressed bytes: " + compress.Length);

    cts = new CancellationTokenSource();
    decompress = CompressedNet.DecompressGZipSpec(compress, cts.Token);
    Console.WriteLine("Length of decompressed bytes: " + decompress.Length);
    Console.WriteLine();
    if (!message.SequenceEqual(decompress)) throw new Exception();

    Console.WriteLine();
  }

  private async static void TestGzipAsync()
  {
    var message = Encoding.UTF8.GetBytes(OriginalString);

    //GZip Async 
    Console.WriteLine("GZip Async");
    using var compressasync1 = CompressedNet.CompressGZipAsync(message);
    var compressed = await compressasync1;
    Console.WriteLine("Length of compressed bytes: " + (compressed).Length);

    using var decompressasync1 = CompressedNet.DecompressGZipAsync(compressed);
    var decompressed = await decompressasync1;
    Console.WriteLine("Length of decompressed bytes: " + (decompressed).Length);
    Console.WriteLine();

    if (!message.SequenceEqual(decompressed)) throw new Exception();


    //GZip Async with CancellationToken I
    Console.WriteLine("GZip Async with CancellationToken I");
    var cts = new CancellationTokenSource();
    using var compressasync2 = CompressedNet.CompressGZipAsync(message, cts.Token);
    compressed = await compressasync2;
    Console.WriteLine("Length of compressed string: " + (compressed).Length);

    cts = new CancellationTokenSource();
    using var decompressasync2 = CompressedNet.DecompressGZipAsync(compressed, cts.Token);
    decompressed = await decompressasync2;
    Console.WriteLine("Length of decompressed string: " + (decompressed).Length);
    Console.WriteLine();
    if (!message.SequenceEqual(decompressed)) throw new Exception();


    //GZip Async with CancellationToken II
    Console.WriteLine("GZip Async with CancellationToken II");
    cts = new CancellationTokenSource();
    using var compressasync3 = CompressedNet.CompressGZipSpecAsync(message, cts.Token);
    compressed = await compressasync3;
    Console.WriteLine("Length of compressed string: " + (compressed).Length);

    cts = new CancellationTokenSource();
    using var decompressasync3 = CompressedNet.DecompressGZipSpecAsync(compressed, cts.Token);
    decompressed = await decompressasync3;
    Console.WriteLine("Length of decompressed string: " + (decompressed).Length);
    Console.WriteLine();
    if (!message.SequenceEqual(decompressed)) throw new Exception();
    Console.WriteLine();
  }

  private static void TestGzipStream()
  {

    string src = "data.txt", dest = "datacompress", destr = "datar.txt";
    File.Delete(dest); File.Delete(destr);

    //GZip Stream Classic
    Console.WriteLine("GZip Stream Classic");
    CompressedNet.CompressGZip(src, dest);
    Console.WriteLine("Length of compressed bytes: " + ServicesCompress.FileSize(dest));

    CompressedNet.DecompressGZip(dest, destr);
    Console.WriteLine("Length of decompressed bytes: " + ServicesCompress.FileSize(destr));
    Console.WriteLine();
    if (!ServicesCompress.FileEquals(src, destr)) throw new Exception();


    //GZip Stream with CancellationToken I
    Console.WriteLine("GZip Stream with CancellationToken I");
    var cts = new CancellationTokenSource();
    CompressedNet.CompressGZip(src, dest, cts.Token);
    Console.WriteLine("Length of compressed bytes: " + ServicesCompress.FileSize(dest));

    cts = new CancellationTokenSource();
    CompressedNet.DecompressGZip(dest, destr, cts.Token);
    Console.WriteLine("Length of decompressed bytes: " + ServicesCompress.FileSize(destr));
    Console.WriteLine();
    if (!ServicesCompress.FileEquals(src, destr)) throw new Exception();

    //GZip Stream with CancellationToken II
    Console.WriteLine("GZip Stream with CancellationToken II");
    cts = new CancellationTokenSource();
    CompressedNet.CompressGZipSpec(src, dest, cts.Token);
    Console.WriteLine("Length of compressed bytes: " + ServicesCompress.FileSize(dest));

    cts = new CancellationTokenSource();
    CompressedNet.DecompressGZipSpec(dest, destr, cts.Token);
    Console.WriteLine("Length of decompressed bytes: " + ServicesCompress.FileSize(destr));
    Console.WriteLine();
    if (!ServicesCompress.FileEquals(src, destr)) throw new Exception();
    Console.WriteLine();
  }

  private static void TestGzipStreamAsync()
  {
    string src = "data.txt", dest = "datacompress", destr = "datar.txt";
    File.Delete(dest); File.Delete(destr);

    //GZip Stream Async
    Console.WriteLine("GZip Stream Async");
    using var compressasync1 = CompressedNet.CompressGZipAsync(src, dest);
    compressasync1.Wait();
    Console.WriteLine("Length of compressed bytes: " + ServicesCompress.FileSize(dest));

    using var decompressasync1 = CompressedNet.DecompressGZipAsync(dest, destr);
    decompressasync1.Wait();
    Console.WriteLine("Length of decompressed bytes: " + ServicesCompress.FileSize(destr));
    Console.WriteLine();
    if (!ServicesCompress.FileEquals(src, destr)) throw new Exception();

    File.Delete(dest); File.Delete(destr);

    //GZip Stream Async with CancellationToken I
    Console.WriteLine("GZip Stream Async with CancellationToken I");

    var cts = new CancellationTokenSource();
    using var compressasync2 = CompressedNet.CompressGZipAsync(src, dest);
    compressasync2.Wait(cts.Token);
    Console.WriteLine("Length of compressed bytes: " + ServicesCompress.FileSize(dest));

    cts = new CancellationTokenSource();
    using var decompressasync2 = CompressedNet.DecompressGZipAsync(dest, destr);
    decompressasync2.Wait(cts.Token);
    Console.WriteLine("Length of decompressed bytes: " + ServicesCompress.FileSize(destr));
    Console.WriteLine();
    if (!ServicesCompress.FileEquals(src, destr)) throw new Exception();

    File.Delete(dest); File.Delete(destr);

    //GZip Stream Async with CancellationToken II
    Console.WriteLine("GZip Stream Async with CancellationToken II");
    cts = new CancellationTokenSource();
    using var compressasync3 = CompressedNet.CompressGZipAsync(src, dest, cts.Token);
    compressasync3.Wait();
    Console.WriteLine("Length of compressed bytes: " + ServicesCompress.FileSize(dest));

    cts = new CancellationTokenSource();
    using var decompressasync3 = CompressedNet.DecompressGZipAsync(dest, destr, cts.Token);
    decompressasync3.Wait();
    Console.WriteLine("Length of decompressed bytes: " + ServicesCompress.FileSize(destr));
    Console.WriteLine();
    if (!ServicesCompress.FileEquals(src, destr)) throw new Exception();

    Console.WriteLine();
  }


  private static void TestBrotli()
  {
    var message = Encoding.UTF8.GetBytes(OriginalString);

    //Brotli Classic
    Console.WriteLine("Brotli Classic");
    var compress = CompressedNet.CompressBrotli(message);
    Console.WriteLine("Length of compressed bytes: " + compress.Length);

    var decompress = CompressedNet.DecompressBrotli(compress);
    Console.WriteLine("Length of decompressed bytes: " + decompress.Length);
    Console.WriteLine();
    if (!message.SequenceEqual(decompress)) throw new Exception();


    //Brotli with CancellationToken I
    Console.WriteLine("Brotli with CancellationToken I");
    var cts = new CancellationTokenSource();
    compress = CompressedNet.CompressBrotli(message, cts.Token);
    Console.WriteLine("Length of compressed bytes: " + compress.Length);

    cts = new CancellationTokenSource();
    decompress = CompressedNet.DecompressBrotli(compress, cts.Token);
    Console.WriteLine("Length of decompressed bytes: " + decompress.Length);
    Console.WriteLine();
    if (!message.SequenceEqual(decompress)) throw new Exception();


    //Brotli with CancellationToken II
    Console.WriteLine("Brotli with CancellationToken II");
    cts = new CancellationTokenSource();
    compress = CompressedNet.CompressBrotliSpec(message, cts.Token);
    Console.WriteLine("Length of compressed bytes: " + compress.Length);

    cts = new CancellationTokenSource();
    decompress = CompressedNet.DecompressBrotliSpec(compress, cts.Token);
    Console.WriteLine("Length of decompressed bytes: " + decompress.Length);
    Console.WriteLine();
    if (!message.SequenceEqual(decompress)) throw new Exception();


    //Brotli Try
    Console.WriteLine("Brotli Try");
    int quality = 4, window = 22;
    if (!CompressedNet.TryCompressBrotli(
      message, out compress, out var written, quality, window))
      throw new Exception();
    Console.WriteLine("Length of compressed bytes: " + written);

    if (!CompressedNet.TryDecompressBrotli(compress, out decompress, out written))
      throw new Exception();
    Console.WriteLine("Length of decompressed bytes: " + written);
    Console.WriteLine();
    if (!message.SequenceEqual(decompress)) throw new Exception();


    //Brotli Try
    Console.WriteLine("Brotli Try with CancellationToken");
    cts = new CancellationTokenSource();
    if (!CompressedNet.TryCompressBrotli(
      message, cts.Token, out compress, out written, quality, window))
      throw new Exception();
    Console.WriteLine("Length of compressed bytes: " + written);

    if (!CompressedNet.TryDecompressBrotli(
      compress, cts.Token, out decompress, out written))
      throw new Exception();
    Console.WriteLine("Length of decompressed bytes: " + written);
    Console.WriteLine();
    if (!message.SequenceEqual(decompress)) throw new Exception();
    Console.WriteLine();
  }

  private async static void TestBrotliAsync()
  {
    var message = Encoding.UTF8.GetBytes(OriginalString);

    //Brotli Async 
    Console.WriteLine("Brotli Async");
    using var compressasync1 = CompressedNet.CompressBrotliAsync(message);
    var compressed = await compressasync1;
    Console.WriteLine("Length of compressed bytes: " + (compressed).Length);

    using var decompressasync1 = CompressedNet.DecompressBrotliAsync(compressed);
    var decompressed = await decompressasync1;
    Console.WriteLine("Length of decompressed bytes: " + decompressed.Length);
    Console.WriteLine();
    if (!message.SequenceEqual(decompressed)) throw new Exception();


    //Brotli Async with CancellationToken I
    Console.WriteLine("Brotli Async with CancellationToken I");
    var cts = new CancellationTokenSource();
    using var compressasync2 = CompressedNet.CompressBrotliAsync(message, cts.Token);
    compressed = await compressasync2;
    Console.WriteLine("Length of compressed string: " + (compressed).Length);

    cts = new CancellationTokenSource();
    using var decompressasync2 = CompressedNet.DecompressBrotliAsync(compressed, cts.Token);
    decompressed = await decompressasync2;
    Console.WriteLine("Length of decompressed string: " + (decompressed).Length);
    Console.WriteLine();
    if (!message.SequenceEqual(decompressed)) throw new Exception();


    //Brotli Async with CancellationToken II
    Console.WriteLine("Brotli Async with CancellationToken II");
    cts = new CancellationTokenSource();
    using var compressasync3 = CompressedNet.CompressBrotliAsyncSpec(message, cts.Token);
    compressed = await compressasync3;
    Console.WriteLine("Length of compressed string: " + compressed.Length);

    cts = new CancellationTokenSource();
    using var decompressasync3 = CompressedNet.DecompressBrotliAsyncSpec(compressed, cts.Token);
    decompressed = await decompressasync3;
    Console.WriteLine("Length of decompressed string: " + decompressed.Length);
    Console.WriteLine();
    if (!message.SequenceEqual(decompressed)) throw new Exception();


    //Brotli Try Async
    Console.WriteLine("Brotli Try Async");
    using var compressasync4 = CompressedNet.TryCompressBrotliAsync(message, 4, 22); //, 4, 22, 6776
    compressed = await compressasync4;
    Console.WriteLine("Length of compressed string: " + compressed.Length);

    using var decompressasync4 = CompressedNet.TryDecompressBrotliAsync(compressed);
    decompressed = await decompressasync4;
    Console.WriteLine("Length of decompressed string: " + decompressed.Length);
    Console.WriteLine();
    if (!message.SequenceEqual(decompressed)) throw new Exception();


    //Brotli Try Async with CancellationToken
    Console.WriteLine("Brotli Try Async with CancellationToken");
    cts = new CancellationTokenSource();
    using var compressasync5 = CompressedNet.TryCompressBrotliAsync(message, 4, 22); //, 4, 22, 6776
    compressed = await compressasync5;
    Console.WriteLine("Length of compressed string: " + compressed.Length);

    cts = new CancellationTokenSource();
    using var decompressasync5 = CompressedNet.TryDecompressBrotliAsync(compressed);
    decompressed = await decompressasync5;
    Console.WriteLine("Length of decompressed string: " + decompressed.Length);
    Console.WriteLine();
    if (!message.SequenceEqual(decompressed)) throw new Exception();


    Console.WriteLine();
  }

  private static void TestBrotliStream()
  {
    string src = "data.txt", dest = "datacompress", destr = "datar.txt";
    File.Delete(dest); File.Delete(destr);

    //Brotli Stream Classic
    Console.WriteLine("Brotli Stream Classic");
    CompressedNet.CompressBrotli(src, dest);
    Console.WriteLine("Length of compressed bytes: " + ServicesCompress.FileSize(dest));

    CompressedNet.DecompressBrotli(dest, destr);
    Console.WriteLine("Length of decompressed bytes: " + ServicesCompress.FileSize(destr));
    Console.WriteLine();
    if (!ServicesCompress.FileEquals(src, destr)) throw new Exception();


    //Brotli Stream with CancellationToken I
    Console.WriteLine("Brotli Stream with CancellationToken I");
    var cts = new CancellationTokenSource();
    CompressedNet.CompressBrotli(src, dest, cts.Token);
    Console.WriteLine("Length of compressed bytes: " + ServicesCompress.FileSize(dest));

    cts = new CancellationTokenSource();
    CompressedNet.DecompressBrotli(dest, destr, cts.Token);
    Console.WriteLine("Length of decompressed bytes: " + ServicesCompress.FileSize(destr));
    Console.WriteLine();
    if (!ServicesCompress.FileEquals(src, destr)) throw new Exception();

    //Brotli Stream with CancellationToken II
    Console.WriteLine("Brotli Stream with CancellationToken II");
    cts = new CancellationTokenSource();
    CompressedNet.CompressBrotliSpec(src, dest, cts.Token);
    Console.WriteLine("Length of compressed bytes: " + ServicesCompress.FileSize(dest));

    cts = new CancellationTokenSource();
    CompressedNet.DecompressBrotliSpec(dest, destr, cts.Token);
    Console.WriteLine("Length of decompressed bytes: " + ServicesCompress.FileSize(destr));
    Console.WriteLine();
    if (!ServicesCompress.FileEquals(src, destr)) throw new Exception();
    Console.WriteLine();
  }

  private static void TestBrotliStreamAsync()
  {

    string src = "data.txt", dest = "datacompress", destr = "datar.txt";
    File.Delete(dest); File.Delete(destr);

    //Brotli Stream Async
    Console.WriteLine("Brotli Stream Async");
    using var compressasync1 = CompressedNet.CompressBrotliAsync(src, dest);
    compressasync1.Wait();
    Console.WriteLine("Length of compressed bytes: " + ServicesCompress.FileSize(dest));

    using var decompressasync1 = CompressedNet.DecompressBrotliAsync(dest, destr);
    decompressasync1.Wait();
    Console.WriteLine("Length of decompressed bytes: " + ServicesCompress.FileSize(destr));
    Console.WriteLine();
    if (!ServicesCompress.FileEquals(src, destr)) throw new Exception();

    File.Delete(dest); File.Delete(destr);

    //Brotli Stream Async with CancellationToken I
    Console.WriteLine("Brotli Stream Async with CancellationToken I");

    var cts = new CancellationTokenSource();
    using var compressasync2 = CompressedNet.CompressBrotliAsync(src, dest);
    compressasync2.Wait(cts.Token);
    Console.WriteLine("Length of compressed bytes: " + ServicesCompress.FileSize(dest));

    cts = new CancellationTokenSource();
    using var decompressasync2 = CompressedNet.DecompressBrotliAsync(dest, destr);
    decompressasync2.Wait(cts.Token);
    Console.WriteLine("Length of decompressed bytes: " + ServicesCompress.FileSize(destr));
    Console.WriteLine();
    if (!ServicesCompress.FileEquals(src, destr)) throw new Exception();

    File.Delete(dest); File.Delete(destr);

    //Brotli Stream Async with CancellationToken II
    Console.WriteLine("Brotli Stream Async with CancellationToken II");
    cts = new CancellationTokenSource();
    using var compressasync3 = CompressedNet.CompressBrotliAsync(src, dest, cts.Token);
    compressasync3.Wait();
    Console.WriteLine("Length of compressed bytes: " + ServicesCompress.FileSize(dest));

    cts = new CancellationTokenSource();
    using var decompressasync3 = CompressedNet.DecompressBrotliAsync(dest, destr, cts.Token);
    decompressasync3.Wait();
    Console.WriteLine("Length of decompressed bytes: " + ServicesCompress.FileSize(destr));
    Console.WriteLine();
    if (!ServicesCompress.FileEquals(src, destr)) throw new Exception();

    Console.WriteLine();
  }


}
