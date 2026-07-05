using Certus.Domain.SharedKernel;
using Certus.Domain.Execution.Enums;

namespace Certus.Application.Trading.DTOs;

public record TradeFilter(
    Guid? PortfolioId = null,
    Guid? StrategyId = null,
    string? Symbol = null,
    TradeSide? Side = null,
    TradeStatus? Status = null,
    DateRange? Period = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 20);
