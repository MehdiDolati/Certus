using Certus.Domain.Platform.Enums;
using Certus.Domain.SharedKernel;

namespace Certus.Domain.Platform.Aggregates;

public class PortfolioDeployment : AggregateRoot
{
    public Guid PortfolioId { get; private set; }
    public Guid ConnectionId { get; private set; }
    public string SourceFolderPath { get; private set; } = string.Empty;
    public string TargetMT4Path { get; private set; } = string.Empty;
    public DeploymentStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? DeployedAt { get; private set; }
    public DateTime? MonitoringStartedAt { get; private set; }
    public int EACount { get; private set; }
    public string? EANames { get; private set; }
    public string? ErrorMessage { get; private set; }

    public bool IsMonitoring => Status == DeploymentStatus.Monitoring;

    private PortfolioDeployment() { }

    public PortfolioDeployment(
        Guid id,
        Guid portfolioId,
        Guid connectionId,
        string sourceFolderPath,
        string targetMT4Path,
        int eaCount,
        string? eaNames = null) : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceFolderPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetMT4Path);

        PortfolioId = portfolioId;
        ConnectionId = connectionId;
        SourceFolderPath = sourceFolderPath;
        TargetMT4Path = targetMT4Path;
        EACount = eaCount;
        EANames = eaNames;
        Status = DeploymentStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public static PortfolioDeployment Create(
        Guid portfolioId,
        Guid connectionId,
        string sourceFolderPath,
        string targetMT4Path,
        int eaCount,
        string? eaNames = null)
    {
        return new PortfolioDeployment(
            Guid.NewGuid(),
            portfolioId,
            connectionId,
            sourceFolderPath,
            targetMT4Path,
            eaCount,
            eaNames);
    }

    public void StartCopying()
    {
        if (Status != DeploymentStatus.Pending)
            throw new InvalidOperationException($"Cannot start copying from {Status} status");

        Status = DeploymentStatus.Copying;
    }

    public void MarkDeployed()
    {
        if (Status != DeploymentStatus.Copying)
            throw new InvalidOperationException($"Cannot mark deployed from {Status} status");

        Status = DeploymentStatus.Deployed;
        DeployedAt = DateTime.UtcNow;
    }

    public void StartMonitoring()
    {
        if (Status != DeploymentStatus.Deployed)
            throw new InvalidOperationException($"Cannot start monitoring from {Status} status");

        Status = DeploymentStatus.Monitoring;
        MonitoringStartedAt = DateTime.UtcNow;
    }

    public void MarkError(string errorMessage)
    {
        Status = DeploymentStatus.Error;
        ErrorMessage = errorMessage;
    }

    public void Stop()
    {
        if (Status != DeploymentStatus.Monitoring)
            throw new InvalidOperationException($"Cannot stop from {Status} status");

        Status = DeploymentStatus.Stopped;
    }
}
