using Certus.Application.Portfolios;
using Certus.Application.Portfolios.DTOs;
using Certus.Domain.RiskAndPortfolio.Enums;
using Certus.Domain.SharedKernel;
using Certus.Domain.Strategy.Enums;
using Certus.Domain.Execution.Enums;
using FluentAssertions;
using Moq;

namespace Certus.Application.Tests;

public class PortfolioCrudTests
{
    private readonly Mock<Certus.Domain.RiskAndPortfolio.Repositories.IPortfolioRepository> _portfolioRepo = new();
    private readonly Mock<Certus.Domain.Strategy.Repositories.IStrategyDefinitionRepository> _strategyRepo = new();
    private readonly Mock<Certus.Domain.Execution.Repositories.ITradeRepository> _tradeRepo = new();
    private readonly Mock<Certus.Domain.Evaluation.Repositories.IPerformanceSnapshotRepository> _snapshotRepo = new();

    [Fact]
    public async Task CreateAsync_Should_Return_Portfolio_With_Id()
    {
        _portfolioRepo.Setup(r => r.AddAsync(It.IsAny<Certus.Domain.RiskAndPortfolio.Aggregates.Portfolio>()))
            .Returns(Task.CompletedTask);
        var service = CreateService();

        var result = await service.CreateAsync(new CreatePortfolioRequest(
            "Test Fund", "Description", 0.15m, 1.5m, 1_000_000m));

        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("Test Fund");
        result.Status.Should().Be(PortfolioStatus.Active);
    }

    [Fact]
    public async Task UpdateAsync_Should_Persist_Changes()
    {
        var existing = new Certus.Domain.RiskAndPortfolio.Aggregates.Portfolio(
            Guid.NewGuid(), "Old Name", 0.1m, 1.0m,
            new Money(500_000m, Currency.USD), "Old");
        _portfolioRepo.Setup(r => r.GetByIdAsync(existing.Id)).ReturnsAsync(existing);
        _portfolioRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        var service = CreateService();

        var result = await service.UpdateAsync(existing.Id, new UpdatePortfolioRequest(
            "New Name", "New Desc", 0.2m, 2.0m, 2_000_000m));

        result.Should().NotBeNull();
        result!.Name.Should().Be("New Name");
        result.Description.Should().Be("New Desc");
    }

    [Fact]
    public async Task DeleteAsync_Should_Set_Status_Closed()
    {
        var existing = new Certus.Domain.RiskAndPortfolio.Aggregates.Portfolio(
            Guid.NewGuid(), "Fund", 0.1m, 1.0m,
            new Money(100_000m, Currency.USD));
        _portfolioRepo.Setup(r => r.GetByIdAsync(existing.Id)).ReturnsAsync(existing);
        _portfolioRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        var service = CreateService();

        var result = await service.DeleteAsync(existing.Id);

        result.Should().BeTrue();
        existing.Status.Should().Be(PortfolioStatus.Closed);
    }

    [Fact]
    public async Task DeleteAsync_Should_Return_False_When_Not_Found()
    {
        _portfolioRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Certus.Domain.RiskAndPortfolio.Aggregates.Portfolio?)null);
        var service = CreateService();

        var result = await service.DeleteAsync(Guid.NewGuid());

        result.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAsync_Should_Throw_On_Empty_Name()
    {
        var service = CreateService();

        Func<Task> act = () => service.CreateAsync(new CreatePortfolioRequest(
            "", "Desc", 0.1m, 1.0m, 100_000m));

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*name*");
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
