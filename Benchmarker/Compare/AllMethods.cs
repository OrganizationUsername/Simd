using System.Diagnostics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Benchmarker;

public partial class AllMethods : BaseBenchmarker
{
    public string[] GetAllFiveLetterWords() => File.ReadAllLines("FiveLetterWords.txt").ToArray();

    public int GetWordCountSimd(string[] words, List<uint> filters)
    {
        //wordList = new[] { "robot", "doggy", "mints", "shots", "abash", "ayala", "aural", "brine", "chive", "chili" };
        var intArray = words.Select(x => StringToInt(x)).ToArray();
        WordCheckSimd(intArray, filters.ToArray());
        return intArray.Count(x => x > 0);
    }

    public int GetWordCountScalar(string[] words, List<uint> filters)
    {
        //wordList = new[] { "robot", "doggy", "mints", "shots", "abash", "ayala", "aural", "brine", "chive", "chili" };
        var intArray = words.Select(x => StringToInt(x)).ToArray();
        WordCheckScalar(intArray, filters.ToArray());
        return intArray.Count(x => x > 0);
    }


    public int FilterAllWordsForSSimd()
    {
        var am = new AllMethods() { Count = 12 };
        am.GlobalSetup();
        var filters = new List<uint>();
        filters.Add(AllMethods.GetLetterFilter(1, 2, 's'));
        return am.GetWordCountSimd(am.RealFullWordList, filters);
    }



    public int FilterAllWordsForSScalar()
    {
        var am = new AllMethods() { Count = 12 };
        am.GlobalSetup();
        var filters = new List<uint>();
        filters.Add(AllMethods.GetLetterFilter(1, 2, 's'));
        return am.GetWordCountScalar(am.RealFullWordList, filters);
    }


    public int CheckWordFilterMultipleChars()
    {
        var am = new AllMethods() { Count = 12 };
        am.GlobalSetup();
        var filters = new List<uint>();
        filters.Add(AllMethods.GetLetterFilter(1, 2, 's'));
        return am.GetWordCountSimd(am.wordList, filters);
    }

    public static uint GetLetterFilter(uint minLetters, uint maxLetters, uint letter)
    {
        return (minLetters << 8) + (maxLetters << 5) + letter - 'a';
    }

    unsafe uint WordCheckSimd(uint[] left, uint[] right)
    {
        uint result = 0;
        var offset = Vector128<uint>.Count;
        var Summer = Vector128<uint>.Zero;
        var RunningCount = Vector128<uint>.Zero;
        var oneMask = Vector128.Create(1u, 1u, 1u, 1u);
        var LetterCountMask = Vector128.Create((uint)0b111, 0b111, 0b111, 0b111);
        var product = Vector128<uint>.Zero;
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
                    var letterMask = Sse2.And(Letters, FiveMax);

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
                    product = Sse2.And(Sse2.And(CopyOfWord, hh), product);
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
            foreach (var y in right)
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

    unsafe uint WordCheckScalar(uint[] left, uint[] right)
    {
        uint result = 0;
        var offset = Vector128<uint>.Count;
        var Summer = Vector128<uint>.Zero;
        var RunningCount = Vector128<uint>.Zero;
        var oneMask = Vector128.Create(1u, 1u, 1u, 1u);
        var LetterCountMask = Vector128.Create((uint)0b111, 0b111, 0b111, 0b111);
        var product = Vector128<uint>.Zero;
        var FiveMax = Vector128.Create((uint)0b11111, (uint)0b11111, (uint)0b11111, (uint)0b11111);
        result = 0;
        var i = 0;
        fixed (uint* qSource = right)
        fixed (uint* pSource = left)
        {
            var lastBlockIndex = left.Length - (left.Length % offset);

            
        }
        for (; i < left.Count(); i++)
        {
            var word = left[i];
            foreach (var y in right)
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
}