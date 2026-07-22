using Certus.Domain.Platform.Entities;

namespace Certus.Domain.Platform.Repositories;

public interface IStrategyMappingRepository
{
    Task<StrategyMapping?> GetByConnectionAndExternalIdAsync(Guid connectionId, string strategyExternalId);
    Task<IReadOnlyList<StrategyMapping>> GetByConnectionIdAsync(Guid connectionId);
    Task AddAsync(StrategyMapping mapping);
}
