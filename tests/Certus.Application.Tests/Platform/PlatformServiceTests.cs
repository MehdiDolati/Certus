using Certus.Application.Platform;
using Certus.Application.Platform.DTOs;
using Certus.Domain.Platform.Aggregates;
using Certus.Domain.Platform.Entities;
using Certus.Domain.Platform.Enums;
using Certus.Domain.Platform.Events;
using Certus.Domain.Platform.Interfaces;
using Certus.Domain.Platform.Repositories;
using Certus.Domain.Platform.ValueObjects;
using Certus.Domain.RiskAndPortfolio.Repositories;
using Certus.Domain.SharedKernel;
using Certus.Domain.Strategy.Aggregates;
using Certus.Domain.Strategy.Enums;
using Certus.Domain.Strategy.Repositories;
using Certus.Domain.Strategy.ValueObjects;
using Certus.Infrastructure.Platform;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
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
    private readonly InMemoryConnectionStatusStore _statusStore;
    private readonly Mock<IDomainEventDispatcher> _eventDispatcherMock;
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IStrategyMappingRepository> _strategyMappingRepoMock;
    private readonly Mock<IStrategyDefinitionRepository> _strategyRepoMock;
    private readonly PlatformService _sut;

    public PlatformServiceTests()
    {
        _pluginLoaderMock = new Mock<IPlatformPluginLoader>();
        _connectionRepoMock = new Mock<IPlatformConnectionRepository>();
        _dataRepoMock = new Mock<IPlatformDataRepository>();
        _portfolioRepoMock = new Mock<IPortfolioRepository>();
        _tradeRepoMock = new Mock<IImportedTradeRepository>();
        _fileImportServiceMock = new Mock<IFileImportService>();
        _statusStore = new InMemoryConnectionStatusStore();
        _eventDispatcherMock = new Mock<IDomainEventDispatcher>();
        _scopeFactoryMock = new Mock<IServiceScopeFactory>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _strategyMappingRepoMock = new Mock<IStrategyMappingRepository>();
        _strategyRepoMock = new Mock<IStrategyDefinitionRepository>();

        _sut = new PlatformService(
            _pluginLoaderMock.Object,
            _connectionRepoMock.Object,
            _dataRepoMock.Object,
            _portfolioRepoMock.Object,
            _tradeRepoMock.Object,
            _fileImportServiceMock.Object,
            _statusStore,
            _eventDispatcherMock.Object,
            _scopeFactoryMock.Object,
            _unitOfWorkMock.Object,
            _strategyMappingRepoMock.Object,
            _strategyRepoMock.Object);
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
        _connectionRepoMock.Setup(r => r.GetByPlatformAndPathAsync("metatrader4", @"C:\MT4\Data\portfolio.json"))
            .ReturnsAsync(new List<PlatformConnection>());
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
    public async Task DisconnectAsync_Should_Stop_Watching_And_Update_Status_Store()
    {
        var connection = CreateConnection();
        _connectionRepoMock.Setup(r => r.GetByIdAsync(connection.Id)).ReturnsAsync(connection);
        _statusStore.Set(connection.Id, ConnectionStatus.Disconnected.WithConnected());

        await _sut.DisconnectAsync(connection.Id);

        _fileImportServiceMock.Verify(f => f.StopWatching(connection.Id), Times.Once);
        _statusStore.Get(connection.Id).State.Should().Be(PlatformConnectionStatus.Disconnected);
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

        var strategyId = Guid.NewGuid();
        _strategyMappingRepoMock
            .Setup(r => r.GetByConnectionAndExternalIdAsync(connection.Id, "EA_01"))
            .ReturnsAsync(new StrategyMapping(Guid.NewGuid(), connection.Id, "EA_01", strategyId));

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
            .ReturnsAsync(new ImportedTrade { ExternalId = "12345", StrategyId = Guid.NewGuid() });

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

        var strategyId = Guid.NewGuid();
        _strategyMappingRepoMock
            .Setup(r => r.GetByConnectionAndExternalIdAsync(connection.Id, "EA_01"))
            .ReturnsAsync(new StrategyMapping(Guid.NewGuid(), connection.Id, "EA_01", strategyId));

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

    [Fact]
    public async Task ConnectAsync_Should_Return_Existing_Connection_When_Same_Platform_And_Path()
    {
        // Arrange
        var existingConnection = CreateConnection();
        _statusStore.Set(existingConnection.Id, ConnectionStatus.Disconnected.WithConnected());

        var pluginMock = new Mock<IPlatformPlugin>();
        pluginMock.Setup(p => p.SupportedPlatforms).Returns(new[] { "metatrader4" });
        pluginMock.Setup(p => p.PluginName).Returns("MetaTrader 4");

        _pluginLoaderMock.Setup(l => l.GetPluginForPlatform("metatrader4")).Returns(pluginMock.Object);
        _connectionRepoMock.Setup(r => r.GetByPlatformAndPathAsync("metatrader4", @"C:\MT4\Data\portfolio.json"))
            .ReturnsAsync(new List<PlatformConnection> { existingConnection });

        var request = new ConnectPlatformRequest
        {
            PlatformId = "metatrader4",
            FilePath = @"C:\MT4\Data\portfolio.json"
        };

        // Act
        var result = await _sut.ConnectAsync(request);

        // Assert
        result.Id.Should().Be(existingConnection.Id);
        _connectionRepoMock.Verify(r => r.AddAsync(It.IsAny<PlatformConnection>()), Times.Never);
    }

    [Fact]
    public async Task ConnectAsync_Should_Create_New_Connection_When_No_Match_Exists()
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
        _connectionRepoMock.Setup(r => r.GetByPlatformAndPathAsync("metatrader4", @"C:\MT4\Data\portfolio.json"))
            .ReturnsAsync(new List<PlatformConnection>());
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
        result.Status.Should().Be(PlatformConnectionStatus.Connected);
        _connectionRepoMock.Verify(r => r.AddAsync(It.IsAny<PlatformConnection>()), Times.Once);
    }

    [Fact]
    public async Task ConnectAsync_Should_Reconnect_Existing_Disconnected_Connection()
    {
        // Arrange
        var existingConnection = CreateConnection();
        _statusStore.Set(existingConnection.Id, ConnectionStatus.Disconnected);

        var pluginMock = new Mock<IPlatformPlugin>();
        var adapterMock = new Mock<IPlatformAdapter>();

        pluginMock.Setup(p => p.SupportedPlatforms).Returns(new[] { "metatrader4" });
        pluginMock.Setup(p => p.CreateAdapter(It.IsAny<PlatformConfig>())).Returns(adapterMock.Object);
        pluginMock.Setup(p => p.PluginName).Returns("MetaTrader 4");

        adapterMock.Setup(a => a.ConnectAsync(It.IsAny<PlatformConfig>()))
            .ReturnsAsync(new PlatformConnectionResult { Success = true, ConnectedAt = DateTime.UtcNow });

        _pluginLoaderMock.Setup(l => l.GetPluginForPlatform("metatrader4")).Returns(pluginMock.Object);
        _connectionRepoMock.Setup(r => r.GetByPlatformAndPathAsync("metatrader4", @"C:\MT4\Data\portfolio.json"))
            .ReturnsAsync(new List<PlatformConnection> { existingConnection });

        var request = new ConnectPlatformRequest
        {
            PlatformId = "metatrader4",
            FilePath = @"C:\MT4\Data\portfolio.json"
        };

        // Act
        var result = await _sut.ConnectAsync(request);

        // Assert
        result.Id.Should().Be(existingConnection.Id);
        result.Status.Should().Be(PlatformConnectionStatus.Connected);
        _statusStore.Get(existingConnection.Id).State.Should().Be(PlatformConnectionStatus.Connected);
        _connectionRepoMock.Verify(r => r.AddAsync(It.IsAny<PlatformConnection>()), Times.Never);
    }

    [Fact]
    public async Task GetDashboardAsync_Should_Detect_Stale_Connection()
    {
        // Arrange - create a real temp dir with portfolio file
        var tempDir = Path.Combine(Path.GetTempPath(), $"certus_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var portfolioFile = Path.Combine(tempDir, "portfolio_status.json");
        File.WriteAllText(portfolioFile, "{}");

        try
        {
            var config = new PlatformConfig
            {
                PlatformType = PlatformType.MetaTrader4,
                DataFormat = DataFormat.Json,
                FilePath = tempDir,
                StaleThresholdSeconds = 0 // immediate staleness for testing
            };
            var connection = PlatformConnection.Create("metatrader4", "MetaTrader 4", config);
            _statusStore.Set(connection.Id, ConnectionStatus.Disconnected.WithConnected());

            _connectionRepoMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<PlatformConnection> { connection });
            _tradeRepoMock.Setup(r => r.GetCountByStrategyAsync(Guid.Empty)).ReturnsAsync(0);
            _tradeRepoMock.Setup(r => r.GetTotalPnLByStrategyAsync(Guid.Empty)).ReturnsAsync(0m);
            _portfolioRepoMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Domain.RiskAndPortfolio.Aggregates.Portfolio>());

            // Act
            var result = await _sut.GetDashboardAsync();

            // Assert - with threshold=0, any file is stale
            result.Connections.Should().HaveCount(1);
            result.Connections[0].Status.Should().Be(PlatformConnectionStatus.Error);
            result.Connections[0].ErrorMessage.Should().Contain("stale");
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task GetDashboardAsync_Should_Detect_Missing_File()
    {
        // Arrange - use a non-existent file path
        var config = new PlatformConfig
        {
            PlatformType = PlatformType.MetaTrader4,
            DataFormat = DataFormat.Json,
            FilePath = @"C:\NonExistent_FakePath_12345\portfolio_status.json"
        };
        var connection = PlatformConnection.Create("metatrader4", "MetaTrader 4", config);
        _statusStore.Set(connection.Id, ConnectionStatus.Disconnected.WithConnected());
        _fileImportServiceMock.Setup(f => f.IsWatching(connection.Id)).Returns(true);

        _connectionRepoMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<PlatformConnection> { connection });
        _tradeRepoMock.Setup(r => r.GetCountByStrategyAsync(Guid.Empty)).ReturnsAsync(0);
        _tradeRepoMock.Setup(r => r.GetTotalPnLByStrategyAsync(Guid.Empty)).ReturnsAsync(0m);
        _portfolioRepoMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Domain.RiskAndPortfolio.Aggregates.Portfolio>());

        // Act
        var result = await _sut.GetDashboardAsync();

        // Assert
        result.Connections[0].Status.Should().Be(PlatformConnectionStatus.Error);
        result.Connections[0].ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task GetDashboardAsync_Should_Not_Mark_Fresh_Connection_As_Stale()
    {
        // Arrange - create a temp dir with portfolio_status.json
        var tempDir = Path.Combine(Path.GetTempPath(), $"certus_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var portfolioFile = Path.Combine(tempDir, "portfolio_status.json");
        File.WriteAllText(portfolioFile, "{}");

        try
        {
            var config = new PlatformConfig
            {
                PlatformType = PlatformType.MetaTrader4,
                DataFormat = DataFormat.Json,
                FilePath = tempDir
            };
            var connection = PlatformConnection.Create("metatrader4", "MetaTrader 4", config);
            _statusStore.Set(connection.Id, ConnectionStatus.Disconnected.WithConnected());
            _fileImportServiceMock.Setup(f => f.IsWatching(connection.Id)).Returns(true);
            _fileImportServiceMock.Setup(f => f.GetLastModifiedTime(connection.Id))
                .Returns(DateTime.UtcNow.AddSeconds(-5)); // 5s ago, threshold is 10s

            _connectionRepoMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<PlatformConnection> { connection });
            _tradeRepoMock.Setup(r => r.GetCountByStrategyAsync(Guid.Empty)).ReturnsAsync(0);
            _tradeRepoMock.Setup(r => r.GetTotalPnLByStrategyAsync(Guid.Empty)).ReturnsAsync(0m);
            _portfolioRepoMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Domain.RiskAndPortfolio.Aggregates.Portfolio>());

            // Act
            var result = await _sut.GetDashboardAsync();

            // Assert
            result.Connections[0].Status.Should().Be(PlatformConnectionStatus.Connected);
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task GetDashboardAsync_Should_Check_Staleness_Even_When_Not_Watching()
    {
        // Arrange - use a real temp file
        var tempDir = Path.Combine(Path.GetTempPath(), $"certus_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var portfolioFile = Path.Combine(tempDir, "portfolio_status.json");
        File.WriteAllText(portfolioFile, "{}");

        try
        {
            var config = new PlatformConfig
            {
                PlatformType = PlatformType.MetaTrader4,
                DataFormat = DataFormat.Json,
                FilePath = tempDir
            };
            var connection = PlatformConnection.Create("metatrader4", "MetaTrader 4", config);
            _statusStore.Set(connection.Id, ConnectionStatus.Disconnected.WithConnected());
            _fileImportServiceMock.Setup(f => f.IsWatching(connection.Id)).Returns(false);

            _connectionRepoMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<PlatformConnection> { connection });
            _tradeRepoMock.Setup(r => r.GetCountByStrategyAsync(Guid.Empty)).ReturnsAsync(0);
            _tradeRepoMock.Setup(r => r.GetTotalPnLByStrategyAsync(Guid.Empty)).ReturnsAsync(0m);
            _portfolioRepoMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Domain.RiskAndPortfolio.Aggregates.Portfolio>());

            // Act
            var result = await _sut.GetDashboardAsync();

            // Assert - file exists and is fresh, so should stay Connected
            result.Connections[0].Status.Should().Be(PlatformConnectionStatus.Connected);
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task GetDashboardAsync_Should_Recover_When_File_Returns()
    {
        // Arrange - create a temp dir with portfolio file
        var tempDir = Path.Combine(Path.GetTempPath(), $"certus_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var portfolioFile = Path.Combine(tempDir, "portfolio_status.json");
        File.WriteAllText(portfolioFile, "{}");

        try
        {
            var config = new PlatformConfig
            {
                PlatformType = PlatformType.MetaTrader4,
                DataFormat = DataFormat.Json,
                FilePath = tempDir
                // Default threshold 10s, file was just created so it's fresh
            };
            var connection = PlatformConnection.Create("metatrader4", "MetaTrader 4", config);
            // Start in Error state (file was previously missing/stale)
            _statusStore.Set(connection.Id, ConnectionStatus.Disconnected.WithConnected().WithError("Previously stale"));

            _connectionRepoMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<PlatformConnection> { connection });
            _tradeRepoMock.Setup(r => r.GetCountByStrategyAsync(Guid.Empty)).ReturnsAsync(0);
            _tradeRepoMock.Setup(r => r.GetTotalPnLByStrategyAsync(Guid.Empty)).ReturnsAsync(0m);
            _portfolioRepoMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Domain.RiskAndPortfolio.Aggregates.Portfolio>());

            // Act
            var result = await _sut.GetDashboardAsync();

            // Assert - file exists and is fresh, should recover
            result.Connections[0].Status.Should().Be(PlatformConnectionStatus.Connected);
            result.Connections[0].ErrorMessage.Should().BeNull();
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ImportTradesAsync_Should_Create_Strategy_And_Mapping_When_None_Exists()
    {
        var connection = CreateConnection();
        _connectionRepoMock.Setup(r => r.GetByIdAsync(connection.Id)).ReturnsAsync(connection);

        var strategyId = Guid.NewGuid();
        var strategyDef = new StrategyDefinition(strategyId, "EA_01", new StrategyType(StrategyCategory.Custom, "ExpertAdvisor"), 0);

        _strategyMappingRepoMock
            .Setup(r => r.GetByConnectionAndExternalIdAsync(connection.Id, "EA_01"))
            .ReturnsAsync((StrategyMapping?)null);
        _strategyRepoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<StrategyDefinition>());
        _strategyRepoMock
            .Setup(r => r.AddAsync(It.IsAny<StrategyDefinition>()))
            .Returns(Task.CompletedTask);
        _strategyRepoMock
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);
        _strategyMappingRepoMock
            .Setup(r => r.AddAsync(It.IsAny<StrategyMapping>()))
            .Returns(Task.CompletedTask);

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

        var result = await _sut.ImportTradesAsync(connection.Id, "EA_01");

        result.TradesImported.Should().Be(1);
        _strategyRepoMock.Verify(r => r.AddAsync(It.IsAny<StrategyDefinition>()), Times.Once);
        _strategyMappingRepoMock.Verify(r => r.AddAsync(It.IsAny<StrategyMapping>()), Times.Once);
    }

    [Fact]
    public async Task ImportTradesAsync_Should_Use_Existing_Mapping_When_Found()
    {
        var connection = CreateConnection();
        _connectionRepoMock.Setup(r => r.GetByIdAsync(connection.Id)).ReturnsAsync(connection);

        var strategyId = Guid.NewGuid();
        var existingMapping = new StrategyMapping(Guid.NewGuid(), connection.Id, "EA_01", strategyId);

        _strategyMappingRepoMock
            .Setup(r => r.GetByConnectionAndExternalIdAsync(connection.Id, "EA_01"))
            .ReturnsAsync(existingMapping);

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

        var result = await _sut.ImportTradesAsync(connection.Id, "EA_01");

        result.TradesImported.Should().Be(1);
        _strategyRepoMock.Verify(r => r.AddAsync(It.IsAny<StrategyDefinition>()), Times.Never);
        _strategyMappingRepoMock.Verify(r => r.AddAsync(It.IsAny<StrategyMapping>()), Times.Never);
    }

    [Fact]
    public async Task ImportTradesAsync_Should_Return_Empty_When_StrategyExternalId_Is_Empty()
    {
        var connection = CreateConnection();
        _connectionRepoMock.Setup(r => r.GetByIdAsync(connection.Id)).ReturnsAsync(connection);

        var trades = new List<PlatformTrade>
        {
            new()
            {
                ExternalId = "12345",
                StrategyExternalId = "",
                Symbol = "EURUSD",
                Side = TradeSide.Buy,
                Volume = 0.1m,
                OpenPrice = 1.085m,
                Profit = 25m,
                OpenTime = DateTime.UtcNow.AddHours(-1)
            }
        };
        _dataRepoMock.Setup(r => r.GetTradesByStrategyAsync(connection.Id, ""))
            .ReturnsAsync(trades);
        _tradeRepoMock.Setup(r => r.GetByExternalIdAsync("12345"))
            .ReturnsAsync((ImportedTrade?)null);
        _tradeRepoMock.Setup(r => r.AddAsync(It.IsAny<ImportedTrade>())).Returns(Task.CompletedTask);

        var result = await _sut.ImportTradesAsync(connection.Id, "");

        result.TradesImported.Should().Be(1);
        // StrategyId should be Guid.Empty when external ID is empty
        _tradeRepoMock.Verify(r => r.AddAsync(
            It.Is<ImportedTrade>(t => t.StrategyId == Guid.Empty)), Times.Once);
        _strategyRepoMock.Verify(r => r.AddAsync(It.IsAny<StrategyDefinition>()), Times.Never);
        _strategyMappingRepoMock.Verify(r => r.AddAsync(It.IsAny<StrategyMapping>()), Times.Never);
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
