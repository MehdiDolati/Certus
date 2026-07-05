using Certus.Application.Strategies.DTOs;
using Certus.Application.Trading.DTOs;

namespace Certus.Application.Portfolios.DTOs;

public record PortfolioDetailDto(
    PortfolioDto Summary,
    List<StrategyPerformanceDto> Strategies,
    List<PerformanceSnapshotDto> EquityCurve,
    List<PerformanceSnapshotDto> DrawdownCurve,
    List<MonthlyReturnDto> MonthlyReturns);