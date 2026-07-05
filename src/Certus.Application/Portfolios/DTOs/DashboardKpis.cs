namespace Certus.Application.Portfolios.DTOs;

public record DashboardKpis(
    decimal TotalAum,
    decimal TotalPnl,
    decimal AvgSharpe,
    decimal MaxDrawdown,
    decimal WinRate,
    int ActiveCount);