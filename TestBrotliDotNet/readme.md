## Best Practice: Brotli and GZip from DotNet in Test.
[Brotli](https://en.wikipedia.org/wiki/Brotli) and [GZip](https://en.wikipedia.org/wiki/Gzip) are both compression methods for data. Both are implemented in the .Net Framework Core. Both compression methods can compress text as well as bytes, and are also used for sending data over the Internet, i.e., from one web server to another. 

Incidentally, both methods are based on LZ77 and Huffman coding. LZ77 was developed in 1977 by Abraham Lempel and Jacob Ziv. It is based on the dictionary principle, which has always proven to be extremely effective in compression. Compression rates of up to 90% can be achieved in some cases.

## Applying the Exponential Golomb Codes:
There is a [test file C#](https://github.com/michelenatale/Compress/blob/main/TestBrotliDotNet/TestBrotliGZip/CompressedNetTest.cs) and a [test file vb.net](https://github.com/michelenatale/Compress/blob/main/TestBrotliDotNet/TestBrotliGZipVB/CompressedNetTest.vb) that shows how to use Exponential Golomb Codes.

Here is a small piece of code for compressing and decompressing. Of course, all methods are documented to explain why it is done this way: 
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

## Console Output

And this is what the console output looks like:

![](https://github.com/michelenatale/Compress/blob/main/TestBrotliDotNet/Documentation/ConsolOutput.png)

