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
        return WordCheck(a, b);
    }

    public unsafe uint WordCheck(uint[] left, uint[] right)
    {
        uint result = 0;
        var offset = Vector128<uint>.Count;
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
                var r = Sse2.LoadVector128(qSource + i);

                vresult = Sse2.And(l, r);
            }

            for (int j = 0; j < offset; j++)
            {
                result += vresult.GetElement(j);
            }

            result = vresult.ToScalar();
            for (; i < left.Length; i++) { }
        }
        return result;
    }
}