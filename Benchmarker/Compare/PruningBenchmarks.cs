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
}