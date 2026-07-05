namespace Certus.Domain.Market.ValueObjects;

public record PriceRange(decimal Low, decimal High)
{
    public decimal Spread => High - Low;
    public decimal Midpoint => (Low + High) / 2m;

    public bool Contains(decimal price) => price >= Low && price <= High;
}
