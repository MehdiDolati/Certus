using Certus.Domain.Market.ValueObjects;
using Certus.Domain.SharedKernel;

namespace Certus.Domain.Market.Aggregates;

public class MarketDataSeries : AggregateRoot
{
    private readonly List<PriceBar> _priceBars = [];
    public IReadOnlyList<PriceBar> PriceBars => _priceBars.AsReadOnly();

    public Symbol Symbol { get; private set; }
    public TimeFrame TimeFrame { get; private set; }
    public DateTime LastUpdated { get; private set; }

    private MarketDataSeries() { }

    public MarketDataSeries(Guid id, Symbol symbol, TimeFrame timeFrame) : base(id)
    {
        Symbol = symbol;
        TimeFrame = timeFrame;
        LastUpdated = DateTime.UtcNow;
    }

    public void AddPriceBar(PriceBar priceBar)
    {
        _priceBars.Add(priceBar);
        LastUpdated = DateTime.UtcNow;
    }

    public void UpdatePriceBars(IEnumerable<PriceBar> priceBars)
    {
        _priceBars.Clear();
        _priceBars.AddRange(priceBars);
        LastUpdated = DateTime.UtcNow;
    }
}
