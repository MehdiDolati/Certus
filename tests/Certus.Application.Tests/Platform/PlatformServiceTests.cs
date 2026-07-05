using Certus.Application.Platform;
using Certus.Application.Platform.DTOs;
using Certus.Domain.Platform.Aggregates;
using Certus.Domain.Platform.Enums;
using Certus.Domain.Platform.Events;
using Certus.Domain.Platform.Interfaces;
using Certus.Domain.Platform.Repositories;
using Certus.Domain.Platform.ValueObjects;
using Certus.Domain.RiskAndPortfolio.Repositories;
using Certus.Domain.SharedKernel;
using FluentAssertions;
using Moq;

namespace Certus.Application.Tests.Platform;

public class PlatformServiceTests
{
    private readonly Mock<IPlatformPluginLoader> _pluginLoaderMock;
    private readonly Mock<IPlatformConnectionRepository> _connectionRepoMock;
    private readonly Mock<IPlatformDataRepository> _dataRepoMock;
    private readonly Mock<IPortfolioRepository> _portfolioRepoMock;
    private readonly Mock<IImportedTradeRepository> _tradeRepoMock;
    private readonly Mock<IFileImportService> _fileImportServiceMock;
    private readonly Mock<IDomainEventDispatcher> _eventDispatcherMock;
    private readonly PlatformService _sut;

    public PlatformServiceTests()
    {
        _pluginLoaderMock = new Mock<IPlatformPluginLoader>();
        _connectionRepoMock = new Mock<IPlatformConnectionRepository>();
        _dataRepoMock = new Mock<IPlatformDataRepository>();
        _portfolioRepoMock = new Mock<IPortfolioRepository>();
        _tradeRepoMock = new Mock<IImportedTradeRepository>();
        _fileImportServiceMock = new Mock<IFileImportService>();
        _eventDispatcherMock = new Mock<IDomainEventDispatcher>();

        _sut = new PlatformService(
            _pluginLoaderMock.Object,
            _connectionRepoMock.Object,
            _dataRepoMock.Object,
            _portfolioRepoMock.Object,
            _tradeRepoMock.Object,
            _fileImportServiceMock.Object,
            _eventDispatcherMock.Object);
    }

    [Fact]
    public async Task ConnectAsync_Should_Create_Connection_When_Plugin_Exists()
    {
        // Arrange
        var pluginMock = new Mock<IPlatformPlugin>();
        var adapterMock = new Mock<IPlatformAdapter>();

        pluginMock.Setup(p => p.SupportedPlatforms).Returns(new[] { "metatrader4" });
        pluginMock.Setup(p => p.CreateAdapter(It.IsAny<PlatformConfig>())).Returns(adapterMock.Object);
        pluginMock.Setup(p => p.PluginName).Returns("MetaTrader 4");

        adapterMock.Setup(a => a.ConnectAsync(It.IsAny<PlatformConfig>()))
            .ReturnsAsync(new PlatformConnectionResult { Success = true, ConnectedAt = DateTime.UtcNow });

        _pluginLoaderMock.Setup(l => l.GetPluginForPlatform("metatrader4")).Returns(pluginMock.Object);
        _connectionRepoMock.Setup(r => r.AddAsync(It.IsAny<PlatformConnection>())).Returns(Task.CompletedTask);

        var request = new ConnectPlatformRequest
        {
            PlatformId = "metatrader4",
            FilePath = @"C:\MT4\Data\portfolio.json"
        };

        // Act
        var result = await _sut.ConnectAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.PlatformId.Should().Be("metatrader4");
        result.PlatformName.Should().Be("MetaTrader 4");
        result.Status.Should().Be(PlatformConnectionStatus.Connected);
        _connectionRepoMock.Verify(r => r.AddAsync(It.IsAny<PlatformConnection>()), Times.Once);
    }

    [Fact]
    public async Task ConnectAsync_Should_Throw_When_No_Plugin_Found()
    {
        _pluginLoaderMock.Setup(l => l.GetPluginForPlatform("unknown")).Returns((IPlatformPlugin?)null);

        var request = new ConnectPlatformRequest { PlatformId = "unknown" };

        var act = () => _sut.ConnectAsync(request);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No plugin found*");
    }

    [Fact]
    public async Task DisconnectAsync_Should_Stop_Watching_And_Disconnect()
    {
        var connection = CreateConnection();
        _connectionRepoMock.Setup(r => r.GetByIdAsync(connection.Id)).ReturnsAsync(connection);

        await _sut.DisconnectAsync(connection.Id);

        _fileImportServiceMock.Verify(f => f.StopWatching(connection.Id), Times.Once);
        _connectionRepoMock.Verify(r => r.Update(It.IsAny<PlatformConnection>()), Times.Once);
    }

