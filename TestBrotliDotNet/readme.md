# Best Practice: Brotli and GZip from DotNet in Test.
[Brotli](https://en.wikipedia.org/wiki/Brotli) and [GZip](https://en.wikipedia.org/wiki/Gzip) are both compression methods for data. Both are implemented in the .Net Framework Core. Both compression methods can compress text as well as bytes, and are also used for sending data over the Internet, i.e., from one web server to another. 

Incidentally, both methods are based on [LZ77](https://en.wikipedia.org/wiki/LZ77_and_LZ78#LZ77) and [Huffman Coding](https://en.wikipedia.org/wiki/Huffman_coding). LZ77 was developed in 1977 by [Abraham Lempel](https://en.wikipedia.org/wiki/Abraham_Lempel) and [Jacob Ziv](https://en.wikipedia.org/wiki/Jacob_Ziv). It is based on the [dictionary principle](https://en.wikipedia.org/wiki/Dictionary_coder), which has always proven to be extremely effective in compression. Compression rates of up to 90% can be achieved in some cases.

## Applying the Brotli GZip Library:
There is a [test file C#](https://github.com/michelenatale/Compress/blob/main/TestBrotliDotNet/TestBrotliGZip/CompressedNetTest.cs) and a [test file vb.net](https://github.com/michelenatale/Compress/blob/main/TestBrotliDotNet/TestBrotliGZipVB/CompressedNetTest.vb) that shows how to use the brotli gzip library Codes.

Here is a little code for compressing and decompressing. Of course, all methods are documented to explain why it is done this way: 
```
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
```
```
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
```

## Bonus

Afterwards, I added a very slim, simple, and fast archiver (similar to TAR, ZIP, GZ) called “FileCompressPackage.” 

FileCompressPackage simply takes the desired files (filepath) and compresses (PACK) them all together into a single stream or file. All files can be extracted again with UNPACK into a folder of your choice.
```
public async static Task PackAsync(
    string[] filepathlist, string archivepath, CompressionType compressiontype,
    int buffersize = 81920, CompressionLevel compresslevel = CompressionLevel.Optimal) =>
      await PackAsync(filepathlist, archivepath, (byte)compressiontype, buffersize, compresslevel);

public async static Task PackAsync(
    string[] filepathlist, string archivepath, byte compressiontype,
    int buffersize = 81920, CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
      var archiv = await ServicesCompress.CheckFCPFileExtensionAsync(archivepath);
      await using var fsout = new FileStream(archiv.FullName, FileMode.Create, FileAccess.Write);

      foreach (var src in filepathlist)
      {
          await using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
          await WriteFileAsync(fsin, fsout, src, compressiontype, buffersize, compresslevel);
      }

      Console.WriteLine();
  }
```
```
public async static Task UnPackAsync(string archivepath, string outputfolder)
{
  ServicesCompress.DeleteFolder(outputfolder, true);
  Directory.CreateDirectory(outputfolder);
  await using var fsin = new FileStream(archivepath, FileMode.Open, FileAccess.Read);
  await ReadFilesAsync(fsin, outputfolder);
}
```

## Strengths

**Clear structure:** Separation between compression logic (Brotli/GZip) and test projects is clean and comprehensible.

**Robust implementation:** Methods such as TryCompressBrotli and TryDecompressBrotli use CancellationToken, error handling, and dynamic buffer sizes. Ultimately, however, it can be said that each individual method has been carefully thought out with the aim of stability and performance.

**Cross-language examples:** C# and VB.NET tests are included to make the project more accessible to other DotNet developers.

**Audit-friendly:** When designing this project, I focused on transparency and effectiveness. All methods have been developed with the aim of being very lean but still DotNet-compliant. This is also the case with FileCompressPackage, which is simple but still does exactly what an archive format requires. 

## Console Output

And this is what the console output looks like:

![](https://github.com/michelenatale/Compress/blob/main/TestBrotliDotNet/Documentation/ConsolOutput.png)

