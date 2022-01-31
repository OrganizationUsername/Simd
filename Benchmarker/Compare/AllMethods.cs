using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Benchmarker;

public partial class AllMethods : BaseBenchmarker
{
    public int GetWordCountSimd128(string[] words, List<uint> filters)
    {
        var intArray = words.Select(x => StringToInt(x)).ToArray();
        WordCheckSimd128(intArray, filters.ToArray());
        var tempResult = intArray.Where(x => x > 0).Select(x => IntToString(x)).ToArray();
        return intArray.Count(x => x > 0);
    }

    public int GetWordCountSimd128Benchmark(uint[] words, List<uint> filters)
    {
        WordCheckSimd128(words, filters.ToArray());
        return 0;
    }

    public void GetWordCountScalarBenchmark(uint[] words, List<uint> filters) => WordCheckScalar(words, filters.ToArray());

    public int GetWordCountScalar(string[] words, List<uint> filters)
    {
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
        return am.GetWordCountSimd128(am.RealFullWordList, filters);
    }

    public int FilterAllWordsForSScalar()
    {
        var am = new AllMethods() { Count = 12 };
        am.GlobalSetup();
        var filters = new List<uint>();
        filters.Add(AllMethods.GetLetterFilter(1, 2, 's'));
        return am.GetWordCountScalar(am.RealFullWordList, filters);
    }

    public void FilterAllWordsForSScalarBenchmark()
    {
        var filters = new List<uint>();
        filters.Add(AllMethods.GetLetterFilter(1, 2, 's'));
        GetWordCountScalarBenchmark(UintWordList, filters);
    }

    public int CheckWordFilterMultipleCharsSimd()
    {
        var am = new AllMethods() { Count = 12 };
        am.GlobalSetup();
        var filters = new List<uint>();
        filters.Add(AllMethods.GetLetterFilter(1, 2, 's'));
        return am.GetWordCountSimd128(am.wordList, filters);
    }

    public int CurrentImplementationTest((char, int)[] minCounts, (char, int)[] maxCounts)
    {
        var wordListCopy = RealFullWordList.ToArray();
        PrunePossibleWords(wordListCopy, minCounts, maxCounts);
        return wordListCopy.Count(x => x is not null);
    }

    public int CurrentImplementationCompactTest((char, int, int)[] letterCounts)
    {
        var wordListCopy = RealFullWordList.ToArray();
        PrunePossibleWordsCompact(wordListCopy, letterCounts);
        return wordListCopy.Count(x => x is not null);
    }

    public void CurrentImplementationLinqBenchmark((char, int)[] minCounts, (char, int)[] maxCounts)
    {
        PrunePossibleWordsLinq(RealFullWordList, minCounts, maxCounts);
    }

    public void CurrentImplementationCompactBenchmark((char, int, int)[] letterCounts)
    {
        PrunePossibleWordsCompact(RealFullWordList, letterCounts);
    }

    public void CurrentImplementationBenchmark((char, int)[] minCounts, (char, int)[] maxCounts)
    {
        PrunePossibleWords(RealFullWordList, minCounts, maxCounts);
    }

    public void PrunePossibleWordsCompact(string[] wordList, (char letter, int minCount, int maxCount)[] letterCounts)
    {
        for (var i = 0; i < wordList.Length; i++)
        {
            var word = wordList[i];
            if (word is null) continue;
            for (var index = 0; index < letterCounts.Length; index++)
            {
                var n = letterCounts[index];

                int count = 0;
                foreach (var l in word) { if (l == n.letter) count++; }

                if (count < n.minCount || count > n.maxCount)
                {
                    wordList[i] = null;
                    word = null;
                    break;
                }
            }
        }
    }

    public void PrunePossibleWords(IList<string> wordList, (char letter, int count)[] minCounts, (char letter, int count)[] maxCounts)
    {
        for (var i = wordList.Count - 1; i >= 0; i--)
        {
            var word = wordList[i];
            if (word is null) continue;
            for (var index = 0; index < minCounts.Length; index++)
            {
                var n = minCounts[index];

                int count = 0;
                foreach (var l in word) { if (l == n.letter) count++; }

                if (count < n.count)
                {
                    wordList[i] = null;
                    word = null;
                    break;
                }
            }

            if (word is null) continue;
            for (var index = 0; index < maxCounts.Length; index++)
            {
                var n = maxCounts[index];

                int count = 0;
                foreach (var l in word) { if (l == n.letter) count++; }

                if (count > n.count)
                {
                    wordList[i] = null;
                    word = null;
                    break;
                }
            }
        }
    }

    public void PrunePossibleWordsLinq(IList<string> wordList, (char letter, int count)[] minCounts, (char letter, int count)[] maxCounts)
    {
        for (var i = wordList.Count - 1; i >= 0; i--)
        {
            var word = wordList[i];
            if (word is null) continue;
            for (var index = 0; index < minCounts.Length; index++)
            {
                var n = minCounts[index];

                if (word.Count(l => l == n.letter) < n.count)
                {
                    wordList[i] = null;
                    word = null;
                    break;
                }
            }

            if (word is null) continue;
            for (var index = 0; index < maxCounts.Length; index++)
            {
                var n = maxCounts[index];

                if (word.Count(l => l == n.letter) > n.count)
                {
                    wordList[i] = null;
                    word = null;
                    break;
                }
            }
        }
    }

    public int CheckWordFilterMultipleCharsSimdBenchmark()
    {
        var filters = new List<uint>();
        filters.Add(AllMethods.GetLetterFilter(1, 2, 's'));
        return GetWordCountSimd128Benchmark(UintWordList, filters);
    }

