


using System.Runtime.CompilerServices;

namespace michele.natale.Compress.ExpGolombCodes;

using MinDiffScallers;

/// <summary>
/// Provides tools such as Encode and Decode for 
/// compression using Exponential Golom Code.
/// <para>Created by <see href="https://github.com/michelenatale">© Michele Natale 2025</see></para>  
/// </summary>
public class EgcCompress
{

  /// <summary>
  /// Encodes or compresses a byte array using the EGC algorithm. 
  /// <para>Encodes or compresses a byte array using the EGC algorithm. 
  /// EGC is used here to create a prefix-free unary code that can be 
  /// implemented very efficiently for subsequent decoding.</para>
  /// <para>Created by <see href="https://github.com/michelenatale">© Michele Natale 2025</see></para>  
  /// </summary>
  /// <param name="bytes">Desired array of byte</param>
  /// <returns>Array of byte</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte[] ToEgc(ReadOnlySpan<byte> bytes)
  {
    //Das unäre präfixfreie Beispiel der Exponential-Golomb coding.

    //The unary prefix-free example of exponential golomb coding.

    if (!EGC.Isready) EGC.StartEGC();

    var score = Assessment(bytes);
    //var dbytes = FromDict(score);
    var dbytes = MinDiffScaller.DiffEncode(FromDict(score));
    var gc = ExpGolomnEncode(bytes, score);

    var result = new byte[gc.Length + dbytes.Length + 1];
    result[0] -= (byte)dbytes.Length;
    Array.Copy(gc, 0, result, 1, gc.Length);
    Array.Copy(dbytes, 0, result, gc.Length + 1, dbytes.Length);
    return result;
  }


  /// <summary>
  /// Decodes the previously created compression code 
  /// back into the original plain text.
  /// <para>Created by <see href="https://github.com/michelenatale">© Michele Natale 2025</see></para>  
  /// </summary>
  /// <param name="bytes"></param>
  /// <returns></returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte[] FromEgc(ReadOnlySpan<byte> bytes)
  {
    if (!EGC.Isready) EGC.StartEGC();

    var fzc = bytes[1];
    var length = bytes.Length;
    var cb = (byte)(-bytes[0]);
    //var dbytes = MinScaller.Decode(bytes[(length - cb)..]);
    var score = ToDict(MinDiffScaller.DiffDecode(bytes[(length - cb)..]));
    return ExpGolomnDecode(bytes[2..(length - cb)], score, fzc);
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static Dictionary<byte, byte> Assessment(ReadOnlySpan<byte> bytes)
  {
    var qualities = Enumerable.Range(0, 256).Select(val => ((byte)val, 0)).ToArray();
    foreach (var itm in bytes)
      qualities[itm].Item2++;

    qualities = [.. qualities.OrderByDescending(x => x.Item2)];

    var length = qualities.Length;
    var result = new Dictionary<byte, byte>();
    for (var i = 0; i < length; i++)
      if (qualities[i].Item2 == 0) break;
      else result[qualities[i].Item1] = (byte)i;

    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static Dictionary<byte, byte> ToDict(ReadOnlySpan<byte> bytes)
  {
    var copy = bytes.ToArray();
    var result = new Dictionary<byte, byte>();
    for (var i = 0; i < copy.Length; i++)
      result[(byte)i] = copy[i];

    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static byte[] FromDict(Dictionary<byte, byte> input)
  {
    //I will think about how I can make this array even smaller.
    //For the moment, however, I'll leave it as it is.

    var bytes = new List<byte>();
    var dict = input.ToDictionary(x => x.Value, x => x.Key);
    var keys = dict.Keys.OrderBy(x => x).ToArray();
    foreach (var key in keys)
      bytes.Add(dict[key]);

    return [.. bytes];
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static byte[] ExpGolomnEncode(
    ReadOnlySpan<byte> bytes,
    Dictionary<byte, byte> score)
  {
    var gc = EGC.ExpGolombCode;
    var result = new List<byte>(8 * bytes.Length);
    foreach (var itm in bytes)
    {
      var bits = gc(score[itm]);
      result.AddRange(bits);
    }

    var cnt = 0;
    while (result[cnt++] == 0) ; cnt--;

    byte[] first = [(byte)cnt];
    return [.. first, .. Converter([.. result], 2, 256)];
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static byte[] ExpGolomnDecode(
    ReadOnlySpan<byte> bytes,
    Dictionary<byte, byte> score,
    int firstzeroscount)
  {
    var gc = EGC.ExpGolombCodeR;
    var fzc = Enumerable.Repeat<byte>(0, firstzeroscount);
    ReadOnlySpan<byte> span = fzc.Concat(Converter(bytes, 256, 2)).ToArray();

    var start = 0;
    var result = new List<byte>(span.Length);
    for (var i = 1; i <= span.Length; i++)
    {
      var code = span.Slice(start, i - start);
      var num = gc(code);
      if (num >= 0)
      {
        result.Add(score[(byte)num]);
        start = i;
      }
    }

    return [.. result];
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static byte[] Converter(
    ReadOnlySpan<byte> data, int startbase, int targetbase)
  {
    if (data.Length == 0) return new byte[1];
    var cap = Convert.ToInt32(
      data.Length * Math.Log(startbase) /
        Math.Log(targetbase)) + 1;

    var exit = true;
    byte accumulator, remainder;
    Span<byte> input = data.ToArray();
    var result = new Stack<byte>(cap);
    while (exit)
    {
      remainder = 0; exit = false;
      for (int i = 0; i < input.Length; i++)
      {
        var value = startbase * remainder;
        accumulator = (byte)((value + input[i]) % targetbase);
        input[i] = (byte)((value + input[i]) / targetbase);
        remainder = accumulator;
        if (input[i] > 0) exit = true;
      }
      result.Push(remainder);
    }

    return [.. result];
  }
}
