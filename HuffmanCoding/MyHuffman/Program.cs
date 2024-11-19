
 
using System.Text;
using System.Diagnostics;


namespace HuffmanTest;


using michele.natale.Compresses.Huffman;


public class Program
{
  public static void Main()
  {
    TestCompress();

    TestCompressWitNewInstance();

    TestBits();

    TestRandoms();

    Console.WriteLine();
    Console.WriteLine("Finish");
    Console.ReadLine();
  }

  private static void TestCompress()
  {
    var sw = Stopwatch.StartNew();

    var message = "Mississippi"u8.ToArray();
    var hf = new Huffman(message);
    var enc = hf.Encode();
    var dec = hf.Decode(enc);
    var txt = Encoding.UTF8.GetString(dec);
    sw.Stop();

    if (!message.SequenceEqual(dec))
      throw new Exception();

    Console.WriteLine($"Huffman {nameof(TestCompress)}_I: length = {message.Length}; t = {sw.ElapsedMilliseconds}ms.");


    sw = Stopwatch.StartNew();
    //From 6 repetitions there is a compression
    message = Encoding.UTF8.GetBytes(Repeat("Mississippi", 6));
    hf = new Huffman(message);
    enc = hf.Encode();
    dec = hf.Decode(enc);
    txt = Encoding.UTF8.GetString(dec);
    sw.Stop();
    if (!message.SequenceEqual(dec))
      throw new Exception();

    if (enc.Length >= message.Length)
      throw new Exception();

    Console.WriteLine($"Huffman {nameof(TestCompress)}_II: length = {message.Length}; t = {sw.ElapsedMilliseconds}ms.\n");
  }


  private static void TestCompressWitNewInstance()
  {
    var sw = Stopwatch.StartNew();
    var message = "Mississippi"u8.ToArray();
    var hf = new Huffman(message);
    var enc = hf.Encode();

    hf = new Huffman();//Here a new Instance
    var dec = hf.Decode(enc);
    var txt = Encoding.UTF8.GetString(dec);

    if (!message.SequenceEqual(dec))
      throw new Exception();
    sw.Stop();

    Console.WriteLine($"Huffman {nameof(TestCompressWitNewInstance)}_I: length = {message.Length}; t = {sw.ElapsedMilliseconds}ms.");

    sw = Stopwatch.StartNew();
    //From 6 repetitions there is a compression
    message = Encoding.UTF8.GetBytes(Repeat("Mississippi", 6));
    hf = new Huffman(message);
    enc = hf.Encode();

    hf = new Huffman();//Here a new Instance
    dec = hf.Decode(enc);
    txt = Encoding.UTF8.GetString(dec);
    sw.Stop();
    if (!message.SequenceEqual(dec))
      throw new Exception();

    if (enc.Length >= message.Length)
      throw new Exception();

    Console.WriteLine($"Huffman {nameof(TestCompressWitNewInstance)}_II: length = {message.Length}; t = {sw.ElapsedMilliseconds}ms.\n");
  }

  private static void TestBits()
  {
    //Only Bits

    var sw = Stopwatch.StartNew();
    var message = "Mississippi"u8.ToArray();
    var hf = new Huffman(message);

    var bits = hf.EncodeBitStr();
    var decb = hf.Decode(bits);
    sw.Stop();
    if (!message.SequenceEqual(decb))
      throw new Exception();

    Console.WriteLine($"Huffman {nameof(TestBits)}_I: length = {message.Length}; t = {sw.ElapsedMilliseconds}ms.");

    sw = Stopwatch.StartNew();
    message = Encoding.UTF8.GetBytes(Repeat("Mississippi", 10));
    hf = new Huffman(message);
    bits = hf.EncodeBitStr();
    decb = hf.Decode(bits);
    sw.Stop();

    if (!message.SequenceEqual(decb))
      throw new Exception();

    Console.WriteLine($"Huffman {nameof(TestBits)}_II: length = {message.Length}; t = {sw.ElapsedMilliseconds}ms.\n");

  }

  private static void TestRandoms()
  {
    //Note: Randomizations are always difficult to compress.
    //      English text, on the other hand, can be compressed very well.

    var rand = Random.Shared;

    var sw = Stopwatch.StartNew();
    var message = new byte[rand.Next(1000, 100_000)];
    rand.NextBytes(message);

    var hf = new Huffman(message);

    var bits = hf.EncodeBitStr();
    var decb = hf.Decode(bits);
    sw.Stop();

    if (!message.SequenceEqual(decb))
      throw new Exception();

    Console.WriteLine($"Huffman {nameof(TestRandoms)}_I: length = {message.Length}; t = {sw.ElapsedMilliseconds}ms.");

    sw = Stopwatch.StartNew();
    message = new byte[rand.Next(100, 100_000)];
    rand.NextBytes(message);
    hf = new Huffman(message);
    var enc = hf.Encode();

    hf = new Huffman(); //Here a new Instance
    var dec = hf.Decode(enc);
    var txt = Encoding.UTF8.GetString(dec);
    sw.Stop();

    if (!message.SequenceEqual(dec))
      throw new Exception();

    Console.WriteLine($"Huffman {nameof(TestRandoms)}_II: length = {message.Length}; t = {sw.ElapsedMilliseconds}ms.\n");

  }

  private static string Repeat(string str, int cnt, string separator = " ")
  {
    var result = new StringBuilder();
    for (int i = 0; i < cnt; i++)
      if (i < cnt - 1)
        result.Append(str + separator);
      else result.Append(str);
    return result.ToString();
  }
}