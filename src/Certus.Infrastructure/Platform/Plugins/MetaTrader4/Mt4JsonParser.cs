using System.Text.Json;
using Certus.Domain.Platform.Interfaces;
using Certus.Domain.Platform.ValueObjects;

namespace Certus.Infrastructure.Platform.Plugins.MetaTrader4;

public class Mt4JsonParser : IPlatformDataParser
{
    public string FormatId => "json";
    public string[] SupportedExtensions => new[] { ".json" };

    public PlatformPortfolio ParsePortfolio(string data)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var json = JsonSerializer.Deserialize<Mt4PortfolioData>(data, options);
        
        if (json?.Portfolio == null)
            throw new InvalidOperationException("Invalid MT4 portfolio data format");

        return new PlatformPortfolio
        {
            ExternalId = json.Portfolio.Id,
            Name = json.Portfolio.Name,
            Balance = json.Portfolio.Balance,
            Equity = json.Portfolio.Equity,
            Margin = json.Portfolio.Margin,
            FreeMargin = json.Portfolio.FreeMargin,
            Profit = json.Portfolio.Profit,
            Timestamp = json.Timestamp,
            Strategies = json.Strategies?.Select(s => new PlatformStrategy
            {
                ExternalId = s.Id,
                Name = s.Name,
                IsActive = s.Active,
                Profit = s.Profit,
                TotalTrades = s.TotalTrades,
                LastTradeTime = s.LastTradeTime,
                Timestamp = json.Timestamp
            }).ToList() ?? new List<PlatformStrategy>()
        };
    }

    public PlatformStrategy ParseStrategy(string data)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var strategy = JsonSerializer.Deserialize<PlatformStrategy>(data, options);
        return strategy ?? throw new InvalidOperationException("Invalid MT4 strategy data format");
    }

    public PlatformTrade ParseTrade(string data)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var trade = JsonSerializer.Deserialize<PlatformTrade>(data, options);
        return trade ?? throw new InvalidOperationException("Invalid MT4 trade data format");
    }

    public IReadOnlyList<PlatformTrade> ParseTrades(string data)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var json = JsonSerializer.Deserialize<Mt4TradesData>(data, options);
        
        if (json?.Trades == null)
            return new List<PlatformTrade>();

        return json.Trades.Select(t => new PlatformTrade
        {
            ExternalId = t.Id,
            StrategyExternalId = t.StrategyId,
            Symbol = t.Symbol,
            Side = t.Side.ToLower() == "buy" ? TradeSide.Buy : TradeSide.Sell,
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
            Timestamp = json.Timestamp
        }).ToList();
    }
}
