


using System.Security.Cryptography;

namespace michele.natale.Tests;

using Compresses;


public class FileCompressPackageTest
{
  public async static Task Start()
  {
    await TestPackNoneFileAsync();
    await TestPackGZipFileAsync();
    await TestPackBrotliFileAsync();

    string srcfolder = "sourcefolder";
    await PreparationAsync(srcfolder);
    await TestPackNoneArchivAsync(srcfolder);
    await TestPackGZipArchivAsync(srcfolder);
    await TestPackBrotliArchivAsync(srcfolder);
    await FinishAsync(srcfolder);

    Console.WriteLine();
  }

  private async static Task TestPackNoneFileAsync()
  {
    string outputfolder = "output", archivepath = "test.fcp";

    // Pack Files
    string[] packlist = ["data2.txt", "data3.txt", "data2.txt", "data3.txt"];
    var (totalfilesize, totalcompresssize) = await FileCompressPackage.PackFileAsync(packlist, archivepath, CompressionType.None);

    //With HeaderInformation
    Console.WriteLine($"Total File Size = {totalfilesize} Bytes, Total Compression Size = {totalcompresssize} Bytes and Total Compression Ratio = {totalcompresssize/(double)totalfilesize}\n");

    // UnPack Files
    await FileCompressPackage.UnPackAsync(archivepath, outputfolder);

    if (!FileEqualsSpec(packlist, outputfolder))
      throw new Exception();

    Console.WriteLine();
  }

  private async static Task TestPackGZipFileAsync()
  {
    string outputfolder = "output", archivepath = "test.fcp";

    // Pack Files
    string[] packlist = ["data2.txt", "data3.txt", "data2.txt", "data3.txt"];
    var (totalfilesize, totalcompresssize) = await FileCompressPackage.PackFileAsync(packlist, archivepath, CompressionType.GZip);
    Console.WriteLine($"Total File Size = {totalfilesize} Bytes, Total Compression Size = {totalcompresssize} Bytes and Total Compression Ratio = {totalcompresssize / (double)totalfilesize}\n");

    // UnPack Files
    await FileCompressPackage.UnPackAsync(archivepath, outputfolder);

    if (!FileEqualsSpec(packlist, outputfolder))
      throw new Exception();

    Console.WriteLine();
  }

  private async static Task TestPackBrotliFileAsync()
  {
    string outputfolder = "output", archivepath = "test.fcp";

    // Pack Files
    string[] packlist = ["data2.txt", "data3.txt", "data2.txt", "data3.txt"];
    var (totalfilesize, totalcompresssize) = await FileCompressPackage.PackFileAsync(packlist, archivepath, CompressionType.Brotli);
    Console.WriteLine($"Total File Size = {totalfilesize} Bytes, Total Compression Size = {totalcompresssize} Bytes and Total Compression Ratio = {totalcompresssize / (double)totalfilesize}\n");

    // UnPack Files
    await FileCompressPackage.UnPackAsync(archivepath, outputfolder);

    if (!FileEqualsSpec(packlist, outputfolder))
      throw new Exception();

    Console.WriteLine();
  }

  private async static Task TestPackNoneArchivAsync(string srcfolder)
  {
    string outputfolder = "output", archivepath = "test.fcp";

    // Pack Folder
    var (totalfilesize, totalcompresssize) = await FileCompressPackage.PackArchivAsync(srcfolder, archivepath, CompressionType.None);

    //With HeaderInformation
    Console.WriteLine($"Total File Size = {totalfilesize} Bytes, Total Compression Size = {totalcompresssize} Bytes and Total Compression Ratio = {totalcompresssize / (double)totalfilesize}\n");

    // UnPack Archiv
    await FileCompressPackage.UnPackAsync(archivepath, outputfolder);

    if (!FileEqualsSpec(srcfolder, outputfolder))
      throw new Exception();

    Console.WriteLine();
  }

  private async static Task TestPackGZipArchivAsync(string srcfolder)
  {
    string outputfolder = "output", archivepath = "test.fcp";

    // Pack Folder
    var (totalfilesize, totalcompresssize) = await FileCompressPackage.PackArchivAsync(srcfolder, archivepath, CompressionType.GZip);
    Console.WriteLine($"Total File Size = {totalfilesize} Bytes, Total Compression Size = {totalcompresssize} Bytes and Total Compression Ratio = {totalcompresssize / (double)totalfilesize}\n");


    // UnPack Archiv
    await FileCompressPackage.UnPackAsync(archivepath, outputfolder);

    if (!FileEqualsSpec(srcfolder, outputfolder))
      throw new Exception();

    Console.WriteLine();
  }

