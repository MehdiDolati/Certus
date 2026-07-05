# Market Data

## Status
Active

## Overview
Market Data manages price data ingestion, technical indicator computation, and signal generation for trading instruments.

## Data Model

### MarketDataSeries (Aggregate Root)
- Symbol: Symbol (trading pair)
- TimeFrame: TimeFrame (analysis period)
- PriceBars: PriceBar[] (OHLCV bars)

**Methods**: AddPriceBar, UpdatePriceBars

### Signal (Entity)
- Symbol: Symbol
- Direction: SignalDirection (Bullish/Bearish/Neutral)
- Strength: SignalStrength (0.0-1.0 confidence)
- IndicatorType: IndicatorType
- CreatedAt, ExpiresAt
- IsExpired: computed property

### Value Objects

| Value Object | Key Fields | Notes |
|--------------|------------|-------|
| PriceBar | Open, High, Low, Close, Volume, Timestamp | TypicalPrice, BodySize, IsBullish computed |
| TechnicalIndicator | Type, Value, Period | RSI, MACD, BollingerBands, etc. |
| SignalStrength | Value (0.0-1.0), Confidence | IsStrong (>0.7), IsWeak (<0.3) |
| TimeFrame | Value (e.g., "1h") | Static: M1, M5, M15, M30, H1, H4, D1 |
| PriceRange | Low, High | Spread, Midpoint, Contains |
| VolumeProfile | TotalVolume, BuyVolume, SellVolume | BuyRatio, SellRatio, IsBuyDominated |

### Enums
- IndicatorType: RSI, MACD, BollingerBands, ATR, OBV, Stochastic, CCI, WilliamsR, Custom
- SignalDirection: Bullish, Bearish, Neutral
- FundamentalFactorType: FundingRate, OpenInterest, SocialSentiment, OnChainMetric, NewsSentiment, EconomicData

### Domain Events
- MarketDataReceived: Symbol, TimeFrame, BarCount
- SignalPublished: Symbol, Direction, Strength, IndicatorType
- AnomalyDetected: Symbol, AnomalyType, Severity

## Acceptance Criteria

- [ ] AC-001: Given a PriceBar with Close > Open, when checking IsBullish, then it returns true
- [ ] AC-002: Given a SignalStrength with Value 0.8, when checking IsStrong, then it returns true
- [ ] AC-003: Given a TimeFrame H1, when checking Value, then it returns "1h"
- [ ] AC-004: Given a MarketDataSeries, when adding a PriceBar, then PriceBars list grows by 1
