using Certus.Domain.Market.ValueObjects;
using Certus.Domain.Strategy.Entities;
using Certus.Domain.Strategy.Enums;
using Certus.Domain.Strategy.ValueObjects;
using Certus.Domain.SharedKernel;
using FluentAssertions;

namespace Certus.Domain.Tests.Strategy;

public class BacktestRunTests
{
    [Fact]
    public void Constructor_Should_Set_Properties_Correctly()
    {
        var id = Guid.NewGuid();
        var strategyId = Guid.NewGuid();
        var config = CreateConfig();

        var run = new BacktestRun(id, strategyId, config);

        run.Id.Should().Be(id);
        run.StrategyId.Should().Be(strategyId);
        run.Config.Should().Be(config);
        run.Status.Should().Be(BacktestStatus.Pending);
        run.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        run.CompletedAt.Should().BeNull();
        run.ErrorMessage.Should().BeNull();
        run.Trades.Should().BeEmpty();
    }

    [Fact]
    public void Start_Should_Set_Status_To_Running()
    {
        var run = CreateRun();

        run.Start();

        run.Status.Should().Be(BacktestStatus.Running);
    }

    [Fact]
    public void Start_Should_Update_StartedAt()
    {
        var run = CreateRun();
        var beforeStart = DateTime.UtcNow;

        run.Start();

        run.StartedAt.Should().BeOnOrAfter(beforeStart);
    }

    [Fact]
    public void Complete_Should_Set_Status_To_Completed()
    {
        var run = CreateRun();
        run.Start();
        var metrics = CreateMetrics();

        run.Complete(metrics);

        run.Status.Should().Be(BacktestStatus.Completed);
    }

    [Fact]
    public void Complete_Should_Store_Metrics()
    {
        var run = CreateRun();
        var metrics = CreateMetrics();

        run.Complete(metrics);

        run.Metrics.Should().Be(metrics);
    }

    [Fact]
    public void Complete_Should_Set_CompletedAt()
    {
        var run = CreateRun();
        var beforeComplete = DateTime.UtcNow;

        run.Complete(CreateMetrics());

        run.CompletedAt.Should().BeOnOrAfter(beforeComplete);
    }

    [Fact]
    public void Fail_Should_Set_Status_To_Failed()
    {
        var run = CreateRun();

        run.Fail("Something went wrong");

        run.Status.Should().Be(BacktestStatus.Failed);
    }

    [Fact]
    public void Fail_Should_Store_ErrorMessage()
    {
        var run = CreateRun();

        run.Fail("Timeout error");

        run.ErrorMessage.Should().Be("Timeout error");
    }

    [Fact]
    public void Fail_Should_Set_CompletedAt()
    {
        var run = CreateRun();
        var beforeFail = DateTime.UtcNow;

        run.Fail("Error");

        run.CompletedAt.Should().BeOnOrAfter(beforeFail);
    }

    [Fact]
    public void AddTrade_Should_Add_To_Trades_Collection()
    {
        var run = CreateRun();
        var trade = CreateTrade(run.Id);

        run.AddTrade(trade);

        run.Trades.Should().ContainSingle().Which.Should().Be(trade);
    }

    [Fact]
    public void AddTrade_Should_Multiple_Trades()
    {
        var run = CreateRun();
        var trade1 = CreateTrade(run.Id);
        var trade2 = CreateTrade(run.Id);

        run.AddTrade(trade1);
        run.AddTrade(trade2);

        run.Trades.Should().HaveCount(2);
    }

    private static BacktestRun CreateRun()
    {
        return new BacktestRun(Guid.NewGuid(), Guid.NewGuid(), CreateConfig());
    }

    private static BacktestConfig CreateConfig()
    {
        return new BacktestConfig(
            new DateTime(2024, 1, 1),
            new DateTime(2024, 12, 31),
            new Money(100_000m, Currency.USD),
            new List<Symbol> { new("BTCUSD", AssetClass.Crypto) },
            new TimeFrame("1h"));
    }

    private static BacktestMetrics CreateMetrics()
    {
        return new BacktestMetrics(
            0.15m, 1.5m, 0.1m, 0.6m, 2.0m, 1.2m, 1.8m, 100, 2.5m);
    }

    private static BacktestTrade CreateTrade(Guid backtestRunId)
    {
        return new BacktestTrade(
            Guid.NewGuid(),
            backtestRunId,
            "BTCUSD",
            "Long",
            DateTime.UtcNow,
            50_000m,
            0.1m);
    }
}
