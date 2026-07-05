using Certus.Domain.SharedKernel;

namespace Certus.Domain.Platform.Events;

public record StrategyStatusChanged : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid StrategyId { get; init; }
    public string ExternalId { get; init; } = string.Empty;
    public bool IsNowActive { get; init; }
    public bool WasActive { get; init; }
}
