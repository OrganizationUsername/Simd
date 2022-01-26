using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using BenchmarkDotNet.Attributes;

namespace OtherBenchmarks;

//https://fiigii.com/2019/03/03/Hardware-intrinsic-in-NET-Core-3-0-Introduction/
//https://devblogs.microsoft.com/dotnet/hardware-intrinsics-in-net-core/
//https://docs.microsoft.com/en-us/dotnet/standard/simd
//https://github.com/goldshtn/simd-workshop
//https://store.steampowered.com/hwsurvey Other Settings

[ShortRunJob]
[MemoryDiagnoser]
public class OtherBenchmarks
{
    [Params(1_000, 100_000, 100_000_000)]
    public int Count { get; set; }
    private double[] _left;
    private double[] _right;
    private int[] _ints;

    [GlobalSetup]
    public void GlobalSetup()
    {
        var ran = new Random(1);
        _left = new double[Count];
        _right = new double[Count];
        for (var i = 0; i < Count; i++)
        {
            _left[i] = ran.NextDouble();
            _right[i] = ran.NextDouble();
        }

        _ints = new int[Count];
        for (var i = 0; i < Count; i++)
        {
            _ints[i] = ran.Next(1, 10000);
        }
    }

    //[Benchmark]
    public int AddIntLinq() => _ints.Sum();

    //[Benchmark]
    public int AddIntFor()
    {
        var result = 0;
        foreach (var x in _ints) { result += x; }
        return result;
    }


    //[Benchmark]
    public int AddIntForSpan()
    {
        var result = 0;
        foreach (var x in _ints.AsSpan()) { result += x; }
        return result;
    }

    //[Benchmark(Baseline = true)]
    public int AddIntSse2Span() => SumVectorizedSse2(_ints.AsSpan());

    public unsafe int SumVectorizedSse2(ReadOnlySpan<int> source)
    {
        int result;

        fixed (int* pSource = source)
        {
            var vresult = Vector128<int>.Zero;

            var i = 0;
            var lastBlockIndex = source.Length - (source.Length % 4);

            while (i < lastBlockIndex)
            {
                vresult = Sse2.Add(vresult, Sse2.LoadVector128(pSource + i));
                i += 4;
            }

            if (Ssse3.IsSupported)
            {
                vresult = Ssse3.HorizontalAdd(vresult, vresult);
                vresult = Ssse3.HorizontalAdd(vresult, vresult);
            }
            else
            {
                vresult = Sse2.Add(vresult, Sse2.Shuffle(vresult, 0x4E));
                vresult = Sse2.Add(vresult, Sse2.Shuffle(vresult, 0xB1));
            }
            result = vresult.ToScalar();

            while (i < source.Length)
            {
                result += pSource[i];
                i += 1;
            }
        }

        return result;
    }

    //[Benchmark]
    public int AddIntSse2Array() => SumVectorizedSse2Array(_ints);
    public unsafe int SumVectorizedSse2Array(int[] source)
    {
        int result;

        fixed (int* pSource = source)
        {
            var vresult = Vector128<int>.Zero;

            var i = 0;
            var lastBlockIndex = source.Length - (source.Length % 4);

            while (i < lastBlockIndex)
            {
                vresult = Sse2.Add(vresult, Sse2.LoadVector128(pSource + i));
                i += 4;
            }

            if (Ssse3.IsSupported)
            {
                vresult = Ssse3.HorizontalAdd(vresult, vresult);
                vresult = Ssse3.HorizontalAdd(vresult, vresult);
            }
            else
            {
                vresult = Sse2.Add(vresult, Sse2.Shuffle(vresult, 0x4E));
                vresult = Sse2.Add(vresult, Sse2.Shuffle(vresult, 0xB1));
            }
            result = vresult.ToScalar();

            while (i < source.Length)
            {
                result += pSource[i];
                i += 1;
            }
        }

        return result;
    }

