



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
}
