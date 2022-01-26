using BenchmarkDotNet.Running;

namespace OtherBenchmarks;

public class Program
{
    static void Main(string[] args)
    {
        BenchmarkRunner.Run<OtherBenchmarks>();
    }
}