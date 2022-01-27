using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Benchmarker;

public partial class AllMethods : BaseBenchmarker
{
    
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

    //[Benchmark]
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

    //[Benchmark]
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