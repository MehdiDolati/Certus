using Certus.Application.Portfolios.DTOs;
using Certus.Application.Strategies.DTOs;
using Certus.Domain.RiskAndPortfolio.Enums;
using Certus.Domain.Strategy.Enums;
using FluentAssertions;

namespace Certus.IntegrationTests;

public class StrategyIntegrationTests : IntegrationTestBase
{
    private readonly Guid _testPortfolioId;

    public StrategyIntegrationTests(CustomWebApplicationFactory factory) : base(factory)
    {
        var portfolioService = GetPortfolioService();
        var portfolio = portfolioService.CreateAsync(new CreatePortfolioRequest(
            Name: "Strategy Test Portfolio",
            Description: "Parent portfolio for strategy tests",
            TargetReturn: 0.15m,
            TargetSharpe: 1.5m,
            AllocatedCapital: 500_000m)).GetAwaiter().GetResult();
        _testPortfolioId = portfolio.Id;
        ClearTracker();
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistAndReturnDto()
    {
        var service = GetStrategyService();
        var request = new CreateStrategyRequest(
            PortfolioId: _testPortfolioId,
            Name: "Momentum Alpha",
            Type: "Momentum",
            TargetReturn: 0.25m,
            Weight: 0.6m);

        var result = await service.CreateAsync(request);
        ClearTracker();

        result.Should().NotBeNull();
        result.Name.Should().Be("Momentum Alpha");
        result.Status.Should().Be(StrategyStatus.Draft);

        var persisted = await DbContext.Strategies.FindAsync(result.Id);
        persisted.Should().NotBeNull();
        persisted!.Name.Should().Be("Momentum Alpha");
        persisted.Status.Should().Be(StrategyStatus.Draft);
    }

    [Fact]
    public async Task CreateAsync_EmptyName_ShouldThrowArgumentException()
    {
        var service = GetStrategyService();
        var request = new CreateStrategyRequest(
            PortfolioId: _testPortfolioId,
            Name: "",
            Type: "Momentum",
            TargetReturn: 0.2m,
            Weight: 0.5m);

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_InvalidWeight_ShouldThrowArgumentException()
    {
        var service = GetStrategyService();
        var request = new CreateStrategyRequest(
            PortfolioId: _testPortfolioId,
            Name: "Bad Weight",
            Type: "Momentum",
            TargetReturn: 0.2m,
            Weight: 1.5m);

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_NonExistentPortfolio_ShouldThrowInvalidOperationException()
    {
        var service = GetStrategyService();
        var request = new CreateStrategyRequest(
            PortfolioId: Guid.NewGuid(),
            Name: "Orphan Strategy",
            Type: "Momentum",
            TargetReturn: 0.2m,
            Weight: 0.5m);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_ClosedPortfolio_ShouldThrowInvalidOperationException()
    {
        var portfolioService = GetPortfolioService();
        var closed = await portfolioService.CreateAsync(new CreatePortfolioRequest(
            Name: "Closed Parent",
            Description: "Will be closed",
            TargetReturn: 0.1m,
            TargetSharpe: 1.0m,
            AllocatedCapital: 100_000m));
        ClearTracker();
        await portfolioService.DeleteAsync(closed.Id);

        var strategyService = GetStrategyService();
        var request = new CreateStrategyRequest(
            PortfolioId: closed.Id,
            Name: "Child of Closed",
            Type: "Momentum",
            TargetReturn: 0.2m,
            Weight: 0.5m);

        await Assert.ThrowsAsync<InvalidOperationException>(() => strategyService.CreateAsync(request));
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyPersistedData()
    {
        var service = GetStrategyService();
        var created = await service.CreateAsync(new CreateStrategyRequest(
            PortfolioId: _testPortfolioId,
            Name: "Update Me",
            Type: "TrendFollowing",
            TargetReturn: 0.15m,
            Weight: 0.3m));
        ClearTracker();

        var updateRequest = new UpdateStrategyRequest(
            Name: "Updated Strategy",
            Type: "MeanReversion",
            TargetReturn: 0.20m,
            Weight: 0.4m);

        var result = await service.UpdateAsync(created.Id, updateRequest);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Strategy");

        var persisted = await DbContext.Strategies.FindAsync(created.Id);
        persisted!.Name.Should().Be("Updated Strategy");
    }

    [Fact]
    public async Task UpdateAsync_NonExistent_ShouldReturnNull()
    {
        var service = GetStrategyService();
        var request = new UpdateStrategyRequest(
            Name: "Ghost",
            Type: "Momentum",
            TargetReturn: 0.1m,
            Weight: 0.5m);

        var result = await service.UpdateAsync(Guid.NewGuid(), request);

        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldSetStatusToRetired()
    {
        var service = GetStrategyService();
        var created = await service.CreateAsync(new CreateStrategyRequest(
            PortfolioId: _testPortfolioId,
            Name: "Delete Me",
            Type: "Arbitrage",
            TargetReturn: 0.1m,
            Weight: 0.2m));
        ClearTracker();

        var deleted = await service.DeleteAsync(created.Id);

        deleted.Should().BeTrue();

        var persisted = await DbContext.Strategies.FindAsync(created.Id);
        persisted!.Status.Should().Be(StrategyStatus.Retired);
    }

    [Fact]
    public async Task DeleteAsync_NonExistent_ShouldReturnFalse()
    {
        var service = GetStrategyService();

        var result = await service.DeleteAsync(Guid.NewGuid());

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetStrategiesByPortfolioAsync_ShouldReturnOnlyChildStrategies()
    {
        var service = GetStrategyService();
        await service.CreateAsync(new CreateStrategyRequest(
            PortfolioId: _testPortfolioId,
            Name: "Child A",
            Type: "Momentum",
            TargetReturn: 0.1m,
            Weight: 0.5m));
        await service.CreateAsync(new CreateStrategyRequest(
            PortfolioId: _testPortfolioId,
            Name: "Child B",
            Type: "Arbitrage",
            TargetReturn: 0.12m,
            Weight: 0.5m));

        var results = await service.GetStrategiesByPortfolioAsync(_testPortfolioId, null!);

        results.Should().HaveCountGreaterThanOrEqualTo(2);
        results.Should().AllSatisfy(s =>
            s.Id.Should().NotBeEmpty());
    }

    [Fact]
    public async Task FullLifecycle_CreateUpdateDelete_ShouldWorkEndToEnd()
    {
        var service = GetStrategyService();

        var created = await service.CreateAsync(new CreateStrategyRequest(
            PortfolioId: _testPortfolioId,
            Name: "Lifecycle Strategy",
            Type: "TrendFollowing",
            TargetReturn: 0.18m,
            Weight: 0.4m));
        created.Status.Should().Be(StrategyStatus.Draft);
        ClearTracker();

        var updated = await service.UpdateAsync(created.Id, new UpdateStrategyRequest(
            Name: "Lifecycle Strategy V2",
            Type: "MeanReversion",
            TargetReturn: 0.22m,
            Weight: 0.5m));
        updated!.Name.Should().Be("Lifecycle Strategy V2");
        ClearTracker();

        var deleted = await service.DeleteAsync(created.Id);
        deleted.Should().BeTrue();

        var strategies = await service.GetStrategiesByPortfolioAsync(_testPortfolioId, null!);
        var lifecycle = strategies.Single(s => s.Id == created.Id);
        lifecycle.Status.Should().Be(StrategyStatus.Retired);
        lifecycle.Name.Should().Be("Lifecycle Strategy V2");
    }
}
