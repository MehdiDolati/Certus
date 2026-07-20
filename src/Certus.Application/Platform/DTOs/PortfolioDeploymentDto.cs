using Certus.Domain.Platform.Enums;

namespace Certus.Application.Platform.DTOs;

public record PortfolioDeploymentDto
{
    public Guid Id { get; init; }
    public Guid PortfolioId { get; init; }
    public string PortfolioName { get; init; } = string.Empty;
    public Guid ConnectionId { get; init; }
    public string ConnectionName { get; init; } = string.Empty;
    public string SourceFolderPath { get; init; } = string.Empty;
    public DeploymentStatus Status { get; init; }
    public int EACount { get; init; }
    public DateTime? DeployedAt { get; init; }
    public DateTime? MonitoringStartedAt { get; init; }
    public DateTime? LastDataReceivedAt { get; init; }
    public string? ErrorMessage { get; init; }
}
