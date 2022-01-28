using BenchmarkDotNet.Attributes;

namespace Benchmarker;

public class BaseBenchmarker
{
    [Params(1_000, 100_000)]
    public int Count { get; set; }
    public double[] _left;
    public double[] _right;
    public int[] _ints;
    public string[] wordList;
    public int[][] WordListInts;

    [GlobalSetup]
    public void GlobalSetup()
    {
        var ran = new Random(1);
        _left = new double[Count];
        _right = new double[Count];
        for (var i = 0; i < Count; i++)
        {
            _left[i] = ran.NextDouble();
            _right[i] = ran.NextDouble();
        }

        _ints = new int[Count];

        _ints = new int[Count];
        for (var i = 0; i < Count; i++)
        {
            _ints[i] = ran.Next(1, 10000);
        }
        //"book", "dogs", "plus", "shot" 
        wordList = new[] { "robot", "doggy", "mints", "shots", "abash", "ayala", "aural", "brine", "chive", "chili" };



    }
}