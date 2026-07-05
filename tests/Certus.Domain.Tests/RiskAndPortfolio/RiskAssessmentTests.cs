using Certus.Domain.RiskAndPortfolio.Entities;
using Certus.Domain.RiskAndPortfolio.Enums;
using FluentAssertions;

namespace Certus.Domain.Tests.RiskAndPortfolio;

public class RiskAssessmentTests
{
    [Fact]
    public void Constructor_Should_Set_All_Properties()
    {
        var id = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();

        var assessment = new RiskAssessment(
            id, portfolioId, RiskLevel.Warning, 0.05m, 0.03m, 0.1m, 1.5m, 0.2m, "Test notes");

        assessment.Id.Should().Be(id);
        assessment.PortfolioId.Should().Be(portfolioId);
        assessment.OverallLevel.Should().Be(RiskLevel.Warning);
        assessment.PortfolioVaR.Should().Be(0.05m);
        assessment.CurrentDrawdown.Should().Be(0.03m);
        assessment.MaxDrawdown.Should().Be(0.1m);
        assessment.SharpeRatio.Should().Be(1.5m);
        assessment.Volatility.Should().Be(0.2m);
        assessment.Notes.Should().Be("Test notes");
    }

    [Fact]
    public void Constructor_Should_Set_AssessedAt_To_UtcNow()
    {
        var before = DateTime.UtcNow;

        var assessment = new RiskAssessment(
            Guid.NewGuid(), Guid.NewGuid(), RiskLevel.Normal, 0m, 0m, 0m, 0m, 0m);

        var after = DateTime.UtcNow;

        assessment.AssessedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Constructor_DefaultNotes_Should_Be_Empty()
    {
        var assessment = new RiskAssessment(
            Guid.NewGuid(), Guid.NewGuid(), RiskLevel.Normal, 0m, 0m, 0m, 0m, 0m);

        assessment.Notes.Should().BeEmpty();
    }

    [Fact]
    public void Equals_SameId_Should_Be_Equal()
    {
        var id = Guid.NewGuid();
        var a = new RiskAssessment(id, Guid.NewGuid(), RiskLevel.Normal, 0m, 0m, 0m, 0m, 0m);
        var b = new RiskAssessment(id, Guid.NewGuid(), RiskLevel.Critical, 1m, 1m, 1m, 1m, 1m);

        a.Should().Be(b);
    }

    [Fact]
    public void Equals_DifferentId_Should_Not_Be_Equal()
    {
        var a = new RiskAssessment(Guid.NewGuid(), Guid.NewGuid(), RiskLevel.Normal, 0m, 0m, 0m, 0m, 0m);
        var b = new RiskAssessment(Guid.NewGuid(), Guid.NewGuid(), RiskLevel.Normal, 0m, 0m, 0m, 0m, 0m);

        a.Should().NotBe(b);
    }
}