    [Fact]
    public async Task GetConnectionsAsync_Should_Return_All_Connections()
    {
        var connections = new List<PlatformConnection>
        {
            CreateConnection(),
            CreateConnection()
        };
        _connectionRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(connections);

        var result = await _sut.GetConnectionsAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task ImportPortfolioAsync_Should_Create_Portfolio_From_Platform_Data()
    {
        var connection = CreateConnection();
        _connectionRepoMock.Setup(r => r.GetByIdAsync(connection.Id)).ReturnsAsync(connection);

        var platformPortfolio = new PlatformPortfolio
        {
            ExternalId = "MT4_12345",
            Name = "Main Account",
            Balance = 100000m,
            Equity = 102500m,
            Strategies = new List<PlatformStrategy>
            {
                new() { ExternalId = "EA_01", Name = "Momentum", IsActive = true }
            }
        };
        _dataRepoMock.Setup(r => r.GetLatestPortfolioSnapshotAsync(connection.Id, "MT4_12345"))
            .ReturnsAsync(platformPortfolio);
        _portfolioRepoMock.Setup(r => r.AddAsync(It.IsAny<Domain.RiskAndPortfolio.Aggregates.Portfolio>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.ImportPortfolioAsync(connection.Id, "MT4_12345", "My Portfolio");

        result.Should().NotBeNull();
        result!.ExternalId.Should().Be("MT4_12345");
        result.Name.Should().Be("Main Account");
        result.CertusPortfolioId.Should().NotBeNull();
        result.Strategies.Should().HaveCount(1);
        _portfolioRepoMock.Verify(r => r.AddAsync(It.IsAny<Domain.RiskAndPortfolio.Aggregates.Portfolio>()), Times.Once);
    }

    [Fact]
    public async Task ImportPortfolioAsync_Should_Return_Null_When_No_Data()
    {
        var connection = CreateConnection();
        _connectionRepoMock.Setup(r => r.GetByIdAsync(connection.Id)).ReturnsAsync(connection);
        _dataRepoMock.Setup(r => r.GetLatestPortfolioSnapshotAsync(connection.Id, "MT4_99999"))
            .ReturnsAsync((PlatformPortfolio?)null);

        var result = await _sut.ImportPortfolioAsync(connection.Id, "MT4_99999");

        result.Should().BeNull();
    }

    [Fact]
    public async Task ImportTradesAsync_Should_Import_New_Trades()
    {
        var connection = CreateConnection();
        _connectionRepoMock.Setup(r => r.GetByIdAsync(connection.Id)).ReturnsAsync(connection);

        var trades = new List<PlatformTrade>
        {
            new()
            {
                ExternalId = "12345",
                StrategyExternalId = "EA_01",
                Symbol = "EURUSD",
                Side = TradeSide.Buy,
                Volume = 0.1m,
                OpenPrice = 1.085m,
                Profit = 25m,
                OpenTime = DateTime.UtcNow.AddHours(-1)
            }
        };
        _dataRepoMock.Setup(r => r.GetTradesByStrategyAsync(connection.Id, "EA_01"))
            .ReturnsAsync(trades);
        _tradeRepoMock.Setup(r => r.GetByExternalIdAsync("12345"))
            .ReturnsAsync((ImportedTrade?)null);
        _tradeRepoMock.Setup(r => r.AddAsync(It.IsAny<ImportedTrade>())).Returns(Task.CompletedTask);
        _tradeRepoMock.Setup(r => r.GetByStrategyIdAsync(Guid.Empty))
            .ReturnsAsync(new List<ImportedTrade>());

        var result = await _sut.ImportTradesAsync(connection.Id, "EA_01");

        result.TradesImported.Should().Be(1);
        result.TotalPnL.Should().Be(25m);
        _tradeRepoMock.Verify(r => r.AddAsync(It.IsAny<ImportedTrade>()), Times.Once);
    }

    [Fact]
    public async Task ImportTradesAsync_Should_Skip_Existing_Trades()
    {
        var connection = CreateConnection();
        _connectionRepoMock.Setup(r => r.GetByIdAsync(connection.Id)).ReturnsAsync(connection);

        var trades = new List<PlatformTrade>
        {
            new() { ExternalId = "12345", StrategyExternalId = "EA_01" }
        };
        _dataRepoMock.Setup(r => r.GetTradesByStrategyAsync(connection.Id, "EA_01"))
            .ReturnsAsync(trades);
        _tradeRepoMock.Setup(r => r.GetByExternalIdAsync("12345"))
            .ReturnsAsync(new ImportedTrade { ExternalId = "12345" });
        _tradeRepoMock.Setup(r => r.GetByStrategyIdAsync(Guid.Empty))
            .ReturnsAsync(new List<ImportedTrade>());

        var result = await _sut.ImportTradesAsync(connection.Id, "EA_01");

        result.TradesSkipped.Should().Be(1);
        result.TradesImported.Should().Be(0);
    }

    [Fact]
    public async Task GetDashboardAsync_Should_Return_Correct_Stats()
    {
        var connections = new List<PlatformConnection> { CreateConnection() };
        _connectionRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(connections);
        _tradeRepoMock.Setup(r => r.GetCountByStrategyAsync(Guid.Empty)).ReturnsAsync(10);
        _tradeRepoMock.Setup(r => r.GetTotalPnLByStrategyAsync(Guid.Empty)).ReturnsAsync(500m);
        _portfolioRepoMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Domain.RiskAndPortfolio.Aggregates.Portfolio>());

        var result = await _sut.GetDashboardAsync();

        result.TotalConnections.Should().Be(1);
        result.ActiveConnections.Should().Be(0); // Connection is in Disconnected state
        result.TotalTrades.Should().Be(10);
        result.TotalPnL.Should().Be(500m);
    }

    [Fact]
    public async Task GetConnectionAsync_Should_Return_Null_When_Not_Found()
    {
        _connectionRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Domain.Platform.Aggregates.PlatformConnection?)null);

        var result = await _sut.GetConnectionAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetStatusAsync_Should_Throw_When_Not_Found()
    {
        _connectionRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Domain.Platform.Aggregates.PlatformConnection?)null);

