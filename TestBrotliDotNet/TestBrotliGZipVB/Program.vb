


Imports TestBrotliGZipVB.michele.natale.Tests

Public Module Program
  Public Sub Main()
    StartAsyncHelper.Start().
      GetAwaiter().GetResult()
  End Sub
End Module

Namespace michele.natale.Tests
  Public Class StartAsyncHelper
    Public Shared Async Function Start() As Task
      Await TestCompressedNet()
      Await TestFileCompressPackage()
    End Function

    Private Shared Async Function TestCompressedNet() As Task
      Await CompressedNetTest.Start()
    End Function

    Private Shared Async Function TestFileCompressPackage() As Task
      Await FileCompressPackageTest.Start()
    End Function
  End Class
End Namespace
