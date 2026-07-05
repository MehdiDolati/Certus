using Certus.Domain.Platform.Enums;
using Certus.Domain.Platform.ValueObjects;
using FluentAssertions;

namespace Certus.Domain.Tests.Platform;

public class PlatformConfigTests
{
    [Fact]
    public void PlatformConfig_Should_Have_Default_Values()
    {
        var config = new PlatformConfig();

        config.PlatformType.Should().Be(default(PlatformType));
        config.DataFormat.Should().Be(default(DataFormat));
        config.FilePath.Should().BeNull();
        config.ServerAddress.Should().BeNull();
        config.Port.Should().BeNull();
        config.ApiKey.Should().BeNull();
        config.PollingIntervalMs.Should().Be(5000);
        config.UseFileWatcher.Should().BeTrue();
    }

    [Fact]
    public void PlatformConfig_Should_Be_Created_With_Values()
    {
        var config = new PlatformConfig
        {
            PlatformType = PlatformType.MetaTrader4,
            DataFormat = DataFormat.Json,
            FilePath = @"C:\MT4\Data\portfolio.json",
            ServerAddress = "localhost",
            Port = 443,
            ApiKey = "test-key",
            PollingIntervalMs = 1000,
            UseFileWatcher = false
        };

        config.PlatformType.Should().Be(PlatformType.MetaTrader4);
        config.DataFormat.Should().Be(DataFormat.Json);
        config.FilePath.Should().Be(@"C:\MT4\Data\portfolio.json");
        config.ServerAddress.Should().Be("localhost");
        config.Port.Should().Be(443);
        config.ApiKey.Should().Be("test-key");
        config.PollingIntervalMs.Should().Be(1000);
        config.UseFileWatcher.Should().BeFalse();
    }

    [Fact]
    public void FilePlatformConfig_Should_Inherit_From_PlatformConfig()
    {
        var config = new FilePlatformConfig
        {
            PlatformType = PlatformType.MetaTrader4,
            FilePath = @"C:\test.json",
            WatchForChanges = true,
            DebounceMs = 200
        };

        config.PlatformType.Should().Be(PlatformType.MetaTrader4);
        config.FilePath.Should().Be(@"C:\test.json");
        config.WatchForChanges.Should().BeTrue();
        config.DebounceMs.Should().Be(200);
    }

    [Fact]
    public void FilePlatformConfig_Default_Values()
    {
        var config = new FilePlatformConfig();

        config.WatchForChanges.Should().BeTrue();
        config.DebounceMs.Should().Be(100);
    }
}