  private async static Task TestPackBrotliArchivAsync(string srcfolder)
  {
    string outputfolder = "output", archivepath = "test.fcp";

    // Pack Folder
    var (totalfilesize, totalcompresssize) = await FileCompressPackage.PackArchivAsync(srcfolder, archivepath, CompressionType.Brotli);
    Console.WriteLine($"Total File Size = {totalfilesize} Bytes, Total Compression Size = {totalcompresssize} Bytes and Total Compression Ratio = {totalcompresssize / (double)totalfilesize}\n");

    // UnPack Archiv
    await FileCompressPackage.UnPackAsync(archivepath, outputfolder);

    if (!FileEqualsSpec(srcfolder, outputfolder))
      throw new Exception();

    Console.WriteLine();
  }

  private static bool FileEqualsSpec(string[] filelist, string outputfolder)
  {
    //Special Tester
    foreach (var file in filelist)
      if (!FileEquals(file, Path.Combine(outputfolder, file)))
        return false;
    return true;
  }

  private static bool FileEqualsSpec(string srcfolder, string destfolder)
  {
    var left = new DirectoryInfo(srcfolder)
      .GetFiles("*.*", SearchOption.AllDirectories).OrderBy(x => x.FullName).ToArray();

    var right = new DirectoryInfo(destfolder)
      .GetFiles("*.*", SearchOption.AllDirectories).OrderBy(x => x.FullName).ToArray();

    if (EqualitySpec(left, right, srcfolder))
    {
      var length = left.Length;
      for (var i = 0; i < length; i++)
        if (!FileEquals(left[i].FullName, right[i].FullName))
          return false;
      return true;
    }

    return false;
  }

  private static bool FileEquals(string left, string right)
  {
    using var fleft = new FileStream(left, FileMode.Open, FileAccess.Read);
    using var fright = new FileStream(right, FileMode.Open, FileAccess.Read);

    using var sha = SHA512.Create();
    return sha.ComputeHash(fleft).SequenceEqual(sha.ComputeHash(fright));
  }


  private static async Task FinishAsync(string srcfolder)
  {
    Console.WriteLine($"The source directory is deleted again.\n");
    if (Directory.Exists(srcfolder))
      await Task.Run(() => Directory.Delete(srcfolder, true));
  }

  private static async Task PreparationAsync(string srcfolder)
  {
    Console.WriteLine($"A SourceFolder with many files and directories is created.\n");
    string[] packlist = { "data.txt", "data2.txt", "data3.txt" };
    await CreateRngFolders(srcfolder, packlist);
  }

  public static async Task CopyFileAsync(
    string sourcepath, string destinationPath, bool overwrite = false)
  {
    if (!File.Exists(sourcepath))
      throw new FileNotFoundException("Source file not found.", sourcepath);

    if (File.Exists(destinationPath) && overwrite)
      File.Delete(destinationPath);

    await using var fsin = new FileStream(sourcepath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
    await using var fsout = new FileStream(destinationPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);

    await fsin.CopyToAsync(fsout);
  }

  private async static Task CreateRngFolders(
   string basefolder, string[] files)
  {
    var rand = Random.Shared;

    if (Directory.Exists(basefolder))
      Directory.Delete(basefolder, true);

    Directory.CreateDirectory(basefolder);
    var file = files[rand.Next(files.Length)];
    var dest = Path.Combine(basefolder, file);
    await CopyFileAsync(file, dest, overwrite: true);
    for (int i = 0; i < 3; i++)
    {
      var subroot = Path.Combine(basefolder, RngFolderName(8));
      Directory.CreateDirectory(subroot);

      var current = subroot;
      file = files[rand.Next(files.Length)];
      dest = Path.Combine(current, file);
      await CopyFileAsync(file, dest, overwrite: true);
      for (var depth = 0; depth < 3; depth++)
      {
        current = Path.Combine(current, RngFolderName(8));
        Directory.CreateDirectory(current);

        var c = rand.Next(files.Length) + 1;
        for (int j = 0; j < c; j++)
          if (rand.NextDouble() < 0.95) // 95% Chance
          {
            file = files[rand.Next(files.Length)];
            dest = Path.Combine(current, file);
            await CopyFileAsync(file, dest, overwrite: true);
          }
      }
    }
  }

  private static string RngFolderName(int size) =>
    Guid.NewGuid().ToString("N").Substring(0, size);

  private static bool EqualitySpec(
    ReadOnlySpan<FileInfo> left, ReadOnlySpan<FileInfo> right, string srcfolder)
  {
    if (left.Length != right.Length)
      return false;

    var length = left.Length;
    for (int i = 0; i < length; i++)
    {
      var idx1 = left[i].FullName.IndexOf(srcfolder, StringComparison.Ordinal);
      var idx2 = right[i].FullName.IndexOf(srcfolder, StringComparison.Ordinal);
      if (!left[i].FullName.Substring(idx1).SequenceEqual(right[i].FullName.Substring(idx2)))
        return false;
    }
    return true;
  }
}
