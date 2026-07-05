namespace Certus.Application.Strategies.DTOs;

public record CreateStrategyRequest(
    Guid PortfolioId,
    string Name,
    string Type,
    decimal TargetReturn,
    decimal Weight);
