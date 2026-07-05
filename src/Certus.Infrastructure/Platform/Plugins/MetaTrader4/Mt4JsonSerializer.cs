using System.Text.Json;
using Certus.Domain.Platform.Interfaces;
using Certus.Domain.Platform.ValueObjects;

namespace Certus.Infrastructure.Platform.Plugins.MetaTrader4;

public class Mt4JsonSerializer : IPlatformDataSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public string SerializePortfolio(PlatformPortfolio portfolio)
    {
        var data = new
        {
            timestamp = portfolio.Timestamp,
            portfolio = new
            {
                id = portfolio.ExternalId,
                name = portfolio.Name,
                balance = portfolio.Balance,
                equity = portfolio.Equity,
                margin = portfolio.Margin,
                free_margin = portfolio.FreeMargin,
                profit = portfolio.Profit
            },
            strategies = portfolio.Strategies.Select(s => new
            {
                id = s.ExternalId,
                name = s.Name,
                active = s.IsActive,
                profit = s.Profit,
                total_trades = s.TotalTrades,
                winning_trades = s.WinningTrades,
                losing_trades = s.LosingTrades,
                last_trade_time = s.LastTradeTime
            })
        };

        return JsonSerializer.Serialize(data, Options);
    }

    public string SerializeStrategy(PlatformStrategy strategy)
    {
        return JsonSerializer.Serialize(strategy, Options);
    }

    public string SerializeTrade(PlatformTrade trade)
    {
        return JsonSerializer.Serialize(trade, Options);
    }

    public string SerializeTrades(IReadOnlyList<PlatformTrade> trades)
    {
        var data = new
        {
            timestamp = DateTime.UtcNow,
            trades = trades.Select(t => new
            {
                id = t.ExternalId,
                strategy_id = t.StrategyExternalId,
                symbol = t.Symbol,
                side = t.Side.ToString().ToLower(),
                volume = t.Volume,
                open_price = t.OpenPrice,
                close_price = t.ClosePrice,
                stop_loss = t.StopLoss,
                take_profit = t.TakeProfit,
                profit = t.Profit,
                commission = t.Commission,
                swap = t.Swap,
                open_time = t.OpenTime,
                close_time = t.CloseTime,
                comment = t.Comment
            })
        };

        return JsonSerializer.Serialize(data, Options);
    }
}
