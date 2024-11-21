


using System.Text;
using System.Runtime.CompilerServices;

namespace michele.natale.Compresses.Services;


internal class ServiceSerialize
{


  public static byte[] Serialize(Dictionary<byte, string> obj)
  {
    var v = obj.Values.Select(s => s.Select(c => (char)(c + 1)).ToArray()).ToArray();
    var k = obj.Keys.Select(x => Convert.ToString(x, 2).Select(c => (char)(c + 1)).ToArray()).ToArray();

    var str = string.Join("0", k.Select(c => string.Join("", c))) + "0";
    str += string.Join("0", v.Select(c => string.Join("", c)));

    var result = new List<byte>();
    var tmp = str.Select(x => byte.Parse(x.ToString())).ToArray();
    var bits = Converter(tmp, 3, 2).Chunk(8).ToArray();
    var last = (byte)bits.Last().Length;
    foreach (var itm in bits)
      result.Add(Convert.ToByte(string.Join("", itm).PadLeft(8, '0'), 2));

    result.Add(last);
    return result.ToArray();
  }

  public static Dictionary<byte, string> Deserialize(ReadOnlySpan<byte> bytes)
  {

    var end = bytes[^1];
    var last = bytes[^2];
    var bits = new StringBuilder();
    for (int i = 0; i < bytes.Length - 2; i++)
      bits.Append(Convert.ToString(bytes[i], 2).PadLeft(8, '0'));

    bits.Append(Convert.ToString(last, 2).PadLeft(8, '0').AsSpan(8 - end, end));
    var tmp = bits.ToString().Select(x => byte.Parse(x.ToString())).ToArray();

    var strs = string.Join("", Converter(tmp, 2, 3)).Split('0',StringSplitOptions.RemoveEmptyEntries)
        .Select(x=>string.Join("", x.Select(c=>(char)(c-1)))).ToArray();

    var length = strs.Length / 2;
    var result = new Dictionary<byte, string>();
    for (var i = 0; i < length; i++)
      result[Convert.ToByte(strs[i],2)] = strs[i + length]!;

    return result;
  }



  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static byte[] Converter(byte[] data, int startbase, int targetbase)
  {
    if (data.Length == 0) return new byte[1];
    int cap = Convert.ToInt32(data.Length * Math.Log(startbase) / Math.Log(targetbase)) + 1;
    var result = new Stack<byte>(cap);

    byte remainder;
    bool ext = true;
    byte accumulator;
    var input = data.ToArray();
    while (ext)
    {
      remainder = 0; ext = false;
      for (int i = 0; i < input.Length; i++)
      {
        accumulator = (byte)(((startbase * remainder) + input[i]) % targetbase);
        input[i] = (byte)(((startbase * remainder) + input[i]) / targetbase);
        remainder = accumulator;
        if (input[i] > 0) ext = true;
      }
      result.Push(remainder);
    }
    return result.ToArray();
  }

}
