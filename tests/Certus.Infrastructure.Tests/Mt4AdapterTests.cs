using Certus.Domain.Platform.Enums;
using Certus.Domain.Platform.ValueObjects;
using Certus.Infrastructure.Platform.Plugins.MetaTrader4;
using FluentAssertions;

namespace Certus.Infrastructure.Tests;

public class Mt4AdapterTests : IDisposable
{
    private readonly string _testDir;

    public Mt4AdapterTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), $"certus_mt4_adapter_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
            Directory.Delete(_testDir, true);
    }

    [Fact]
    public async Task ConnectAsync_Should_Succeed_When_File_Path_Exists()
    {
        var portfolioPath = Path.Combine(_testDir, "portfolio_status.json");
        File.WriteAllText(portfolioPath, "{}");

        var config = new PlatformConfig
        {
            PlatformType = PlatformType.MetaTrader4,
            DataFormat = DataFormat.Json,
            FilePath = portfolioPath
        };
        var adapter = new Mt4Adapter(config);

        var result = await adapter.ConnectAsync(config);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ConnectAsync_Should_Succeed_When_Directory_Path_Contains_Portfolio_File()
    {
        var portfolioPath = Path.Combine(_testDir, "portfolio_status.json");
        File.WriteAllText(portfolioPath, "{}");

        var config = new PlatformConfig
        {
            PlatformType = PlatformType.MetaTrader4,
            DataFormat = DataFormat.Json,
            FilePath = _testDir  // directory path, not file path
        };
        var adapter = new Mt4Adapter(config);

        var result = await adapter.ConnectAsync(config);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ConnectAsync_Should_Fail_When_Path_Does_Not_Exist()
    {
        var config = new PlatformConfig
        {
            PlatformType = PlatformType.MetaTrader4,
            DataFormat = DataFormat.Json,
            FilePath = @"C:\NonExistent_FakePath_12345\portfolio_status.json"
        };
        var adapter = new Mt4Adapter(config);

        var result = await adapter.ConnectAsync(config);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ConnectAsync_Should_Fail_When_Directory_Missing_Portfolio_File()
    {
        var config = new PlatformConfig
        {
            PlatformType = PlatformType.MetaTrader4,
            DataFormat = DataFormat.Json,
            FilePath = _testDir  // directory exists but no portfolio_status.json
        };
        var adapter = new Mt4Adapter(config);

        var result = await adapter.ConnectAsync(config);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("portfolio_status.json");
    }

    [Fact]
    public async Task ConnectAsync_Should_Fail_When_File_Path_Not_Found()
    {
        var config = new PlatformConfig
        {
            PlatformType = PlatformType.MetaTrader4,
            DataFormat = DataFormat.Json,
            FilePath = Path.Combine(_testDir, "nonexistent.json")
        };
        var adapter = new Mt4Adapter(config);

        var result = await adapter.ConnectAsync(config);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Path not found");
    }

    [Fact]
    public async Task ConnectAsync_Should_Not_Create_Directory()
    {
        var fakeDir = Path.Combine(_testDir, "should_not_exist");
        var config = new PlatformConfig
        {
            PlatformType = PlatformType.MetaTrader4,
            DataFormat = DataFormat.Json,
            FilePath = Path.Combine(fakeDir, "portfolio_status.json")
        };
        var adapter = new Mt4Adapter(config);

        await adapter.ConnectAsync(config);

        Directory.Exists(fakeDir).Should().BeFalse();
    }

    [Fact]
    public async Task ConnectAsync_Should_Succeed_With_Empty_FilePath()
    {
        var config = new PlatformConfig
        {
            PlatformType = PlatformType.MetaTrader4,
            DataFormat = DataFormat.Json,
            FilePath = null
        };
        var adapter = new Mt4Adapter(config);

        var result = await adapter.ConnectAsync(config);

        result.Success.Should().BeTrue();
    }
}
