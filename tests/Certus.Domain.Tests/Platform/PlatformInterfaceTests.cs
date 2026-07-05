using Certus.Domain.Platform.Interfaces;
using FluentAssertions;

namespace Certus.Domain.Tests.Platform;

public class PlatformInterfaceTests
{
    [Fact]
    public void FileImportEventArgs_Should_Have_Properties()
    {
        var args = new FileImportEventArgs
        {
            ConnectionId = Guid.NewGuid(),
            FilePath = @"C:\test.json",
            Content = "{}",
            Timestamp = DateTime.UtcNow
        };

        args.ConnectionId.Should().NotBeEmpty();
        args.FilePath.Should().Be(@"C:\test.json");
        args.Content.Should().Be("{}");
        args.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void PlatformConnectionResult_Should_Have_Properties()
    {
        var result = new PlatformConnectionResult
        {
            Success = true,
            ErrorMessage = null,
            ConnectedAt = DateTime.UtcNow
        };

        result.Success.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
        result.ConnectedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void PlatformConnectionResult_Failure_Should_Have_Error()
    {
        var result = new PlatformConnectionResult
        {
            Success = false,
            ErrorMessage = "Connection refused"
        };

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Connection refused");
    }

    [Fact]
    public void PlatformStatus_Should_Have_Properties()
    {
        var status = new PlatformStatus
        {
            IsConnected = true,
            LastDataReceived = DateTime.UtcNow,
            PortfolioCount = 5,
            StrategyCount = 10,
            TradeCount = 100,
            ErrorMessage = null
        };

        status.IsConnected.Should().BeTrue();
        status.PortfolioCount.Should().Be(5);
        status.StrategyCount.Should().Be(10);
        status.TradeCount.Should().Be(100);
    }

    [Fact]
    public void PlatformStatus_Error_Should_Have_Message()
    {
        var status = new PlatformStatus
        {
            IsConnected = false,
            ErrorMessage = "Timeout"
        };

        status.IsConnected.Should().BeFalse();
        status.ErrorMessage.Should().Be("Timeout");
    }
}
