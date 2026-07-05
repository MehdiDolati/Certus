using Certus.Application.Portfolios;
using Certus.Application.Portfolios.DTOs;
using Certus.Application.Trading.DTOs;
using Certus.Domain.RiskAndPortfolio.Enums;
using Certus.Domain.SharedKernel;
using Certus.Domain.Strategy.Enums;
using Certus.Domain.Execution.Enums;
using FluentAssertions;
using Moq;

namespace Certus.Application.Tests;

public class PortfolioServiceTests
{
    private readonly Mock<Certus.Domain.RiskAndPortfolio.Repositories.IPortfolioRepository> _portfolioRepo = new();
    private readonly Mock<Certus.Domain.Strategy.Repositories.IStrategyDefinitionRepository> _strategyRepo = new();
    private readonly Mock<Certus.Domain.Execution.Repositories.ITradeRepository> _tradeRepo = new();
    private readonly Mock<Certus.Domain.Evaluation.Repositories.IPerformanceSnapshotRepository> _snapshotRepo = new();

    [Fact]
    public async Task GetAllPortfoliosAsync_Should_Return_Empty_When_No_Portfolios()
    {
        _portfolioRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Certus.Domain.RiskAndPortfolio.Aggregates.Portfolio>());
        var service = CreateService();

        var result = await service.GetAllPortfoliosAsync(new PortfolioFilter());

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPortfolioDetailAsync_Should_Return_Null_When_Portfolio_Not_Found()
    {
        _portfolioRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Certus.Domain.RiskAndPortfolio.Aggregates.Portfolio?)null);
        var service = CreateService();

        var result = await service.GetPortfolioDetailAsync(Guid.NewGuid(), DateRange.All);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetDashboardKpisAsync_Should_Return_Zero_KPIs_When_No_Data()
    {
        _portfolioRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Certus.Domain.RiskAndPortfolio.Aggregates.Portfolio>());
        var service = CreateService();

        var result = await service.GetDashboardKpisAsync(DateRange.All);

        result.TotalAum.Should().Be(0m);
        result.TotalPnl.Should().Be(0m);
        result.ActiveCount.Should().Be(0);
    }

    private PortfolioService CreateService()
    {
        return new PortfolioService(
            _portfolioRepo.Object,
            _strategyRepo.Object,
            _tradeRepo.Object,
            _snapshotRepo.Object);
    }
}
