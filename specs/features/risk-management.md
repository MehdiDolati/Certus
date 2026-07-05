# Risk Management

## Status
Active

## Overview
Risk Management enforces risk limits, manages capital allocation, and assesses portfolio risk across strategies.

## Data Model

### RiskLimit (Entity)
- PortfolioId: Guid
- Type: RiskLimitType
- Threshold: decimal
- CurrentValue: decimal
- WarningLevel: decimal
- CriticalLevel: decimal

**Methods**: UpdateValue(newValue) — updates CurrentValue and evaluates breach

**Computed**: IsBreached (CurrentValue > Threshold)

### AllocationRule (Entity)
- PortfolioId, StrategyId: Guid
- TargetWeight, MinWeight, MaxWeight: decimal

### RiskAssessment (Entity)
- PortfolioId: Guid
- RiskLevel: RiskLevel
- AssessedAt: DateTime
- Metrics: dictionary of risk metric values

### CapitalAllocation (Entity)
- PortfolioId, StrategyId: Guid
- AllocatedAmount: Money
- Percentage: decimal

### Value Objects

| Value Object | Key Fields | Notes |
|--------------|------------|-------|
| AdaptiveRiskParams | VolatilityScalingFactor, ConfidenceThreshold, MarketRegime | Dynamic risk adjustment |
| AllocationTarget | StrategyId, TargetWeight, MinWeight, MaxWeight | Target allocation per strategy |
| DrawdownLimit | MaxDrawdown, WarningLevel, HaltLevel | Validates: HaltLevel < WarningLevel < MaxDrawdown |
| PositionLimit | MaxPositionSize, MaxLeverage, MaxConcurrentPositions | Position constraints |
| VaRMetric | Value, ConfidenceLevel, TimeHorizon | Value at Risk |

### Enums
- RiskLimitType: MaxDrawdown, MaxPositionSize, MaxLeverage, VaR, Correlation, Concentration
- RiskLevel: Normal, Elevated, Warning, Critical
- MarketRegime: Trending, Ranging, Volatile, Crisis

### Domain Events
- PortfolioCreated: PortfolioId, Name, Capital
- CapitalReallocated: PortfolioId, NewCapital
- RiskLimitBreached: PortfolioId, LimitType, CurrentValue, Threshold
- RiskLimitAdjusted: PortfolioId, LimitType, OldValue, NewValue
- RiskAssessmentCompleted: PortfolioId, RiskLevel

## Acceptance Criteria

- [ ] AC-001: Given a RiskLimit with Threshold 100, when CurrentValue becomes 110, then IsBreached is true
- [ ] AC-002: Given a RiskLimit with WarningLevel 80, when CurrentValue is 85, then risk level is Warning
- [ ] AC-003: Given a DrawdownLimit, when creating with HaltLevel > WarningLevel, then ArgumentException is thrown
