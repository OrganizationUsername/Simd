using System.Diagnostics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Benchmarker;

public partial class AllMethods : BaseBenchmarker
{


    public int GetWordCount(string[] words, List<uint> filters)
    {
        //wordList = new[] { "robot", "doggy", "mints", "shots", "abash", "ayala", "aural", "brine", "chive", "chili" };
        var intArray = words.Select(x => StringToInt(x)).ToArray();
        WordCheck(intArray, filters.ToArray());
        return intArray.Count(x => x > 0);
    }

    public static uint GetLetterFilter(uint minLetters, uint maxLetters, uint letter)
    {
        return (minLetters << 8) + (maxLetters << 5) + letter - 'a';
    }

    unsafe uint WordCheck(uint[] left, uint[] right)
    {
        uint result = 0;
        var offset = Vector128<uint>.Count;
        Vector128<uint> shifter = Vector128.Create(5u, 5u, 5u, 5u);
        Vector128<uint> Summer = Vector128<uint>.Zero;
        Vector128<uint> RunningCount = Vector128<uint>.Zero;
        Vector128<uint> oneMask = Vector128.Create(1u, 1u, 1u, 1u);
        Vector128<uint> letterOnlyMask = Vector128.Create(31u, 31u, 31u, 31u);
        Vector128<uint> TwentyFiveMask = Vector128.Create(33554431u, 33554431u, 33554431u, 33554431u);
        Vector128<uint> LetterCountMask = Vector128.Create((uint)0b111, 0b111, 0b111, 0b111);
        Vector128<uint> product = Vector128<uint>.Zero;
        var FiveMax = Vector128.Create((uint)0b11111, (uint)0b11111, (uint)0b11111, (uint)0b11111);
        result = 0;
        var i = 0;
        fixed (uint* qSource = right)
        fixed (uint* pSource = left)
        {
            var lastBlockIndex = left.Length - (left.Length % offset);

            for (; i < lastBlockIndex; i += offset)
            {
                for (var j = 0; j < right.Length; j++)
                {
                    var TempThing = Sse2.LoadVector128(pSource + i);
                    var CopyOfWord = TempThing;
                    RunningCount = Vector128<uint>.Zero;
                    var Letters = Vector128.Create((uint)right[j], (uint)right[j], (uint)right[j], (uint)right[j]);
                    Vector128<uint> letterMask = Sse2.And(Letters, FiveMax);

                    //Check 5th position
                    Summer = Sse2.CompareEqual(Sse2.And(TempThing, FiveMax), letterMask);
                    RunningCount = Sse2.Add(RunningCount, Sse2.And(Summer, oneMask));
                    TempThing = Sse3.ShiftRightLogical(TempThing, 5);

                    //Check 4th position
                    Summer = Sse2.CompareEqual(Sse2.And(TempThing, FiveMax), letterMask);
                    RunningCount = Sse2.Add(RunningCount, Sse2.And(Summer, oneMask));
                    TempThing = Sse3.ShiftRightLogical(TempThing, 5);

                    //Check 3rd position
                    Summer = Sse2.CompareEqual(Sse2.And(TempThing, FiveMax), letterMask);
                    RunningCount = Sse2.Add(RunningCount, Sse2.And(Summer, oneMask));
                    TempThing = Sse3.ShiftRightLogical(TempThing, 5);

                    //Check 2nd position
                    Summer = Sse2.CompareEqual(Sse2.And(TempThing, FiveMax), letterMask);
                    RunningCount = Sse2.Add(RunningCount, Sse2.And(Summer, oneMask));
                    TempThing = Sse3.ShiftRightLogical(TempThing, 5);

                    //Check 1st position
                    Summer = Sse2.CompareEqual(Sse2.And(TempThing, FiveMax), letterMask);
                    RunningCount = Sse2.Add(RunningCount, Sse2.And(Summer, oneMask));

                    //If the letter count is greater, then null it out.
                    var ac = Sse2.And(LetterCountMask, Sse3.ShiftRightLogical(Letters, 5));
                    var ff = Sse2.CompareGreaterThan(RunningCount.AsInt32(), ac.AsInt32());
                    var gg = Sse2.AndNot(ff, oneMask.AsInt32());
                    var hh = Sse2.CompareEqual(gg.AsUInt32(), oneMask);
                    product = Sse2.And(CopyOfWord, hh);

                    //If the letter count is less, then null it out.
                    ac = Sse2.And(LetterCountMask, Sse3.ShiftRightLogical(Letters, 8));
                    ff = Sse2.CompareGreaterThan(ac.AsInt32(), RunningCount.AsInt32());
                    gg = Sse2.AndNot(ff, oneMask.AsInt32());
                    hh = Sse2.And(hh, Sse2.CompareEqual(gg.AsUInt32(), oneMask));
                    product = Sse2.And(CopyOfWord, hh);
                }
                left[i + 0] = product.GetElement(0);
                left[i + 1] = product.GetElement(1);
                left[i + 2] = product.GetElement(2);
                left[i + 3] = product.GetElement(3);
            }
        }
        for (; i < left.Count(); i++)
        {
            var word = left[i];
            foreach (uint y in right)
            {
                var targetLetter = y & 0b11111;
                var runningCount = 0;
                var maxCount = (y >> 5) & 0b000_111;
                var minCount = ((y >> 5) & 0b111_000) >> 3;
                //count.Dump("count");
                for (var j = 0; j < 5; j++)
                {
                    var letter = word & 0b11111;
                    if (letter == targetLetter)
                    {
                        runningCount++;
                    }
                    word >>= 5;
                }
                if (runningCount > maxCount || runningCount < minCount)
                {
                    left[i] = 0;
                }
            }
        }
        //left.Dump("Final left");
        return result;
    }

    public uint StringToInt(string ss)
    {

        uint l = 0;
        for (var i = 0; i < 5; i++)
        {
            l |= (byte)(ss[i] - 'a');
            l <<= 5;
        }
        return l >> 5;
    }

    public string IntToString(uint l)
    {
        var ii = new int[5];
        for (var i = 0; i < 5; i++)
        {
            ii[5 - i - 1] = (char)((l & 0b11111) + 97);
            l >>= 5;
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