using BenchmarkDotNet.Running;
using Benchmarker.Adding;

namespace Benchmarker;

public class Program
{
    static void Main(string[] args)
    {
        //BenchmarkRunner.Run<AddingBenchmarks>();
        BenchmarkRunner.Run<ProductSumBenchmarks>();
    }
}