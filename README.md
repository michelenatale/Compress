# Compress

## Huffman Coding
The [Huffman Code](https://en.wikipedia.org/wiki/Huffman_coding) is the typical classic among compression codes, as this is an algorithm for lossless data compression. The C# code has a simple structure and fulfills the very fast derivation of the Huffman code, as well as the decoding. It's worth taking a look.

https://github.com/michelenatale/Compress/tree/main/HuffmanCoding

## Exponential-Golomb-Codes
[Exponential-Golomb-Codes](https://en.wikipedia.org/wiki/Exponential-Golomb_coding) (also known as Exp-Golomb-Coding) represent integers with bit patterns that become longer for larger numbers. One would think. The exponential Golomb code created here performs an analysis beforehand and adjusts the length of the code to the frequency.

https://github.com/michelenatale/Compress/tree/main/ExponentialGolombCode


## Best Practice: Brotli and GZip from DotNet in Test (Async-Await).
[Brotli](https://en.wikipedia.org/wiki/Brotli) and [GZip](https://en.wikipedia.org/wiki/Gzip) are both compression methods for data. Both are implemented in the .Net Framework Core. Both compression methods can compress text as well as bytes, and are also used for sending data over the Internet, i.e., from one web server to another. 

Incidentally, both methods are based on [LZ77](https://en.wikipedia.org/wiki/LZ77_and_LZ78#LZ77) and [Huffman Coding](https://en.wikipedia.org/wiki/Huffman_coding). LZ77 was developed in 1977 by [Abraham Lempel](https://en.wikipedia.org/wiki/Abraham_Lempel) and [Jacob Ziv](https://en.wikipedia.org/wiki/Jacob_Ziv). It is based on the [dictionary principle](https://en.wikipedia.org/wiki/Dictionary_coder), which has always proven to be extremely effective in compression. Compression rates of up to 90% can be achieved in some cases.

I subsequently added a very slim, simple, and fast archiving program (similar to TAR, ZIP, GZ) called “FileCompressPackage,” but with minimal functionality.

https://github.com/michelenatale/Compress/tree/main/TestBrotliDotNet
