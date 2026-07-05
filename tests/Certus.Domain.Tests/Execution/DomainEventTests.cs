using Certus.Domain.Execution.Enums;
using Certus.Domain.Execution.Events;
using Certus.Domain.SharedKernel;
using FluentAssertions;

namespace Certus.Domain.Tests.Execution;

public class DomainEventTests
{
    // --- OrderSubmitted ---

    [Fact]
    public void OrderSubmitted_Should_Have_Default_EventId_And_OccurredOn()
    {
        var evt = new OrderSubmitted
        {
            TradeId = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            Symbol = new Symbol("BTC/USD", AssetClass.Crypto),
            Side = TradeSide.Long
        };

        evt.EventId.Should().NotBe(Guid.Empty);
        evt.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        evt.Should().BeAssignableTo<IDomainEvent>();
    }

    [Fact]
    public void OrderSubmitted_Should_Store_Required_Properties()
    {
        var tradeId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var symbol = new Symbol("ETH/USD", AssetClass.Crypto);

        var evt = new OrderSubmitted
        {
            TradeId = tradeId,
            OrderId = orderId,
            Symbol = symbol,
            Side = TradeSide.Short
        };

        evt.TradeId.Should().Be(tradeId);
        evt.OrderId.Should().Be(orderId);
        evt.Symbol.Should().Be(symbol);
        evt.Side.Should().Be(TradeSide.Short);
    }

    // --- OrderFilled ---

    [Fact]
    public void OrderFilled_Should_Have_Default_EventId_And_OccurredOn()
    {
        var evt = new OrderFilled
        {
            TradeId = Guid.NewGuid(),
            OrderId = Guid.NewGuid()
        };

        evt.EventId.Should().NotBe(Guid.Empty);
        evt.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        evt.Should().BeAssignableTo<IDomainEvent>();
    }

    [Fact]
    public void OrderFilled_Should_Store_All_Properties()
    {
        var tradeId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        var evt = new OrderFilled
        {
            TradeId = tradeId,
            OrderId = orderId,
            FillQuantity = 100m,
            AveragePrice = 50000m
        };

        evt.TradeId.Should().Be(tradeId);
        evt.OrderId.Should().Be(orderId);
        evt.FillQuantity.Should().Be(100m);
        evt.AveragePrice.Should().Be(50000m);
    }

    // --- TradeOpened ---

    [Fact]
    public void TradeOpened_Should_Have_Default_EventId_And_OccurredOn()
    {
        var evt = new TradeOpened
        {
            TradeId = Guid.NewGuid(),
            PortfolioId = Guid.NewGuid(),
            StrategyId = Guid.NewGuid(),
            Symbol = new Symbol("BTC/USD", AssetClass.Crypto),
            Side = TradeSide.Long
        };

        evt.EventId.Should().NotBe(Guid.Empty);
        evt.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        evt.Should().BeAssignableTo<IDomainEvent>();
    }

    [Fact]
    public void TradeOpened_Should_Store_All_Required_Properties()
    {
        var tradeId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var strategyId = Guid.NewGuid();
        var symbol = new Symbol("BTC/USD", AssetClass.Crypto);

        var evt = new TradeOpened
        {
            TradeId = tradeId,
            PortfolioId = portfolioId,
            StrategyId = strategyId,
            Symbol = symbol,
            Side = TradeSide.Short,
            Size = 2.5m
        };

        evt.TradeId.Should().Be(tradeId);
        evt.PortfolioId.Should().Be(portfolioId);
        evt.StrategyId.Should().Be(strategyId);
        evt.Symbol.Should().Be(symbol);
        evt.Side.Should().Be(TradeSide.Short);
        evt.Size.Should().Be(2.5m);
    }

    // --- TradeClosed ---

    [Fact]
    public void TradeClosed_Should_Have_Default_EventId_And_OccurredOn()
    {
        var evt = new TradeClosed
        {
            TradeId = Guid.NewGuid()
        };

        evt.EventId.Should().NotBe(Guid.Empty);
        evt.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        evt.Should().BeAssignableTo<IDomainEvent>();
    }

    [Fact]
    public void TradeClosed_Should_Store_All_Properties()
    {
        var tradeId = Guid.NewGuid();
        var duration = TimeSpan.FromMinutes(5);

        var evt = new TradeClosed
        {
            TradeId = tradeId,
            PnL = 500m,
            Duration = duration
        };

        evt.TradeId.Should().Be(tradeId);
        evt.PnL.Should().Be(500m);
        evt.Duration.Should().Be(duration);
    }

    // --- TradeCancelled ---

    [Fact]
    public void TradeCancelled_Should_Have_Default_EventId_And_OccurredOn()
    {
        var evt = new TradeCancelled
        {
            TradeId = Guid.NewGuid(),
            Reason = "test"
        };

        evt.EventId.Should().NotBe(Guid.Empty);
        evt.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        evt.Should().BeAssignableTo<IDomainEvent>();
    }

    [Fact]
    public void TradeCancelled_Should_Store_All_Properties()
    {
        var tradeId = Guid.NewGuid();

        var evt = new TradeCancelled
        {
            TradeId = tradeId,
            Reason = "stop loss triggered"
        };

        evt.TradeId.Should().Be(tradeId);
        evt.Reason.Should().Be("stop loss triggered");
    }

    // --- SlippageReported ---

    [Fact]
    public void SlippageReported_Should_Have_Default_EventId_And_OccurredOn()
    {
        var evt = new SlippageReported
        {
            TradeId = Guid.NewGuid()
        };

        evt.EventId.Should().NotBe(Guid.Empty);
        evt.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        evt.Should().BeAssignableTo<IDomainEvent>();
    }

    [Fact]
    public void SlippageReported_Should_Store_All_Properties()
    {
        var tradeId = Guid.NewGuid();

        var evt = new SlippageReported
        {
            TradeId = tradeId,
            ExpectedPrice = 50000m,
            ActualPrice = 50100m,
            SlippagePercent = 0.2m
        };

        evt.TradeId.Should().Be(tradeId);
        evt.ExpectedPrice.Should().Be(50000m);
        evt.ActualPrice.Should().Be(50100m);
        evt.SlippagePercent.Should().Be(0.2m);
    }

    // --- Record equality ---

    [Fact]
    public void DomainEvents_Should_Be_Value_Equal_When_Same_Data()
    {
        var id = Guid.NewGuid();
        var occurredOn = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var e1 = new TradeClosed { EventId = id, OccurredOn = occurredOn, TradeId = Guid.NewGuid(), PnL = 100m, Duration = TimeSpan.Zero };
        var e2 = new TradeClosed { EventId = id, OccurredOn = occurredOn, TradeId = e1.TradeId, PnL = 100m, Duration = TimeSpan.Zero };

        e1.Should().Be(e2);
    }
}
