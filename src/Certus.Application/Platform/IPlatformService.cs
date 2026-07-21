using Certus.Application.Platform.DTOs;
using Certus.Domain.Platform.Enums;

namespace Certus.Application.Platform;

public interface IPlatformService
{
    Task<PlatformConnectionDto> ConnectAsync(ConnectPlatformRequest request);
    Task DisconnectAsync(Guid connectionId);
    Task<List<PlatformConnectionDto>> GetConnectionsAsync();
    Task<PlatformConnectionDto?> GetConnectionAsync(Guid connectionId);
    Task<PlatformStatusDto> GetStatusAsync(Guid connectionId);
    Task<PlatformPortfolioDto?> ImportPortfolioAsync(Guid connectionId, string externalPortfolioId, string? portfolioName = null);
    Task<List<PlatformStrategyDto>> GetStrategiesAsync(Guid connectionId, string portfolioExternalId);
    Task<ImportTradesResult> ImportTradesAsync(Guid connectionId, string strategyExternalId, DateTime? from = null, DateTime? to = null);
    Task<PlatformDashboardDto> GetDashboardAsync();
    Task ImportFromChangeAsync(Guid connectionId, string filePath);
}
