namespace Certus.Domain.Market.ValueObjects;

public record VolumeProfile(
    decimal TotalVolume,
    decimal BuyVolume,
    decimal SellVolume)
{
    public decimal BuyRatio => TotalVolume > 0 ? BuyVolume / TotalVolume : 0m;
    public decimal SellRatio => TotalVolume > 0 ? SellVolume / TotalVolume : 0m;
    public bool IsBuyDominated => BuyVolume > SellVolume;
}
