using Certus.Domain.RiskAndPortfolio.Aggregates;
using Certus.Domain.RiskAndPortfolio.Enums;
using Certus.Domain.Strategy.Aggregates;
using Certus.Domain.Strategy.Enums;
using Certus.Domain.Strategy.ValueObjects;
using Certus.Domain.Execution.Aggregates;
using Certus.Domain.Execution.Enums;
using Certus.Domain.Evaluation.Entities;
using Certus.Domain.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Certus.Infrastructure.Persistence;

public static class SeedData
{
    private static readonly Guid AlphaId = new("a1111111-1111-1111-1111-111111111111");
    private static readonly Guid BetaId = new("b2222222-2222-2222-2222-222222222222");
    private static readonly Guid GammaId = new("c3333333-3333-3333-3333-333333333333");
    private static readonly Guid DeltaId = new("d4444444-4444-4444-4444-444444444444");
    private static readonly Guid EpsilonId = new("e5555555-5555-5555-5555-555555555555");

    private static readonly Guid S_AlphaMomentum = new("a1000001-0000-0000-0000-000000000001");
    private static readonly Guid S_AlphaTrend = new("a1000002-0000-0000-0000-000000000002");
    private static readonly Guid S_BetaMeanRev = new("b2000001-0000-0000-0000-000000000001");
    private static readonly Guid S_BetaStatArb = new("b2000002-0000-0000-0000-000000000002");
    private static readonly Guid S_BetaArb = new("b2000003-0000-0000-0000-000000000003");
    private static readonly Guid S_GammaArb = new("c3000001-0000-0000-0000-000000000001");
    private static readonly Guid S_GammaMomentum = new("c3000002-0000-0000-0000-000000000002");
    private static readonly Guid S_DeltaTrend = new("d4000001-0000-0000-0000-000000000001");
    private static readonly Guid S_DeltaMeanRev = new("d4000002-0000-0000-0000-000000000002");
    private static readonly Guid S_DeltaStatArb = new("d4000003-0000-0000-0000-000000000003");
    private static readonly Guid S_EpsilonMomentum = new("e5000001-0000-0000-0000-000000000001");
    private static readonly Guid S_EpsilonArb = new("e5000002-0000-0000-0000-000000000002");

    private static readonly Guid ReportId = new("f0000001-0000-0000-0000-000000000001");

    public static async Task SeedAsync(CertusDbContext db)
    {
        if (await db.Portfolios.AnyAsync())
            return;

        var now = DateTime.UtcNow;
        var startDate = DateOnly.FromDateTime(now.AddDays(-90));

        var portfolios = CreatePortfolios(now);
        db.Portfolios.AddRange(portfolios);

        var strategies = CreateStrategies(now);
        db.Strategies.AddRange(strategies);

        var trades = CreateTrades(now);
        db.Trades.AddRange(trades);

        var snapshots = CreatePerformanceSnapshots(startDate, now);
        db.PerformanceSnapshots.AddRange(snapshots);

        await db.SaveChangesAsync();
    }

    private static List<Portfolio> CreatePortfolios(DateTime now) =>
    [
        new Portfolio(AlphaId, "Alpha Fund", 0.25m, 1.8m, new Money(2_500_000m, Currency.USD),
            "High-frequency momentum portfolio targeting aggressive returns")
        {
        },
        new Portfolio(BetaId, "Beta Fund", 0.15m, 1.2m, new Money(1_800_000m, Currency.USD),
            "Multi-strategy diversified portfolio with steady growth")
        {
        },
        new Portfolio(GammaId, "Gamma Fund", 0.10m, 2.0m, new Money(3_200_000m, Currency.USD),
            "Arbitrage-focused portfolio for consistent low-risk returns")
        {
        },
        new Portfolio(DeltaId, "Delta Fund", 0.20m, 1.5m, new Money(900_000m, Currency.USD),
            "Experimental strategy fund in monitoring phase")
        {
        },
        new Portfolio(EpsilonId, "Epsilon Fund", 0.12m, 1.0m, new Money(600_000m, Currency.USD),
            "Legacy fund that completed its mandate")
        {
        }
    ];