    public static uint GetLetterFilter(uint minLetters, uint maxLetters, uint letter) => (minLetters << 8) + (maxLetters << 5) + letter - 'a';

    //ToDo: Make 256 variant
    unsafe void WordCheckSimd128(uint[] allWords, uint[] letterCountFilters)
    {
        var offset = Vector128<uint>.Count;
        var oneMask = Vector128.Create(1u, 1u, 1u, 1u);
        var letterCountMask = Vector128.Create((uint)0b111, 0b111, 0b111, 0b111);
        var fiveMax = Vector128.Create((uint)0b11111, (uint)0b11111, (uint)0b11111, (uint)0b11111);
        var i = 0;
        fixed (uint* pSource = allWords)
        {
            var lastBlockIndex = allWords.Length - (allWords.Length % offset);

            for (; i < lastBlockIndex; i += offset)
            {
                var product = Vector128<uint>.AllBitsSet;
                foreach (var t in letterCountFilters)
                {
                    var tempThing = Sse2.LoadVector128(pSource + i);
                    var copyOfWord = tempThing;
                    var runningCount = Vector128<uint>.Zero;
                    var letters = Vector128.Create((uint)t, (uint)t, (uint)t, (uint)t);
                    var letterMask = Sse2.And(letters, fiveMax);
                    Vector128<uint> summer;

                    //Check 5th position

                    summer = Sse2.CompareEqual(Sse2.And(tempThing, fiveMax), letterMask);
                    runningCount = Sse2.Add(runningCount, Sse2.And(summer, oneMask));
                    tempThing = Sse3.ShiftRightLogical(tempThing, 5);

                    //Check 4th position
                    summer = Sse2.CompareEqual(Sse2.And(tempThing, fiveMax), letterMask);
                    runningCount = Sse2.Add(runningCount, Sse2.And(summer, oneMask));
                    tempThing = Sse3.ShiftRightLogical(tempThing, 5);

                    //Check 3rd position
                    summer = Sse2.CompareEqual(Sse2.And(tempThing, fiveMax), letterMask);
                    runningCount = Sse2.Add(runningCount, Sse2.And(summer, oneMask));
                    tempThing = Sse3.ShiftRightLogical(tempThing, 5);

                    //Check 2nd position
                    summer = Sse2.CompareEqual(Sse2.And(tempThing, fiveMax), letterMask);
                    runningCount = Sse2.Add(runningCount, Sse2.And(summer, oneMask));
                    tempThing = Sse3.ShiftRightLogical(tempThing, 5);

                    //Check 1st position
                    summer = Sse2.CompareEqual(Sse2.And(tempThing, fiveMax), letterMask);
                    runningCount = Sse2.Add(runningCount, Sse2.And(summer, oneMask));

                    //If the letter count is greater, then null it out.
                    var ac = Sse2.And(letterCountMask, Sse3.ShiftRightLogical(letters, 5));
                    var ff = Sse2.CompareGreaterThan(runningCount.AsInt32(), ac.AsInt32());
                    var gg = Sse2.AndNot(ff, oneMask.AsInt32());
                    var hh = Sse2.CompareEqual(gg.AsUInt32(), oneMask);

                    //If the letter count is less, then null it out.
                    ac = Sse2.And(letterCountMask, Sse3.ShiftRightLogical(letters, 8));
                    ff = Sse2.CompareGreaterThan(ac.AsInt32(), runningCount.AsInt32());
                    gg = Sse2.AndNot(ff, oneMask.AsInt32());
                    hh = Sse2.And(hh, Sse2.CompareEqual(gg.AsUInt32(), oneMask));
                    product = Sse2.And(Sse2.And(copyOfWord, hh), product);
                }

                allWords[i + 0] = product.GetElement(0);
                allWords[i + 1] = product.GetElement(1);
                allWords[i + 2] = product.GetElement(2);
                allWords[i + 3] = product.GetElement(3);
            }
        }

        WordCheckScalar(allWords, letterCountFilters, i);
    }

    public new uint StringToInt(string ss)
    {
        uint l = 0;
        for (var i = 0; i < 5; i++) { l |= (byte)(ss[i] - 'a'); l <<= 5; }
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
        return new(ii.Select(i => (char)i).ToArray());
    }

    void WordCheckScalar(uint[] allWords, uint[] letterCountFilters, int i = 0)
    {
        //bool x; //Used for branchless.
        for (; i < allWords.Length; i++)
        {
            var word = allWords[i];
            foreach (var y in letterCountFilters)
            {
                //var debugWord = IntToString(word);
                var targetLetter = y & 0b11111;
                //var targetL = (char)(targetLetter + 'a');
                var runningCount = 0;
                var maxCount = (y >> 5) & 0b000_111;
                var minCount = ((y >> 5) & 0b111_000) >> 3;

                //x = (word & 0b11111 << 00) == (targetLetter << 00); runningCount += (Unsafe.As<bool, byte>(ref x)); //option for branchless: 
                //for (int j = 0; j < 5; j++) { runningCount += (word & 0b11111 << 5 * j) == (targetLetter << 5 * j) ? 1 : 0; } //twice as slow

                runningCount += (word & 0b11111 << 00) == (targetLetter << 00) ? 1 : 0;
                runningCount += (word & 0b11111 << 05) == (targetLetter << 05) ? 1 : 0;
                runningCount += (word & 0b11111 << 10) == (targetLetter << 10) ? 1 : 0;
                runningCount += (word & 0b11111 << 15) == (targetLetter << 15) ? 1 : 0;
                runningCount += (word & 0b11111 << 20) == (targetLetter << 20) ? 1 : 0;

                if (runningCount > maxCount || runningCount < minCount) { allWords[i] = 0; break; }
            }
        }
    }
}