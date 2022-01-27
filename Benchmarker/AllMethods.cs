using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using BenchmarkDotNet.Attributes;

namespace Benchmarker;

[ShortRunJob]
[MemoryDiagnoser]
public class AllMethods : BaseBenchmarker
{
    public int AddIntLinq() => _ints.Sum();

    public int AddIntFor()
    {
        var result = 0;
        foreach (var x in _ints) { result += x; }
        return result;
    }

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

    public int AddIntAvx2Span() => SumVectorizedAvx2(_ints.AsSpan());

    public unsafe int SumVectorizedAvx2(ReadOnlySpan<int> source)
    {
        int result = 0;
        var offset = Vector256<int>.Count;

        fixed (int* pSource = source)
        {
            var vectorResult = Vector256<int>.Zero;
            var i = 0;
            var lastBlockIndex = source.Length - (source.Length % offset);
            for (; i < lastBlockIndex; i += offset)
            {
                vectorResult = Avx2.Add(vectorResult, Avx2.LoadVector256(pSource + i));
            }

            var vectorResult2 = Avx2.HorizontalAdd(vectorResult, vectorResult);

            result = vectorResult2.GetElement(0) +
                     vectorResult2.GetElement(1) +
 
                     vectorResult2.GetElement(4) +
                     vectorResult2.GetElement(5) ;

            for (; i < source.Length; i++)
            {
                result += pSource[i];
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
        var sum = Vector<double>.Zero;
        var doubleResult = 0d;
        var offset = Vector256<double>.Count;
        int i;

        for (i = 0; i + offset < _left.Length; i += offset)
        {
            var v1 = new Vector<double>(_left, i);
            var v2 = new Vector<double>(_right, i);
            sum += (v1 * v2);
        }

        //remaining items
        for (; i < _left.Length; ++i) { doubleResult += _left[i] * _right[i]; }
        return new[] { doubleResult + Vector.Dot(sum, Vector<double>.One) };
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
            var i = 0;
            var lastBlockIndex = left.Length - (left.Length % offset);
            while (i < lastBlockIndex)
            {
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