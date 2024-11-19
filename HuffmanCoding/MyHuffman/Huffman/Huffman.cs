
using System.Text; 


namespace michele.natale.Compresses.Huffman;


/// <summary>
/// Provides tools that are necessary for the 
/// power fast generation and decoding of Huffman code. 
/// </summary>
public class Huffman
{
  private Dictionary<byte, string> Codes = [];

  /// <summary>
  /// Data provided for the Huffman code.
  /// </summary>
  public byte[] Data { get; private set; } = [];

  /// <summary>
  /// C-Tor
  /// </summary>
  public Huffman()
  {
  }

  /// <summary>
  /// C-Tor
  /// </summary>
  /// <param name="bytes"></param>
  public Huffman(ReadOnlySpan<byte> bytes)
  {
    this.Data = bytes.ToArray();
    var groups = bytes.ToArray().GroupBy(x => x)
      .OrderBy(x => x.Count()).ThenBy(x => x.Key);

    var pq = new PriorityQueue<HNode, int>();
    var nodes = groups.Select(x => new HNode(x.Key.ToString(), x.Count(), null!, null!));
    foreach (var itm in nodes) pq.Enqueue(itm, itm.Frequency);
 
    while (pq.Count > 1)
    {
      var a = pq.Dequeue();
      var b = pq.Dequeue();
      if (a.Frequency > b.Frequency) (a, b) = (b, a);
      var id = new StringBuilder(a.Id.ToString());
      id.Append(b.Id.ToString());
      var freq = a.Frequency + b.Frequency;
      pq.Enqueue(new HNode(id, freq, a, b), freq);
    } 

    SetCodes(pq.Dequeue(), string.Empty, nodes.Count()); 
  }

  /// <summary>
  /// Returns the HuffmanCode (compressed as bytes) for the current data.
  /// </summary>
  /// <returns>Array of Byte</returns>
  /// <exception cref="ArgumentNullException"></exception>
  public byte[] Encode()
  {
    if (this.Data is null || this.Data.Length == 0)
      throw new ArgumentNullException(nameof(this.Data), $"{nameof(this.Data)}");

    var bytes = new List<byte>();
    var bits = EncodeBitStr().Chunk(8).ToArray();
    var last = (byte)bits.Last().Length;
    foreach (var itm in bits)
      bytes.Add(Convert.ToByte(string.Join("", itm).PadLeft(8, '0'), 2));

    bytes.Add(last);
    var serialize = SerializerCodes();
    var result = new byte[bytes.Count + serialize.Length + 2];

    Array.Copy(bytes.ToArray(), 0, result, 2, bytes.Count);
    Array.Copy(serialize, 0, result, bytes.Count + 2, serialize.Length);
    result[0] = (byte)(serialize.Length / 256);
    result[1] = (byte)(serialize.Length % 256);

    return result;
  }


  /// <summary>
  /// Returns the HuffmanCode for the current data.
  /// </summary>
  /// <returns>Returns the HuffmanCode for the current data.</returns>
  public string EncodeBitStr()
  {
    var result = new StringBuilder();
    foreach (var itm in Data)
      result.Append(this.Codes[itm]);
    return result.ToString();
  }

  /// <summary>
  /// Decodes the HuffmanCode
  /// </summary>
  /// <param name="bits">desired HuffmanCode in Bits</param>
  /// <returns>Array of Byte</returns>
  /// <exception cref="Exception"></exception>
  public byte[] Decode(string bits)
  {
    int start = 0, offset = 1;
    var result = new List<byte>();
    var c = Codes.ToDictionary(x => x.Value, y => y.Key);
    while (start + offset <= bits.Length)
      if (c.TryGetValue(bits.ToString().Substring(start, offset++), out byte v))
      {
        start = start + offset - 1;
        offset = 1;
        result.Add(v);
      }

    if (!result.SequenceEqual(this.Data))
      throw new Exception();

    return [.. result];
  }

