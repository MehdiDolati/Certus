using Certus.Domain.RiskAndPortfolio.Enums;
using Certus.Domain.SharedKernel;

namespace Certus.Application.Portfolios.DTOs;

public enum PortfolioSortBy
{
    Name,
    ActualReturn,
    SupposedReturn,
    Variance,
    Sharpe,
    MaxDrawdown,
    TradeCount
}

public record PortfolioFilter(
    PortfolioStatus? Status = null,
    decimal? MinAum = null,
    string? Search = null,
    PortfolioSortBy SortBy = PortfolioSortBy.Variance,
    bool Ascending = false,
    DateRange? Period = null);
