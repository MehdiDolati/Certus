using Certus.Domain.Execution.ValueObjects;
using Certus.Domain.SharedKernel;
using FluentAssertions;

namespace Certus.Domain.Tests.Execution;

public class TradeIdentifierTests
{
    [Fact]
    public void Constructor_Should_Set_All_Properties()
    {
        var strategyId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var symbol = new Symbol("BTC/USD", AssetClass.Crypto);
        var entryTime = new DateTime(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc);

        var identifier = new TradeIdentifier(strategyId, portfolioId, symbol, entryTime);

        identifier.StrategyId.Should().Be(strategyId);
        identifier.PortfolioId.Should().Be(portfolioId);
        identifier.Symbol.Should().Be(symbol);
        identifier.EntryTime.Should().Be(entryTime);
    }

    [Fact]
    public void TradeIdentifier_Should_Be_Equivalent_By_Value()
    {
        var strategyId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var symbol = new Symbol("ETH/USD", AssetClass.Crypto);
        var entryTime = DateTime.UtcNow;

        var id1 = new TradeIdentifier(strategyId, portfolioId, symbol, entryTime);
        var id2 = new TradeIdentifier(strategyId, portfolioId, symbol, entryTime);

        id1.Should().Be(id2);
    }

    [Fact]
    public void TradeIdentifier_Should_Not_Be_Equal_When_StrategyId_Differs()
    {
        var symbol = new Symbol("BTC/USD", AssetClass.Crypto);
        var entryTime = DateTime.UtcNow;

        var id1 = new TradeIdentifier(Guid.NewGuid(), Guid.NewGuid(), symbol, entryTime);
        var id2 = new TradeIdentifier(Guid.NewGuid(), Guid.NewGuid(), symbol, entryTime);

        id1.Should().NotBe(id2);
    }
}
