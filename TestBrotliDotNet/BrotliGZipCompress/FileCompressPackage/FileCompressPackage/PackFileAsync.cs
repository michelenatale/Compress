
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;


namespace michele.natale.Compresses;

using Services;
using System;

partial class FileCompressPackage
{
  /// <summary>
  /// Asynchronously packs multiple files into a single archive using the specified compression type.
  /// </summary>
  /// <param name="filepathlist">
  /// An array of file paths to include in the archive. Each file will be compressed and written sequentially.
  /// </param>
  /// <param name="archivepath">
  /// The destination path of the archive file. The extension is validated and corrected if necessary.
  /// </param>
  /// <param name="compressiontype">
  /// The compression type to use for each file entry.
  /// 1 = Brotli, 2 = GZip.
  /// </param>
  /// <param name="buffersize">
  /// The size of the buffer used for reading and writing during compression. 
  /// Default is 81,920 bytes (80 KB).
  /// </param>
  /// <param name="compresslevel">
  /// The <see cref="CompressionLevel"/> that determines the balance between compression speed and ratio.
  /// Default is <see cref="CompressionLevel.Optimal"/>.
  /// </param>
  /// <returns>
  /// A task that represents the asynchronous packing operation. 
  /// The task completes when all files have been compressed and written to the archive.
  /// </returns>
  /// <remarks>
  /// <para>
  /// This method creates a new archive file and writes each file entry sequentially.
  /// </para>
  /// <para>
  /// The archive path is validated using <c>ServicesCompress.CheckFCPFileExtensionAsync</c>
  /// to ensure the correct extension is applied.
  /// </para>
  /// <para>
  /// Each file is opened as a <see cref="FileStream"/> and passed to <c>WriteFileAsync</c>,
  /// which handles compression and header writing.
  /// </para>
  /// </remarks>
  /// <example>
  /// Example of packing multiple files into a GZip-based archive:
  /// <code>
  /// string[] files = { "file1.txt", "file2.log" };
  /// string archive = "myarchive.fcp";
  ///
  /// await PackFileAsync(files, archive, compressiontype: 2);
  /// </code>
  /// </example>
  public async static Task PackFileAsync(
      string[] filepathlist, string archivepath, CompressionType compressiontype,
      int buffersize = 81920, CompressionLevel compresslevel = CompressionLevel.Optimal) =>
        await PackFileAsync(filepathlist, archivepath, (byte)compressiontype, buffersize, compresslevel);

  /// <summary>
  /// Asynchronously packs multiple files into a single archive using the specified compression type.
  /// </summary>
  /// <param name="filepathlist">
  /// An array of file paths to include in the archive. Each file will be compressed and written sequentially.
  /// </param>
  /// <param name="archivepath">
  /// The destination path of the archive file. The extension is validated and corrected if necessary.
  /// </param>
  /// <param name="compressiontype">
  /// The compression type to use for each file entry.
  /// 1 = Brotli, 2 = GZip.
  /// </param>
  /// <param name="buffersize">
  /// The size of the buffer used for reading and writing during compression. 
  /// Default is 81,920 bytes (80 KB).
  /// </param>
  /// <param name="compresslevel">
  /// The <see cref="CompressionLevel"/> that determines the balance between compression speed and ratio.
  /// Default is <see cref="CompressionLevel.Optimal"/>.
  /// </param>
  /// <returns>
  /// A task that represents the asynchronous packing operation. 
  /// The task completes when all files have been compressed and written to the archive.
  /// </returns>
  /// <remarks>
  /// <para>
  /// This method creates a new archive file and writes each file entry sequentially.
  /// </para>
  /// <para>
  /// The archive path is validated using <c>ServicesCompress.CheckFCPFileExtensionAsync</c>
  /// to ensure the correct extension is applied.
  /// </para>
  /// <para>
  /// Each file is opened as a <see cref="FileStream"/> and passed to <c>WriteFileAsync</c>,
  /// which handles compression and header writing.
  /// </para>
  /// </remarks>
  /// <example>
  /// Example of packing multiple files into a GZip-based archive:
  /// <code>
  /// string[] files = { "file1.txt", "file2.log" };
  /// string archive = "myarchive.fcp";
  ///
  /// await PackFileAsync(files, archive, compressiontype: 2);
  /// </code>
  /// </example>
  public async static Task PackFileAsync(
      string[] filepathlist, string archivepath, byte compressiontype,
      int buffersize = 81920, CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    var archiv = await ServicesCompress.CheckFCPFileExtensionAsync(archivepath);
    await using var fsout = new FileStream(archiv.FullName, FileMode.Create, FileAccess.Write);
    Console.WriteLine($"© FileCompressPackage 2025 - FILE PACKAGE - Created by © Michele Natale 2025");

    foreach (var src in filepathlist)
    {
      await using var fsin = new FileStream(src, FileMode.Open, FileAccess.Read);
      await WriteFileAsync(fsin, fsout, src, compressiontype, buffersize, compresslevel);
    }

    Console.WriteLine();
  }


  private async static Task WriteFileAsync(
    Stream input, Stream output, string filepath, byte compresstype,
    int buffersize = 81920, CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    await output.FlushAsync();

    var pos = output.Position;
    var fileinfos = new FileInfo(filepath);
    var namebytes = Encoding.UTF8.GetBytes(fileinfos.Name);
    var headerbuffer = new byte[Unsafe.SizeOf<FileInfosHeader>()];

    var ct = (CompressionType)compresstype;
    output.Position = pos + headerbuffer.Length + namebytes.LongLength;

    var compresslength = ct switch
    {
      CompressionType.None => await ServicesCompress.CopyChunkAsync(input, output, 0, input.Length, buffersize),
      CompressionType.GZip => await CompressedNet
        .CompressGZipAsyncSpec(input, output, buffersize, compresslevel),
      CompressionType.Brotli => await CompressedNet
        .CompressBrotliAsyncSpec(input, output, buffersize, compresslevel),
      _ => throw new InvalidOperationException(),
    };

    var header = new FileInfosHeader
    {
      FCP_Type = (byte)FcpType.File,
      CompressionType = compresstype,
      OriginalLength = fileinfos.Length,
      CompressedLength = compresslength,
      NameLength = (byte)namebytes.Length,
      CreationTimeUtcTicks = fileinfos.CreationTimeUtc.Ticks,
      LastWriteTimeUtcTicks = fileinfos.LastWriteTimeUtc.Ticks,
      LastAccessTimeUtcTicks = fileinfos.LastAccessTimeUtc.Ticks,
    };

    await output.FlushAsync();

    output.Position = pos;
    MemoryMarshal.Write(headerbuffer, in header);
    output.Write(headerbuffer);
    output.Write(namebytes, 0, namebytes.Length);

    var newlength = pos + headerbuffer.LongLength + namebytes.LongLength + compresslength;
    output.Position = newlength;
    output.SetLength(newlength);

    Console.WriteLine($"CompressType = {(CompressionType)header.CompressionType}, " +
    $"PACK FILE: {Path.GetFileNameWithoutExtension(filepath)} ({header.OriginalLength} Bytes), " +
    $"Created: {fileinfos.CreationTimeUtc}, Last Accessed: {fileinfos.LastAccessTimeUtc}, " +
    $"Last Updated: {fileinfos.LastWriteTimeUtc}, Compression Ratio = {header.CompressedLength / (double)header.OriginalLength}");
  }
}