    private static StrategyType Momentum() => new(StrategyCategory.Momentum);
    private static StrategyType TrendFollowing() => new(StrategyCategory.TrendFollowing);
    private static StrategyType MeanReversion() => new(StrategyCategory.MeanReversion);
    private static StrategyType StatisticalArb() => new(StrategyCategory.StatisticalArb);
    private static StrategyType Arbitrage() => new(StrategyCategory.Arbitrage);

    private static List<StrategyDefinition> CreateStrategies(DateTime now)
    {
        var strategies = new List<StrategyDefinition>
        {
            CreateStrategy(S_AlphaMomentum, "Alpha Momentum", Momentum(), 0.30m, "Alpha momentum strategy"),
            CreateStrategy(S_AlphaTrend, "Alpha Trend Following", TrendFollowing(), 0.20m, "Alpha trend following"),
            CreateStrategy(S_BetaMeanRev, "Beta Mean Reversion", MeanReversion(), 0.12m, "Beta mean reversion"),
            CreateStrategy(S_BetaStatArb, "Beta Statistical Arbitrage", StatisticalArb(), 0.18m, "Beta stat arb"),
            CreateStrategy(S_BetaArb, "Beta Arbitrage", Arbitrage(), 0.10m, "Beta arbitrage"),
            CreateStrategy(S_GammaArb, "Gamma Arbitrage", Arbitrage(), 0.12m, "Gamma arbitrage"),
            CreateStrategy(S_GammaMomentum, "Gamma Momentum", Momentum(), 0.08m, "Gamma momentum"),
            CreateStrategy(S_DeltaTrend, "Delta Trend Following", TrendFollowing(), 0.22m, "Delta trend following"),
            CreateStrategy(S_DeltaMeanRev, "Delta Mean Reversion", MeanReversion(), 0.15m, "Delta mean reversion"),
            CreateStrategy(S_DeltaStatArb, "Delta Stat Arb", StatisticalArb(), 0.18m, "Delta stat arb"),
            CreateStrategy(S_EpsilonMomentum, "Epsilon Momentum", Momentum(), 0.14m, "Epsilon momentum"),
            CreateStrategy(S_EpsilonArb, "Epsilon Arbitrage", Arbitrage(), 0.10m, "Epsilon arbitrage"),
        };

        return strategies;
    }

    private static StrategyDefinition CreateStrategy(Guid id, string name, StrategyType type, decimal targetReturn, string description)
    {
        var strategy = new StrategyDefinition(id, name, type, targetReturn, description);
        strategy.Activate();
        return strategy;
    }

    private static readonly string[] Symbols = ["BTC/USD", "ETH/USD", "SOL/USD"];
    private static readonly Random Rng = new(42);

