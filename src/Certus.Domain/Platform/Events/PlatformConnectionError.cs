using Certus.Domain.SharedKernel;

namespace Certus.Domain.Platform.Events;

public record PlatformConnectionError : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid ConnectionId { get; init; }
    public string PlatformId { get; init; } = string.Empty;
    public string Error { get; init; } = string.Empty;
}
