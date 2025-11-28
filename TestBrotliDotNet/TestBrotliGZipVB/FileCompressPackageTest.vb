
Imports System.IO
Imports michele.natale.Compresses
Imports System.Security.Cryptography

Namespace michele.natale.Tests
  Public Class FileCompressPackageTest
    Public Shared Async Function Start() As Task
      Await TestPackNoneFileAsync()
      Await TestPackGZipFileAsync()
      Await TestPackBrotliFileAsync()


      Dim srcfolder = "sourcefolder"
      Await PreparationAsync(srcfolder)
      Await TestPackNoneArchivAsync(srcfolder)
      Await TestPackGZipArchivAsync(srcfolder)
      Await TestPackBrotliArchivAsync(srcfolder)
      Await FinishAsync(srcfolder)

      Console.WriteLine()
    End Function

    Private Shared Async Function TestPackNoneFileAsync() As Task
      Dim outputfolder As String = "output", archivepath As String = "test.fcp"
      Dim packlist = New String() {"data2.txt", "data3.txt", "data2.txt", "data3.txt"}
      Await FileCompressPackage.PackFileAsync(packlist, archivepath, CompressionType.None)
      Await FileCompressPackage.UnPackFileAsync(archivepath, outputfolder)
      If Not FileEqualsSpec(packlist, outputfolder) Then Throw New Exception()
      Console.WriteLine()
    End Function

    Private Shared Async Function TestPackGZipFileAsync() As Task
      Dim outputfolder As String = "output", archivepath As String = "test.fcp"
      Dim packlist = New String() {"data2.txt", "data3.txt", "data2.txt", "data3.txt"}
      Await FileCompressPackage.PackFileAsync(packlist, archivepath, CompressionType.GZip)
      Await FileCompressPackage.UnPackFileAsync(archivepath, outputfolder)
      If Not FileEqualsSpec(packlist, outputfolder) Then Throw New Exception()
      Console.WriteLine()
    End Function

    Private Shared Async Function TestPackBrotliFileAsync() As Task
      Dim outputfolder As String = "output", archivepath As String = "test.fcp"
      Dim packlist = New String() {"data2.txt", "data3.txt", "data2.txt", "data3.txt"}
      Await FileCompressPackage.PackFileAsync(packlist, archivepath, CompressionType.Brotli)
      Await FileCompressPackage.UnPackFileAsync(archivepath, outputfolder)
      If Not FileEqualsSpec(packlist, outputfolder) Then Throw New Exception()
      Console.WriteLine()
    End Function

    Private Shared Async Function TestPackNoneArchivAsync(srcfolder As String) As Task
      Dim outputfolder As String = "output"
      Dim archivepath As String = "test.fcp"

      ' Pack Folder
      Await FileCompressPackage.PackArchivAsync(srcfolder, archivepath, CompressionType.None)

      ' UnPack Archiv
      Await FileCompressPackage.UnPackArchivAsync(archivepath, outputfolder)

      If Not FileEqualsSpec(srcfolder, outputfolder) Then
        Throw New Exception()
      End If

      Console.WriteLine()
    End Function

    Private Shared Async Function TestPackGZipArchivAsync(srcfolder As String) As Task
      Dim outputfolder As String = "output"
      Dim archivepath As String = "test.fcp"

      ' Pack Folder
      Await FileCompressPackage.PackArchivAsync(srcfolder, archivepath, CompressionType.GZip)

      ' UnPack Archiv
      Await FileCompressPackage.UnPackArchivAsync(archivepath, outputfolder)

      If Not FileEqualsSpec(srcfolder, outputfolder) Then
        Throw New Exception()
      End If

      Console.WriteLine()
    End Function

    Private Shared Async Function TestPackBrotliArchivAsync(srcfolder As String) As Task
      Dim outputfolder As String = "output"
      Dim archivepath As String = "test.fcp"

      ' Pack Folder
      Await FileCompressPackage.PackArchivAsync(srcfolder, archivepath, CompressionType.Brotli)

      ' UnPack Archiv
      Await FileCompressPackage.UnPackArchivAsync(archivepath, outputfolder)

      If Not FileEqualsSpec(srcfolder, outputfolder) Then
        Throw New Exception()
      End If

      Console.WriteLine()
    End Function

    Private Shared Function FileEqualsSpec(filelist As String(), outputfolder As String) As Boolean
      For Each file In filelist
        If Not FileEquals(file, Path.Combine(outputfolder, file)) Then Return False
      Next
      Return True
    End Function

    Private Shared Function FileEqualsSpec(srcfolder As String, destfolder As String) As Boolean
      Dim left = New DirectoryInfo(srcfolder) _
          .GetFiles("*.*", SearchOption.AllDirectories) _
          .OrderBy(Function(x) x.FullName) _
          .ToArray()

      Dim right = New DirectoryInfo(destfolder) _
          .GetFiles("*.*", SearchOption.AllDirectories) _
          .OrderBy(Function(x) x.FullName) _
          .ToArray()

      If EqualitySpec(left, right, srcfolder) Then
        Dim length = left.Length
        For i As Integer = 0 To length - 1
          If Not FileEquals(left(i).FullName, right(i).FullName) Then
            Return False
          End If
        Next
        Return True
      End If

      Return False
    End Function

    Private Shared Function FileEquals(left As String, right As String) As Boolean
      Using fleft As New FileStream(left, FileMode.Open, FileAccess.Read)
        Using fright As New FileStream(right, FileMode.Open, FileAccess.Read)
          Using sha As SHA512 = SHA512.Create()
            Return sha.ComputeHash(fleft).SequenceEqual(sha.ComputeHash(fright))
          End Using
        End Using
      End Using
    End Function

    Private Shared Async Function FinishAsync(srcfolder As String) As Task
      If Directory.Exists(srcfolder) Then
        Await Task.Run(Sub() Directory.Delete(srcfolder, True))
      End If
    End Function

    Private Shared Async Function PreparationAsync(srcfolder As String) As Task
      Dim packlist As String() = {"data.txt", "data2.txt", "data3.txt"}
      Await CreateRngFolders(srcfolder, packlist)
    End Function

    Public Shared Async Function CopyFileAsync(sourcepath As String, destinationPath As String, Optional overwrite As Boolean = False) As Task
      If Not File.Exists(sourcepath) Then
        Throw New FileNotFoundException("Source file not found.", sourcepath)
      End If

      If File.Exists(destinationPath) AndAlso overwrite Then
        File.Delete(destinationPath)
      End If

      Using fsin As New FileStream(sourcepath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize:=4096, useAsync:=True)
        Using fsout As New FileStream(destinationPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, bufferSize:=4096, useAsync:=True)
          Await fsin.CopyToAsync(fsout)
        End Using
      End Using
    End Function

    Private Shared Async Function CreateRngFolders(basefolder As String, files As String()) As Task
      Dim rand = Random.Shared

      If Directory.Exists(basefolder) Then
        Directory.Delete(basefolder, True)
      End If

      Directory.CreateDirectory(basefolder)
      For i As Integer = 0 To 2
        Dim subroot = Path.Combine(basefolder, RngFolderName(8))
        Directory.CreateDirectory(subroot)

        Dim current = subroot
        For depth As Integer = 0 To 2
          current = Path.Combine(current, RngFolderName(8))
          Directory.CreateDirectory(current)

          If rand.NextDouble() < 0.95 Then ' 95% Chance
            Dim file = files(rand.Next(files.Length))
            Dim dest = Path.Combine(current, file)
            Await CopyFileAsync(file, dest, overwrite:=True)
          End If
        Next
      Next
    End Function

    Private Shared Function RngFolderName(size As Integer) As String
      Return Guid.NewGuid().ToString("N").Substring(0, size)
    End Function

    Private Shared Function EqualitySpec(left As FileInfo(), right As FileInfo(), srcfolder As String) As Boolean
      If left.Length <> right.Length Then
        Return False
      End If

      Dim length = left.Length
      For i As Integer = 0 To length - 1
        Dim idx1 = left(i).FullName.IndexOf(srcfolder, StringComparison.Ordinal)
        Dim idx2 = right(i).FullName.IndexOf(srcfolder, StringComparison.Ordinal)
        If Not left(i).FullName.Substring(idx1).SequenceEqual(right(i).FullName.Substring(idx2)) Then
          Return False
        End If
      Next
      Return True
    End Function
  End Class
End Namespace

