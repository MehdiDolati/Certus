using Certus.Application.Trading;
using Certus.Application.Trading.DTOs;
using Certus.Domain.SharedKernel;
using Certus.Domain.Strategy.Enums;
using Certus.Domain.Execution.Enums;
using Certus.Domain.Evaluation.Entities;
using FluentAssertions;
using Moq;

namespace Certus.Application.Tests;

public class TradeServiceTests
{
    private readonly Mock<Certus.Domain.Execution.Repositories.ITradeRepository> _tradeRepo = new();
    private readonly Mock<Certus.Domain.Evaluation.Repositories.IPerformanceSnapshotRepository> _snapshotRepo = new();

    [Fact]
    public async Task GetTradesAsync_Should_Return_Empty_When_No_Trades()
    {
        _tradeRepo.Setup(r => r.GetFilteredAsync(It.IsAny<Guid?>(), It.IsAny<Guid?>(),
            It.IsAny<Symbol?>(), It.IsAny<TradeSide?>(), It.IsAny<TradeStatus?>(),
            It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<string?>(),
            It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new List<Domain.Execution.Aggregates.Trade>());
        _tradeRepo.Setup(r => r.CountFilteredAsync(It.IsAny<Guid?>(), It.IsAny<Guid?>(),
            It.IsAny<Symbol?>(), It.IsAny<TradeSide?>(), It.IsAny<TradeStatus?>(),
            It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<string?>()))
            .ReturnsAsync(0);

        var service = CreateService();

        var result = await service.GetTradesAsync(new TradeFilter());

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetTradeDetailAsync_Should_Return_Null_When_Trade_Not_Found()
    {
        _tradeRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Domain.Execution.Aggregates.Trade?)null);
        var service = CreateService();

        var result = await service.GetTradeDetailAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCumulativePnlAsync_Should_Return_Empty_When_No_Snapshots()
    {
        _snapshotRepo.Setup(r => r.GetByPortfolioIdAsync(It.IsAny<Guid>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(new List<PerformanceSnapshot>());

        var service = CreateService();

        var result = await service.GetCumulativePnlAsync(null, null, DateRange.All);

        result.Should().BeEmpty();
    }

    private TradeService CreateService()
    {
        return new TradeService(_tradeRepo.Object, _snapshotRepo.Object);
    }
}
