

Imports System.IO
Imports System.Text
Imports System.Threading
Imports michele.natale.Services
Imports michele.natale.Compresses


Namespace michele.natale.Tests
  Public Class CompressedNetTest

    Private Shared ReadOnly Property OriginalString As String
      Get
        Return File.ReadAllText("data.txt") '5.294 MB
      End Get
    End Property

    Public Shared Async Function Start() As Task
      TestGzip()
      Await TestGzipAsync()

      TestGzipFileStream()
      Await TestGzipFileStreamAsync()

      'Await TestGzipStressAsync() 'Approx 135 MB

      TestBrotli()
      Await TestBrotliAsync()

      TestBrotliStream()
      Await TestBrotliStreamAsync()

      'Await TestBrotliStressAsync() 'Approx 135 MB

      Console.WriteLine()
    End Function

    Private Shared Sub TestGzip()
      Dim message = Encoding.UTF8.GetBytes(OriginalString)

      'GZip Classic
      Console.WriteLine("GZip Classic")
      Dim compress = CompressedNet.CompressGZip(message)
      Console.WriteLine("Length of compressed bytes: " & compress.Length)

      Dim decompress = CompressedNet.DecompressGZip(compress)
      Console.WriteLine("Length of decompressed bytes: " & decompress.Length)
      Console.WriteLine()
      If Not message.SequenceEqual(decompress) Then Throw New Exception()

      'GZip with CancellationToken
      Console.WriteLine("GZip with CancellationToken")
      Dim cts = New CancellationTokenSource()
      compress = CompressedNet.CompressGZip(message, cts.Token)
      Console.WriteLine("Length of compressed bytes: " & compress.Length)

      cts = New CancellationTokenSource()
      decompress = CompressedNet.DecompressGZip(compress, cts.Token)
      Console.WriteLine("Length of decompressed bytes: " & decompress.Length)
      Console.WriteLine()
      If Not message.SequenceEqual(decompress) Then Throw New Exception()

      Console.WriteLine()
    End Sub

    Private Shared Async Function TestGzipAsync() As Task
      Dim message = Encoding.UTF8.GetBytes(OriginalString)

      'GZip Async 
      Console.WriteLine("GZip Async")
      Dim compress1 = Await CompressedNet.CompressGZipAsync(message)
      Console.WriteLine("Length of compressed bytes: " & compress1.Length)

      Dim decompress1 = Await CompressedNet.DecompressGZipAsync(compress1)
      Console.WriteLine("Length of decompressed bytes: " & decompress1.Length)
      Console.WriteLine()

      If Not message.SequenceEqual(decompress1) Then Throw New Exception()

      'GZip Async with CancellationToken
      Console.WriteLine("GZip Async with CancellationToken")
      Dim cts = New CancellationTokenSource()
      Dim compress2 = Await CompressedNet.CompressGZipAsync(message, cts.Token)
      Console.WriteLine("Length of compressed string: " & compress2.Length)

      cts = New CancellationTokenSource()
      Dim decompress2 = Await CompressedNet.DecompressGZipAsync(compress2, cts.Token)
      Console.WriteLine("Length of decompressed string: " & decompress2.Length)
      Console.WriteLine()
      If Not message.SequenceEqual(decompress2) Then Throw New Exception()

      Console.WriteLine()
    End Function

    Private Shared Sub TestGzipFileStream()
      Dim src As String = "data.txt"
      Dim dest As String = "datacompress"
      Dim destr As String = "datar.txt"
      File.Delete(dest)
      File.Delete(destr)

      'GZip Stream Classic
      Console.WriteLine("GZip Stream Classic")
      CompressedNet.CompressGZip(src, dest)
      Console.WriteLine("Length of compressed bytes: " & ServicesCompress.FileSize(dest))

      CompressedNet.DecompressGZip(dest, destr)
      Console.WriteLine("Length of decompressed bytes: " & ServicesCompress.FileSize(destr))
      Console.WriteLine()
      If Not ServicesCompress.FileEquals(src, destr) Then Throw New Exception()

      'GZip Stream with CancellationToken
      Console.WriteLine("GZip Stream with CancellationToken")
      Dim cts = New CancellationTokenSource()
      CompressedNet.CompressGZip(src, dest, cts.Token)
      Console.WriteLine("Length of compressed bytes: " & ServicesCompress.FileSize(dest))

      cts = New CancellationTokenSource()
      CompressedNet.DecompressGZip(dest, destr, cts.Token)
      Console.WriteLine("Length of decompressed bytes: " & ServicesCompress.FileSize(destr))
      Console.WriteLine()
      If Not ServicesCompress.FileEquals(src, destr) Then Throw New Exception()

      Console.WriteLine()
    End Sub


    Private Shared Async Function TestGzipFileStreamAsync() As Task
      Dim src As String = "data.txt"
      Dim dest As String = "datacompress"
      Dim destr As String = "datar.txt"
      File.Delete(dest) : File.Delete(destr)

      'GZip Stream Async
      Console.WriteLine("GZip Stream Async")
      Await CompressedNet.CompressGZipAsync(src, dest)
      Console.WriteLine("Length of compressed bytes: " & ServicesCompress.FileSize(dest))

      Await CompressedNet.DecompressGZipAsync(dest, destr)
      Console.WriteLine("Length of decompressed bytes: " & ServicesCompress.FileSize(destr))
      Console.WriteLine()
      If Not ServicesCompress.FileEquals(src, destr) Then Throw New Exception()

      File.Delete(dest) : File.Delete(destr)

      'GZip Stream Async with CancellationToken
      Console.WriteLine("GZip Stream Async with CancellationToken")

      Dim cts As New CancellationTokenSource()
      Await CompressedNet.CompressGZipAsync(src, dest, cts.Token)
      Console.WriteLine("Length of compressed bytes: " & ServicesCompress.FileSize(dest))

      cts = New CancellationTokenSource()
      Await CompressedNet.DecompressGZipAsync(dest, destr, cts.Token)
      Console.WriteLine("Length of decompressed bytes: " & ServicesCompress.FileSize(destr))
      Console.WriteLine()
      If Not ServicesCompress.FileEquals(src, destr) Then Throw New Exception()

      Console.WriteLine()
    End Function

    Private Shared Sub TestBrotli()
      Dim message As Byte() = Encoding.UTF8.GetBytes(OriginalString)

      'Brotli Classic
      Console.WriteLine("Brotli Classic")
      Dim compress As Byte() = CompressedNet.CompressBrotli(message)
      Console.WriteLine("Length of compressed bytes: " & compress.Length)

      Dim decompress As Byte() = CompressedNet.DecompressBrotli(compress)
      Console.WriteLine("Length of decompressed bytes: " & decompress.Length)
      Console.WriteLine()
      If Not message.SequenceEqual(decompress) Then Throw New Exception()

      'Brotli with CancellationToken
      Console.WriteLine("Brotli with CancellationToken")
      Dim cts As New CancellationTokenSource()
      compress = CompressedNet.CompressBrotli(message, cts.Token)
      Console.WriteLine("Length of compressed bytes: " & compress.Length)

      cts = New CancellationTokenSource()
      decompress = CompressedNet.DecompressBrotli(compress, cts.Token)
      Console.WriteLine("Length of decompressed bytes: " & decompress.Length)
      Console.WriteLine()
      If Not message.SequenceEqual(decompress) Then Throw New Exception()

      'Brotli Try
      Console.WriteLine("Brotli Try")
      Dim quality As Integer = 4
      Dim window As Integer = 22
      Dim written As Integer
      If Not CompressedNet.TryCompressBrotli(message, compress, written, quality, window) Then
        Throw New Exception()
      End If
      Console.WriteLine("Length of compressed bytes: " & written)

      If Not CompressedNet.TryDecompressBrotli(compress, decompress, written) Then
        Throw New Exception()
      End If
      Console.WriteLine("Length of decompressed bytes: " & written)
      Console.WriteLine()
      If Not message.SequenceEqual(decompress) Then Throw New Exception()

      'Brotli Try with CancellationToken
      Console.WriteLine("Brotli Try with CancellationToken")
      cts = New CancellationTokenSource()
      If Not CompressedNet.TryCompressBrotli(message, cts.Token, compress, written, quality, window) Then
        Throw New Exception()
      End If
      Console.WriteLine("Length of compressed bytes: " & written)

      If Not CompressedNet.TryDecompressBrotli(compress, cts.Token, decompress, written) Then
        Throw New Exception()
      End If
      Console.WriteLine("Length of decompressed bytes: " & written)
      Console.WriteLine()
      If Not message.SequenceEqual(decompress) Then Throw New Exception()

      Console.WriteLine()
    End Sub

    Private Shared Async Function TestBrotliAsync() As Task
      Dim message = Encoding.UTF8.GetBytes(OriginalString)

      'Brotli Async 
      Console.WriteLine("Brotli Async")
      Dim compress1 = Await CompressedNet.CompressBrotliAsync(message)
      Console.WriteLine("Length of compressed bytes: " & compress1.Length)

      Dim decompress1 = Await CompressedNet.DecompressBrotliAsync(compress1)
      Console.WriteLine("Length of decompressed bytes: " & decompress1.Length)
      Console.WriteLine()
      If Not message.SequenceEqual(decompress1) Then Throw New Exception()

      'Brotli Async with CancellationToken
      Console.WriteLine("Brotli Async with CancellationToken")
      Dim cts = New CancellationTokenSource()
      Dim compress2 = Await CompressedNet.CompressBrotliAsync(message, cts.Token)
      Console.WriteLine("Length of compressed string: " & compress2.Length)

      cts = New CancellationTokenSource()
      Dim decompress2 = Await CompressedNet.DecompressBrotliAsync(compress2, cts.Token)
      Console.WriteLine("Length of decompressed string: " & decompress2.Length)
      Console.WriteLine()
      If Not message.SequenceEqual(decompress2) Then Throw New Exception()

      Console.WriteLine()
    End Function

    Private Shared Sub TestBrotliStream()
      Dim src As String = "data.txt"
      Dim dest As String = "datacompress"
      Dim destr As String = "datar.txt"
      File.Delete(dest)
      File.Delete(destr)

      'Brotli Stream Classic
      Console.WriteLine("Brotli Stream Classic")
      CompressedNet.CompressBrotli(src, dest)
      Console.WriteLine("Length of compressed bytes: " & ServicesCompress.FileSize(dest))

      CompressedNet.DecompressBrotli(dest, destr)
      Console.WriteLine("Length of decompressed bytes: " & ServicesCompress.FileSize(destr))
      Console.WriteLine()
      If Not ServicesCompress.FileEquals(src, destr) Then Throw New Exception()

      'Brotli Stream with CancellationToken
      Console.WriteLine("Brotli Stream with CancellationToken")
      Dim cts = New CancellationTokenSource()
      CompressedNet.CompressBrotli(src, dest, cts.Token)
      Console.WriteLine("Length of compressed bytes: " & ServicesCompress.FileSize(dest))

      cts = New CancellationTokenSource()
      CompressedNet.DecompressBrotli(dest, destr, cts.Token)
      Console.WriteLine("Length of decompressed bytes: " & ServicesCompress.FileSize(destr))
      Console.WriteLine()
      If Not ServicesCompress.FileEquals(src, destr) Then Throw New Exception()

      Console.WriteLine()
    End Sub

    Private Shared Async Function TestBrotliStreamAsync() As Task
      Dim src As String = "data.txt"
      Dim destr As String = "datar.txt"
      Dim dest As String = "datacompress"
      File.Delete(dest) : File.Delete(destr)

      'Brotli Stream Async
      Console.WriteLine("Brotli Stream Async")
      Await CompressedNet.CompressBrotliAsync(src, dest)
      Console.WriteLine("Length of compressed bytes: " & ServicesCompress.FileSize(dest))

      Await CompressedNet.DecompressBrotliAsync(dest, destr)
      Console.WriteLine("Length of decompressed bytes: " & ServicesCompress.FileSize(destr))
      Console.WriteLine()
      If Not ServicesCompress.FileEquals(src, destr) Then Throw New Exception()

      File.Delete(dest)
      File.Delete(destr)

      'Brotli Stream Async with CancellationToken
      Console.WriteLine("Brotli Stream Async with CancellationToken")
      Dim cts As New CancellationTokenSource()
      Await CompressedNet.CompressBrotliAsync(src, dest, cts.Token)
      Console.WriteLine("Length of compressed bytes: " & ServicesCompress.FileSize(dest))

      cts = New CancellationTokenSource()
      Await CompressedNet.DecompressBrotliAsync(dest, destr, cts.Token)
      Console.WriteLine("Length of decompressed bytes: " & ServicesCompress.FileSize(destr))
      Console.WriteLine()
      If Not ServicesCompress.FileEquals(src, destr) Then Throw New Exception()

      Console.WriteLine()
    End Function
  End Class
End Namespace
