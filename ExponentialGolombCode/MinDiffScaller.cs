

using System.Data;
using System.Runtime.CompilerServices;

namespace michele.natale.Compress.MinDiffScallers;


/// <summary>
/// Provides tools for MinDiffScaller compression.
/// <para>Created by <see href="https://github.com/michelenatale">© Michele Natale 2025</see></para>  
/// </summary>
public class MinDiffScaller
{
  //Is a proprietary construction, and is used to
  //compress at bit level by means of conversion
  //via the number system. 
  // ==> Only for small strings

  /// <summary>
  /// Encodes or compresses a byte array using the 'MinDiffScaller' 
  /// algorithm using difference amounts. 
  /// <para>Created by <see href="https://github.com/michelenatale">© Michele Natale 2025</see></para>  
  /// </summary>
  /// <param name="bytes">Desired Array of byte for Coding</param>
  /// <returns></returns>

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte[] DiffEncode(ReadOnlySpan<byte> bytes)
  {
    byte a = 0;
    var deltas = new byte[bytes.Length];
    for (var i = 0; i < bytes.Length; i++)
    {
      deltas[i] = (byte)(bytes[i] - a);
      a = bytes[i];
    }

    return Encode(deltas);
  }

  /// <summary>
  /// Decodes the previously created compression code 
  /// back into the original plain text using difference amounts.
  /// <para>Created by <see href="https://github.com/michelenatale">© Michele Natale 2025</see></para>  
  /// </summary>
  /// <param name="deltas">Desired Code as array of byte</param>
  /// <returns></returns>

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte[] DiffDecode(ReadOnlySpan<byte> deltas)
  {
    byte a = 0;
    var bytes = Decode(deltas);
    var result = new byte[bytes.Length];
    for (var i = 0; i < result.Length; i++)
    {
      result[i] = a = (byte)(a + bytes[i]); 
    }

    return result;
  }

  /// <summary>
  /// Encodes or compresses a byte array using the 'MinDiffScaller' algorithm. 
  /// <para>Created by <see href="https://github.com/michelenatale">© Michele Natale 2025</see></para>  
  /// </summary>
  /// <param name="bytes">Desired Array of byte for Coding</param>
  /// <returns></returns>

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte[] Encode(ReadOnlySpan<byte> bytes)
  {
    var min = Minimum(bytes);
    var length = bytes.Length;
    var tmp = bytes.ToArray();

    if (min > 0)
      for (var i = 0; i < length; i++)
        tmp[i] -= min;

    var first = new byte[] { 2 };
    var result = new List<byte>();
    result.AddRange(first.Concat(Convert
            .ToString(min, 2).Select(x => (byte)(x - 48))));

    foreach (var itm in tmp)
      result.AddRange(first.Concat(Convert
        .ToString(itm, 2).Select(x => (byte)(x - 48))));

    return Converter(result.ToArray(), 3, 256);
  }

  /// <summary>
  /// Decodes the previously created compression code 
  /// back into the original plain text.
  /// <para>Created by <see href="https://github.com/michelenatale">© Michele Natale 2025</see></para>  
  /// </summary>
  /// <param name="deltas">Desired Code as array of byte</param>
  /// <returns></returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte[] Decode(ReadOnlySpan<byte> bytes)
  {
    var tmp = Converter(bytes, 256, 3);

    var result = Splitter(tmp);

    var min = result[0];
    var length = result.Length;
    if (min > 0)
      for (var i = 1; i < length; i++)
        result[i] += min;

    return result.AsSpan(1).ToArray();
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static byte[] Splitter(ReadOnlySpan<byte> bytes)
  {
    var length = bytes.Length;
    var idxs = new List<int>();

    if (bytes[0] != 2)
      idxs.Add(-1);

    for (var i = 0; i < length; i++)
      if (bytes[i] == 2) idxs.Add(i);

    if (bytes[^1] != 2)
      idxs.Add(length);

    length = idxs.Count;
    var result = new List<byte>();
    for (var i = 1; i < length; i++)
    {
      var idx2 = idxs[i];
      var idx1 = idxs[i - 1];
      var code = bytes.Slice(idx1 + 1, idx2 - idx1 - 1).ToArray();
      result.Add(Convert.ToByte(string.Join("", code), 2));
    }

    return [.. result];
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static byte Minimum(ReadOnlySpan<byte> bytes)
  {
    var result = byte.MaxValue;
    foreach (var itm in bytes)
      if (itm < result) result = itm;
    return result;
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
