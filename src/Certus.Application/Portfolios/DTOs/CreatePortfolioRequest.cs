namespace Certus.Application.Portfolios.DTOs;

public record CreatePortfolioRequest(
    string Name,
    string Description,
    decimal TargetReturn,
    decimal TargetSharpe,
    decimal AllocatedCapital);
