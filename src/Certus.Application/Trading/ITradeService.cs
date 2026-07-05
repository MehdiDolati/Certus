using Certus.Application.Trading.DTOs;
using Certus.Domain.SharedKernel;

namespace Certus.Application.Trading;

public interface ITradeService
{
    Task<PaginatedResult<TradeDto>> GetTradesAsync(TradeFilter filter);
    Task<TradeDetailDto?> GetTradeDetailAsync(Guid tradeId);
    Task<List<PerformanceSnapshotDto>> GetCumulativePnlAsync(Guid? portfolioId, Guid? strategyId, DateRange period);
}
