using System.Reflection;
using Certus.Domain.Platform.Interfaces;

namespace Certus.Infrastructure.Platform;

public class PluginLoader : IPlatformPluginLoader
{
    private readonly List<IPlatformPlugin> _plugins = new();
    private readonly object _lock = new();

    public IReadOnlyList<IPlatformPlugin> GetAllPlugins()
    {
        lock (_lock)
        {
            return _plugins.AsReadOnly();
        }
    }

    public IPlatformPlugin? GetPlugin(string pluginId)
    {
        lock (_lock)
        {
            return _plugins.FirstOrDefault(p => p.PluginId == pluginId);
        }
    }

    public IPlatformPlugin? GetPluginForPlatform(string platformId)
    {
        lock (_lock)
        {
            return _plugins.FirstOrDefault(p => 
                p.SupportedPlatforms.Contains(platformId, StringComparer.OrdinalIgnoreCase));
        }
    }

    public void LoadPlugins(string pluginDirectory)
    {
        if (!Directory.Exists(pluginDirectory))
        {
            Directory.CreateDirectory(pluginDirectory);
            return;
        }

        var assemblies = Directory.GetFiles(pluginDirectory, "*.dll")
            .Select(Assembly.LoadFrom)
            .ToList();

        foreach (var assembly in assemblies)
        {
            try
            {
                var pluginTypes = assembly.GetTypes()
                    .Where(t => typeof(IPlatformPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                foreach (var type in pluginTypes)
                {
                    if (Activator.CreateInstance(type) is IPlatformPlugin plugin)
                    {
                        lock (_lock)
                        {
                            _plugins.Add(plugin);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but continue loading other plugins
                Console.WriteLine($"Failed to load plugin from {assembly.FullName}: {ex.Message}");
            }
        }
    }

    public void RegisterPlugin(IPlatformPlugin plugin)
    {
        lock (_lock)
        {
            if (!_plugins.Any(p => p.PluginId == plugin.PluginId))
            {
                _plugins.Add(plugin);
            }
        }
    }
}
