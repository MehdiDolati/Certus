using Certus.Domain.Platform.Interfaces;
using Certus.Domain.Platform.ValueObjects;

namespace Certus.Infrastructure.Platform.Plugins.MetaTrader4;

public class Mt4Plugin : IPlatformPlugin
{
    public string PluginId => "metatrader4";
    public string PluginName => "MetaTrader 4 Plugin";
    public string Version => "1.0.0";
    public string Description => "Plugin for MetaTrader 4 platform integration via file-based data exchange";
    public IReadOnlyList<string> SupportedPlatforms => new[] { "metatrader4", "mt4" };

    public IPlatformAdapter CreateAdapter(PlatformConfig config)
    {
        return new Mt4Adapter(config);
    }

    public IReadOnlyList<IPlatformDataParser> GetParsers()
    {
        return new List<IPlatformDataParser>
        {
            new Mt4JsonParser()
        };
    }

    public IPlatformDataSerializer GetSerializer()
    {
        return new Mt4JsonSerializer();
    }

    public bool CanHandle(PlatformConfig config)
    {
        return config.DataFormat == Domain.Platform.Enums.DataFormat.Json;
    }
}
