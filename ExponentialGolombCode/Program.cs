﻿



using System.Text;

namespace EGCTest;

using michele.natale.Compress.ExpGolombCodes;
using michele.natale.Compress.MinDiffScallers;

public class Program
{
  public static void Main()
  {
    //Important.
    //Randomly created alpha texts are very difficult to compress.
    //Compression is only effective if different characters are
    //present several times in a text.

    //Wichtig.
    //Zufällig erzeugte Alphatexte sind sehr schwer zu komprimieren.
    //Eine Komprimierung ist nur effektiv, wenn verschiedene Zeichen
    //mehrmals in einem Text vorkommen.

    TestEgc();
    TestMinDiffScaller();

    Console.WriteLine();
    Console.WriteLine("FINISH");
    Console.ReadLine();
  }

  private static void TestMinDiffScaller()
  {

    Console.WriteLine(nameof(TestMinDiffScaller));
    Console.WriteLine("**************");

    var rand = Random.Shared;

    var sz = rand.Next(0, 32); //2^5 = 32
    var bytes = Enumerable.Range(0, sz).Select(x => (byte)x).ToArray();

    var enc = MinDiffScaller.Encode(bytes);
    var dec = MinDiffScaller.Decode(enc);
    Console.WriteLine($"compress = {100 - (100.0 / bytes.Length * enc.Length)}% ");

    sz = rand.Next(32, 256);
    bytes = Enumerable.Range(0, sz).Select(x => (byte)x).ToArray();

    enc = MinDiffScaller.DiffEncode(bytes);
    dec = MinDiffScaller.DiffDecode(enc);
    Console.WriteLine($"compress diff = {100 - (100.0 / bytes.Length * enc.Length)}% ");

    Console.WriteLine();
  }

  private static void TestEgc()
  {
    Console.WriteLine($"© ExponentialGolombCode 2025 - created by © Michele Natale 2025");
    Console.WriteLine("***************************************************************\n");

    EGC.StartEGC();
    TestLittleString();
  }

  private static void TestLittleString()
  {
    //Bitte beachten, das diese Kompression aus Performence-
    //Gründen nur für Längen bis ca. 3000 Byte konzipiert ist.

    //Please note that this compression is only designed for
    //lengths up to approx. 3000 bytes for performance reasons.


    Console.WriteLine(nameof(TestLittleString));
    Console.WriteLine("****************");

    var rand = Random.Shared;

    if (!EGC.Isready) EGC.StartEGC();

    var plain = "0123456789 "u8;
    plain = Mult(plain, 10);
    var encode = EgcCompress.ToEgc(plain);
    Console.WriteLine($"plain = {Encoding.UTF8.GetString(plain)}");
    Console.WriteLine($"Egc compress = {100.0 - (100.00 / plain.Length * encode.Length)}%\n");

    var decode = EgcCompress.FromEgc(encode);

    plain = "Mississippi"u8;
    plain = Mult(plain, 10);
    encode = EgcCompress.ToEgc(plain);
    Console.WriteLine($"plain = {Encoding.UTF8.GetString(plain)}");
    Console.WriteLine($"Egc compress = {100.0 - (100.00 / plain.Length * encode.Length)}%\n");

    decode = EgcCompress.FromEgc(encode);

    plain = "Michele Natale"u8;
    plain = Mult(plain, 10);
    encode = EgcCompress.ToEgc(plain);
    Console.WriteLine($"plain = {Encoding.UTF8.GetString(plain)}");
    Console.WriteLine($"Egc compress = {100.0 - (100.00 / plain.Length * encode.Length)}%\n");

    decode = EgcCompress.FromEgc(encode);



    int s = 0, l = int.MaxValue;
    plain = File.ReadAllBytes("test.txt");
    while (s + l > plain.Length)
    {
      s = rand.Next(0, plain.Length);
      l = rand.Next(150, 513);
    }

    plain = plain.Slice(s, l);
    encode = EgcCompress.ToEgc(plain);

    Console.WriteLine("**** Text-Compress **** **** **** **** **** **** **** ");
    Console.WriteLine($"plain = {Encoding.UTF8.GetString(plain.Slice(0, 50))} [... Only a part of the string is output ...]");
    Console.WriteLine($"Egc compress = {100.0 - (100.00 / plain.Length * encode.Length)}%");
    Console.WriteLine("**** Text-Compress **** **** **** **** **** **** **** \n");

    decode = EgcCompress.FromEgc(encode);



    var alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 "u8;

    //Important.
    //Randomly created alpha texts are very difficult to compress.
    //Compression is only effective if different characters are
    //present several times in a text.

    var length = rand.Next(150, 257);
    //var length = rand.Next(1000, 3073);
    plain = rand.GetItems(alpha, length);

    encode = EgcCompress.ToEgc(plain);

    Console.WriteLine("**** Randomly **** **** **** **** **** **** **** ");
    Console.WriteLine($"plain = {Encoding.UTF8.GetString(plain)}");
    Console.WriteLine($"Egc compress = {100.0 - (100.00 / plain.Length * encode.Length)}%");
    Console.WriteLine("**** Randomly **** **** **** **** **** **** **** \n");

    decode = EgcCompress.FromEgc(encode);

    Console.WriteLine();
  }


  private static byte[] Mult(ReadOnlySpan<byte> bytes, int mult)
  {
    var length = bytes.Length;
    var input = bytes.ToArray();
    var result = new byte[bytes.Length * mult];
    for (int i = 0; i < mult; i++)
      Array.Copy(input, 0, result, i * length, length);
    return result;
  }


  private static byte[] RngBytes(int size)
  {
    var rand = Random.Shared;
    var bytes = new byte[size];
    rand.NextBytes(bytes);
    return bytes;
  }

  private static byte[] RngBytes(int size, byte min, byte max)
  {
    var rand = Random.Shared;
    var bytes = new byte[size];
    for (int i = 0; i < size; i++)
      bytes[i] = (byte)rand.Next(min, max);
    return bytes;
  }
}