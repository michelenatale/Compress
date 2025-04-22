# Compress

## Huffman Coding
The [Huffman Code](https://en.wikipedia.org/wiki/Huffman_coding) is the typical classic among compression codes, as this is an algorithm for lossless data compression. The C# code has a simple structure and fulfills the very fast derivation of the Huffman code, as well as the decoding. It's worth taking a look.


## Applying the Huffman Coding:
Bytes are always used. If a text is to be compressed, it must always be converted to bytes first. It is best to use UTF8, as this also works with Unicode without any problems.

There is a [test file](https://github.com/michelenatale/Compress/blob/main/HuffmanCoding/MyHuffman/Program.cs) available that shows how to use the Huffman encoding.

Here is a little code for a very simple compression and decompression:
```
//Convert text to UTF8
var message = "Mississippi"u8.ToArray();

//A new instance of the huffman class 
var huffman = new Huffman(message);

//Compress and Decompress
var compress = huffman.Encode();
var decompress = hf.Decode(compress);

//Convert decompress from Bytes to Text
var newmessage = Encoding.UTF8.GetString(decompress);

```
