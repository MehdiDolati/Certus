using Certus.Application.Strategies;
using Certus.Application.Strategies.DTOs;
using Certus.Domain.RiskAndPortfolio.Enums;
using Certus.Domain.Strategy.Enums;
using Certus.Domain.Strategy.Aggregates;
using Certus.Domain.Strategy.ValueObjects;
using Certus.Domain.Execution.Enums;
using Certus.Domain.SharedKernel;
using FluentAssertions;
using Moq;

namespace Certus.Application.Tests;

public class StrategyCrudTests
{
    private readonly Mock<Certus.Domain.Strategy.Repositories.IStrategyDefinitionRepository> _strategyRepo = new();
    private readonly Mock<Certus.Domain.Execution.Repositories.ITradeRepository> _tradeRepo = new();
    private readonly Mock<Certus.Domain.RiskAndPortfolio.Repositories.IPortfolioRepository> _portfolioRepo = new();
    private readonly Mock<Certus.Domain.Evaluation.Repositories.IPerformanceSnapshotRepository> _snapshotRepo = new();

    [Fact]
    public async Task CreateAsync_Should_Return_Strategy_With_Id()
    {
        var portfolio = new Certus.Domain.RiskAndPortfolio.Aggregates.Portfolio(
            Guid.NewGuid(), "Fund", 0.1m, 1.0m, new Money(1_000_000m, Currency.USD));
        _portfolioRepo.Setup(r => r.GetByIdAsync(portfolio.Id)).ReturnsAsync(portfolio);
        _strategyRepo.Setup(r => r.AddAsync(It.IsAny<StrategyDefinition>()))
            .Returns(Task.CompletedTask);
        var service = CreateService();

        var result = await service.CreateAsync(new CreateStrategyRequest(
            portfolio.Id, "Momentum", "Momentum", 0.25m, 0.6m));

        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("Momentum");
        result.Status.Should().Be(StrategyStatus.Draft);
        result.ContributionPercent.Should().Be(60m);
    }

