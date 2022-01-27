using BenchmarkDotNet.Attributes;

namespace Benchmarker.Adding;

[SimpleJob]
[MemoryDiagnoser]
public class AddingBenchmarks : AllMethods
{
    [Benchmark]
    public int LinqAddition() => AddIntLinq();

    [Benchmark]
    public int ForLoopAddition() => AddIntFor();

    [Benchmark]
    public int SpanLoopAddition() => AddIntForSpan();

    [Benchmark]
    public int SSe2SAddition() => AddIntSse2Span();

    [Benchmark(Baseline = true)]
    public int Avx2Addition() => AddIntAvx2Span();
}