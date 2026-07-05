using Certus.Domain.Coordination.Enums;
using Certus.Domain.SharedKernel;

namespace Certus.Domain.Coordination.Events;

public record AgentStatusChanged : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public required AgentType AgentType { get; init; }
    public required AgentStatus OldStatus { get; init; }
    public required AgentStatus NewStatus { get; init; }
}
