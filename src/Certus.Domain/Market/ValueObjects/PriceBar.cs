using Certus.Domain.SharedKernel;

namespace Certus.Domain.Market.ValueObjects;

public record PriceBar(
    DateTime Timestamp,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    decimal Volume)
{
    public decimal TypicalPrice => (High + Low + Close) / 3m;
    public decimal BodySize => Math.Abs(Close - Open);
    public bool IsBullish => Close > Open;
}
