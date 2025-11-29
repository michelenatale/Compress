

using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;


namespace michele.natale.Compresses;

using Services;

partial class FileCompressPackage
{

  /// <summary>
  /// Extracts all files from a given archive into the specified output folder.
  /// </summary>
  /// <param name="archivepath">
  /// Path to the archive file to be unpacked.
  /// </param>
  /// <param name="outputfolder">
  /// Destination folder where the extracted files will be written.
  /// If the folder already exists, it will be deleted and recreated.
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
  /// This method deletes the output folder before extraction to ensure a clean state.
  /// </remarks>
  public async static Task UnPackArchivAsync(
    string archivepath, string outputfolder, int buffersize = 81920)
  {
    ServicesCompress.DeleteFolder(outputfolder, true);
    Directory.CreateDirectory(outputfolder);
    await using var fsin = new FileStream(archivepath, FileMode.Open, FileAccess.Read);
    await ReadArchivAsync(fsin, outputfolder, buffersize);
  }

  private async static Task ReadArchivAsync(
  Stream input, string outputfolder, int buffersize = 81920)
  {
    var start = 0L;
    var headersize = Unsafe.SizeOf<FileInfosHeader>();
    var headerbuffer = new byte[headersize];
    Console.WriteLine($"© FileCompressPackage 2025 - ARCHIV PACKAGE - Created by © Michele Natale 2025");

    while (input.Position < input.Length)
    {
      input.ReadExactly(headerbuffer);
      var header = MemoryMarshal.Read<FileInfosHeader>(headerbuffer);

      var namebytes = new byte[header.NameLength];
      input.ReadExactly(namebytes, 0, header.NameLength);

      var filename = Encoding.UTF8.GetString(namebytes);
      var outpath = Path.Combine(outputfolder, filename);
      ServicesCompress.CheckFolderFromFilePath(outpath);

      start += headersize + namebytes.Length;
      await using var output = new FileStream(outpath, FileMode.Create, FileAccess.Write);

      var decompresslength = (CompressionType)header.CompressionType switch
      {
        CompressionType.None => await ServicesCompress.CopyChunkAsync(input, output, start, header.CompressedLength, buffersize),
        CompressionType.GZip => await CompressedNet
          .DecompressGZipAsyncSpec(input, output, start, header.CompressedLength, buffersize),
        CompressionType.Brotli => await CompressedNet
          .DecompressBrotliAsyncSpec(input, output, start, header.CompressedLength, buffersize),
        _ => throw new InvalidOperationException(),
      };

      start += header.CompressedLength;

      // Restore timestamp
      var creationtime = new DateTime(header.CreationTimeUtcTicks, DateTimeKind.Utc);
      var lastwritetime = new DateTime(header.LastWriteTimeUtcTicks, DateTimeKind.Utc);
      var lastaccesstime = new DateTime(header.LastAccessTimeUtcTicks, DateTimeKind.Utc);

      File.SetCreationTimeUtc(outpath, creationtime);
      File.SetLastWriteTimeUtc(outpath, lastwritetime);
      File.SetLastAccessTimeUtc(outpath, lastaccesstime);

      Console.WriteLine($"CompressType = {(CompressionType)header.CompressionType}, " +
        $"UNPACK ARCHIV: {filename} ({header.OriginalLength} Bytes), " +
        $"Created: {creationtime}, Last Accessed: {lastaccesstime}, " +
        $"Last Updated: {lastwritetime}");
    }


    Console.WriteLine();
  }
}
