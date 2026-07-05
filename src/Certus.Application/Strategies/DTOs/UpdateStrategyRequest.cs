namespace Certus.Application.Strategies.DTOs;

public record UpdateStrategyRequest(
    string Name,
    string Type,
    decimal TargetReturn,
    decimal Weight);