    private static List<Trade> CreateTrades(DateTime now)
    {
        var trades = new List<Trade>();
        var strategyIds = new[]
        {
            (S_AlphaMomentum, 18), (S_AlphaTrend, 15),
            (S_BetaMeanRev, 14), (S_BetaStatArb, 13), (S_BetaArb, 12),
            (S_GammaArb, 16), (S_GammaMomentum, 10),
            (S_DeltaTrend, 11), (S_DeltaMeanRev, 9), (S_DeltaStatArb, 8),
            (S_EpsilonMomentum, 12), (S_EpsilonArb, 10)
        };

        var portfolioMap = new Dictionary<Guid, Guid>
        {
            [S_AlphaMomentum] = AlphaId, [S_AlphaTrend] = AlphaId,
            [S_BetaMeanRev] = BetaId, [S_BetaStatArb] = BetaId, [S_BetaArb] = BetaId,
            [S_GammaArb] = GammaId, [S_GammaMomentum] = GammaId,
            [S_DeltaTrend] = DeltaId, [S_DeltaMeanRev] = DeltaId, [S_DeltaStatArb] = DeltaId,
            [S_EpsilonMomentum] = EpsilonId, [S_EpsilonArb] = EpsilonId
        };

        var agentReasons = new[]
        {
            "Signal strength exceeded threshold on 4h timeframe",
            "Mean reversion setup detected at key support level",
            "Breakout confirmed with volume surge above 2x average",
            "Arbitrage spread widened beyond 0.5% on cross-exchange",
            "Trend exhaustion signal — trailing stop triggered",
            "Volatility contraction pattern completed, breakout imminent",
            "Funding rate flip signaled directional opportunity",
            "Correlation breakdown between pair created hedge opportunity",
            "Order flow imbalance detected with smart money accumulation",
            "RSI divergence at swing point with declining volume"
        };

        var tradeIdCounter = 0;
        foreach (var (stratId, count) in strategyIds)
        {
            for (var i = 0; i < count; i++)
            {
                tradeIdCounter++;
                var tradeId = new Guid($"f{tradeIdCounter:D3}0000-0000-0000-0000-000000000000");
                var symbolStr = Symbols[Rng.Next(Symbols.Length)];
                var side = Rng.Next(2) == 0 ? TradeSide.Long : TradeSide.Short;
                var entryDayOffset = Rng.Next(0, 85);
                var entryTime = now.AddDays(-entryDayOffset).AddHours(-Rng.Next(0, 12));

                var basePrice = symbolStr switch
                {
                    "BTC/USD" => 95_000m + Rng.Next(-5000, 10000),
                    "ETH/USD" => 3_200m + Rng.Next(-300, 500),
                    "SOL/USD" => 180m + Rng.Next(-30, 40),
                    _ => 100m
                };

                var entryPrice = basePrice + Rng.Next(-100, 100);
                var pnlPercent = (decimal)(Rng.NextDouble() * 16 - 6) / 100m;
                var quantity = Math.Round((decimal)(5_000 + Rng.Next(0, 45_000)) / entryPrice, 8);
                var agentReason = agentReasons[Rng.Next(agentReasons.Length)];

                var symbol = new Symbol(symbolStr, AssetClass.Crypto);
                var trade = new Trade(tradeId, stratId, portfolioMap[stratId], symbol, side, entryPrice, quantity, agentReason);
                trades.Add(trade);
            }
        }

        return trades;
    }

    private static List<PerformanceSnapshot> CreatePerformanceSnapshots(DateOnly startDate, DateTime now)
    {
        var snapshots = new List<PerformanceSnapshot>();
        var rng = new Random(42);

        var portfolioConfigs = new[]
        {
            (AlphaId, 2_500_000m, 0.25m, 0.0025m),
            (BetaId, 1_800_000m, 0.15m, 0.0015m),
            (GammaId, 3_200_000m, 0.10m, 0.0010m),
            (DeltaId, 900_000m, 0.20m, 0.0020m),
            (EpsilonId, 600_000m, 0.12m, 0.0012m)
        };

        foreach (var (portId, capital, targetReturn, dailyTarget) in portfolioConfigs)
        {
            var equity = capital;
            var maxEquity = capital;
            var cumulativeReturn = 0m;
            var supposedCumulative = 0m;
            var dailyReturns = new List<decimal>();

            for (var d = 0; d < 90; d++)
            {
                var date = startDate.AddDays(d);
                var dailyReturn = (decimal)(rng.NextDouble() * 0.06 - 0.022);
                dailyReturns.Add(dailyReturn);
                cumulativeReturn += dailyReturn;
                supposedCumulative += dailyTarget;
                equity = capital * (1 + cumulativeReturn);
                maxEquity = Math.Max(maxEquity, equity);
                var drawdown = maxEquity > 0 ? (maxEquity - equity) / maxEquity : 0m;

                var sharpeValues = dailyReturns.TakeLast(30).ToList();
                var avg = sharpeValues.Count > 0 ? sharpeValues.Average() : 0m;
                var std = sharpeValues.Count > 1
                    ? (decimal)Math.Sqrt((double)sharpeValues.Select(x => (x - avg) * (x - avg)).Average())
                    : 1m;
                var sharpe = std > 0 ? avg / std * (decimal)Math.Sqrt(365) : 0m;

                snapshots.Add(new PerformanceSnapshot(
                    Guid.NewGuid(), ReportId, portId, null, date,
                    cumulativeReturn, supposedCumulative,
                    Math.Round(capital * dailyReturn, 2), Math.Round(equity, 2),
                    Math.Round(drawdown, 6), Math.Round(sharpe, 6)));
            }
        }

        return snapshots;
    }
}
