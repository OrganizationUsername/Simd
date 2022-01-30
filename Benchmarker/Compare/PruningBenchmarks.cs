using BenchmarkDotNet.Attributes;

namespace Benchmarker.Adding;

[SimpleJob]
[MemoryDiagnoser]
public class PruningBenchmarks : AllMethods
{
    [Benchmark(Baseline = true)]
    public void ScalarFilterAllWordsForS() => FilterAllWordsForSScalarBenchmark();

    [Benchmark]
    public void SimdFilterAllWordsForSForeach() => CheckWordFilterMultipleCharsSimdBenchmark();
}