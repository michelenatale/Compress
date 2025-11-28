

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
  public int NameLength;                // Länge des Dateinamens (UTF-8)
  public long CompressedLength;         // Länge der komprimierten Daten
  public long OriginalLength;           // Länge der Originaldatei
  public byte CompressionType;          // 1 = Brotli, 2 = GZip
  public byte FCP_Type;                 // 1 = File, 2 = Archiv

  public long CreationTimeUtcTicks;     // Erstellungsdatum der Datei
  public long LastWriteTimeUtcTicks;    // Datum der letzten Änderung
  public long LastAccessTimeUtcTicks;   // Datum der letzten Speicherung  
}
