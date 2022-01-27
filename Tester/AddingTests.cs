using System;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Benchmarker;
using Xunit;

namespace Tester;

public class AddingTests
{
    public int Count { get; set; } = 12;
    private readonly double[] _left;
    private readonly double[] _right;
    private readonly int[] _int;

    public AddingTests()
    {
        var ran = new Random(1);
        _left = new double[Count];
        _right = new double[Count];
        for (var i = 0; i < 10; i++)
        {
            _left[i] = ran.NextDouble();
            _right[i] = ran.NextDouble();
        }

        _int = new int[12];
        for (var i = 0; i < 12; i++)
        {
            _int[i] = ran.Next(1, 10000);
        }
    }

    [Fact]
    public void Avx2Sum()
    {
        var am = new AllMethods() { Count = 10 };
        am.GlobalSetup();
        var result = am.AddIntAvx2Span();
        Assert.Equal(66368, result);
    }

    [Fact]
    public void AddIntScalar() => Assert.Equal(66368, _int.Sum());

    [Fact]
    public void AddIntScalarSpan()
    {
        var result = 0;
        foreach (var x in _int.AsSpan()) { result += x; }
        Assert.Equal(66368, result);
    }

    [Fact]
    public void AddIntSse2() => Assert.Equal(66368, SumVectorizedSse2(_int.AsSpan()));

    public unsafe int SumVectorizedSse2(ReadOnlySpan<int> source)
    {
        int result;
        var offset = Vector128<double>.Count;

        fixed (int* pSource = source)
        {
            var vectorResult = Vector128<int>.Zero;

            var i = 0;
            var lastBlockIndex = source.Length - (source.Length % offset);

            for (; i < lastBlockIndex; i += offset)
            {
                vectorResult = Sse2.Add(vectorResult, Sse2.LoadVector128(pSource + i));
            }

            vectorResult = Ssse3.HorizontalAdd(vectorResult, vectorResult);

            result = vectorResult.ToScalar();

            for (; i < source.Length; i++) { result += pSource[i]; }
        }
        return result;
    }


}