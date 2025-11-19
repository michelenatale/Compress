

using System.Security.Cryptography;

namespace michele.natale.Services;


/// <summary>
/// Provides utility methods for file comparison, size inspection, and compression/decompression
/// services using Brotli and GZip.
/// </summary>
/// <remarks>
/// <para>
/// The <c>ServicesCompress</c> class is declared as <c>partial</c> to allow its implementation
/// to be split across multiple files. This design enables modular organization of related
/// functionality, such as separating Brotli and GZip operations or file utilities.
/// </para>
/// <para>
/// Key responsibilities include:
/// <list type="bullet">
/// <item><description>File integrity checks using SHA-512 hashing.</description></item>
/// <item><description>File size inspection with both 32-bit and 64-bit precision.</description></item>
/// <item><description>Compression and decompression methods for Brotli and GZip.</description></item>
/// <item><description>Support for synchronous and asynchronous operations, including cancellation tokens.</description></item>
/// </list>
/// </para>
/// <para>
/// This class is designed for production-ready scenarios where correctness, efficiency,
/// and explicit resource control are required. It is suitable for applications that need
/// reliable compression services with clear exception handling and cancellation support.
/// </para>
/// </remarks>
/// <example>
/// Example usage:
/// <code>
/// // Compare two files for equality
/// bool areEqual = ServicesCompress.FileEquals("left.bin", "right.bin");
///
/// // Get file size
/// long size = ServicesCompress.FileSizeLong("data.txt");
///
/// // Compress a file with Brotli
/// ServicesCompress.CompressBrotli("input.txt", "output.br");
/// </code>
/// </example>
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
    using var fs = new FileStream(src, FileMode.Open, FileAccess.Read);
    return fs.Length;
  }

}