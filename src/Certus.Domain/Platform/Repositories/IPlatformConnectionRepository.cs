using Certus.Domain.Platform.Aggregates;
using Certus.Domain.Platform.Enums;

namespace Certus.Domain.Platform.Repositories;

public interface IPlatformConnectionRepository
{
    Task<PlatformConnection?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<PlatformConnection>> GetAllAsync();
    Task<IReadOnlyList<PlatformConnection>> GetByPlatformAsync(string platformId);
    Task<IReadOnlyList<PlatformConnection>> GetByStatusAsync(PlatformConnectionStatus status);
    Task AddAsync(PlatformConnection connection);
    void Update(PlatformConnection connection);
}
