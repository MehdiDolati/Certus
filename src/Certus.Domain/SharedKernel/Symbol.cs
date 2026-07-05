namespace Certus.Domain.SharedKernel;

public enum AssetClass
{
    Forex = 0,
    Crypto = 1,
    CFD = 2,
    Futures = 3
}

public record Symbol(string Value, AssetClass AssetClass)
{
    public override string ToString() => Value;
}
