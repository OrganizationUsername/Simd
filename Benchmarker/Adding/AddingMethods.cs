using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Benchmarker;

public partial class AllMethods : BaseBenchmarker
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
        var offset = Vector128<int>.Count;
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

            vresult = Ssse3.HorizontalAdd(vresult, vresult);
            vresult = Ssse3.HorizontalAdd(vresult, vresult);

            result = vresult.ToScalar();
            for (; i < source.Length; i++)
            {
                result += pSource[i];
            }
        }
        return result;
    }

    public int AddIntAvx2Span() => SumVectorizedAvx2(_ints.AsSpan());

    public unsafe int SumVectorizedAvx2(ReadOnlySpan<int> source)
    {
        var result = 0;
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
                     vectorResult2.GetElement(5);

            for (; i < source.Length; i++)
            {
                result += pSource[i];
            }
        }
        return result;
    }


}