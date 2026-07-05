using Certus.Domain.Execution.Aggregates;
using Certus.Domain.Execution.Enums;
using Certus.Domain.SharedKernel;

namespace Certus.Domain.Execution.Repositories;

public interface ITradeRepository
{
    Task<Trade?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<Trade>> GetByPortfolioIdAsync(Guid portfolioId);
    Task<IReadOnlyList<Trade>> GetByStrategyIdAsync(Guid strategyId);
    Task<IReadOnlyList<Trade>> GetOpenTradesAsync(Guid? portfolioId, Guid? strategyId);
    Task<IReadOnlyList<Trade>> GetFilteredAsync(
        Guid? portfolioId, Guid? strategyId, Symbol? symbol,
        TradeSide? side, TradeStatus? status,
        DateTime? dateFrom, DateTime? dateTo, string? search,
        int page, int pageSize);
    Task<int> CountFilteredAsync(
        Guid? portfolioId, Guid? strategyId, Symbol? symbol,
        TradeSide? side, TradeStatus? status,
        DateTime? dateFrom, DateTime? dateTo, string? search);
    Task AddAsync(Trade trade);
    void Update(Trade trade);
    Task<int> SaveChangesAsync();
}
