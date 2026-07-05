using Certus.Domain.Platform.ValueObjects;
using FluentAssertions;

namespace Certus.Domain.Tests.Platform;

public class PlatformPortfolioTests
{
    [Fact]
    public void PlatformPortfolio_Should_Have_Correct_Defaults()
    {
        var portfolio = new PlatformPortfolio();

        portfolio.ExternalId.Should().BeEmpty();
        portfolio.Name.Should().BeEmpty();
        portfolio.Balance.Should().Be(0);
        portfolio.Equity.Should().Be(0);
        portfolio.Margin.Should().Be(0);
        portfolio.FreeMargin.Should().Be(0);
        portfolio.Profit.Should().Be(0);
        portfolio.Strategies.Should().BeEmpty();
    }

    [Fact]
    public void PlatformPortfolio_Should_Be_Created_With_Values()
    {
        var strategies = new List<PlatformStrategy>
        {
            new() { ExternalId = "EA_01", Name = "Momentum", IsActive = true }
        };

        var portfolio = new PlatformPortfolio
        {
            ExternalId = "MT4_12345",
            Name = "Main Account",
            Balance = 100000m,
            Equity = 102500m,
            Margin = 15000m,
            FreeMargin = 87500m,
            Profit = 2500m,
            Timestamp = DateTime.UtcNow,
            Strategies = strategies
        };

        portfolio.ExternalId.Should().Be("MT4_12345");
        portfolio.Name.Should().Be("Main Account");
        portfolio.Balance.Should().Be(100000m);
        portfolio.Equity.Should().Be(102500m);
        portfolio.Strategies.Should().HaveCount(1);
    }

    [Fact]
    public void PlatformPortfolio_Should_Be_Record()
    {
        var portfolio1 = new PlatformPortfolio { ExternalId = "123", Name = "Test" };
        var portfolio2 = new PlatformPortfolio { ExternalId = "123", Name = "Test" };

        portfolio1.Should().Be(portfolio2);
        portfolio1.GetHashCode().Should().Be(portfolio2.GetHashCode());
    }
}
