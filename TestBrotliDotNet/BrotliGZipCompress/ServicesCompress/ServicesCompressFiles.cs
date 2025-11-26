






namespace michele.natale.Services;

using Compresses;

partial class ServicesCompress
{

  /// <summary>
  /// Deletes the specified folder if it exists.
  /// </summary>
  /// <param name="foldername">
  /// The path of the folder to delete.
  /// </param>
  /// <param name="explicitly">
  /// If <c>true</c>, the folder is always deleted recursively.
  /// If <c>false</c>, the folder is deleted only if it is empty.
  /// </param>
  /// <remarks>
  /// This method checks whether the folder exists before attempting deletion.
  /// </remarks>
  public static void DeleteFolder(string foldername, bool explicitly)
  {
    if (!Directory.Exists(foldername)) return;
    if (explicitly)
      Directory.Delete(foldername, true);
    else if (AllFilesInFolder(foldername).Length == 0)
      Directory.Delete(foldername, true);
  }

  /// <summary>
  /// Returns all files in the specified folder.
  /// </summary>
  /// <param name="foldername">
  /// The path of the folder to search.
  /// </param>
  /// <returns>
  /// An array of file paths contained in the folder.
  /// </returns>
  /// <remarks>
  /// The search pattern is <c>*.*</c>, so all files are returned regardless of extension.
  /// </remarks>
  public static string[] AllFilesInFolder(string foldername)
  {
    return Directory.GetFiles(foldername, "*.*");
  }

  /// <summary>
  /// Ensures that the specified filename has the correct archive extension (<c>.fcp</c>).
  /// </summary>
  /// <param name="filename">
  /// The input filename to check.
  /// </param>
  /// <returns>
  /// A <see cref="FileInfo"/> object with the corrected extension if necessary.
  /// </returns>
  /// <remarks>
  /// If the filename already ends with <c>.fcp</c>, it is returned unchanged.
  /// Otherwise, the extension is replaced with <c>.fcp</c>.
  /// </remarks>
  public static FileInfo CheckFCPFileExtension(string filename)
  {
    var result = new FileInfo(filename);
    var ext = FileCompressPackage.EXTENSION;
    if (result.Extension.SequenceEqual(ext))
      return result;

    var fullname = result.FullName;
    var folder = result.DirectoryName;
    var fname = Path.GetFileNameWithoutExtension(fullname);
    return new FileInfo(Path.Combine(folder!, fname + ext));
  }

  /// <summary>
  /// Asynchronously ensures that the specified filename has the correct archive extension (<c>.fcp</c>).
  /// </summary>
  /// <param name="filename">
  /// The input filename to check.
  /// </param>
  /// <returns>
  /// A task that represents the asynchronous operation. 
  /// The task result is a <see cref="FileInfo"/> object with the corrected extension if necessary.
  /// </returns>
  /// <remarks>
  /// This method currently simulates asynchronous execution using <c>Task.Yield</c>.
  /// It can be extended to include real asynchronous I/O operations in the future.
  /// </remarks>
  public static async Task<FileInfo> CheckFCPFileExtensionAsync(string filename)
  {
    //Simulate asynchronous execution if I/O is added later
    await Task.Yield();

    var result = new FileInfo(filename);
    var ext = FileCompressPackage.EXTENSION;

    if (result.Extension.SequenceEqual(ext))
      return result;

    var fullname = result.FullName;
    var folder = result.DirectoryName;
    var fname = Path.GetFileNameWithoutExtension(fullname);

    return new FileInfo(Path.Combine(folder!, fname + ext));

  }
}
