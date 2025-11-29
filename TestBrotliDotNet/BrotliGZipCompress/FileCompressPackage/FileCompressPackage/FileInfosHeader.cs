

using System.Runtime.InteropServices;


namespace michele.natale.Compresses;


/// <summary>
/// Represents the header information for a single file entry in the custom archive format.
/// </summary>
/// <remarks>
/// <para>
/// The structure is laid out sequentially with a packing size of 1 byte to ensure a compact,
/// audit-friendly binary representation. It contains metadata about the file, including
/// compression details and timestamps.
/// </para>
/// <para>
/// This header precedes the actual file data in the archive and allows the decompression
/// routines to correctly interpret the payload.
/// </para>
/// </remarks>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct FileInfosHeader
{
  public int NameLength;                // Length of the file name (UTF-8)
  public long CompressedLength;         // Length of the compressed data
  public long OriginalLength;           // Length of the original file
  public byte CompressionType;          // 1 = Brotli, 2 = GZip
  public byte FCP_Type;                 // 1 = File, 2 = Archiv

  public long CreationTimeUtcTicks;     // Date the file was created
  public long LastWriteTimeUtcTicks;    // Date of last modification
  public long LastAccessTimeUtcTicks;   // Date of last save 
}
