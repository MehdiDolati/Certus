namespace Certus.Application.Portfolios.DTOs;

public record UpdatePortfolioRequest(
    string Name,
    string Description,
    decimal TargetReturn,
    decimal TargetSharpe,
    decimal AllocatedCapital);
