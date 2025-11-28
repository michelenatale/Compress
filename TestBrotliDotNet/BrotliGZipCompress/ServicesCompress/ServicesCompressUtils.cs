
using System.Security.Cryptography;

namespace michele.natale.Services;


public partial class ServicesCompress
{
  /// <summary>
  /// Compares two files by computing their SHA-512 hash values.
  /// </summary>
  /// <param name="left">The path to the first file.</param>
  /// <param name="right">The path to the second file.</param>
  /// <returns>
  /// <c>true</c> if both files exist and their SHA-512 hashes are identical; otherwise <c>false</c>.
  /// </returns>
  /// <remarks>
  /// <para>
  /// This method opens both files in read-only mode and computes their SHA-512 hashes using
  /// <see cref="SHA512.HashData(Stream)"/>. It returns <c>false</c> if either file does not exist.
  /// </para>
  /// <para>
  /// This comparison is cryptographically strong and suitable for verifying file integrity.
  /// </para>
  /// </remarks>
  /// <exception cref="IOException">Thrown if an I/O error occurs while reading the files.</exception>
  public static bool FileEquals(string left, string right)
  {
    if (!File.Exists(left)) return false; if (!File.Exists(right)) return false;
    using var fsleft = new FileStream(left, FileMode.Open, FileAccess.Read);
    using var fsright = new FileStream(right, FileMode.Open, FileAccess.Read);
    return SHA512.HashData(fsleft).SequenceEqual(SHA512.HashData(fsright));
  }

  /// <summary>
  /// Asynchronously compares two files by computing their SHA-512 hashes.
  /// </summary>
  /// <param name="left">
  /// The path of the first file to compare.
  /// </param>
  /// <param name="right">
  /// The path of the second file to compare.
  /// </param>
  /// <returns>
  /// A <see cref="Task{Boolean}"/> representing the asynchronous operation,
  /// with <c>true</c> if both files exist and have identical SHA-512 hashes,
  /// otherwise <c>false</c>.
  /// </returns>
  /// <remarks>
  /// - Returns <c>false</c> if either file does not exist.
  /// - Uses <see cref="FileStream"/> with <c>useAsync: true</c> for async I/O.
  /// - Hashes are computed asynchronously via <see cref="HashAlgorithm.ComputeHashAsync(Stream, CancellationToken)"/>.
  /// - <see cref="ConfigureAwait(bool)"/> is used with <c>false</c> to avoid deadlocks in synchronization contexts.
  /// </remarks>
  public static async Task<bool> FileEqualsAsync(string left, string right)
  {
    if (!File.Exists(left)) return false;
    if (!File.Exists(right)) return false;

    await using var fsleft = new FileStream(left, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 20, true);
    await using var fsright = new FileStream(right, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 20, true);

    using var sha = SHA512.Create();
    var hashleft = await sha.ComputeHashAsync(fsleft).ConfigureAwait(false);
    var hashright = await sha.ComputeHashAsync(fsright).ConfigureAwait(false);

    return hashleft.SequenceEqual(hashright);
  }

  /// <summary>
  /// Gets the size of a file in bytes as a 32-bit integer.
  /// </summary>
  /// <param name="src">The path to the file.</param>
  /// <returns>
  /// The file size in bytes, truncated to <see cref="int"/>. Returns <c>-1</c> if the file does not exist.
  /// </returns>
  /// <remarks>
  /// <para>
  /// This method delegates to <see cref="FileSizeLong(string)"/> and casts the result to <see cref="int"/>.
  /// </para>
  /// <para>
  /// For files larger than <see cref="int.MaxValue"/>, the result will be truncated.
  /// Use <see cref="FileSizeLong(string)"/> for full precision.
  /// </para>
  /// </remarks>
  /// <exception cref="IOException">Thrown if an I/O error occurs while reading the file.</exception>
  public static int FileSize(string src) =>
    (int)FileSizeLong(src);

