using Certus.Domain.Execution.Entities;
using FluentAssertions;

namespace Certus.Domain.Tests.Execution;

public class ExecutionLogTests
{
    [Fact]
    public void Constructor_Should_Set_All_Properties()
    {
        var id = Guid.NewGuid();
        var tradeId = Guid.NewGuid();

        var log = new ExecutionLog(id, tradeId, "Trade opened", "Entry at 50000");

        log.Id.Should().Be(id);
        log.TradeId.Should().Be(tradeId);
        log.Action.Should().Be("Trade opened");
        log.Details.Should().Be("Entry at 50000");
        log.LoggedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Constructor_Should_Set_LoggedAt_To_UtcNow()
    {
        var before = DateTime.UtcNow;
        var log = new ExecutionLog(Guid.NewGuid(), Guid.NewGuid(), "action", "details");
        var after = DateTime.UtcNow;

        log.LoggedAt.Should().BeOnOrAfter(before);
        log.LoggedAt.Should().BeOnOrBefore(after);
    }
}
