
 
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;


namespace michele.natale.Compresses;

using Services;

partial class FileCompressPackage
{
  public async static Task UnPackArchivAsync(string archivepath, string outputfolder)
  {
    ServicesCompress.DeleteFolder(outputfolder, true);
    Directory.CreateDirectory(outputfolder);
    await using var fsin = new FileStream(archivepath, FileMode.Open, FileAccess.Read);
    await ReadArchivAsync(fsin, outputfolder);
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
