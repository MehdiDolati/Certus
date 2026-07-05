using Certus.Application.Portfolios.DTOs;
using Certus.Application.Trading.DTOs;
using Certus.Domain.SharedKernel;

namespace Certus.Application.Portfolios;

public interface IPortfolioService
{
    Task<List<PortfolioDto>> GetAllPortfoliosAsync(PortfolioFilter filter);
    Task<PortfolioDetailDto?> GetPortfolioDetailAsync(Guid id, DateRange period);
    Task<DashboardKpis> GetDashboardKpisAsync(DateRange period);
    Task<List<PerformanceSnapshotDto>> GetPerformanceHistoryAsync(Guid id, DateRange period);
    Task<PortfolioDto> CreateAsync(CreatePortfolioRequest request);
    Task<PortfolioDto?> UpdateAsync(Guid id, UpdatePortfolioRequest request);
    Task<bool> DeleteAsync(Guid id);
}
