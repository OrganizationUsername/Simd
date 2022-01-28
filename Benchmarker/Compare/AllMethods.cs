using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Benchmarker;

public partial class AllMethods : BaseBenchmarker
{

    public uint StringToInt(string ss)
    {
        uint l = 0;
        for (var i = 0; i < 5; i++)
        {
            l |= (byte)(ss[i] - 'a');
            l <<= 5;
        }
        return l;
    }

    public string IntToString(uint l)
    {
        var ii = new int[5];
        for (var i = 0; i < 5; i++)
        {
            l >>= 5;
            ii[5 - i - 1] = (char)((l & 0b11111) + 97);
        }
        return new string(ii.Select(i => (char)i).ToArray());
    }

    public uint WordCompare()
    {
        //wordList = new[] { "robot", "doggy", "mints", "shots", "abash", "ayala", "aural", "brine", "chive", "chili" };
        var a = wordList.Select(w => StringToInt(w)).ToArray();
        var letters = new[] { "sssss", "sssss", "sssss", "sssss", "sssss", "sssss", "sssss", "sssss", "sssss", "sssss" };
        var b = letters.Select(w => StringToInt(w)).ToArray();
        return WordCheck(a.AsSpan(), b);
    }

	unsafe uint WordCheck(ReadOnlySpan<uint> left, ReadOnlySpan<uint> right)
    {
        uint result = 0;
        var offset = Vector128<uint>.Count;
        uint[] sArray = new uint[] { 0U, 0U, 0U, 0U };
        Vector128<uint> sMask = Vector128.Create(623462976U, 623462976, 623462976, 623462976);
        Vector128<uint> firstTwo = Vector128.Create((uint)0b11111, 0b11111, 0b11111, 0b11111);
        //firstTwo.Dump();
        result = 0;
        fixed (uint* qSource = right)
        fixed (uint* pSource = left)
        {
            var vresult = Vector128<uint>.Zero;
            var i = 0;
            var lastBlockIndex = left.Length - (left.Length % offset);

            for (; i < lastBlockIndex; i += offset)
            {
                var l = Sse2.LoadVector128(pSource + i);
                //IntToString(l.GetElement(0)).Dump();
                //l.Dump();
                l = Sse3.ShiftRightLogical128BitLane(l, 2);
                var TempThing = Sse2.And(l, sMask);
                //Sse2.And(TempThing, firstTwo).Dump();
                //"Temp Thing".Dump();
                //TempThing.Dump();
                //IntToString(l.GetElement(0)).Dump();
                //l.Dump();
                var r = Sse2.LoadVector128(qSource + i);
                r = Sse3.ShiftRightLogical128BitLane(r, 2);
                vresult = Sse2.And(l, r);
            }

            for (int j = 0; j < offset; j++)
            {
                result += vresult.GetElement(j);
            }

        }
        return result;
    }
}