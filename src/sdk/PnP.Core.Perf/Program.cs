using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using PnP.Core.Perf.BenchMarks;
using System;
using System.Security.Cryptography;

namespace PnP.Core.Perf
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            //JsonMapping jm = new JsonMapping();
            //Console.WriteLine(jm.CamelCase());
            //Console.WriteLine(jm.CamelCaseSpan());
            

#else
            var summary = BenchmarkRunner.Run<JsonMapping>();
#endif
        }
    }

    //[MemoryDiagnoser]
    //public class Md5VsSha256
    //{
    //    private const int N = 10000;
    //    private readonly byte[] data;

    //    private readonly SHA256 sha256 = SHA256.Create();
    //    private readonly MD5 md5 = MD5.Create();

    //    public Md5VsSha256()
    //    {
    //        data = new byte[N];
    //        new Random(42).NextBytes(data);
    //    }

    //    [Benchmark]
    //    public byte[] Sha256() => sha256.ComputeHash(data);

    //    [Benchmark]
    //    public byte[] Md5() => md5.ComputeHash(data);
    //}
}
