using Certus.Domain.Platform.ValueObjects;

namespace Certus.Domain.Platform.Interfaces;

public interface IPlatformPlugin
{
    string PluginId { get; }
    string PluginName { get; }
    string Version { get; }
    string Description { get; }
    IReadOnlyList<string> SupportedPlatforms { get; }
    IPlatformAdapter CreateAdapter(PlatformConfig config);
    IReadOnlyList<IPlatformDataParser> GetParsers();
    IPlatformDataSerializer GetSerializer();
    bool CanHandle(PlatformConfig config);
}

public interface IPlatformPluginLoader
{
    IReadOnlyList<IPlatformPlugin> GetAllPlugins();
    IPlatformPlugin? GetPlugin(string pluginId);
    IPlatformPlugin? GetPluginForPlatform(string platformId);
    void LoadPlugins(string pluginDirectory);
}
