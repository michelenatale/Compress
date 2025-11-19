


using System.Text;

namespace michele.natale.Tests;

using Compressors;
using Services;
using System.Threading.Tasks;

public class CompressedNetTest
{
    private static string OriginalString =>
      File.ReadAllText("data.txt"); //5.294 MB

    public async static Task Start()
    {
        TestGzip();
        await TestGzipAsync();

        TestGzipFileStream();
        await TestGzipFileStreamAsync();


        TestBrotli();
        await TestBrotliAsync();

        TestBrotliStream();
        await TestBrotliStreamAsync();

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


        //GZip with CancellationToken
        Console.WriteLine("GZip with CancellationToken");
        var cts = new CancellationTokenSource();
        compress = CompressedNet.CompressGZip(message, cts.Token);
        Console.WriteLine("Length of compressed bytes: " + compress.Length);

        cts = new CancellationTokenSource();
        decompress = CompressedNet.DecompressGZip(compress, cts.Token);
        Console.WriteLine("Length of decompressed bytes: " + decompress.Length);
        Console.WriteLine();
        if (!message.SequenceEqual(decompress)) throw new Exception();

        Console.WriteLine();
    }

    private async static Task TestGzipAsync()
    {
        var message = Encoding.UTF8.GetBytes(OriginalString);

        //GZip Async 
        Console.WriteLine("GZip Async");
        var compress1 = await CompressedNet.CompressGZipAsync(message);
        Console.WriteLine("Length of compressed bytes: " + (compress1).Length);

        var decompress1 = await CompressedNet.DecompressGZipAsync(compress1);
        Console.WriteLine("Length of decompressed bytes: " + (decompress1).Length);
        Console.WriteLine();

        if (!message.SequenceEqual(decompress1)) throw new Exception();


        //GZip Async with CancellationToken
        Console.WriteLine("GZip Async with CancellationToken");
        var cts = new CancellationTokenSource();
        var compress2 = await CompressedNet.CompressGZipAsync(message, cts.Token);
        Console.WriteLine("Length of compressed string: " + (compress2).Length);

        cts = new CancellationTokenSource();
        var decompress2 = await CompressedNet.DecompressGZipAsync(compress2, cts.Token);
        Console.WriteLine("Length of decompressed string: " + (decompress2).Length);
        Console.WriteLine();
        if (!message.SequenceEqual(decompress2)) throw new Exception();

        Console.WriteLine();
    }

    private static void TestGzipFileStream()
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


        //GZip Stream with CancellationToken
        Console.WriteLine("GZip Stream with CancellationToken");
        var cts = new CancellationTokenSource();
        CompressedNet.CompressGZip(src, dest, cts.Token);
        Console.WriteLine("Length of compressed bytes: " + ServicesCompress.FileSize(dest));

        cts = new CancellationTokenSource();
        CompressedNet.DecompressGZip(dest, destr, cts.Token);
        Console.WriteLine("Length of decompressed bytes: " + ServicesCompress.FileSize(destr));
        Console.WriteLine();
        if (!ServicesCompress.FileEquals(src, destr)) throw new Exception();

