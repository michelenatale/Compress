


using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace michele.natale.Services;

using Compresses;

partial class ServicesCompress
{
  /// <summary>
  /// Writes a <see cref="FileInfosHeader"/> structure to the specified output stream.
  /// </summary>
  /// <param name="output">
  /// The destination <see cref="Stream"/> where the header will be written.
  /// Must be writable.
  /// </param>
  /// <param name="header">
  /// The <see cref="FileInfosHeader"/> instance to serialize into the stream.
  /// </param>
  /// <remarks>
  /// The header is serialized into a compact binary representation using <see cref="MemoryMarshal.Write{T}(Span{byte}, ref T)"/>.
  /// The stream remains open after writing.
  /// </remarks>
  public static void WriteHeader(Stream output, in FileInfosHeader header)
  {
    Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<FileInfosHeader>()];
    MemoryMarshal.Write(buffer, in header);
    output.Write(buffer);
  }

  /// <summary>
  /// Reads a <see cref="FileInfosHeader"/> structure from the specified input stream.
  /// </summary>
  /// <param name="input">
  /// The source <see cref="Stream"/> containing the serialized header.
  /// Must be readable and positioned at the start of the header.
  /// </param>
  /// <returns>
  /// A <see cref="FileInfosHeader"/> instance reconstructed from the binary data in the stream.
  /// </returns>
  /// <remarks>
  /// The method reads exactly the number of bytes required for the <see cref="FileInfosHeader"/> structure
  /// using <c>ReadExactly</c>, then deserializes it with <see cref="MemoryMarshal.Read{T}(ReadOnlySpan{byte})"/>.
  /// The stream remains open after reading.
  /// </remarks>
  public static FileInfosHeader ReadHeader(Stream input)
  {
    Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<FileInfosHeader>()];
    input.ReadExactly(buffer);
    return MemoryMarshal.Read<FileInfosHeader>(buffer);
  }
}