    [Benchmark]
    public double[] ScalarProd()
    {
        var doubleResult = 0d;
        int i;
        for (i = 0; i < _left.Length; ++i)
        {
            doubleResult = _left[i] * _right[i];
        }
        return new[] { doubleResult };
    }

    //ToDo: This one needs to be fixed up. It allocates much more than it should (nearly zero).
    [Benchmark]
    public double[] VectorProd()
    {
        var doubleResult = 0d;
        var offset = Vector256<double>.Count;
        var result = new double[_left.Length];
        int i;
        for (i = 0; i + offset < _left.Length; i += offset)
        {
            var v1 = new Vector<double>(_left, i);
            var v2 = new Vector<double>(_right, i);
            (v1 * v2).CopyTo(result, i);
            doubleResult += result[i];
        }
        //remaining items
        for (; i < _left.Length; ++i) { doubleResult += _left[i] * _right[i]; }
        return new[] { doubleResult };
    }

    [Benchmark(Baseline = true)]
    public double[] SimdProdRunnerAvx2() => SimdProdAvx2(_left, _right);

    public unsafe double[] SimdProdAvx2(double[] left, double[] right)
    {

        double result;
        var offset = Vector256<double>.Count;

        fixed (double* pSource = left)
        fixed (double* qSource = right)
        {
            var vresult = Vector256<double>.Zero;
            Vector256<double> vTemp;
            var i = 0;
            var lastBlockIndex = left.Length - (left.Length % offset);

            while (i < lastBlockIndex)
            {
                vTemp = Avx2.Multiply(
                    Avx2.LoadVector256(qSource + i),
                    Avx2.LoadVector256(pSource + i)
                );
                vresult = Avx2.Add(vresult, vTemp);
                i += offset;
            }


            vresult = Avx2.HorizontalAdd(vresult, vresult);

            result = vresult.ToScalar();

            for (; i < left.Length; i++) { result += pSource[i] * qSource[i]; }
        }
        return new[] { result };
    }

    [Benchmark]
    public double[] SimdProdRunner() => SimdProdSse2(_left, _right);

    public unsafe double[] SimdProdSse2(double[] left, double[] right)
    {
        double result;
        var offset = Vector128<double>.Count;
        fixed (double* pSource = left)
        fixed (double* qSource = right)
        {
            var vresult = Vector128<double>.Zero;
            Vector128<double> vTemp;
            var i = 0;
            var lastBlockIndex = left.Length - (left.Length % offset);
            while (i < lastBlockIndex)
            {
                //vTemp = Sse2.Multiply(
                //    Sse2.LoadVector128(qSource + i),
                //    Sse2.LoadVector128(pSource + i)
                //);
                vresult = Sse2.Add(vresult, Sse2.Multiply(
                    Sse2.LoadVector128(qSource + i),
                    Sse2.LoadVector128(pSource + i)
                ));
                i += offset;
            }
            vresult = Ssse3.HorizontalAdd(vresult, vresult);
            result = vresult.ToScalar();
            for (; i < left.Length; i++) { result += pSource[i] * qSource[i]; }
        }
        return new[] { result };
    }
}

/*
|         Method |  Count |        Mean |     Error |    StdDev | Ratio | RatioSD |    Gen 0 |    Gen 1 |    Gen 2 | Allocated |
|--------------- |------- |------------:|----------:|----------:|------:|--------:|---------:|---------:|---------:|----------:|
|     ScalarProd | 100000 |    51.95 us |  0.733 us |  0.685 us |  1.64 |    0.02 |        - |        - |        - |      32 B |
|     VectorProd | 100000 | 1,089.50 us | 21.105 us | 24.304 us | 34.67 |    0.62 | 177.6123 | 177.4902 | 177.4902 | 800,129 B |
| SimdProdRunner | 100000 |    31.68 us |  0.094 us |  0.084 us |  1.00 |    0.00 |        - |        - |        - |      32 B |
*/

