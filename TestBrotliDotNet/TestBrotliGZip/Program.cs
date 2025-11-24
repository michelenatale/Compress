

 

namespace michele.natale.Tests;


public class Program
{
  public async static Task Main()
  {

    await CompressedNetTest.Start();
    await FileCompressPackageTest.Start();


    Console.WriteLine();
    Console.WriteLine("Finish");
    Console.ReadLine();
  }
}
