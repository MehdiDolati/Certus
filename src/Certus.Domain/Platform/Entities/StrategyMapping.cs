using Certus.Domain.SharedKernel;

namespace Certus.Domain.Platform.Entities;

public class StrategyMapping : Entity
{
    public Guid ConnectionId { get; private set; }
    public string StrategyExternalId { get; private set; } = string.Empty;
    public Guid StrategyDefinitionId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private StrategyMapping() { }

    public StrategyMapping(Guid id, Guid connectionId, string strategyExternalId, Guid strategyDefinitionId) : base(id)
    {
        ConnectionId = connectionId;
        StrategyExternalId = strategyExternalId;
        StrategyDefinitionId = strategyDefinitionId;
        CreatedAt = DateTime.UtcNow;
    }
}