  /// <summary>
  /// Gets the size of a file in bytes as a 64-bit integer.
  /// </summary>
  /// <param name="src">The path to the file.</param>
  /// <returns>
  /// The file size in bytes as a <see cref="long"/>. Returns <c>-1</c> if the file does not exist.
  /// </returns>
  /// <remarks>
  /// <para>
  /// This method opens the file in read-only mode and returns its <see cref="FileStream.Length"/>.
  /// </para>
  /// <para>
  /// Use this method when working with files larger than <see cref="int.MaxValue"/>.
  /// </para>
  /// </remarks>
  /// <exception cref="IOException">Thrown if an I/O error occurs while reading the file.</exception>
  public static long FileSizeLong(string src)
  {
    if (!File.Exists(src)) return -1;
    return new FileInfo(src).Length;
  }

  /// <summary>
  /// Asynchronously retrieves the size of a file in bytes.
  /// </summary>
  /// <param name="src">
  /// The path of the file whose size is requested.
  /// </param>
  /// <returns>
  /// A <see cref="Task{Int64}"/> representing the asynchronous operation,
  /// with the file size in bytes as result, or -1 if the file does not exist.
  /// </returns>
  /// <remarks>
  /// - The method checks existence with <see cref="File.Exists"/>.
  /// - The file is opened with <see cref="FileMode.Open"/> and <see cref="FileAccess.Read"/>.
  /// - The <see cref="FileStream.Length"/> property is accessed synchronously,
  /// but wrapped in <see cref="Task"/> for async API consistency.
  /// </remarks>
  public static async Task<long> FileSizeLongAsync(string src)
  {
    var exists = await Task.Run(() => File.Exists(src));
    if (!exists) return -1;
    return new FileInfo(src).Length;

    //await using var fs = new FileStream(src, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 1, useAsync: true);
    //return await Task.FromResult(fs.Length).ConfigureAwait(false);
  }

  /// <summary>
  /// Asynchronously writes the specified byte array multiple times into a new file.
  /// </summary>
  /// <param name="filename">
  /// The name of the file to be created and written to.
  /// </param>
  /// <param name="bytes">
  /// The byte array whose contents will be written.
  /// </param>
  /// <param name="mult">
  /// The number of times the byte array will be written to the file.
  /// </param>
  /// <returns>
  /// A <see cref="Task"/> representing the asynchronous write operation.
  /// </returns>
  /// <remarks>
  /// - The file is created using <see cref="FileMode.Create"/>.
  /// - The entire contents of <paramref name="bytes"/> are written <paramref name="mult"/> times.
  /// - The method uses <see cref="MemoryExtensions.AsMemory"/> for efficient writing.
  /// - <see cref="ConfigureAwait(bool)"/> is called with <c>false</c> to avoid deadlocks in synchronization contexts.
  /// </remarks>
  public async static Task MultBytesInFileAsync(
    string filename, byte[] bytes, int mult)
  {
    await using var fsout = new FileStream(
      filename, FileMode.Create, FileAccess.Write);

    var length = bytes.Length;
    for (var i = 0; i < mult; i++)
      await fsout.WriteAsync(bytes.AsMemory(0, length))
        .ConfigureAwait(false);
  }


  //public async static Task RngByteFileAsync(
  //  string filename, int size, int buffersize = 1 << 20)
  //{
  //  await using var fsout = new FileStream(filename, FileMode.Create, FileAccess.Write);

  //  int sumbytes = size;
  //  var rand = Random.Shared;
  //  var buffer = new byte[buffersize];

  //  while (sumbytes > 0)
  //  {
  //    rand.NextBytes(buffer);
  //    var sz = sumbytes - buffersize >= 0 ? buffersize : sumbytes;
  //    await fsout.WriteAsync(buffer.AsMemory(0, sz)).ConfigureAwait(false);
  //    sumbytes -= sz;
  //  }
  //}
}