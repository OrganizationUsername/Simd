using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Xunit;

namespace Tester;

public class ShiftingTests
{
    public int Count { get; set; } = 12;
    private readonly double[] _left;
    private readonly double[] _right;
    private readonly int[] _ints;

    public ShiftingTests()
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


}