

using System.Security.Cryptography;

namespace michele.natale.Services;

public partial class ServicesCompress
{
  public static bool FileEquals(string left, string right)
  {
    if (!File.Exists(left)) return false; if (!File.Exists(right)) return false;
    using var fsleft = new FileStream(left, FileMode.Open, FileAccess.Read);
    using var fsright = new FileStream(right, FileMode.Open, FileAccess.Read);
    return SHA512.HashData(fsleft).SequenceEqual(SHA512.HashData(fsright));
  }

  public async static Task<int> FileSizeAsync(Task task, string src)
  {
    await task;
    return await Task.Run(() => FileSize(src));
  }

  public async static Task<long> FileSizeLongAsync(Task task, string src)
  {
    await task;
    return await Task.Run(() => FileSizeLong(src));
  }

  public async static Task<int> FileSizeAsync(string src)
  {
    return await Task.Run(() => FileSize(src));
  }

  public async static Task<long> FileSizeLongAsync(string src)
  {
    return await Task.Run(() => FileSizeLong(src));
  }

  public static int FileSize(string src) =>
    (int)FileSizeLong(src);

  public static long FileSizeLong(string src)
  {
    if (!File.Exists(src)) return -1;
    using var fs = new FileStream(src, FileMode.Open, FileAccess.Read);
    return fs.Length;
  }

}
