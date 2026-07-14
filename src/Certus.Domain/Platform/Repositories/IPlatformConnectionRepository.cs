using Certus.Domain.Platform.Aggregates;

namespace Certus.Domain.Platform.Repositories;

public interface IPlatformConnectionRepository
{
    Task<PlatformConnection?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<PlatformConnection>> GetAllAsync();
    Task<IReadOnlyList<PlatformConnection>> GetByPlatformAsync(string platformId);
    Task<IReadOnlyList<PlatformConnection>> GetByPlatformAndPathAsync(string platformId, string filePath);
    Task AddAsync(PlatformConnection connection);
    void Update(PlatformConnection connection);
}
