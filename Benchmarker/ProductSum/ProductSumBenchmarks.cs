using BenchmarkDotNet.Attributes;

namespace Benchmarker.Adding;

[SimpleJob]
[MemoryDiagnoser]
public class ProductSumBenchmarks : AllMethods
{
    [Benchmark]
    public double ScalarProductSum() => ScalarProd();

    [Benchmark]
    public double VectorProductSum() => VectorProd();

    [Benchmark]
    public double SSe2SProductSum() => SimdProdRunnerSse2();

    [Benchmark(Baseline = true)]
    public double Avx2ProductSum() => SimdProdRunnerAvx2();
}