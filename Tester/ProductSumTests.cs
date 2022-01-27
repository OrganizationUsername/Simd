using System;
using System.Linq;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Xunit;

namespace Tester;

public class ProductSumTests
{
    public int Count { get; set; } = 12;
    private readonly double[] _left;
    private readonly double[] _right;
    private readonly int[] _ints;

    public ProductSumTests()
    {
        var ran = new Random(1);
        _left = new double[Count];
        _right = new double[Count];
        for (var i = 0; i < 10; i++)
        {
            _left[i] = ran.NextDouble();
            _right[i] = ran.NextDouble();
        }

        _ints = new int[12];
        for (var i = 0; i < 12; i++)
        {
            _ints[i] = ran.Next(1, 10000);
        }
    }

    [Fact]
    public void ScalarProductSum()
    {
        var result = new double[_left.Length];
        int i;
        for (i = 0; i < _left.Length; ++i)
        {
            result[i] = _left[i] * _right[i];
        }

        Assert.Equal(2.5105833765556116, result.Sum());
    }

    [Fact]
    public void VectorProductSum()
    {
        var sum = Vector<double>.Zero;
        var offset = Vector256<double>.Count;
        var result = new double[_left.Length];
        int i;
        for (i = 0; i + offset < _left.Length; i += offset)
        {
            var v1 = new Vector<double>(_left, i);
            var v2 = new Vector<double>(_right, i);
            sum += (v1 * v2);
        }
        for (; i < _left.Length; ++i)
        {
            result[i] = _left[i] * _right[i];
        }
        Assert.Equal(2.5105833765556116, result.Sum() + Vector.Dot(sum, Vector<double>.One));
    }

    [Fact]
    public unsafe void SimdProductSum()
    {
        double result;

        fixed (double* pSource = _left)
        fixed (double* qSource = _right)
        {
            var offset = Vector128<double>.Count;

            var vresult = Vector128<double>.Zero;
            var vTemp = Vector128<double>.Zero;
            var i = 0;
            var lastBlockIndex = _left.Length - (_left.Length % offset);

            while (i < lastBlockIndex)
            {
                var a = Sse2.LoadVector128(qSource + i);
                var b = Sse2.LoadVector128(pSource + i);
                vTemp = Sse2.Multiply(a, b);
                vresult = Sse2.Add(vresult, vTemp);
                i += offset;
            }

            var vresult1 = Ssse3.HorizontalAdd(vresult, vresult);

            result = vresult1.ToScalar();

            for (; i < _left.Length; i++) { result += _left[i] * _right[i]; }
        }

        Assert.Equal(2.5105833765556116, result);
    }
    
    [Fact]
    public void SimdProdRunnerAvx2()
    {
        var result = SimdProdAvx2(_left, _right);
        Assert.Equal(2.5105833765556116, result);
    }

    public unsafe double SimdProdAvx2(double[] left, double[] right)
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

            result = vresult.GetElement(0) + vresult.GetElement(2);

            for (; i < left.Length; i++) { result += pSource[i] * qSource[i]; }
        }
        return   result ;
    }

}