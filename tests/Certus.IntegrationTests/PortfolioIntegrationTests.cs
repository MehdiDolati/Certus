using Certus.Application.Portfolios.DTOs;
using Certus.Domain.RiskAndPortfolio.Enums;
using FluentAssertions;

namespace Certus.IntegrationTests;

public class PortfolioIntegrationTests : IntegrationTestBase
{
    public PortfolioIntegrationTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task CreateAsync_ShouldPersistAndReturnDto()
    {
        var service = GetPortfolioService();
        var request = new CreatePortfolioRequest(
            Name: "Integration Test Fund",
            Description: "Created via integration test",
            TargetReturn: 0.15m,
            TargetSharpe: 1.5m,
            AllocatedCapital: 1_000_000m);

        var result = await service.CreateAsync(request);
        ClearTracker();

        result.Should().NotBeNull();
        result.Name.Should().Be("Integration Test Fund");
        result.Status.Should().Be(PortfolioStatus.Active);

        var persisted = await DbContext.Portfolios.FindAsync(result.Id);
        persisted.Should().NotBeNull();
        persisted!.Name.Should().Be("Integration Test Fund");
        persisted.AllocatedCapital.Amount.Should().Be(1_000_000m);
    }

    [Fact]
    public async Task CreateAsync_WithEmptyName_ShouldThrowArgumentException()
    {
        var service = GetPortfolioService();
        var request = new CreatePortfolioRequest(
            Name: "",
            Description: "No name",
            TargetReturn: 0.1m,
            TargetSharpe: 1.0m,
            AllocatedCapital: 500_000m);

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(request));
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyPersistedData()
    {
        var service = GetPortfolioService();
        var created = await service.CreateAsync(new CreatePortfolioRequest(
            Name: "Update Me",
            Description: "Original",
            TargetReturn: 0.10m,
            TargetSharpe: 1.0m,
            AllocatedCapital: 200_000m));
        ClearTracker();

        var updateRequest = new UpdatePortfolioRequest(
            Name: "Updated Fund",
            Description: "Modified",
            TargetReturn: 0.20m,
            TargetSharpe: 2.0m,
            AllocatedCapital: 300_000m);

        var result = await service.UpdateAsync(created.Id, updateRequest);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Fund");

        var persisted = await DbContext.Portfolios.FindAsync(created.Id);
        persisted!.Name.Should().Be("Updated Fund");
        persisted.AllocatedCapital.Amount.Should().Be(300_000m);
    }

    [Fact]
    public async Task UpdateAsync_NonExistent_ShouldReturnNull()
    {
        var service = GetPortfolioService();
        var request = new UpdatePortfolioRequest(
            Name: "Ghost",
            Description: "Does not exist",
            TargetReturn: 0.1m,
            TargetSharpe: 1.0m,
            AllocatedCapital: 100_000m);

        var result = await service.UpdateAsync(Guid.NewGuid(), request);

        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldSetStatusToClosed()
    {
        var service = GetPortfolioService();
        var created = await service.CreateAsync(new CreatePortfolioRequest(
            Name: "Delete Me",
            Description: "To be closed",
            TargetReturn: 0.1m,
            TargetSharpe: 1.0m,
            AllocatedCapital: 100_000m));
        ClearTracker();

        var deleted = await service.DeleteAsync(created.Id);

        deleted.Should().BeTrue();

        var persisted = await DbContext.Portfolios.FindAsync(created.Id);
        persisted!.Status.Should().Be(PortfolioStatus.Closed);
    }

    [Fact]
    public async Task DeleteAsync_NonExistent_ShouldReturnFalse()
    {
        var service = GetPortfolioService();

        var result = await service.DeleteAsync(Guid.NewGuid());

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllPortfoliosAsync_ShouldReturnSeededPortfolios()
    {
        var service = GetPortfolioService();
        var filter = new PortfolioFilter();

        var results = await service.GetAllPortfoliosAsync(filter);

        results.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetAllPortfoliosAsync_WithStatusFilter_ShouldReturnMatching()
    {
        var service = GetPortfolioService();
        await service.CreateAsync(new CreatePortfolioRequest(
            Name: "Active Only",
            Description: "Test",
            TargetReturn: 0.1m,
            TargetSharpe: 1.0m,
            AllocatedCapital: 100_000m));

        var filter = new PortfolioFilter(Status: PortfolioStatus.Active);

        var results = await service.GetAllPortfoliosAsync(filter);

        results.Should().AllSatisfy(p => p.Status.Should().Be(PortfolioStatus.Active));
    }

    [Fact]
    public async Task GetAllPortfoliosAsync_WithSearchFilter_ShouldReturnMatching()
    {
        var service = GetPortfolioService();
        await service.CreateAsync(new CreatePortfolioRequest(
            Name: "UniqueXYZ Portfolio",
            Description: "Special fund",
            TargetReturn: 0.1m,
            TargetSharpe: 1.0m,
            AllocatedCapital: 100_000m));

        var filter = new PortfolioFilter(Search: "UniqueXYZ");

        var results = await service.GetAllPortfoliosAsync(filter);

        results.Should().ContainSingle(p => p.Name == "UniqueXYZ Portfolio");
    }
}
