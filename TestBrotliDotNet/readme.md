## Brotli and GZip from DotNet in Test.
[Brotli](https://en.wikipedia.org/wiki/Brotli) and [GZip](https://en.wikipedia.org/wiki/Gzip) are both compression methods for data. Both are implemented in the .Net Framework Core. Both compression methods can compress text as well as bytes, and are also used for sending data over the Internet, i.e., from one web server to another. 

Incidentally, both methods are based on LZ77 and Huffman coding. LZ77 was developed in 1977 by Abraham Lempel and Jacob Ziv. It is based on the dictionary principle, which has always proven to be extremely effective in compression. Compression rates of up to 90% can be achieved in some cases.

## Console Output

And this is what the console output looks like:

![](https://github.com/michelenatale/Compress/blob/main/TestBrotliDotNet/Documentation/ConsolOutput.png)