    [Fact]
    public async Task CreateAsync_Should_Call_AddAsync_And_SaveChangesAsync()
    {
        var portfolio = new Certus.Domain.RiskAndPortfolio.Aggregates.Portfolio(
            Guid.NewGuid(), "Fund", 0.1m, 1.0m, new Money(1_000_000m, Currency.USD));
        _portfolioRepo.Setup(r => r.GetByIdAsync(portfolio.Id)).ReturnsAsync(portfolio);
        _strategyRepo.Setup(r => r.AddAsync(It.IsAny<StrategyDefinition>())).Returns(Task.CompletedTask);
        _strategyRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        var service = CreateService();

        await service.CreateAsync(new CreateStrategyRequest(
            portfolio.Id, "Test", "Arbitrage", 0.1m, 0.3m));

        _strategyRepo.Verify(r => r.AddAsync(It.IsAny<StrategyDefinition>()), Times.Once);
        _strategyRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_Should_Throw_On_Empty_Name()
    {
        var portfolio = new Certus.Domain.RiskAndPortfolio.Aggregates.Portfolio(
            Guid.NewGuid(), "Fund", 0.1m, 1.0m, new Money(1_000_000m, Currency.USD));
        _portfolioRepo.Setup(r => r.GetByIdAsync(portfolio.Id)).ReturnsAsync(portfolio);
        var service = CreateService();

        Func<Task> act = () => service.CreateAsync(new CreateStrategyRequest(
            portfolio.Id, "", "Momentum", 0.1m, 0.5m));

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*name*");
    }

    [Fact]
    public async Task CreateAsync_Should_Throw_On_Whitespace_Name()
    {
        var portfolio = new Certus.Domain.RiskAndPortfolio.Aggregates.Portfolio(
            Guid.NewGuid(), "Fund", 0.1m, 1.0m, new Money(1_000_000m, Currency.USD));
        _portfolioRepo.Setup(r => r.GetByIdAsync(portfolio.Id)).ReturnsAsync(portfolio);
        var service = CreateService();

        Func<Task> act = () => service.CreateAsync(new CreateStrategyRequest(
            portfolio.Id, "   ", "Momentum", 0.1m, 0.5m));

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CreateAsync_Should_Throw_On_Invalid_Weight()
    {
        var portfolio = new Certus.Domain.RiskAndPortfolio.Aggregates.Portfolio(
            Guid.NewGuid(), "Fund", 0.1m, 1.0m, new Money(1_000_000m, Currency.USD));
        _portfolioRepo.Setup(r => r.GetByIdAsync(portfolio.Id)).ReturnsAsync(portfolio);
        var service = CreateService();

        Func<Task> act = () => service.CreateAsync(new CreateStrategyRequest(
            portfolio.Id, "Test", "Momentum", 0.1m, 1.5m));

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Weight*");
    }

    [Fact]
    public async Task CreateAsync_Should_Throw_On_Negative_Weight()
    {
        var portfolio = new Certus.Domain.RiskAndPortfolio.Aggregates.Portfolio(
            Guid.NewGuid(), "Fund", 0.1m, 1.0m, new Money(1_000_000m, Currency.USD));
        _portfolioRepo.Setup(r => r.GetByIdAsync(portfolio.Id)).ReturnsAsync(portfolio);
        var service = CreateService();

        Func<Task> act = () => service.CreateAsync(new CreateStrategyRequest(
            portfolio.Id, "Test", "Momentum", 0.1m, -0.1m));

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Weight*");
    }

    [Fact]
    public async Task CreateAsync_Should_Throw_On_Portfolio_Not_Found()
    {
        _portfolioRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Certus.Domain.RiskAndPortfolio.Aggregates.Portfolio?)null);
        var service = CreateService();

        Func<Task> act = () => service.CreateAsync(new CreateStrategyRequest(
            Guid.NewGuid(), "Test", "Momentum", 0.1m, 0.5m));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task CreateAsync_Should_Throw_On_Inactive_Portfolio()
    {
        var portfolio = new Certus.Domain.RiskAndPortfolio.Aggregates.Portfolio(
            Guid.NewGuid(), "Fund", 0.1m, 1.0m, new Money(1_000_000m, Currency.USD));
        portfolio.Close();
        _portfolioRepo.Setup(r => r.GetByIdAsync(portfolio.Id)).ReturnsAsync(portfolio);
        var service = CreateService();

        Func<Task> act = () => service.CreateAsync(new CreateStrategyRequest(
            portfolio.Id, "Test", "Momentum", 0.1m, 0.5m));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*non-active*");
    }

    [Fact]
    public async Task UpdateAsync_Should_Persist_Changes()
    {
        var existing = new StrategyDefinition(
            Guid.NewGuid(), "Old", new StrategyType(StrategyCategory.Momentum), 0.1m);
        existing.Activate();
        existing.AssignToPortfolio(Guid.NewGuid(), new Weight(0.5m));
        _strategyRepo.Setup(r => r.GetByIdAsync(existing.Id)).ReturnsAsync(existing);
        _strategyRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        var service = CreateService();

        var result = await service.UpdateAsync(existing.Id, new UpdateStrategyRequest(
            "New Name", "Arbitrage", 0.3m, 0.7m));

        result.Should().NotBeNull();
        result!.Name.Should().Be("New Name");
        result.ContributionPercent.Should().Be(70m);
    }

    [Fact]
    public async Task UpdateAsync_Should_Return_Null_When_Not_Found()
    {
        _strategyRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((StrategyDefinition?)null);
        var service = CreateService();

        var result = await service.UpdateAsync(Guid.NewGuid(), new UpdateStrategyRequest(
            "Name", "Momentum", 0.1m, 0.5m));

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_Should_Throw_On_Empty_Name()
    {
        var existing = new StrategyDefinition(
            Guid.NewGuid(), "Strategy", new StrategyType(StrategyCategory.Momentum), 0.1m);
        _strategyRepo.Setup(r => r.GetByIdAsync(existing.Id)).ReturnsAsync(existing);
        var service = CreateService();

        Func<Task> act = () => service.UpdateAsync(existing.Id, new UpdateStrategyRequest(
            "", "Momentum", 0.1m, 0.5m));

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*name*");
    }

    [Fact]
    public async Task UpdateAsync_Should_Throw_On_Invalid_Weight()
    {
        var existing = new StrategyDefinition(
            Guid.NewGuid(), "Strategy", new StrategyType(StrategyCategory.Momentum), 0.1m);
        _strategyRepo.Setup(r => r.GetByIdAsync(existing.Id)).ReturnsAsync(existing);
        var service = CreateService();

        Func<Task> act = () => service.UpdateAsync(existing.Id, new UpdateStrategyRequest(
            "Name", "Momentum", 0.1m, 1.5m));

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Weight*");
    }

    [Fact]
    public async Task DeleteAsync_Should_Set_Status_Retired()
    {
        var existing = new StrategyDefinition(
            Guid.NewGuid(), "Strat", new StrategyType(StrategyCategory.Momentum), 0.1m);
        existing.Activate();
        _strategyRepo.Setup(r => r.GetByIdAsync(existing.Id)).ReturnsAsync(existing);
        _strategyRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        var service = CreateService();

        var result = await service.DeleteAsync(existing.Id);

        result.Should().BeTrue();
        existing.Status.Should().Be(StrategyStatus.Retired);
    }

    [Fact]
    public async Task DeleteAsync_Should_Return_False_When_Not_Found()
    {
        _strategyRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((StrategyDefinition?)null);
        var service = CreateService();

        var result = await service.DeleteAsync(Guid.NewGuid());

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_Should_Call_Update_And_SaveChanges()
    {
        var existing = new StrategyDefinition(
            Guid.NewGuid(), "Strat", new StrategyType(StrategyCategory.Momentum), 0.1m);
        existing.Activate();
        _strategyRepo.Setup(r => r.GetByIdAsync(existing.Id)).ReturnsAsync(existing);
        _strategyRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        var service = CreateService();

        await service.DeleteAsync(existing.Id);

        _strategyRepo.Verify(r => r.Update(It.IsAny<StrategyDefinition>()), Times.Once);
        _strategyRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
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
