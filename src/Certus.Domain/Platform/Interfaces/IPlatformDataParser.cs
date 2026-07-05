using Certus.Domain.Platform.ValueObjects;

namespace Certus.Domain.Platform.Interfaces;

public interface IPlatformDataParser
{
    string FormatId { get; }
    string[] SupportedExtensions { get; }
    PlatformPortfolio ParsePortfolio(string data);
    PlatformStrategy ParseStrategy(string data);
    PlatformTrade ParseTrade(string data);
    IReadOnlyList<PlatformTrade> ParseTrades(string data);
}

public interface IPlatformDataSerializer
{
    string SerializePortfolio(PlatformPortfolio portfolio);
    string SerializeStrategy(PlatformStrategy strategy);
    string SerializeTrade(PlatformTrade trade);
    string SerializeTrades(IReadOnlyList<PlatformTrade> trades);
}
