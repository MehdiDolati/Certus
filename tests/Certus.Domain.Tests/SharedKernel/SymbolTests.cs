using Certus.Domain.SharedKernel;
using FluentAssertions;

namespace Certus.Domain.Tests.SharedKernel;

public class SymbolTests
{
    [Fact]
    public void Symbol_Should_Be_Created_With_Value_And_AssetClass()
    {
        var symbol = new Symbol("BTC/USD", AssetClass.Crypto);
        symbol.Value.Should().Be("BTC/USD");
        symbol.AssetClass.Should().Be(AssetClass.Crypto);
    }

    [Fact]
    public void ToString_Should_Return_Value()
    {
        var symbol = new Symbol("EUR/USD", AssetClass.Forex);
        symbol.ToString().Should().Be("EUR/USD");
    }

    [Fact]
    public void Symbol_Record_Should_Be_Equal_When_Same_Values()
    {
        var left = new Symbol("BTC/USD", AssetClass.Crypto);
        var right = new Symbol("BTC/USD", AssetClass.Crypto);
        left.Should().Be(right);
    }

    [Fact]
    public void AssetClass_All_Values_Should_Be_Covered()
    {
        Enum.GetValues<AssetClass>().Should().HaveCount(4);
    }
}
