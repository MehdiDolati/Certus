using Certus.Application.Strategies;
using Certus.Application.Strategies.DTOs;
using Certus.Application.Trading.DTOs;
using Certus.Domain.RiskAndPortfolio.Enums;
using Certus.Domain.SharedKernel;
using Certus.Domain.Strategy.Enums;
using Certus.Domain.Strategy.Aggregates;
using Certus.Domain.Execution.Enums;
using FluentAssertions;
using Moq;

namespace Certus.Application.Tests;

public class StrategyServiceTests
{
    private readonly Mock<Certus.Domain.Strategy.Repositories.IStrategyDefinitionRepository> _strategyRepo = new();
    private readonly Mock<Certus.Domain.Execution.Repositories.ITradeRepository> _tradeRepo = new();
    private readonly Mock<Certus.Domain.RiskAndPortfolio.Repositories.IPortfolioRepository> _portfolioRepo = new();
    private readonly Mock<Certus.Domain.Evaluation.Repositories.IPerformanceSnapshotRepository> _snapshotRepo = new();

    [Fact]
    public async Task GetStrategiesByPortfolioAsync_Should_Return_Empty_When_No_Strategies()
    {
        _strategyRepo.Setup(r => r.GetByPortfolioIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<StrategyDefinition>());
        var service = CreateService();

        var result = await service.GetStrategiesByPortfolioAsync(Guid.NewGuid(), DateRange.All);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetStrategyPerformanceAsync_Should_Return_Empty_When_No_Snapshots()
    {
        _snapshotRepo.Setup(r => r.GetByStrategyIdAsync(It.IsAny<Guid>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(new List<Certus.Domain.Evaluation.Entities.PerformanceSnapshot>());
        var service = CreateService();

        var result = await service.GetStrategyPerformanceAsync(Guid.NewGuid(), DateRange.All);

        result.Should().BeEmpty();
    }

    private StrategyService CreateService()
    {
        return new StrategyService(
            _strategyRepo.Object,
            _tradeRepo.Object,
            _portfolioRepo.Object,
            _snapshotRepo.Object);
    }
}
