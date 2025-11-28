using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;


namespace michele.natale.Compresses;

using Services;
using System;

partial class FileCompressPackage
{
  public async static Task PackArchivAsync(
      string srcfolder, string archivepath, CompressionType compressiontype,
      int buffersize = 81920, CompressionLevel compresslevel = CompressionLevel.Optimal) =>
        await PackArchivAsync(srcfolder, archivepath, (byte)compressiontype, buffersize, compresslevel);


  public async static Task PackArchivAsync(
     string srcfolder, string archivepath, byte compressiontype,
     int buffersize = 81920, CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    if (!new DirectoryInfo(srcfolder).Exists)
      throw new DirectoryNotFoundException(nameof(srcfolder));

    var archiv = await ServicesCompress.CheckFCPFileExtensionAsync(archivepath);
    await using var fsout = new FileStream(archiv.FullName, FileMode.Create, FileAccess.Write);
    Console.WriteLine($"© FileCompressPackage 2025 - ARCHIV PACKAGE - Created by © Michele Natale 2025");

    foreach (var fisrc in new DirectoryInfo(srcfolder).GetFiles("*.*", SearchOption.AllDirectories))
    {
      var idx = fisrc.FullName.IndexOf(srcfolder);
      var fpath = fisrc.FullName.Substring(idx);
      await using var fsin = new FileStream(fisrc.FullName, FileMode.Open, FileAccess.Read);
      await WriteArchivAsync(fsin, fsout, fpath, compressiontype, buffersize, compresslevel);
    }

    Console.WriteLine();
  }


  private async static Task WriteArchivAsync(
    Stream input, Stream output, string filepath, byte compresstype,
    int buffersize = 81920, CompressionLevel compresslevel = CompressionLevel.Optimal)
  {
    await output.FlushAsync();

    var pos = output.Position;
    var fileinfos = new FileInfo(filepath);
    var namebytes = Encoding.UTF8.GetBytes(filepath);
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
      CompressionType = compresstype,
      FCP_Type = (byte)FcpType.Archiv,
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
    $"PACK ARCHIV: {fileinfos} ({header.OriginalLength} Bytes), " +
    $"Created: {fileinfos.CreationTimeUtc}, Last Accessed: {fileinfos.LastAccessTimeUtc}, " +
    $"Last Updated: {fileinfos.LastWriteTimeUtc}, Compression Ratio = {header.CompressedLength / (double)header.OriginalLength}");
  }
}


