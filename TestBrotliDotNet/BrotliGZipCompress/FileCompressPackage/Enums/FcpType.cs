

namespace michele.natale.Compresses;


public enum FcpType : byte
{
  /// <summary>
  /// The file data is compressed using the Brotli algorithm.
  /// </summary>
  File = 1,

  /// <summary>
  /// The file data is compressed using the GZip algorithm.
  /// </summary>
  Archiv = 2,
}