using System.Text.Json;
using Certus.Domain.Platform.Interfaces;
using Certus.Domain.Platform.ValueObjects;

namespace Certus.Infrastructure.Platform.Plugins.MetaTrader4;

public class Mt4Adapter : IPlatformAdapter
{
    private readonly PlatformConfig _config;
    private bool _isConnected;

    public string PlatformId => "metatrader4";
    public string PlatformName => "MetaTrader 4";

    public Mt4Adapter(PlatformConfig config)
    {
        _config = config;
    }

    public Task<PlatformConnectionResult> ConnectAsync(PlatformConfig config)
    {
        if (!string.IsNullOrEmpty(config.FilePath))
        {
            // Determine if path is a file or directory
            if (File.Exists(config.FilePath))
            {
                // It's a file — check the parent directory exists
                var dir = Path.GetDirectoryName(config.FilePath);
                if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
                {
                    return Task.FromResult(new PlatformConnectionResult
                    {
                        Success = false,
                        ErrorMessage = $"Directory does not exist: {dir}"
                    });
                }
            }
            else if (Directory.Exists(config.FilePath))
            {
                // It's a directory — look for portfolio_status.json inside
                var portfolioFile = Path.Combine(config.FilePath, "portfolio_status.json");
                if (!File.Exists(portfolioFile))
                {
                    return Task.FromResult(new PlatformConnectionResult
                    {
                        Success = false,
                        ErrorMessage = $"File not found: {portfolioFile}. Ensure the EA is running on MetaTrader."
                    });
                }
            }
            else
            {
                return Task.FromResult(new PlatformConnectionResult
                {
                    Success = false,
                    ErrorMessage = $"Path not found: {config.FilePath}"
                });
            }
        }

        _isConnected = true;

        return Task.FromResult(new PlatformConnectionResult
        {
            Success = true,
            ConnectedAt = DateTime.UtcNow
        });
    }

    public Task DisconnectAsync()
    {
        _isConnected = false;
        return Task.CompletedTask;
    }

    public Task<PlatformStatus> GetStatusAsync()
    {
        return Task.FromResult(new PlatformStatus
        {
            IsConnected = _isConnected,
            LastDataReceived = DateTime.UtcNow
        });
    }

    public Task<IReadOnlyList<PlatformPortfolio>> GetPortfoliosAsync()
    {
        if (!_isConnected)
            throw new InvalidOperationException("Not connected to MetaTrader 4");

        if (string.IsNullOrEmpty(_config.FilePath) || !File.Exists(_config.FilePath))
        {
            return Task.FromResult<IReadOnlyList<PlatformPortfolio>>(new List<PlatformPortfolio>());
        }

        var json = File.ReadAllText(_config.FilePath);
        var data = JsonSerializer.Deserialize<Mt4PortfolioData>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (data?.Portfolio == null)
        {
            return Task.FromResult<IReadOnlyList<PlatformPortfolio>>(new List<PlatformPortfolio>());
        }

        var portfolio = new PlatformPortfolio
        {
            ExternalId = data.Portfolio.Id,
            Name = data.Portfolio.Name,
            Balance = data.Portfolio.Balance,
            Equity = data.Portfolio.Equity,
            Margin = data.Portfolio.Margin,
            FreeMargin = data.Portfolio.FreeMargin,
            Profit = data.Portfolio.Profit,
            Timestamp = data.Timestamp,
            Strategies = data.Strategies?.Select(s => new PlatformStrategy
            {
                ExternalId = s.Id,
                Name = s.Name,
                IsActive = s.Active,
                Profit = s.Profit,
                TotalTrades = s.TotalTrades,
                LastTradeTime = s.LastTradeTime
            }).ToList() ?? new List<PlatformStrategy>()
        };

        return Task.FromResult<IReadOnlyList<PlatformPortfolio>>(new[] { portfolio });
    }

    public async Task<PlatformPortfolio?> GetPortfolioAsync(string externalId)
    {
        var portfolios = await GetPortfoliosAsync();
        return portfolios.FirstOrDefault(p => p.ExternalId == externalId);
    }

