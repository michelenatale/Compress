
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace michele.natale.Compresses;


using Services;

partial class FileCompressPackage
{

  /// <summary>
  /// Asynchronously extracts all files from a custom archive into the specified output folder.
  /// </summary>
  /// <param name="archivepath">
  /// The path to the archive file to be unpacked. 
  /// Must be a valid file created by the packing routines.
  /// </param>
  /// <param name="outputfolder">
  /// The destination folder where the unpacked files will be written. 
  /// If the folder already exists, it will be deleted and recreated.
  /// </param>
  /// <returns>
  /// A task that represents the asynchronous unpacking operation. 
  /// The task completes when all files have been read from the archive and written to the output folder.
  /// </returns>
  /// <remarks>
  /// <para>
  /// This method ensures that the output folder is clean by deleting any existing folder 
  /// and recreating it before extraction begins.
  /// </para>
  /// <para>
  /// The archive is opened as a <see cref="FileStream"/> in read mode and passed to <c>ReadFilesAsync</c>,
  /// which handles reading headers, decompressing payloads, and writing files.
  /// </para>
  /// <para>
  /// Both the archive stream and the output folder remain available after the operation completes.
  /// </para>
  /// </remarks>
  /// <example>
  /// Example of unpacking an archive into a folder:
  /// <code>
  /// string archive = "myarchive.fcp";
  /// string output = "extracted_files";
  ///
  /// await UnPackAsync(archive, output);
  /// </code>
  /// </example>
  public async static Task UnPackAsync(string archivepath, string outputfolder)
  {
    ServicesCompress.DeleteFolder(outputfolder, true);
    Directory.CreateDirectory(outputfolder);
    await using var fsin = new FileStream(archivepath, FileMode.Open, FileAccess.Read);
    await ReadFilesAsync(fsin, outputfolder);
  }

  private async static Task ReadFilesAsync(
    Stream input, string outputfolder, int buffersize = 81920)
  {
    var start = 0L;
    var headersize = Unsafe.SizeOf<FileInfosHeader>();
    var headerbuffer = new byte[headersize];

    while (input.Position < input.Length)
    {
      input.ReadExactly(headerbuffer);
      var header = MemoryMarshal.Read<FileInfosHeader>(headerbuffer);

      var namebytes = new byte[header.NameLength];
      input.ReadExactly(namebytes, 0, header.NameLength);

      var filename = Encoding.UTF8.GetString(namebytes);
      var outpath = Path.Combine(outputfolder, filename);

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

      Console.WriteLine($"© FileCompressPackage 2025 - created by © Michele Natale 2025");
      Console.WriteLine($"CompressType = {(CompressionType)header.CompressionType}, " +
        $"UNPACK: {filename} ({header.OriginalLength} Bytes), " +
        $"Created: {creationtime}, Last Accessed: {lastaccesstime}, " +
        $"Last Updated: {lastwritetime}");
    }
    Console.WriteLine();
  }
}