        var act = () => _sut.GetStatusAsync(Guid.NewGuid());
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ImportTradesAsync_Should_Throw_When_Connection_Not_Found()
    {
        _connectionRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Domain.Platform.Aggregates.PlatformConnection?)null);

        var act = () => _sut.ImportTradesAsync(Guid.NewGuid(), "EA_01");
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ImportTradesAsync_Should_Filter_By_Date()
    {
        var connection = CreateConnection();
        _connectionRepoMock.Setup(r => r.GetByIdAsync(connection.Id)).ReturnsAsync(connection);

        var now = DateTime.UtcNow;
        var trades = new List<PlatformTrade>
        {
            new() { ExternalId = "1", StrategyExternalId = "EA_01", OpenTime = now.AddDays(-10) },
            new() { ExternalId = "2", StrategyExternalId = "EA_01", OpenTime = now.AddDays(-2) },
            new() { ExternalId = "3", StrategyExternalId = "EA_01", OpenTime = now }
        };
        _dataRepoMock.Setup(r => r.GetTradesByStrategyAsync(connection.Id, "EA_01"))
            .ReturnsAsync(trades);
        _tradeRepoMock.Setup(r => r.GetByExternalIdAsync(It.IsAny<string>()))
            .ReturnsAsync((ImportedTrade?)null);
        _tradeRepoMock.Setup(r => r.AddAsync(It.IsAny<ImportedTrade>())).Returns(Task.CompletedTask);
        _tradeRepoMock.Setup(r => r.GetByStrategyIdAsync(Guid.Empty))
            .ReturnsAsync(new List<ImportedTrade>());

        var result = await _sut.ImportTradesAsync(connection.Id, "EA_01",
            from: now.AddDays(-5), to: now.AddDays(-1));

        result.TradesImported.Should().Be(1); // Only trade "2" falls in range
    }

    [Fact]
    public async Task ImportPortfolioAsync_Should_Throw_When_Connection_Not_Found()
    {
        _connectionRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Domain.Platform.Aggregates.PlatformConnection?)null);

        var act = () => _sut.ImportPortfolioAsync(Guid.NewGuid(), "MT4_123");
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task DisconnectAsync_Should_Throw_When_Connection_Not_Found()
    {
        _connectionRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Domain.Platform.Aggregates.PlatformConnection?)null);

        var act = () => _sut.DisconnectAsync(Guid.NewGuid());
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    private static PlatformConnection CreateConnection()
    {
        var config = new PlatformConfig
        {
            PlatformType = PlatformType.MetaTrader4,
            DataFormat = DataFormat.Json,
            FilePath = @"C:\MT4\Data\portfolio.json"
        };

        return PlatformConnection.Create("metatrader4", "MetaTrader 4", config);
    }
}
