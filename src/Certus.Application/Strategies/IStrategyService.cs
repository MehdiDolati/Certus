using Certus.Application.Strategies.DTOs;
using Certus.Application.Trading.DTOs;
using Certus.Domain.SharedKernel;

namespace Certus.Application.Strategies;

public interface IStrategyService
{
    Task<List<StrategyPerformanceDto>> GetStrategiesByPortfolioAsync(Guid portfolioId, DateRange period);
    Task<List<PerformanceSnapshotDto>> GetStrategyPerformanceAsync(Guid strategyId, DateRange period);
    Task<StrategyPerformanceDto> CreateAsync(CreateStrategyRequest request);
    Task<StrategyPerformanceDto?> UpdateAsync(Guid id, UpdateStrategyRequest request);
    Task<bool> DeleteAsync(Guid id);
}
