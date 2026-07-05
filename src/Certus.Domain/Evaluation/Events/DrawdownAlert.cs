using Certus.Domain.SharedKernel;

namespace Certus.Domain.Evaluation.Events;

public record DrawdownAlert : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public required Guid PortfolioId { get; init; }
    public decimal CurrentDrawdown { get; init; }
    public decimal Threshold { get; init; }
    public string Message { get; init; } = string.Empty;
}
