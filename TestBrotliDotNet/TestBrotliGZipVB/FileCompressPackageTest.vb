
Imports System.IO
Imports michele.natale.Compresses
Imports System.Security.Cryptography

Namespace michele.natale.Tests
  Public Class FileCompressPackageTest
    Public Shared Async Function Start() As Task
      Await TestPackNoneAsync()
      Await TestPackGZipAsync()
      Await TestPackBrotliAsync()
      Console.WriteLine()
    End Function

    Private Shared Async Function TestPackNoneAsync() As Task
      Dim outputfolder As String = "output", archivepath As String = "test.fcp"
      Dim packlist = New String() {"data2.txt", "data3.txt", "data2.txt", "data3.txt"}
      Await FileCompressPackage.PackAsync(packlist, archivepath, CompressionType.None)
      Await FileCompressPackage.UnPackAsync(archivepath, outputfolder)
      If Not FileEqualsSpec(packlist, outputfolder) Then Throw New Exception()
      Console.WriteLine()
    End Function

    Private Shared Async Function TestPackGZipAsync() As Task
      Dim outputfolder As String = "output", archivepath As String = "test.fcp"
      Dim packlist = New String() {"data2.txt", "data3.txt", "data2.txt", "data3.txt"}
      Await FileCompressPackage.PackAsync(packlist, archivepath, CompressionType.GZip)
      Await FileCompressPackage.UnPackAsync(archivepath, outputfolder)
      If Not FileEqualsSpec(packlist, outputfolder) Then Throw New Exception()
      Console.WriteLine()
    End Function

    Private Shared Async Function TestPackBrotliAsync() As Task
      Dim outputfolder As String = "output", archivepath As String = "test.fcp"
      Dim packlist = New String() {"data2.txt", "data3.txt", "data2.txt", "data3.txt"}
      Await FileCompressPackage.PackAsync(packlist, archivepath, CompressionType.Brotli)
      Await FileCompressPackage.UnPackAsync(archivepath, outputfolder)
      If Not FileEqualsSpec(packlist, outputfolder) Then Throw New Exception()
      Console.WriteLine()
    End Function

    Private Shared Function FileEqualsSpec(filelist As String(), outputfolder As String) As Boolean
      For Each file In filelist
        If Not FileEquals(file, Path.Combine(outputfolder, file)) Then Return False
      Next
      Return True
    End Function

    Private Shared Function FileEquals(left As String, right As String) As Boolean
      Using fleft = New FileStream(left, FileMode.Open, FileAccess.Read)
        Using fright = New FileStream(left, FileMode.Open, FileAccess.Read)
          Using sha = SHA512.Create()
            Return sha.ComputeHash(fleft).SequenceEqual(sha.ComputeHash(fright))
          End Using
        End Using
      End Using
    End Function
  End Class
End Namespace