  /// <summary>
  /// Decodes the HuffmanCode
  /// </summary>
  /// <param name="enc">Desired HuffmanCode</param>
  /// <returns>Array of Byte</returns>
  public byte[] Decode(ReadOnlySpan<byte> enc)
  {
    var l = enc[0] * 256 + enc[1];
    DeserializerCodes(enc.Slice(enc.Length - l, l).ToArray());

    var bytes = new byte[enc.Length - 2 - l];
    Array.Copy(enc.ToArray(), 2, bytes, 0, bytes.Length);

    var last = bytes[^2];
    var end = bytes.Last();
    var bits = new StringBuilder();
    for (int i = 0; i < bytes.Length - 2; i++)
      bits.Append(Convert.ToString(bytes[i], 2).PadLeft(8, '0'));

    bits.Append(Convert.ToString(last, 2).PadLeft(8, '0').AsSpan(8 - end, end));

    int start = 0, offset = 1;
    var sbits = bits.ToString();
    var result = new List<byte>(bits.Length / 4);
    var c = Codes.ToDictionary(x => x.Value, y => y.Key);
    while (start + offset <= bits.Length)
      //source: http://blog.lab49.com/archives/840
      if (c.ContainsKey(sbits.Substring(start, offset++)))
      {
        result.Add(c[sbits.Substring(start, offset - 1)]);
        start = start + offset - 1;
        offset = 1;
      }
    //if (c.TryGetValue(sbits.Substring(start, offset++), out byte v))
    //{
    //  start = start + offset - 1;
    //  offset = 1;
    //  result.Add(v);
    //}

    return [.. result];

  }


  private void SetCodes(HNode htree, string bits, int nodes_cnt)
  {

    if (nodes_cnt == 1)
      this.Codes.Add(byte.Parse(htree.Id.ToString()), "0");

    else if (htree.Left == null && htree.Right == null)
      this.Codes.Add(byte.Parse(htree.Id.ToString()), bits);

    else
    {
      if (htree.Left != null)
        SetCodes(htree.Left, bits + "0", nodes_cnt);

      if (htree.Right != null)
        SetCodes(htree.Right, bits + "1", nodes_cnt);
    }
  }


  private byte[] SerializerCodes()
  {
    var sb = new StringBuilder("[");
    sb.Append(string.Join(";", this.Codes.Keys));
    sb.Append("]-[");
    sb.Append(string.Join(";", this.Codes.Values));
    sb.Append("]");
    return Encoding.UTF8.GetBytes(sb.ToString());
  }


  private void DeserializerCodes(byte[] bytes)
  {
    var str = Encoding.UTF8.GetString(bytes)
      .Split('-', StringSplitOptions.RemoveEmptyEntries)
      .Select(x => x.Split("[];".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));

    var length = str.First().Length;
    this.Codes = new Dictionary<byte, string>();
    for (var i = 0; i < length; i++)
      this.Codes[byte.Parse(str.First()[i])] = str.Last()[i];
  }


  /// <summary>
  /// Private simple node class for handling the Huffman tools.
  /// </summary>
  private class HNode
  {
    public int Frequency = 0;
    public HNode Left = null!;
    public HNode Right = null!;
    public StringBuilder Id = null!;

    /// <summary>
    /// C-Tor
    /// </summary>
    /// <param name="id">Desired Id</param>
    /// <param name="frequence">Desired frequence</param>
    /// <param name="left">Desired Node left</param>
    /// <param name="right">Desired Node right</param>
    public HNode(string id, int frequence, HNode left, HNode right)
    {
      this.Left = left;
      this.Right = right;
      this.Frequency = frequence;
      this.Id = new StringBuilder(id);
    }

    /// <summary>
    /// C-Tor
    /// </summary>
    /// <param name="id">Desired Id</param>
    /// <param name="frequence">Desired frequence</param>
    /// <param name="left">Desired Node left</param>
    /// <param name="right">Desired Node right</param>
    public HNode(StringBuilder id, int frequence, HNode left, HNode right)
    {
      this.Id = id;
      this.Left = left;
      this.Right = right;
      this.Frequency = frequence;
    }
  }
}

