
using System.Runtime.CompilerServices;

namespace michele.natale.Compress.ExpGolombCodes;

/// <summary>
/// Provides tools for compression using Exponential Golom Code.
/// <para>Created by <see href="https://github.com/michelenatale">© Michele Natale 2025</see></para>  
/// </summary>
public class EGC
{

  /// <summary>
  /// Shows whether the basic elements of the EGC are already available.
  /// </summary>
  public static bool Isready { get; private set; } = false;


  /// <summary>
  /// Returns the prefix-free unary EGCode based on the key.
  /// <para>Created by <see href="https://github.com/michelenatale">© Michele Natale 2025</see></para>  
  /// </summary>
  /// <param name="key">Desidered Key</param>
  /// <returns>EGCode as Array of byte</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte[] ExpGolombCode(int key) =>
    Exp_Golomb_Code[key].Select(x => (byte)(x - 48)).ToArray();

  /// <summary>
  /// Returns the designated value based on the prefix-free unary EGCode.
  /// <para>Created by <see href="https://github.com/michelenatale">© Michele Natale 2025</see></para>  
  /// </summary>
  /// <param name="key">Desidered Key</param>
  /// <returns>The intended value.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int ExpGolombCodeR(ReadOnlySpan<byte> key)
  {
    var code = string.Join("", key.ToArray());
    if (Exp_Golomb_Code_R.TryGetValue(code, out var value))
      return value;
    return -1;
  }

  /// <summary>
  /// Holds the key-value from the EGC.
  /// </summary>
  private static Dictionary<int, string> Exp_Golomb_Code { get; set; } = [];

  /// <summary>
  /// Holds the value-key from the EGC.
  /// </summary>
  private static Dictionary<string, int> Exp_Golomb_Code_R { get; set; } = [];

  /// <summary>
  /// The actual entry point for calculating the EGCodes.
  /// <para>Created by <see href="https://github.com/michelenatale">© Michele Natale 2025</see></para>  
  /// </summary>

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void StartEGC()
  {
    if (Isready) return;

    var bytes = Enumerable.Range(0, 256)
      .Select(i => (byte)i).ToArray();

    Exp_Golomb_Code = ToEgc(bytes, 1);
    Exp_Golomb_Code_R = Exp_Golomb_Code.ToDictionary(k => k.Value, v => v.Key);

    Isready = true;
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static Dictionary<int, string> ToEgc(
    ReadOnlySpan<byte> bytes, int k = 0)
  {
    var result = new Dictionary<int, string>();

    foreach (var itm in bytes)
    {
      var b = itm + (1 << k);
      var bl = (int)Math.Log2(b);
      var bits = string.Empty;
      if (bl - k > 0) bits = new string('0', bl - k);
      bits += string.Join("", Convert.ToString(b, 2));
      result[itm] = bits;
    }

    return result;
  }
}