    public Task<IReadOnlyList<PlatformStrategy>> GetStrategiesAsync(string portfolioExternalId)
    {
        if (!_isConnected)
            throw new InvalidOperationException("Not connected to MetaTrader 4");

        // Strategies are embedded in portfolio data for MT4
        return Task.FromResult<IReadOnlyList<PlatformStrategy>>(new List<PlatformStrategy>());
    }

    public Task<IReadOnlyList<PlatformTrade>> GetTradesAsync(string strategyExternalId, DateTime? from = null, DateTime? to = null)
    {
        if (!_isConnected)
            throw new InvalidOperationException("Not connected to MetaTrader 4");

        // trades.json file would be in the same directory as portfolio file
        var tradesPath = Path.Combine(
            Path.GetDirectoryName(_config.FilePath) ?? "",
            "trades.json");

        if (!File.Exists(tradesPath))
        {
            return Task.FromResult<IReadOnlyList<PlatformTrade>>(new List<PlatformTrade>());
        }

        var json = File.ReadAllText(tradesPath);
        var data = JsonSerializer.Deserialize<Mt4TradesData>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (data?.Trades == null)
        {
            return Task.FromResult<IReadOnlyList<PlatformTrade>>(new List<PlatformTrade>());
        }

        var trades = data.Trades
            .Where(t => t.StrategyId == strategyExternalId)
            .Where(t => from == null || t.OpenTime >= from)
            .Where(t => to == null || t.OpenTime <= to)
            .Select(t => new PlatformTrade
            {
                ExternalId = t.Id,
                StrategyExternalId = t.StrategyId,
                Symbol = t.Symbol,
                Side = t.Side.ToLower() == "buy" ? Domain.Platform.ValueObjects.TradeSide.Buy : Domain.Platform.ValueObjects.TradeSide.Sell,
                Volume = t.Volume,
                OpenPrice = t.OpenPrice,
                ClosePrice = t.ClosePrice,
                StopLoss = t.StopLoss,
                TakeProfit = t.TakeProfit,
                Profit = t.Profit,
                Commission = t.Commission,
                Swap = t.Swap,
                OpenTime = t.OpenTime,
                CloseTime = t.CloseTime,
                Comment = t.Comment,
                Timestamp = data.Timestamp
            })
            .ToList();

        return Task.FromResult<IReadOnlyList<PlatformTrade>>(trades);
    }
}

// Internal DTOs for MT4 JSON format
internal class Mt4PortfolioData
{
    public DateTime Timestamp { get; set; }
    public Mt4PortfolioInfo? Portfolio { get; set; }
    public List<Mt4StrategyInfo>? Strategies { get; set; }
}

internal class Mt4PortfolioInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public decimal Equity { get; set; }
    public decimal Margin { get; set; }
    public decimal FreeMargin { get; set; }
    public decimal Profit { get; set; }
}

internal class Mt4StrategyInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool Active { get; set; }
    public decimal Profit { get; set; }
    public int TotalTrades { get; set; }
    public DateTime? LastTradeTime { get; set; }
    public List<Mt4OpenPosition>? OpenPositions { get; set; }
    public Mt4StrategyStats? Stats { get; set; }
}

internal class Mt4OpenPosition
{
    public int Ticket { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Volume { get; set; }
    public decimal OpenPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal StopLoss { get; set; }
    public decimal TakeProfit { get; set; }
    public decimal Profit { get; set; }
    public DateTime OpenTime { get; set; }
    public string Comment { get; set; } = string.Empty;
}

internal class Mt4StrategyStats
{
    public int WinningTrades { get; set; }
    public int LosingTrades { get; set; }
    public decimal TotalProfit { get; set; }
    public decimal TotalLoss { get; set; }
    public decimal WinRate { get; set; }
    public decimal ProfitFactor { get; set; }
    public decimal MaxDrawdown { get; set; }
}

internal class Mt4TradesData
{
    public DateTime Timestamp { get; set; }
    public List<Mt4TradeInfo>? Trades { get; set; }
}

internal class Mt4TradeInfo
{
    public string Id { get; set; } = string.Empty;
    public string StrategyId { get; set; } = string.Empty;
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
    public DateTime OpenTime { get; set; }
    public DateTime? CloseTime { get; set; }
    public string Comment { get; set; } = string.Empty;
}
