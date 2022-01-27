using Benchmarker;
using Xunit;

namespace Tester;

public class AddingTests
{
    readonly AllMethods _am = new() { Count = 10 };
    public const int SumAnswer = 53638;
    public AddingTests() => _am.GlobalSetup();

    [Fact]
    public void Avx2Sum() => Assert.Equal(SumAnswer, _am.AddIntAvx2Span());

    [Fact]
    public void AddIntScalar() => Assert.Equal(SumAnswer, _am.AddIntLinq());

    [Fact]
    public void AddIntScalarSpan() => Assert.Equal(SumAnswer, _am.AddIntForSpan());

    [Fact]
    public void AddIntSse2() => Assert.Equal(SumAnswer, _am.AddIntSse2Span());
}