namespace Certus.Domain.Platform.Repositories;

public interface IImportedTradeRepository
{
    Task<ImportedTrade?> GetByExternalIdAsync(string externalId);
    Task<IReadOnlyList<ImportedTrade>> GetByStrategyIdAsync(Guid strategyId);
    Task<IReadOnlyList<ImportedTrade>> GetByPortfolioIdAsync(Guid portfolioId);
    Task<IReadOnlyList<ImportedTrade>> GetByDateRangeAsync(DateTime from, DateTime to);
    Task<IReadOnlyList<ImportedTrade>> GetOpenTradesAsync(Guid? strategyId = null);
    Task AddAsync(ImportedTrade trade);
    void Update(ImportedTrade trade);
    Task<int> GetCountByStrategyAsync(Guid strategyId);
    Task<decimal> GetTotalPnLByStrategyAsync(Guid strategyId);
}

public class ImportedTrade
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public Guid StrategyId { get; set; }
    public Guid PortfolioId { get; set; }
    public Guid ConnectionId { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string Side { get; set; } = string.Empty;
    public decimal Volume { get; set; }
    public decimal OpenPrice { get; set; }
    public decimal? ClosePrice { get; set; }
    public decimal StopLoss { get; set; }
    public decimal TakeProfit { get; set; }
    public decimal Profit { get; set; }
    public decimal Commission { get; set; }
    public decimal Swap { get; set; }
    public decimal PnL => Profit + Commission + Swap;
    public DateTime OpenTime { get; set; }
    public DateTime? CloseTime { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime ImportedAt { get; set; }
    public bool IsOpen => CloseTime == null;
}
