using Certus.Domain.Market.Aggregates;
using Certus.Domain.Market.Enums;
using Certus.Domain.Market.Repositories;
using Certus.Domain.SharedKernel;

namespace Certus.Infrastructure.Persistence.Repositories.Market;

public class SignalRepository : ISignalRepository
{
    private readonly CertusDbContext _db;

    public SignalRepository(CertusDbContext db)
    {
        _db = db;
    }

    public Task<IReadOnlyList<Signal>> GetActiveSignalsAsync(Symbol? symbol, SignalDirection? direction)
    {
        return Task.FromResult<IReadOnlyList<Signal>>(new List<Signal>());
    }

    public Task<Signal?> GetByIdAsync(Guid id)
    {
        return Task.FromResult<Signal?>(null);
    }

    public Task AddAsync(Signal signal)
    {
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync()
    {
        return Task.FromResult(0);
    }
}
