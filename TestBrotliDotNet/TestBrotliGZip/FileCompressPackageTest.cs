



using michele.natale.Compresses;
using System.Security.Cryptography;

namespace michele.natale.Tests;



public class FileCompressPackageTest
{
  public async static Task Start()
  {
    await TestPackNoneAsync();
    await TestPackGZipAsync();
    await TestPackBrotliAsync();

    Console.WriteLine();
  }

  private async static Task TestPackNoneAsync()
  {
    string outputfolder = "output", archivepath = "test.fcp";

    // Pack Files
    string[] packlist = ["data2.txt", "data3.txt", "data2.txt", "data3.txt"];
    await FileCompressPackage.PackAsync(packlist, archivepath, CompressionType.None);

    // UnPack Files
    await FileCompressPackage.UnPackAsync(archivepath, outputfolder);

    if (!FileEqualsSpec(packlist, outputfolder))
      throw new Exception();

    Console.WriteLine();
  }

  private async static Task TestPackGZipAsync()
  {
    string outputfolder = "output", archivepath = "test.fcp";

    // Pack Files
    string[] packlist = ["data2.txt", "data3.txt", "data2.txt", "data3.txt"];
    await FileCompressPackage.PackAsync(packlist, archivepath, CompressionType.GZip);

    // UnPack Files
    await FileCompressPackage.UnPackAsync(archivepath, outputfolder);

    if (!FileEqualsSpec(packlist, outputfolder))
      throw new Exception();

    Console.WriteLine();
  }

  private async static Task TestPackBrotliAsync()
  {
    string outputfolder = "output", archivepath = "test.fcp";

    // Pack Files
    string[] packlist = ["data2.txt", "data3.txt", "data2.txt", "data3.txt"];
    await FileCompressPackage.PackAsync(packlist, archivepath, CompressionType.Brotli);

    // UnPack Files
    await FileCompressPackage.UnPackAsync(archivepath, outputfolder);

    if (!FileEqualsSpec(packlist, outputfolder))
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

  private static bool FileEquals(string left, string right)
  {
    using var fleft = new FileStream(left, FileMode.Open, FileAccess.Read);
    using var fright = new FileStream(left, FileMode.Open, FileAccess.Read);

    using var sha = SHA512.Create();
    return sha.ComputeHash(fleft).SequenceEqual(sha.ComputeHash(fright));
  }
}
