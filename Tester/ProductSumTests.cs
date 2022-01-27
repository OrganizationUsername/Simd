using Benchmarker;
using Xunit;

namespace Tester;

public class ProductSumTests
{
    readonly AllMethods _am = new() { Count = 12 };
    public const double ProductSumAnswer = 2.6607436337966783;
    public ProductSumTests() => _am.GlobalSetup();

    [Fact]
    public void ScalarProductSum() => Assert.Equal(ProductSumAnswer, _am.ScalarProd());

    [Fact]
    public void VectorProductSum() => Assert.Equal(ProductSumAnswer, _am.VectorProd());

    [Fact]
    public void SimdProdRunnerAvx2() => Assert.Equal(ProductSumAnswer, _am.SimdProdRunnerAvx2());

    [Fact]
    public void SimdProdRunnerSse2() => Assert.Equal(ProductSumAnswer, _am.SimdProdRunnerSse2());


}