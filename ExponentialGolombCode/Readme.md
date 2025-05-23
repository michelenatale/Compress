# Exponential Golomb Codes 

The “ExpGolombCode” created here represents integers as bit patterns, whereby the shorter codes are adapted to the frequency of the numerical values. 

The EGCode is also calculated prefix-free and unary by the algorithm created here and is therefore used for efficient and effective compression. 

It is a very simple and effective method that is very easy for anyone (programmers, mathematicians, etc.) to understand.

## Applying the Exponential Golomb Codes:
There is a [test file](https://github.com/michelenatale/Compress/blob/main/ExponentialGolombCode/Program.cs) that shows how to use Exponential Golomb Codes.

Here is a little bit of code for compressing and decompressing:: 
```
public static byte[] ToEgc(ReadOnlySpan<byte> bytes)
{
  //Announcement of the EGC class
  if (!EGC.Isready) EGC.StartEGC();

  //Brief analysis of the plain text.
  var score = Assessment(bytes); 

  //Coding using a dictionary
  var dbytes = FromDict(score);

  //The actual compression
  var gc = ExpGolomnEncode(bytes, score);

  ....
}
```
```
public static byte[] FromEgc(ReadOnlySpan<byte> bytes)
{
  //Announcement of the EGC class
  if (!EGC.Isready) EGC.StartEGC();

  //Get coding
  var fzc = bytes[1];
  var length = bytes.Length;

  //Get coding
  var cb = (byte)(-bytes[0]);

  //Decoding using a dictionary
  var score = ToDict(bytes[(length - cb)..]);

  //The actual decompression
  return ExpGolomnDecode(bytes[2..(length - cb)], score, fzc);
}
```


