using BenchmarkDotNet.Attributes;

namespace Benchmarker.Adding;

[SimpleJob]
[MemoryDiagnoser]
public class PruningBenchmarks : AllMethods
{
    [Benchmark(Baseline = true)]
    public int ScalarFilterAllWordsForS() => FilterAllWordsForSScalarBenchmark();

    [Benchmark]
    public int SimdFilterAllWordsForS() => FilterAllWordsForSSimdBenchmark();

    [Benchmark]
    public int SimdFilterAllWordsForSForeach() => CheckWordFilterMultipleCharsSimdForEachBenchmark();
}