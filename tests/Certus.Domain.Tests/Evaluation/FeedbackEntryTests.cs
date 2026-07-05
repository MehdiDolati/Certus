using Certus.Domain.Evaluation.Entities;
using Certus.Domain.Evaluation.Enums;
using FluentAssertions;

namespace Certus.Domain.Tests.Evaluation;

public class FeedbackEntryTests
{
    [Fact]
    public void FeedbackEntry_Should_Store_All_Properties()
    {
        var id = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        var strategyId = Guid.NewGuid();

        var entry = new FeedbackEntry(id, reportId, FeedbackType.Positive, "Great performance", strategyId);

        entry.Id.Should().Be(id);
        entry.ReportId.Should().Be(reportId);
        entry.Type.Should().Be(FeedbackType.Positive);
        entry.Message.Should().Be("Great performance");
        entry.StrategyId.Should().Be(strategyId);
        entry.GeneratedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void FeedbackEntry_Should_Allow_Null_StrategyId()
    {
        var entry = new FeedbackEntry(Guid.NewGuid(), Guid.NewGuid(), FeedbackType.Warning, "Watch out");

        entry.StrategyId.Should().BeNull();
    }

    [Fact]
    public void FeedbackEntry_Should_Default_StrategyId_To_Null()
    {
        var entry = new FeedbackEntry(Guid.NewGuid(), Guid.NewGuid(), FeedbackType.Critical, "Critical issue");

        entry.StrategyId.Should().BeNull();
    }

    [Theory]
    [InlineData(FeedbackType.Positive)]
    [InlineData(FeedbackType.Warning)]
    [InlineData(FeedbackType.Critical)]
    [InlineData(FeedbackType.Recommendation)]
    public void FeedbackEntry_Should_Accept_All_FeedbackTypes(FeedbackType type)
    {
        var entry = new FeedbackEntry(Guid.NewGuid(), Guid.NewGuid(), type, "Test message");

        entry.Type.Should().Be(type);
    }
}
