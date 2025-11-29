



using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace michele.natale.Compresses;


partial class FileCompressPackage
{

  /// <summary>
  /// General entry point for unpacking operations.  
  /// Determines the type of archive (single file or multi-file archive)
  /// based on the header and dispatches to the appropriate unpacking method.
  /// </summary>
  /// <param name="archivepath">
  /// Path to the archive file to be unpacked.
  /// </param>
  /// <param name="outputfolder">
  /// Destination folder where the extracted content will be written.
  /// </param>
  /// <param name="buffersize">
  /// Size of the buffer in bytes used for stream copy operations.
  /// Default: 81920.
  /// </param>
  /// <returns>
  /// A task representing the asynchronous unpacking operation.
  /// </returns>
  /// <exception cref="FileNotFoundException">
  /// Thrown if the archive file does not exist.
  /// </exception>
  /// <remarks>
  /// This method acts as the central dispatcher for unpacking.  
  /// It reads the archive header (<see cref="FileInfosHeader"/>) to determine
  /// whether the archive contains a single file (<see cref="UnPackFileAsync"/>)
  /// or a multi-file archive (<see cref="UnPackArchivAsync"/>), and then calls
  /// the corresponding method.
  /// </remarks>
  /// <example>
  /// Example usage:
  /// <code>
  /// await UnPackAsync("data.fcp", "output");
  /// </code>
  /// </example>
  public async static Task UnPackAsync(
    string archivepath, string outputfolder, int buffersize = 81920)
  {
    await using var fsin = new FileStream(
      archivepath, FileMode.Open, FileAccess.Read);

    var headersize = Unsafe.SizeOf<FileInfosHeader>();
    var headerbuffer = new byte[headersize];
    fsin.ReadExactly(headerbuffer);
    await fsin.DisposeAsync();

    var header = MemoryMarshal.Read<FileInfosHeader>(headerbuffer);
    switch ((FcpType)header.FCP_Type)
    {
      case FcpType.File: await UnPackFileAsync(archivepath, outputfolder, buffersize); break;
      case FcpType.Archiv: await UnPackArchivAsync(archivepath, outputfolder, buffersize); break;
    }
  }
}
