using Certus.Application.Platform.DTOs;

namespace Certus.Application.Platform;

public interface IPortfolioDeploymentService
{
    Task<ValidateFolderResult> ValidateFolderAsync(string folderPath);
    Task<List<PlatformConnectionDto>> GetAvailableConnectionsAsync();
    Task<DeriveMT4PathResult> DeriveMT4PathAsync(Guid connectionId);
    Task<CheckConflictsResult> CheckConflictsAsync(string sourceFolder, string targetExpertsPath);
    Task<DeployPortfolioResult> DeployAsync(DeployPortfolioRequest request);
    Task<PortfolioDeploymentDto?> GetDeploymentStatusAsync(Guid deploymentId);
    Task StopMonitoringAsync(Guid deploymentId);
    Task<List<PortfolioDeploymentDto>> GetDeploymentsAsync();
    Task<bool> HasExistingDeploymentAsync(Guid connectionId);
}
