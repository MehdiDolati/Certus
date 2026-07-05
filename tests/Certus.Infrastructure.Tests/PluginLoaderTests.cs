using Certus.Infrastructure.Platform;
using FluentAssertions;

namespace Certus.Infrastructure.Tests;

public class PluginLoaderTests
{
    private readonly PluginLoader _sut;

    public PluginLoaderTests()
    {
        _sut = new PluginLoader();
    }

    // AC-017: Given a plugin directory with DLLs implementing IPlatformPlugin, when loading plugins, then all valid plugins are registered
    [Fact]
    public void RegisterPlugin_Should_Add_Plugin_To_Collection()
    {
        var plugin = new Certus.Infrastructure.Platform.Plugins.MetaTrader4.Mt4Plugin();

        _sut.RegisterPlugin(plugin);

        _sut.GetAllPlugins().Should().HaveCount(1);
        _sut.GetAllPlugins().First().PluginId.Should().Be("metatrader4");
    }

    [Fact]
    public void GetPlugin_Should_Return_Plugin_By_Id()
    {
        var plugin = new Certus.Infrastructure.Platform.Plugins.MetaTrader4.Mt4Plugin();
        _sut.RegisterPlugin(plugin);

        var result = _sut.GetPlugin("metatrader4");

        result.Should().NotBeNull();
        result!.PluginId.Should().Be("metatrader4");
    }

    [Fact]
    public void GetPlugin_Should_Return_Null_For_Unknown_Id()
    {
        var result = _sut.GetPlugin("unknown_platform");
        result.Should().BeNull();
    }

    [Fact]
    public void GetPluginForPlatform_Should_Find_Plugin_By_Supported_Platform()
    {
        var plugin = new Certus.Infrastructure.Platform.Plugins.MetaTrader4.Mt4Plugin();
        _sut.RegisterPlugin(plugin);

        var result = _sut.GetPluginForPlatform("mt4");

        result.Should().NotBeNull();
        result!.PluginId.Should().Be("metatrader4");
    }

    // AC-018: Given a DLL that doesn't implement IPlatformPlugin, when loading, then it is skipped without error
    [Fact]
    public void LoadPlugins_Should_Skip_NonExistent_Directory()
    {
        var act = () => _sut.LoadPlugins(@"C:\NonExistentDirectory_12345");
        act.Should().NotThrow();
        _sut.GetAllPlugins().Should().BeEmpty();
    }

    [Fact]
    public void LoadPlugins_Should_Create_Directory_If_Not_Exists()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"certus_plugins_test_{Guid.NewGuid():N}");

        try
        {
            _sut.LoadPlugins(tempDir);
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
        _sut.GetAllPlugins().Should().BeEmpty();
    }
}