        Console.WriteLine();
    }

    private static async Task TestGzipFileStreamAsync()
    {
        string src = "data.txt", dest = "datacompress", destr = "datar.txt";
        File.Delete(dest); File.Delete(destr);

        //GZip Stream Async
        Console.WriteLine("GZip Stream Async");
        await CompressedNet.CompressGZipAsync(src, dest);
        Console.WriteLine("Length of compressed bytes: " + ServicesCompress.FileSize(dest));

        await CompressedNet.DecompressGZipAsync(dest, destr);
        Console.WriteLine("Length of decompressed bytes: " + ServicesCompress.FileSize(destr));
        Console.WriteLine();
        if (!ServicesCompress.FileEquals(src, destr)) throw new Exception();

        File.Delete(dest); File.Delete(destr);

        //GZip Stream Async with CancellationToken
        Console.WriteLine("GZip Stream Async with CancellationToken");

        var cts = new CancellationTokenSource();
        await CompressedNet.CompressGZipAsync(src, dest, cts.Token);
        Console.WriteLine("Length of compressed bytes: " + ServicesCompress.FileSize(dest));

        cts = new CancellationTokenSource();
        await CompressedNet.DecompressGZipAsync(dest, destr, cts.Token);
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


        //Brotli with CancellationToken
        Console.WriteLine("Brotli with CancellationToken");
        var cts = new CancellationTokenSource();
        compress = CompressedNet.CompressBrotli(message, cts.Token);
        Console.WriteLine("Length of compressed bytes: " + compress.Length);

        cts = new CancellationTokenSource();
        decompress = CompressedNet.DecompressBrotli(compress, cts.Token);
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

    private async static Task TestBrotliAsync()
    {
        var message = Encoding.UTF8.GetBytes(OriginalString);

        //Brotli Async 
        Console.WriteLine("Brotli Async");
        var compress1 = await CompressedNet.CompressBrotliAsync(message);
        Console.WriteLine("Length of compressed bytes: " + compress1.Length);

        var decompress1 = await CompressedNet.DecompressBrotliAsync(compress1);
        Console.WriteLine("Length of decompressed bytes: " + decompress1.Length);
        Console.WriteLine();
        if (!message.SequenceEqual(decompress1)) throw new Exception();


        //Brotli Async with CancellationToken
        Console.WriteLine("Brotli Async with CancellationToken");
        var cts = new CancellationTokenSource();
        var compress2 = await CompressedNet.CompressBrotliAsync(message, cts.Token);
        Console.WriteLine("Length of compressed string: " + compress2.Length);

        cts = new CancellationTokenSource();
        var decompress2 = await CompressedNet.DecompressBrotliAsync(compress2, cts.Token);
        Console.WriteLine("Length of decompressed string: " + decompress2.Length);
        Console.WriteLine();
        if (!message.SequenceEqual(decompress2)) throw new Exception();

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


        //Brotli Stream with CancellationToken
        Console.WriteLine("Brotli Stream with CancellationToken");
        var cts = new CancellationTokenSource();
        CompressedNet.CompressBrotli(src, dest, cts.Token);
        Console.WriteLine("Length of compressed bytes: " + ServicesCompress.FileSize(dest));

        cts = new CancellationTokenSource();
        CompressedNet.DecompressBrotli(dest, destr, cts.Token);
        Console.WriteLine("Length of decompressed bytes: " + ServicesCompress.FileSize(destr));
        Console.WriteLine();
        if (!ServicesCompress.FileEquals(src, destr)) throw new Exception();

        Console.WriteLine();
    }

    private async static Task TestBrotliStreamAsync()
    {

        string src = "data.txt", dest = "datacompress", destr = "datar.txt";
        File.Delete(dest); File.Delete(destr);

        //Brotli Stream Async
        Console.WriteLine("Brotli Stream Async");
        await CompressedNet.CompressBrotliAsync(src, dest);
        Console.WriteLine("Length of compressed bytes: " + ServicesCompress.FileSize(dest));

        await CompressedNet.DecompressBrotliAsync(dest, destr);
        Console.WriteLine("Length of decompressed bytes: " + ServicesCompress.FileSize(destr));
        Console.WriteLine();
        if (!ServicesCompress.FileEquals(src, destr)) throw new Exception();

        File.Delete(dest); File.Delete(destr);

        //Brotli Stream Async with CancellationToken
        Console.WriteLine("Brotli Stream Async with CancellationToken");
        var cts = new CancellationTokenSource();
        await CompressedNet.CompressBrotliAsync(src, dest, cts.Token);
        Console.WriteLine("Length of compressed bytes: " + ServicesCompress.FileSize(dest));

        cts = new CancellationTokenSource();
        await CompressedNet.DecompressBrotliAsync(dest, destr, cts.Token);
        Console.WriteLine("Length of decompressed bytes: " + ServicesCompress.FileSize(destr));
        Console.WriteLine();
        if (!ServicesCompress.FileEquals(src, destr)) throw new Exception();

        Console.WriteLine();
    }

}
