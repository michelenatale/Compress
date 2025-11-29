

namespace michele.natale.Compresses;


/// <summary>
/// Defines the type of FileCompressPackage (FCP) 
/// entry stored in the archive.
/// </summary>
public enum FcpType : byte
{
  /// <summary>
  /// Represents a single file entry.  
  /// The file data is stored in compressed form.
  /// </summary>
  File = 1,

  /// <summary>
  /// Represents a multi-file archive entry.  
  /// The archive contains multiple files and directories packed together.
  /// </summary>
  Archiv = 2,
}



