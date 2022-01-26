using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Xunit;

namespace OtherTests;

public class UnitTest1
{
    public int Count { get; set; } = 10;
    private readonly double[] _left;
    private readonly double[] _right;
    private readonly int[] _ints;

    public UnitTest1()
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
    public void ScalarSum()
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
    public void VectorSum()
    {
        var offset = Vector256<double>.Count;
        var result = new double[_left.Length];
        int i;
        for (i = 0; i + offset < _left.Length; i += offset)
        {
            var v1 = new Vector<double>(_left, i);
            var v2 = new Vector<double>(_right, i);
            (v1 * v2).CopyTo(result, i);
        }
        Debug.WriteLine($"Current sum: {result.Sum()}");
        for (; i < _left.Length; ++i)
        {
            result[i] = _left[i] * _right[i];
        }
        Assert.Equal(2.5105833765556116, result.Sum());
    }

    [Fact]
    public unsafe void SimdSum()
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
    public void AddIntScalar()
    {
        Assert.Equal(66368, _ints.Sum());
    }

    [Fact]
    public void AddIntScalarSpan()
    {
        var result = 0;
        foreach (var x in _ints.AsSpan())
        {
            result += x;
        }
        Assert.Equal(66368, result);
    }

    [Fact]
    public void AddIntSse2()
    {
        var sum = SumVectorizedSse2(_ints.AsSpan());
        Assert.Equal(66368, sum);
    }

    [Fact]
    public unsafe void Sse2ShortBitMaskShuffle()
    {
        var ar = new ushort[] { 1, 2, 3, 4, 5, 6, 7, 8 };

        fixed (ushort* pSource = ar)
        {
            var vector128 = Sse2.LoadVector128(pSource);
            Vector128<ushort> shuffled;

            //Smallest index gets last element, biggest 4 get biggest
            shuffled = Sse2.ShuffleLow(vector128, 0b00_00_00_11);
            shuffled = Sse2.ShuffleHigh(shuffled, 0b11_11_11_11);
            Assert.Equal(4, shuffled.GetElement(0));
            Assert.Equal(1, shuffled.GetElement(1));
            Assert.Equal(1, shuffled.GetElement(2));
            Assert.Equal(1, shuffled.GetElement(3));
            Assert.Equal(8, shuffled.GetElement(4));
            Assert.Equal(8, shuffled.GetElement(5));
            Assert.Equal(8, shuffled.GetElement(6));
            Assert.Equal(8, shuffled.GetElement(7));
        }
    }

    [Fact]
    public unsafe void Sse2IntBitMaskShuffle()
    {
        var ar = new int[] { 1, 2, 3, 4 };

        fixed (int* pSource = ar)
        {
            var vector128 = Sse2.LoadVector128(pSource);
            Vector128<int> shuffled;
            shuffled = Sse2.Shuffle(vector128, BitConverter.GetBytes(0)[0]);
            Assert.Equal(1, shuffled.GetElement(0));
            Assert.Equal(1, shuffled.GetElement(1));
            Assert.Equal(1, shuffled.GetElement(2));
            Assert.Equal(1, shuffled.GetElement(3));

            shuffled = Sse2.Shuffle(vector128, BitConverter.GetBytes(255)[0]);
            Assert.Equal(4, shuffled.GetElement(0));
            Assert.Equal(4, shuffled.GetElement(1));
            Assert.Equal(4, shuffled.GetElement(2));
            Assert.Equal(4, shuffled.GetElement(3));

            //Smallest index gets 3rd element
            shuffled = Sse2.Shuffle(vector128, 0b00_00_00_11);
            Assert.Equal(4, shuffled.GetElement(0));
            Assert.Equal(1, shuffled.GetElement(1));
            Assert.Equal(1, shuffled.GetElement(2));
            Assert.Equal(1, shuffled.GetElement(3));

            //Largest index gets 0th element
            shuffled = Sse2.Shuffle(vector128, 0b11_00_00_00);
            Assert.Equal(1, shuffled.GetElement(0));
            Assert.Equal(1, shuffled.GetElement(1));
            Assert.Equal(1, shuffled.GetElement(2));
            Assert.Equal(4, shuffled.GetElement(3));

            //ascending
            shuffled = Sse2.Shuffle(vector128, 0b11_10_01_00);
            Assert.Equal(1, shuffled.GetElement(0));
            Assert.Equal(2, shuffled.GetElement(1));
            Assert.Equal(3, shuffled.GetElement(2));
            Assert.Equal(4, shuffled.GetElement(3));

            //descending
            int x = 0;
            x += 3 << 0;
            x += 2 << 2;
            x += 1 << 4;
            x += 0 << 6; //Expressing binary in a different way.
            shuffled = Sse2.Shuffle(vector128, BitConverter.GetBytes(x)[0]);
            Assert.Equal(4, shuffled.GetElement(0));
            Assert.Equal(3, shuffled.GetElement(1));
            Assert.Equal(2, shuffled.GetElement(2));
            Assert.Equal(1, shuffled.GetElement(3));
        }
    }

    public unsafe int SumVectorizedSse2(ReadOnlySpan<int> source)
    {
        int result;
        var offset = Vector128<double>.Count;

        fixed (int* pSource = source)
        {
            var vresult = Vector128<int>.Zero;

            var i = 0;
            var lastBlockIndex = source.Length - (source.Length % offset);

            while (i < lastBlockIndex)
            {
                vresult = Sse2.Add(vresult, Sse2.LoadVector128(pSource + i));
                i += offset;
            }
            //
            var vresult2 = Ssse3.HorizontalAdd(vresult, vresult);

            result = vresult2.ToScalar();

            for (; i < _left.Length; i++) { result += pSource[i]; }
        }
        return result;
    }
    [Fact]
    public void SimdProdRunnerAvx2()
    {
        var result = SimdProdAvx2(_left, _right);
        Assert.Equal(2.5105833765556116, result[0]);
    }
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

            result = vresult.GetElement(0) + vresult.GetElement(2);
            //result = vresult.ToScalar();

            for (; i < left.Length; i++) { result += pSource[i] * qSource[i]; }
        }
        return new[] { result };
    }

}