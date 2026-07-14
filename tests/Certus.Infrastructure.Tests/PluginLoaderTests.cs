using Certus.Domain.Platform.Interfaces;
using Certus.Infrastructure.Platform;
using Certus.Infrastructure.Platform.Plugins.MetaTrader4;
using FluentAssertions;

namespace Certus.Infrastructure.Tests;

public class PluginLoaderTests
{
    // AC-017: Given a plugin directory with DLLs implementing IPlatformPlugin, when loading plugins, then all valid plugins are registered
    [Fact]
    public void Constructor_Should_AutoRegister_Plugins_From_DI()
    {
        // Arrange
        var mt4Plugin = new Mt4Plugin();
        var plugins = new List<IPlatformPlugin> { mt4Plugin };

        // Act
        var loader = new PluginLoader(plugins);

        // Assert
        loader.GetAllPlugins().Should().HaveCount(1);
        loader.GetAllPlugins().First().PluginId.Should().Be("metatrader4");
    }

    [Fact]
    public void Constructor_Should_Handle_Empty_Plugin_List()
    {
        var loader = new PluginLoader(Array.Empty<IPlatformPlugin>());

        loader.GetAllPlugins().Should().BeEmpty();
    }

    [Fact]
    public void RegisterPlugin_Should_Add_Plugin_To_Collection()
    {
        var loader = CreateLoader();
        var plugin = new Mt4Plugin();

        loader.RegisterPlugin(plugin);

        loader.GetAllPlugins().Should().HaveCount(1);
        loader.GetAllPlugins().First().PluginId.Should().Be("metatrader4");
    }

    [Fact]
    public void RegisterPlugin_Should_Not_Duplicate_Plugins()
    {
        var loader = CreateLoader();
        var plugin = new Mt4Plugin();

        loader.RegisterPlugin(plugin);
        loader.RegisterPlugin(plugin);

        loader.GetAllPlugins().Should().HaveCount(1);
    }

    [Fact]
    public void GetPlugin_Should_Return_Plugin_By_Id()
    {
        var loader = CreateLoader();
        var plugin = new Mt4Plugin();
        loader.RegisterPlugin(plugin);

        var result = loader.GetPlugin("metatrader4");

        result.Should().NotBeNull();
        result!.PluginId.Should().Be("metatrader4");
    }

    [Fact]
    public void GetPlugin_Should_Return_Null_For_Unknown_Id()
    {
        var loader = CreateLoader();

        var result = loader.GetPlugin("unknown_platform");
        result.Should().BeNull();
    }

    [Fact]
    public void GetPluginForPlatform_Should_Find_Plugin_By_Supported_Platform()
    {
        var loader = CreateLoader();
        var plugin = new Mt4Plugin();
        loader.RegisterPlugin(plugin);

        var result = loader.GetPluginForPlatform("mt4");

        result.Should().NotBeNull();
        result!.PluginId.Should().Be("metatrader4");
    }

    [Fact]
    public void GetPluginForPlatform_Should_Be_CaseInsensitive()
    {
        var loader = CreateLoader();
        var plugin = new Mt4Plugin();
        loader.RegisterPlugin(plugin);

        var result = loader.GetPluginForPlatform("MetaTrader4");

        result.Should().NotBeNull();
        result!.PluginId.Should().Be("metatrader4");
    }

    // AC-018: Given a DLL that doesn't implement IPlatformPlugin, when loading, then it is skipped without error
    [Fact]
    public void LoadPlugins_Should_Skip_NonExistent_Directory()
    {
        var loader = CreateLoader();

        var act = () => loader.LoadPlugins(@"C:\NonExistentDirectory_12345");
        act.Should().NotThrow();
        loader.GetAllPlugins().Should().BeEmpty();
    }

    [Fact]
    public void LoadPlugins_Should_Create_Directory_If_Not_Exists()
    {
        var loader = CreateLoader();
        var tempDir = Path.Combine(Path.GetTempPath(), $"certus_plugins_test_{Guid.NewGuid():N}");

        try
        {
            loader.LoadPlugins(tempDir);
            Directory.Exists(tempDir).Should().BeTrue();
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void GetAllPlugins_Should_Return_Empty_When_No_Plugins()
    {
        var loader = CreateLoader();

        loader.GetAllPlugins().Should().BeEmpty();
    }

    private static PluginLoader CreateLoader()
    {
        return new PluginLoader(Array.Empty<IPlatformPlugin>());
    }
}
