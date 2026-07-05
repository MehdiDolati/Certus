# Evaluation & Feedback

## Status
Active

## Overview
Evaluation & Feedback measures performance, compares against benchmarks, scores strategies, and generates improvement recommendations.

## Requirements

### Functional Requirements
- [ ] FR-001: Generate performance reports for portfolios or strategies over a time period
- [ ] FR-002: Calculate return metrics (total return, annualized return, monthly/daily returns)
- [ ] FR-003: Calculate risk metrics (Sharpe ratio, Sortino ratio, max drawdown, Calmar ratio)
- [ ] FR-004: Calculate trade metrics (win rate, profit factor, expectancy)
- [ ] FR-005: Compare performance against benchmarks (buy-and-hold, risk-free rate)
- [ ] FR-006: Score strategies with an evaluation score (overall, consistency, risk-adjusted, trend)
- [ ] FR-007: Generate feedback entries with recommendations

### Non-Functional Requirements
- [ ] NFR-001: Reports must be immutable once generated (use UpdateMetrics for changes)
- [ ] NFR-002: Evaluation scores must use defined thresholds

## Data Model

### PerformanceReport (Aggregate Root)
```
PerformanceReport
  Id               : Guid
  PortfolioId      : Guid
  StrategyId       : Guid?             // Null = portfolio-level
  Period           : EvaluationPeriod  // Daily | Weekly | Monthly | Quarterly | Yearly | Custom
  StartDate        : DateTime
  EndDate          : DateTime
  ReturnMetrics    : ReturnMetrics
  RiskMetrics      : RiskMetrics
  TradeMetrics     : TradeMetrics
  Score            : EvaluationScore
  GeneratedAt      : DateTime
  Snapshots        : PerformanceSnapshot[]
  Benchmarks       : BenchmarkComparison[]
  Evaluations      : StrategyEvaluation[]
  Feedback         : FeedbackEntry[]
```

### PerformanceSnapshot (Entity)
```
PerformanceSnapshot
  Id               : Guid
  Date             : DateOnly
  ActualReturn     : decimal
  SupposedReturn   : decimal
  DailyPnL         : decimal
  Equity           : decimal
  Drawdown         : decimal
  Sharpe           : decimal
```

### BenchmarkComparison (Entity)
```
BenchmarkComparison
  Id               : Guid
  BenchmarkType    : BenchmarkType     // BuyAndHold | EqualWeight | RiskFreeRate | Custom
  BenchmarkReturn  : decimal
  Alpha            : decimal
  Beta             : decimal
  InformationRatio : decimal
  TrackingError    : decimal
```

### StrategyEvaluation (Entity)
```
StrategyEvaluation
  Id               : Guid
  StrategyId       : Guid
  Score            : EvaluationScore
  Recommendation   : string
  EvaluatedAt      : DateTime
```

### FeedbackEntry (Entity)
```
FeedbackEntry
  Id               : Guid
  Type             : FeedbackType      // Positive | Warning | Critical | Recommendation
  Message          : string
  RelatedStrategyId : Guid?
  GeneratedAt      : DateTime
```

## Value Objects

### EvaluationScore
```
EvaluationScore
  Overall          : decimal           // 0.0 - 1.0
  Consistency      : decimal           // 0.0 - 1.0
  RiskAdjusted     : decimal           // 0.0 - 1.0
  Trend            : TrendDirection    // Improving | Stable | Declining

  IsExcellent      : bool              // Overall > 0.8 AND Consistency > 0.7
  IsGood           : bool              // Overall > 0.6 AND Consistency > 0.5
  IsPoor           : bool              // Overall < 0.4 OR Consistency < 0.3
```

### ReturnMetrics
```
ReturnMetrics
  TotalReturn      : decimal
  AnnualizedReturn : decimal
  MonthlyReturns   : decimal[]
  DailyReturns     : decimal[]
```

### RiskMetrics
```
RiskMetrics
  SharpeRatio      : decimal
  SortinoRatio     : decimal
  MaxDrawdown      : decimal
  CalmarRatio      : decimal
  Volatility       : decimal
  VaR              : decimal
```

### TradeMetrics
```
TradeMetrics
  TotalTrades      : int
  WinRate          : decimal
  ProfitFactor     : decimal
  AvgWin           : decimal
  AvgLoss          : decimal
  Expectancy       : decimal
```

## Business Rules

1. **BR-001**: PerformanceReport is created with initial metrics, updated via UpdateMetrics
2. **BR-002**: EvaluationScore.IsExcellent requires Overall > 0.8 AND Consistency > 0.7
3. **BR-003**: EvaluationScore.IsPoor triggers when Overall < 0.4 OR Consistency < 0.3
4. **BR-004**: StrategyId null means portfolio-level report

## Acceptance Criteria

- [ ] AC-001: Given a PerformanceReport, when adding a snapshot, then snapshot is in the Snapshots list
- [ ] AC-002: Given a PerformanceReport, when updating metrics, then all metrics and GeneratedAt are updated
- [ ] AC-003: Given an EvaluationScore with Overall 0.9 and Consistency 0.8, when checking IsExcellent, then it returns true
- [ ] AC-004: Given an EvaluationScore with Overall 0.3, when checking IsPoor, then it returns true
- [ ] AC-005: Given a report with StrategyId null, then it represents portfolio-level performance
- [ ] AC-006: Given a report with StrategyId set, then it represents strategy-level performance

## Test Mapping

| Acceptance Criteria | Test File | Test Method |
|---------------------|-----------|-------------|
| AC-001 | PerformanceReportTests.cs | AddSnapshot_Should_Add_To_List |
| AC-002 | PerformanceReportTests.cs | UpdateMetrics_Should_Update_All_Fields |
| AC-003 | EvaluationScoreTests.cs | IsExcellent_Should_Be_True_When_High_Scores |
| AC-004 | EvaluationScoreTests.cs | IsPoor_Should_Be_True_When_Low_Scores |
| AC-005 | PerformanceReportTests.cs | StrategyId_Null_Should_Be_Portfolio_Level |
| AC-006 | PerformanceReportTests.cs | StrategyId_Set_Should_Be_Strategy_Level |

## Dependencies

- Domain: SharedKernel (AggregateRoot, IDomainEvent)
- Domain: Evaluation.Enums (EvaluationPeriod, BenchmarkType, FeedbackType)
- Domain: Evaluation.ValueObjects (ReturnMetrics, RiskMetrics, TradeMetrics, BenchmarkResult, EvaluationScore)
