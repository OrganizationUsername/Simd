using BenchmarkDotNet.Attributes;

namespace Benchmarker.Adding;

[SimpleJob]
[MemoryDiagnoser]
public class ProductSumBenchmarks : AllMethods
{
    [Benchmark]
    public double LinqProductSum() => ScalarProd();

    [Benchmark]
    public double ForLoopProductSum() => VectorProd();

    [Benchmark]
    public double SSe2SProductSum() => SimdProdRunnerSse2();

    [Benchmark(Baseline = true)]
    public double SpanLoopProductSum() => SimdProdRunnerAvx2();
}