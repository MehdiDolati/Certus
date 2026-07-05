using Certus.Domain.Market.Aggregates;
using Certus.Domain.Market.Enums;
using Certus.Domain.SharedKernel;

namespace Certus.Domain.Market.Repositories;

public interface ISignalRepository
{
    Task<IReadOnlyList<Signal>> GetActiveSignalsAsync(Symbol? symbol, SignalDirection? direction);
    Task<Signal?> GetByIdAsync(Guid id);
    Task AddAsync(Signal signal);
    Task<int> SaveChangesAsync();
}
