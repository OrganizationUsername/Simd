using BenchmarkDotNet.Attributes;

namespace Benchmarker.Adding;

[SimpleJob]
[MemoryDiagnoser]
public class PruningBenchmarks : AllMethods
{
    [Benchmark(Baseline = true)]
    public int ScalarFilterAllWordsForS() => FilterAllWordsForSScalar();

    [Benchmark]
    public int SimdFilterAllWordsForS() => FilterAllWordsForSSimd();
}