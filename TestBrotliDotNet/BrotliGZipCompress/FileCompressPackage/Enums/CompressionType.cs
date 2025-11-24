

namespace michele.natale.Compresses;

/// <summary>
/// Defines the available compression algorithms for file entries in the archive format.
/// </summary>
/// <remarks>
/// <para>
/// This enumeration is stored as a single byte in the <c>FileInfosHeader</c> structure
/// to indicate which compression method was applied to the file data.
/// </para>
/// <para>
/// The values are aligned with the supported compression streams in .NET:
/// <see cref="System.IO.Compression.BrotliStream"/> and <see cref="System.IO.Compression.GZipStream"/>.
/// </para>
/// </remarks>
public enum CompressionType : byte
{
  /// <summary>
  /// No compression is applied. The file data is stored as raw bytes.
  /// </summary>
  None = 0,

  /// <summary>
  /// The file data is compressed using the Brotli algorithm.
  /// </summary>
  Brotli = 1,

  /// <summary>
  /// The file data is compressed using the GZip algorithm.
  /// </summary>
  GZip = 2,
}