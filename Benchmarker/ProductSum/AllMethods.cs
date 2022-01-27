using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Benchmarker;

public partial class AllMethods : BaseBenchmarker
{
    public double ScalarProd()
    {
        var doubleResult = 0d;
        int i;
        for (i = 0; i < _left.Length; ++i)
        {
            doubleResult += _left[i] * _right[i];
        }
        return doubleResult;
    }

    public double VectorProd()
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
        return doubleResult + Vector.Dot(sum, Vector<double>.One);
    }

    public double SimdProdRunnerAvx2() => SimdProdAvx2(_left, _right);
    public unsafe double SimdProdAvx2(double[] left, double[] right)
    {
        double result;
        var offset = Vector256<double>.Count;
        fixed (double* pSource = left)
        fixed (double* qSource = right)
        {
            var vector = Vector256<double>.Zero;
            Vector256<double> vTemp;
            var i = 0;
            var lastBlockIndex = left.Length - (left.Length % offset);

            for (; i < lastBlockIndex; i += offset)
            {
                vector = Avx2.Add(vector, Avx2.Multiply(
                    Avx2.LoadVector256(qSource + i),
                    Avx2.LoadVector256(pSource + i)
                ));
            }
            var vectorResult = Avx2.HorizontalAdd(vector, vector);
            result = vectorResult.GetElement(0) + vectorResult.GetElement(2);
            for (; i < left.Length; i++) { result += pSource[i] * qSource[i]; }
        }
        return result;
    }

    public double SimdProdRunnerSse2() => SimdProdSse2(_left, _right);
    public unsafe double SimdProdSse2(double[] left, double[] right)
    {
        double result;
        var offset = Vector128<double>.Count;
        fixed (double* pSource = left)
        fixed (double* qSource = right)
        {
            var vresult = Vector128<double>.Zero;
            var i = 0;
            var lastBlockIndex = left.Length - (left.Length % offset);

            for (; i < lastBlockIndex; i += offset)
            {
                vresult = Sse2.Add(vresult, Sse2.Multiply(
                    Sse2.LoadVector128(qSource + i),
                    Sse2.LoadVector128(pSource + i)
                ));
            }
            vresult = Ssse3.HorizontalAdd(vresult, vresult);
            result = vresult.ToScalar();
            for (; i < left.Length; i++) { result += pSource[i] * qSource[i]; }
        }
        return result;
    }
}