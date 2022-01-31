using BenchmarkDotNet.Attributes;

namespace Benchmarker.Adding;

//[SimpleJob]
[MemoryDiagnoser]
public class PruningBenchmarks : AllMethods
{
    [Benchmark]
    public void ScalarFilterAllWordsForS() => FilterAllWordsForSScalarBenchmark();

    [Benchmark(Baseline = true)]
    public void SimdFilterAllWordsForSForeach() => CheckWordFilterMultipleCharsSimdBenchmark();

    [Benchmark]
    public void CurrentImplementation()
    {
        (char, int)[] minCounts = { ('s', 1) };
        (char, int)[] maxCounts = { ('s', 2) };
        CurrentImplementationBenchmark(minCounts, maxCounts);
    }

    [Benchmark]
    public void CurrentImplementationLinq()
    {
        (char, int)[] minCounts = { ('s', 1) };
        (char, int)[] maxCounts = { ('s', 2) };
        CurrentImplementationLinqBenchmark(minCounts, maxCounts);
    }
}