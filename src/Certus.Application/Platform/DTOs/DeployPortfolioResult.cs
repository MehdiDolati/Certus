namespace Certus.Application.Platform.DTOs;

public record DeployPortfolioResult
{
    public bool Success { get; init; }
    public Guid PortfolioId { get; init; }
    public Guid DeploymentId { get; init; }
    public int FilesCopied { get; init; }
    public int FilesSkipped { get; init; }
    public bool ActivationPending { get; init; }
    public string? ErrorMessage { get; init; }
}
