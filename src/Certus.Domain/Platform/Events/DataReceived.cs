using Certus.Domain.SharedKernel;

namespace Certus.Domain.Platform.Events;

public record DataReceived : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid ConnectionId { get; init; }
    public string DataType { get; init; } = string.Empty;
    public int RecordCount { get; init; }
}
