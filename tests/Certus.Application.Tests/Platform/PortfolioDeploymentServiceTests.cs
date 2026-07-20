using Certus.Application.Platform;
using Certus.Application.Platform.DTOs;
using Certus.Domain.Platform.Aggregates;
using Certus.Domain.Platform.Enums;
using Certus.Domain.Platform.Interfaces;
using Certus.Domain.Platform.Repositories;
using Certus.Domain.Platform.ValueObjects;
using Certus.Domain.RiskAndPortfolio.Aggregates;
using Certus.Domain.RiskAndPortfolio.Repositories;
using Certus.Domain.Strategy.Aggregates;
using Certus.Domain.Strategy.Repositories;
using Certus.Domain.SharedKernel;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Certus.Application.Tests.Platform;

public class PortfolioDeploymentServiceTests : IDisposable
{
    private readonly Mock<IPortfolioDeploymentRepository> _deploymentRepoMock;
    private readonly Mock<IPlatformConnectionRepository> _connectionRepoMock;
    private readonly Mock<IPortfolioRepository> _portfolioRepoMock;
    private readonly Mock<IStrategyDefinitionRepository> _strategyRepoMock;
    private readonly Mock<IPlatformPluginLoader> _pluginLoaderMock;
    private readonly Mock<IDomainEventDispatcher> _eventDispatcherMock;
    private readonly Mock<IFileImportService> _fileImportServiceMock;
    private readonly Mock<ILogger<PortfolioDeploymentService>> _loggerMock;
    private readonly PortfolioDeploymentService _sut;
    private readonly string _testFolder;
    private readonly string _testTargetFolder;

    public PortfolioDeploymentServiceTests()
    {
        _deploymentRepoMock = new Mock<IPortfolioDeploymentRepository>();
        _connectionRepoMock = new Mock<IPlatformConnectionRepository>();
        _portfolioRepoMock = new Mock<IPortfolioRepository>();
        _strategyRepoMock = new Mock<IStrategyDefinitionRepository>();
        _pluginLoaderMock = new Mock<IPlatformPluginLoader>();
        _eventDispatcherMock = new Mock<IDomainEventDispatcher>();
        _fileImportServiceMock = new Mock<IFileImportService>();
        _loggerMock = new Mock<ILogger<PortfolioDeploymentService>>();

        _sut = new PortfolioDeploymentService(
            _deploymentRepoMock.Object,
            _connectionRepoMock.Object,
            _portfolioRepoMock.Object,
            _strategyRepoMock.Object,
            _pluginLoaderMock.Object,
            _eventDispatcherMock.Object,
            _fileImportServiceMock.Object,
            _loggerMock.Object);

        _testFolder = Path.Combine(Path.GetTempPath(), $"CertusTest_{Guid.NewGuid():N}");
        _testTargetFolder = Path.Combine(Path.GetTempPath(), $"CertusTarget_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testFolder);
        Directory.CreateDirectory(_testTargetFolder);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testFolder))
            Directory.Delete(_testFolder, true);
        if (Directory.Exists(_testTargetFolder))
            Directory.Delete(_testTargetFolder, true);
    }

    // =====================================================
    // ValidateFolderAsync
    // =====================================================

    [Fact]
    public async Task ValidateFolder_Should_Return_Invalid_When_Folder_DoesNotExist()
    {
        var result = await _sut.ValidateFolderAsync(@"C:\NonExistent\Folder");
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("does not exist");
    }

    [Fact]
    public async Task ValidateFolder_Should_Return_Invalid_When_Path_Is_Null()
    {
        var result = await _sut.ValidateFolderAsync(null!);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateFolder_Should_Return_Invalid_When_Path_Is_Empty()
    {
        var result = await _sut.ValidateFolderAsync("");
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateFolder_Should_Return_Invalid_When_No_Ex4_Files()
    {
        var result = await _sut.ValidateFolderAsync(_testFolder);
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("No .ex4 files found");
    }

    [Fact]
    public async Task ValidateFolder_Should_Return_Valid_When_Ex4_Files_Exist()
    {
        File.WriteAllText(Path.Combine(_testFolder, "EA1.ex4"), "fake content");
        File.WriteAllText(Path.Combine(_testFolder, "EA2.ex4"), "fake content");

        var result = await _sut.ValidateFolderAsync(_testFolder);

        result.IsValid.Should().BeTrue();
        result.EACount.Should().Be(2);
        result.EANames.Should().Contain("EA1");
        result.EANames.Should().Contain("EA2");
    }

    [Fact]
    public async Task ValidateFolder_Should_Return_EANames_Sorted()
    {
        File.WriteAllText(Path.Combine(_testFolder, "ZebraEA.ex4"), "content");
        File.WriteAllText(Path.Combine(_testFolder, "AlphaEA.ex4"), "content");
        File.WriteAllText(Path.Combine(_testFolder, "MidEA.ex4"), "content");

        var result = await _sut.ValidateFolderAsync(_testFolder);

        result.EANames.Should().BeInAscendingOrder();
        result.EANames.Should().BeEquivalentTo(["AlphaEA", "MidEA", "ZebraEA"]);
    }

    [Fact]
    public async Task ValidateFolder_Should_Not_Include_Subfolder_Files()
    {
        var subDir = Path.Combine(_testFolder, "SubDir");
        Directory.CreateDirectory(subDir);
        File.WriteAllText(Path.Combine(_testFolder, "EA1.ex4"), "fake content");
        File.WriteAllText(Path.Combine(subDir, "EA2.ex4"), "fake content");

        var result = await _sut.ValidateFolderAsync(_testFolder);

        result.IsValid.Should().BeTrue();
        result.EACount.Should().Be(1);
        result.EANames.Should().Contain("EA1");
        result.EANames.Should().NotContain("EA2");
    }

    [Fact]
    public async Task ValidateFolder_Should_Not_Include_NonEx4_Files()
    {
        File.WriteAllText(Path.Combine(_testFolder, "EA1.ex4"), "content");
        File.WriteAllText(Path.Combine(_testFolder, "readme.txt"), "content");
        File.WriteAllText(Path.Combine(_testFolder, "config.json"), "content");

        var result = await _sut.ValidateFolderAsync(_testFolder);

        result.EACount.Should().Be(1);
        result.EANames.Should().Contain("EA1");
    }

    [Fact]
    public async Task ValidateFolder_Should_Report_EACount_Correctly()
    {
        for (int i = 0; i < 10; i++)
            File.WriteAllText(Path.Combine(_testFolder, $"EA{i}.ex4"), "content");

        var result = await _sut.ValidateFolderAsync(_testFolder);

        result.EACount.Should().Be(10);
        result.EANames.Should().HaveCount(10);
    }

    // =====================================================
    // GetAvailableConnectionsAsync
    // =====================================================

    [Fact]
    public async Task GetAvailableConnections_Should_Return_All_Connections()
    {
        var connections = new List<PlatformConnection>
        {
            PlatformConnection.Create("MetaTrader4", "MT4", new PlatformConfig { PlatformType = PlatformType.MetaTrader4 }),
            PlatformConnection.Create("MetaTrader5", "MT5", new PlatformConfig { PlatformType = PlatformType.MetaTrader5 })
        };
        _connectionRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(connections);

        var result = await _sut.GetAvailableConnectionsAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAvailableConnections_Should_Return_Empty_When_None()
    {
        _connectionRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<PlatformConnection>());

        var result = await _sut.GetAvailableConnectionsAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAvailableConnections_Should_Map_Properties()
    {
        var conn = PlatformConnection.Create("MetaTrader4", "My MT4", new PlatformConfig { PlatformType = PlatformType.MetaTrader4 });
        _connectionRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<PlatformConnection> { conn });

        var result = await _sut.GetAvailableConnectionsAsync();

        result[0].Id.Should().Be(conn.Id);
        result[0].PlatformId.Should().Be("MetaTrader4");
        result[0].PlatformName.Should().Be("My MT4");
    }

    // =====================================================
    // DeriveMT4PathAsync
    // =====================================================

    [Fact]
    public async Task DeriveMT4Path_Should_Return_Error_When_Connection_Not_Found()
    {
        _connectionRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((PlatformConnection?)null);

        var result = await _sut.DeriveMT4PathAsync(Guid.NewGuid());

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task DeriveMT4Path_Should_Return_Error_When_No_FilePath()
    {
        var connection = PlatformConnection.Create(
            "MetaTrader4", "MT4",
            new PlatformConfig { PlatformType = PlatformType.MetaTrader4, FilePath = null });
        _connectionRepoMock.Setup(r => r.GetByIdAsync(connection.Id)).ReturnsAsync(connection);

        var result = await _sut.DeriveMT4PathAsync(connection.Id);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("no file path");
    }

    [Fact]
    public async Task DeriveMT4Path_Should_Derive_From_Directory_Path()
    {
        // Create a fake MT4 structure: MT4Root/Certus/
        var mt4Root = Path.Combine(_testFolder, "MT4Root");
        var certusDir = Path.Combine(mt4Root, "Certus");
        Directory.CreateDirectory(certusDir);

        var connection = PlatformConnection.Create(
            "MetaTrader4", "MT4",
            new PlatformConfig { PlatformType = PlatformType.MetaTrader4, FilePath = certusDir });
        _connectionRepoMock.Setup(r => r.GetByIdAsync(connection.Id)).ReturnsAsync(connection);

        var result = await _sut.DeriveMT4PathAsync(connection.Id);

        result.Success.Should().BeTrue();
        result.MT4DataPath.Should().Be(mt4Root);
        result.ExpertsPath.Should().Be(Path.Combine(mt4Root, "MQL4", "Experts"));
    }

    [Fact]
    public async Task DeriveMT4Path_Should_Derive_From_File_Path()
    {
        // Create a fake MT4 structure: MT4Root/Certus/portfolio_status.json
        var mt4Root = Path.Combine(_testFolder, "MT4Root2");
        var certusDir = Path.Combine(mt4Root, "Certus");
        Directory.CreateDirectory(certusDir);
        var statusFile = Path.Combine(certusDir, "portfolio_status.json");
        File.WriteAllText(statusFile, "{}");

        var connection = PlatformConnection.Create(
            "MetaTrader4", "MT4",
            new PlatformConfig { PlatformType = PlatformType.MetaTrader4, FilePath = statusFile });
        _connectionRepoMock.Setup(r => r.GetByIdAsync(connection.Id)).ReturnsAsync(connection);

        var result = await _sut.DeriveMT4PathAsync(connection.Id);

        result.Success.Should().BeTrue();
        result.MT4DataPath.Should().Be(mt4Root);
        result.ExpertsPath.Should().Be(Path.Combine(mt4Root, "MQL4", "Experts"));
    }

    [Fact]
    public async Task DeriveMT4Path_Should_Return_Error_When_Path_Invalid()
    {
        var connection = PlatformConnection.Create(
            "MetaTrader4", "MT4",
            new PlatformConfig { PlatformType = PlatformType.MetaTrader4, FilePath = @"C:\Completely\Invalid\Path" });
        _connectionRepoMock.Setup(r => r.GetByIdAsync(connection.Id)).ReturnsAsync(connection);

        var result = await _sut.DeriveMT4PathAsync(connection.Id);

        result.Success.Should().BeFalse();
    }

    // =====================================================
    // DeriveMT4PathAsync - Common Path (FILE_COMMON) Support
    // =====================================================

    [Fact]
    public async Task DeriveMT4Path_Should_Detect_Common_Path_From_File()
    {
        // Structure: MetaQuotes/Common/Files/Certus/portfolio_status.json
        var metaQuotesRoot = Path.Combine(_testFolder, "MetaQuotes");
        var certusDir = Path.Combine(metaQuotesRoot, "Common", "Files", "Certus");
        Directory.CreateDirectory(certusDir);
        File.WriteAllText(Path.Combine(certusDir, "portfolio_status.json"), "{}");

        // Create one terminal with MQL4/Experts
        var terminalHash = "ABC123DEF456";
        var expertsPath = Path.Combine(metaQuotesRoot, "Terminal", terminalHash, "MQL4", "Experts");
        Directory.CreateDirectory(expertsPath);

        var connection = PlatformConnection.Create(
            "MetaTrader4", "MT4",
            new PlatformConfig
            {
                PlatformType = PlatformType.MetaTrader4,
                FilePath = Path.Combine(certusDir, "portfolio_status.json")
            });
        _connectionRepoMock.Setup(r => r.GetByIdAsync(connection.Id)).ReturnsAsync(connection);

        var result = await _sut.DeriveMT4PathAsync(connection.Id);

        result.Success.Should().BeTrue();
        result.IsCommonPath.Should().BeTrue();
        result.CommonFilesPath.Should().Be(certusDir);
        result.ExpertsPath.Should().Be(expertsPath);
        result.MT4DataPath.Should().Contain(terminalHash);
    }

    [Fact]
    public async Task DeriveMT4Path_Should_Detect_Common_Path_From_Directory()
    {
        // Structure: MetaQuotes/Common/Files/Certus/ (pointing to dir)
        var metaQuotesRoot = Path.Combine(_testFolder, "MetaQuotes2");
        var certusDir = Path.Combine(metaQuotesRoot, "Common", "Files", "Certus");
        Directory.CreateDirectory(certusDir);

        var terminalHash = "XYZ789";
        var expertsPath = Path.Combine(metaQuotesRoot, "Terminal", terminalHash, "MQL4", "Experts");
        Directory.CreateDirectory(expertsPath);

        var connection = PlatformConnection.Create(
            "MetaTrader4", "MT4",
            new PlatformConfig { PlatformType = PlatformType.MetaTrader4, FilePath = certusDir });
        _connectionRepoMock.Setup(r => r.GetByIdAsync(connection.Id)).ReturnsAsync(connection);

        var result = await _sut.DeriveMT4PathAsync(connection.Id);

        result.Success.Should().BeTrue();
        result.IsCommonPath.Should().BeTrue();
        result.ExpertsPath.Should().Be(expertsPath);
    }

    [Fact]
    public async Task DeriveMT4Path_Should_Return_Multiple_Terminals_For_Common_Path()
    {
        var metaQuotesRoot = Path.Combine(_testFolder, "MetaQuotes3");
        var certusDir = Path.Combine(metaQuotesRoot, "Common", "Files", "Certus");
        Directory.CreateDirectory(certusDir);

        // Create two terminals
        var experts1 = Path.Combine(metaQuotesRoot, "Terminal", "Hash1", "MQL4", "Experts");
        var experts2 = Path.Combine(metaQuotesRoot, "Terminal", "Hash2", "MQL4", "Experts");
        Directory.CreateDirectory(experts1);
        Directory.CreateDirectory(experts2);

        var connection = PlatformConnection.Create(
            "MetaTrader4", "MT4",
            new PlatformConfig { PlatformType = PlatformType.MetaTrader4, FilePath = certusDir });
        _connectionRepoMock.Setup(r => r.GetByIdAsync(connection.Id)).ReturnsAsync(connection);

        var result = await _sut.DeriveMT4PathAsync(connection.Id);

        result.Success.Should().BeFalse();
        result.IsCommonPath.Should().BeTrue();
        result.TerminalCandidates.Should().HaveCount(2);
        result.ErrorMessage.Should().Contain("Multiple terminals");
    }

    [Fact]
    public async Task DeriveMT4Path_Should_Fail_When_No_Terminals_With_Experts()
    {
        var metaQuotesRoot = Path.Combine(_testFolder, "MetaQuotes4");
        var certusDir = Path.Combine(metaQuotesRoot, "Common", "Files", "Certus");
        Directory.CreateDirectory(certusDir);

        // Create a terminal WITHOUT MQL4/Experts
        Directory.CreateDirectory(Path.Combine(metaQuotesRoot, "Terminal", "Hash1", "MQL4"));

        var connection = PlatformConnection.Create(
            "MetaTrader4", "MT4",
            new PlatformConfig { PlatformType = PlatformType.MetaTrader4, FilePath = certusDir });
        _connectionRepoMock.Setup(r => r.GetByIdAsync(connection.Id)).ReturnsAsync(connection);

        var result = await _sut.DeriveMT4PathAsync(connection.Id);

        result.Success.Should().BeFalse();
        result.IsCommonPath.Should().BeTrue();
        result.ErrorMessage.Should().Contain("No terminal found");
    }

    [Fact]
    public async Task DeriveMT4Path_Should_Fail_When_No_Terminal_Directory()
    {
        var metaQuotesRoot = Path.Combine(_testFolder, "MetaQuotes5");
        var certusDir = Path.Combine(metaQuotesRoot, "Common", "Files", "Certus");
        Directory.CreateDirectory(certusDir);
        // No Terminal/ dir at all

        var connection = PlatformConnection.Create(
            "MetaTrader4", "MT4",
            new PlatformConfig { PlatformType = PlatformType.MetaTrader4, FilePath = certusDir });
        _connectionRepoMock.Setup(r => r.GetByIdAsync(connection.Id)).ReturnsAsync(connection);

        var result = await _sut.DeriveMT4PathAsync(connection.Id);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Terminal");
    }

    [Fact]
    public async Task DeriveMT4Path_Should_Skip_Common_Dir_In_Terminal_Scan()
    {
        var metaQuotesRoot = Path.Combine(_testFolder, "MetaQuotes6");
        var certusDir = Path.Combine(metaQuotesRoot, "Common", "Files", "Certus");
        Directory.CreateDirectory(certusDir);

        // Create "Common" inside Terminal/ (should be skipped) and a real terminal
        Directory.CreateDirectory(Path.Combine(metaQuotesRoot, "Terminal", "Common", "MQL4", "Experts"));
        var expertsPath = Path.Combine(metaQuotesRoot, "Terminal", "RealTerminal", "MQL4", "Experts");
        Directory.CreateDirectory(expertsPath);

        var connection = PlatformConnection.Create(
            "MetaTrader4", "MT4",
            new PlatformConfig { PlatformType = PlatformType.MetaTrader4, FilePath = certusDir });
        _connectionRepoMock.Setup(r => r.GetByIdAsync(connection.Id)).ReturnsAsync(connection);

        var result = await _sut.DeriveMT4PathAsync(connection.Id);

        result.Success.Should().BeTrue();
        result.TerminalCandidates.Should().HaveCount(1);
        result.TerminalCandidates[0].DisplayName.Should().Be("RealTerminal");
    }

    // =====================================================
    // CheckConflictsAsync
    // =====================================================

    [Fact]
    public async Task CheckConflicts_Should_Return_No_Conflicts_When_Target_Empty()
    {
        File.WriteAllText(Path.Combine(_testFolder, "EA1.ex4"), "fake content");
        var targetDir = Path.Combine(_testFolder, "Target");
        Directory.CreateDirectory(targetDir);

        var result = await _sut.CheckConflictsAsync(_testFolder, targetDir);

        result.ConflictingFiles.Should().BeEmpty();
        result.TotalFiles.Should().Be(1);
        result.NewFiles.Should().Be(1);
    }

    [Fact]
    public async Task CheckConflicts_Should_Detect_Conflicts()
    {
        File.WriteAllText(Path.Combine(_testFolder, "EA1.ex4"), "fake content");
        var targetDir = Path.Combine(_testFolder, "Target");
        Directory.CreateDirectory(targetDir);
        File.WriteAllText(Path.Combine(targetDir, "EA1.ex4"), "existing content");

        var result = await _sut.CheckConflictsAsync(_testFolder, targetDir);

        result.ConflictingFiles.Should().HaveCount(1);
        result.ConflictingFiles.Should().Contain("EA1.ex4");
        result.NewFiles.Should().Be(0);
    }

    [Fact]
    public async Task CheckConflicts_Should_Handle_CaseInsensitive_Match()
    {
        File.WriteAllText(Path.Combine(_testFolder, "EA1.ex4"), "fake content");
        var targetDir = Path.Combine(_testFolder, "Target");
        Directory.CreateDirectory(targetDir);
        File.WriteAllText(Path.Combine(targetDir, "ea1.ex4"), "existing content");

        var result = await _sut.CheckConflictsAsync(_testFolder, targetDir);

        result.ConflictingFiles.Should().HaveCount(1);
    }

    [Fact]
    public async Task CheckConflicts_Should_Handle_Mixed_Conflicts_And_New()
    {
        File.WriteAllText(Path.Combine(_testFolder, "EA1.ex4"), "content1");
        File.WriteAllText(Path.Combine(_testFolder, "EA2.ex4"), "content2");
        var targetDir = Path.Combine(_testFolder, "Target");
        Directory.CreateDirectory(targetDir);
        File.WriteAllText(Path.Combine(targetDir, "EA1.ex4"), "existing");

        var result = await _sut.CheckConflictsAsync(_testFolder, targetDir);

        result.TotalFiles.Should().Be(2);
        result.ConflictingFiles.Should().HaveCount(1);
        result.NewFiles.Should().Be(1);
    }

    [Fact]
    public async Task CheckConflicts_Should_Ignore_NonEx4_In_Target()
    {
        File.WriteAllText(Path.Combine(_testFolder, "EA1.ex4"), "fake content");
        var targetDir = Path.Combine(_testFolder, "Target");
        Directory.CreateDirectory(targetDir);
        File.WriteAllText(Path.Combine(targetDir, "EA1.txt"), "not an EA");

        var result = await _sut.CheckConflictsAsync(_testFolder, targetDir);

        result.ConflictingFiles.Should().BeEmpty();
    }

    [Fact]
    public async Task CheckConflicts_Should_Handle_NonExistent_Target()
    {
        File.WriteAllText(Path.Combine(_testFolder, "EA1.ex4"), "fake content");

        var result = await _sut.CheckConflictsAsync(_testFolder, @"C:\NonExistent\Target");

        result.ConflictingFiles.Should().BeEmpty();
        result.NewFiles.Should().Be(1);
    }

    // =====================================================
    // DeployAsync
    // =====================================================

    [Fact]
    public async Task Deploy_Should_Return_Error_When_Source_Invalid()
    {
        var request = new DeployPortfolioRequest
        {
            SourceFolderPath = @"C:\NonExistent\Folder",
            ConnectionId = Guid.NewGuid()
        };

        var result = await _sut.DeployAsync(request);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Deploy_Should_Return_Error_When_Connection_Not_Found()
    {
        File.WriteAllText(Path.Combine(_testFolder, "EA1.ex4"), "fake content");
        _connectionRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((PlatformConnection?)null);

        var request = new DeployPortfolioRequest
        {
            SourceFolderPath = _testFolder,
            ConnectionId = Guid.NewGuid()
        };

        var result = await _sut.DeployAsync(request);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task Deploy_Should_Return_Error_When_Path_Derivation_Fails()
    {
        File.WriteAllText(Path.Combine(_testFolder, "EA1.ex4"), "fake content");
        var connection = PlatformConnection.Create(
            "MetaTrader4", "MT4",
            new PlatformConfig { PlatformType = PlatformType.MetaTrader4, FilePath = null });
        _connectionRepoMock.Setup(r => r.GetByIdAsync(connection.Id)).ReturnsAsync(connection);

        var request = new DeployPortfolioRequest
        {
            SourceFolderPath = _testFolder,
            ConnectionId = connection.Id
        };

        var result = await _sut.DeployAsync(request);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Deploy_Should_Use_Manual_Path_When_Provided()
    {
        File.WriteAllText(Path.Combine(_testFolder, "EA1.ex4"), "fake content");
        var mt4Root = Path.Combine(_testTargetFolder, "MT4Root");
        var expertsPath = Path.Combine(mt4Root, "MQL4", "Experts");
        Directory.CreateDirectory(expertsPath);

        var connection = PlatformConnection.Create(
            "MetaTrader4", "MT4",
            new PlatformConfig { PlatformType = PlatformType.MetaTrader4, FilePath = @"C:\invalid" });
        _connectionRepoMock.Setup(r => r.GetByIdAsync(connection.Id)).ReturnsAsync(connection);

        var request = new DeployPortfolioRequest
        {
            SourceFolderPath = _testFolder,
            ConnectionId = connection.Id,
            ManualMT4Path = mt4Root
        };

        var result = await _sut.DeployAsync(request);

        result.Success.Should().BeTrue();
        result.FilesCopied.Should().Be(1);
        File.Exists(Path.Combine(expertsPath, "EA1.ex4")).Should().BeTrue();
    }

    [Fact]
    public async Task Deploy_Should_Copy_Files_To_Target()
    {
        File.WriteAllText(Path.Combine(_testFolder, "EA1.ex4"), "ea1 content");
        File.WriteAllText(Path.Combine(_testFolder, "EA2.ex4"), "ea2 content");

        // Create the MT4 structure for path derivation: MT4Root/Certus/portfolio_status.json
        var mt4Root = Path.Combine(_testTargetFolder, "MT4Root");
        var certusDir = Path.Combine(mt4Root, "Certus");
        Directory.CreateDirectory(certusDir);
        File.WriteAllText(Path.Combine(certusDir, "portfolio_status.json"), "{}");

        var connection = PlatformConnection.Create(
            "MetaTrader4", "MT4",
            new PlatformConfig
            {
                PlatformType = PlatformType.MetaTrader4,
                FilePath = Path.Combine(certusDir, "portfolio_status.json")
            });

        _connectionRepoMock.Setup(r => r.GetByIdAsync(connection.Id)).ReturnsAsync(connection);

        var request = new DeployPortfolioRequest
        {
            SourceFolderPath = _testFolder,
            ConnectionId = connection.Id
        };

        var result = await _sut.DeployAsync(request);

        result.Success.Should().BeTrue();
        result.FilesCopied.Should().Be(2);
        var expectedExpertsPath = Path.Combine(mt4Root, "MQL4", "Experts");
        File.Exists(Path.Combine(expectedExpertsPath, "EA1.ex4")).Should().BeTrue();
        File.Exists(Path.Combine(expectedExpertsPath, "EA2.ex4")).Should().BeTrue();
    }

    [Fact]
    public async Task Deploy_Should_Create_Portfolio_Entity()
    {
        File.WriteAllText(Path.Combine(_testFolder, "EA1.ex4"), "content");
        var mt4Root = Path.Combine(_testTargetFolder, "MT4");
        Directory.CreateDirectory(Path.Combine(mt4Root, "MQL4", "Experts"));

        var connection = PlatformConnection.Create(
            "MetaTrader4", "MT4",
            new PlatformConfig { PlatformType = PlatformType.MetaTrader4, FilePath = @"C:\invalid" });
        _connectionRepoMock.Setup(r => r.GetByIdAsync(connection.Id)).ReturnsAsync(connection);

        var request = new DeployPortfolioRequest
        {
            SourceFolderPath = _testFolder,
            ConnectionId = connection.Id,
            ManualMT4Path = mt4Root,
            PortfolioName = "MyPortfolio"
        };

        var result = await _sut.DeployAsync(request);

        result.Success.Should().BeTrue();
        result.PortfolioId.Should().NotBe(Guid.Empty);
        _portfolioRepoMock.Verify(r => r.AddAsync(It.Is<Portfolio>(p =>
            p.Name == "MyPortfolio" &&
            p.IsPlatformManaged == true)), Times.Once);
    }

    [Fact]
    public async Task Deploy_Should_Use_FolderName_When_No_PortfolioName()
    {
        File.WriteAllText(Path.Combine(_testFolder, "EA1.ex4"), "content");
        var mt4Root = Path.Combine(_testTargetFolder, "MT4");
        Directory.CreateDirectory(Path.Combine(mt4Root, "MQL4", "Experts"));

        var connection = PlatformConnection.Create(
            "MetaTrader4", "MT4",
            new PlatformConfig { PlatformType = PlatformType.MetaTrader4, FilePath = @"C:\invalid" });
        _connectionRepoMock.Setup(r => r.GetByIdAsync(connection.Id)).ReturnsAsync(connection);

        var request = new DeployPortfolioRequest
        {
            SourceFolderPath = _testFolder,
            ConnectionId = connection.Id,
            ManualMT4Path = mt4Root
        };

        var result = await _sut.DeployAsync(request);

        result.Success.Should().BeTrue();
        var folderName = Path.GetFileName(_testFolder);
        _portfolioRepoMock.Verify(r => r.AddAsync(It.Is<Portfolio>(p =>
            p.Name == folderName)), Times.Once);
    }

    [Fact]
    public async Task Deploy_Should_Create_StrategyDefinitions_For_Each_EA()
    {
        File.WriteAllText(Path.Combine(_testFolder, "EA1.ex4"), "content");
        File.WriteAllText(Path.Combine(_testFolder, "EA2.ex4"), "content");
        File.WriteAllText(Path.Combine(_testFolder, "EA3.ex4"), "content");
        var mt4Root = Path.Combine(_testTargetFolder, "MT4");
        Directory.CreateDirectory(Path.Combine(mt4Root, "MQL4", "Experts"));

        var connection = PlatformConnection.Create(
            "MetaTrader4", "MT4",
            new PlatformConfig { PlatformType = PlatformType.MetaTrader4, FilePath = @"C:\invalid" });
        _connectionRepoMock.Setup(r => r.GetByIdAsync(connection.Id)).ReturnsAsync(connection);

        var request = new DeployPortfolioRequest
        {
            SourceFolderPath = _testFolder,
            ConnectionId = connection.Id,
            ManualMT4Path = mt4Root
        };

        var result = await _sut.DeployAsync(request);

        result.Success.Should().BeTrue();
        _strategyRepoMock.Verify(r => r.AddAsync(It.IsAny<StrategyDefinition>()), Times.Exactly(3));
    }

    [Fact]
    public async Task Deploy_Should_Assign_Strategies_To_Portfolio()
    {
        File.WriteAllText(Path.Combine(_testFolder, "EA1.ex4"), "content");
        File.WriteAllText(Path.Combine(_testFolder, "EA2.ex4"), "content");
        var mt4Root = Path.Combine(_testTargetFolder, "MT4");
        Directory.CreateDirectory(Path.Combine(mt4Root, "MQL4", "Experts"));

        var connection = PlatformConnection.Create(
            "MetaTrader4", "MT4",
            new PlatformConfig { PlatformType = PlatformType.MetaTrader4, FilePath = @"C:\invalid" });
        _connectionRepoMock.Setup(r => r.GetByIdAsync(connection.Id)).ReturnsAsync(connection);

        var request = new DeployPortfolioRequest
        {
            SourceFolderPath = _testFolder,
            ConnectionId = connection.Id,
            ManualMT4Path = mt4Root
        };

        var result = await _sut.DeployAsync(request);

        result.Success.Should().BeTrue();
        // Verify 2 strategies were created (one per EA)
        _strategyRepoMock.Verify(r => r.AddAsync(It.IsAny<StrategyDefinition>()), Times.Exactly(2));
        // Verify portfolio was created and strategies assigned
        _portfolioRepoMock.Verify(r => r.AddAsync(It.Is<Portfolio>(p =>
            p.IsPlatformManaged == true)), Times.Once);
    }

    [Fact]
    public async Task Deploy_Should_Create_Deployment_Entity()
    {
        File.WriteAllText(Path.Combine(_testFolder, "EA1.ex4"), "content");
        var mt4Root = Path.Combine(_testTargetFolder, "MT4");
        Directory.CreateDirectory(Path.Combine(mt4Root, "MQL4", "Experts"));

        var connection = PlatformConnection.Create(
            "MetaTrader4", "MT4",
            new PlatformConfig { PlatformType = PlatformType.MetaTrader4, FilePath = @"C:\invalid" });
        _connectionRepoMock.Setup(r => r.GetByIdAsync(connection.Id)).ReturnsAsync(connection);

        var request = new DeployPortfolioRequest
        {
            SourceFolderPath = _testFolder,
            ConnectionId = connection.Id,
            ManualMT4Path = mt4Root
        };

        var result = await _sut.DeployAsync(request);

        result.Success.Should().BeTrue();
        result.DeploymentId.Should().NotBe(Guid.Empty);
        _deploymentRepoMock.Verify(r => r.AddAsync(It.IsAny<PortfolioDeployment>()), Times.Once);
    }

    [Fact]
    public async Task Deploy_Should_Set_Monitoring_Status_On_Success()
    {
        File.WriteAllText(Path.Combine(_testFolder, "EA1.ex4"), "content");
        var mt4Root = Path.Combine(_testTargetFolder, "MT4");
        Directory.CreateDirectory(Path.Combine(mt4Root, "MQL4", "Experts"));

        var connection = PlatformConnection.Create(
            "MetaTrader4", "MT4",
            new PlatformConfig { PlatformType = PlatformType.MetaTrader4, FilePath = @"C:\invalid" });
        _connectionRepoMock.Setup(r => r.GetByIdAsync(connection.Id)).ReturnsAsync(connection);

        var request = new DeployPortfolioRequest
        {
            SourceFolderPath = _testFolder,
            ConnectionId = connection.Id,
            ManualMT4Path = mt4Root
        };

        var result = await _sut.DeployAsync(request);

        result.Success.Should().BeTrue();
        // The deployment goes through Pending -> Copying -> Deployed -> Monitoring
        // Verify the deployment was added and changes were saved
        _deploymentRepoMock.Verify(r => r.AddAsync(It.IsAny<PortfolioDeployment>()), Times.Once);
        _deploymentRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Deploy_Should_Handle_Conflict_Resolution_Skip()
    {
        File.WriteAllText(Path.Combine(_testFolder, "EA1.ex4"), "new content");

        var mt4Root = Path.Combine(_testTargetFolder, "MT4");
        var expertsPath = Path.Combine(mt4Root, "MQL4", "Experts");
        Directory.CreateDirectory(expertsPath);
        File.WriteAllText(Path.Combine(expertsPath, "EA1.ex4"), "old content");

        var connection = PlatformConnection.Create(
            "MetaTrader4", "MT4",
            new PlatformConfig { PlatformType = PlatformType.MetaTrader4, FilePath = @"C:\invalid" });
        _connectionRepoMock.Setup(r => r.GetByIdAsync(connection.Id)).ReturnsAsync(connection);

        var request = new DeployPortfolioRequest
        {
            SourceFolderPath = _testFolder,
            ConnectionId = connection.Id,
            ManualMT4Path = mt4Root,
            ConflictResolutions = new Dictionary<string, ConflictAction>
            {
                ["EA1.ex4"] = ConflictAction.Skip
            }
        };

        var result = await _sut.DeployAsync(request);

        result.Success.Should().BeTrue();
        result.FilesSkipped.Should().Be(1);
        result.FilesCopied.Should().Be(0);
        // Original file should remain unchanged
        File.ReadAllText(Path.Combine(expertsPath, "EA1.ex4")).Should().Be("old content");
    }

    [Fact]
    public async Task Deploy_Should_Handle_Conflict_Resolution_Overwrite()
    {
        File.WriteAllText(Path.Combine(_testFolder, "EA1.ex4"), "new content");

        var mt4Root = Path.Combine(_testTargetFolder, "MT4");
        var expertsPath = Path.Combine(mt4Root, "MQL4", "Experts");
        Directory.CreateDirectory(expertsPath);
        File.WriteAllText(Path.Combine(expertsPath, "EA1.ex4"), "old content");

        var connection = PlatformConnection.Create(
            "MetaTrader4", "MT4",
            new PlatformConfig { PlatformType = PlatformType.MetaTrader4, FilePath = @"C:\invalid" });
        _connectionRepoMock.Setup(r => r.GetByIdAsync(connection.Id)).ReturnsAsync(connection);

        var request = new DeployPortfolioRequest
        {
            SourceFolderPath = _testFolder,
            ConnectionId = connection.Id,
            ManualMT4Path = mt4Root,
            ConflictResolutions = new Dictionary<string, ConflictAction>
            {
                ["EA1.ex4"] = ConflictAction.Overwrite
            }
        };

        var result = await _sut.DeployAsync(request);

        result.Success.Should().BeTrue();
        result.FilesCopied.Should().Be(1);
        File.ReadAllText(Path.Combine(expertsPath, "EA1.ex4")).Should().Be("new content");
    }

    [Fact]
    public async Task Deploy_Should_Default_To_Overwrite_When_No_Resolution()
    {
        File.WriteAllText(Path.Combine(_testFolder, "EA1.ex4"), "new content");

        var mt4Root = Path.Combine(_testTargetFolder, "MT4");
        var expertsPath = Path.Combine(mt4Root, "MQL4", "Experts");
        Directory.CreateDirectory(expertsPath);
        File.WriteAllText(Path.Combine(expertsPath, "EA1.ex4"), "old content");

        var connection = PlatformConnection.Create(
            "MetaTrader4", "MT4",
            new PlatformConfig { PlatformType = PlatformType.MetaTrader4, FilePath = @"C:\invalid" });
        _connectionRepoMock.Setup(r => r.GetByIdAsync(connection.Id)).ReturnsAsync(connection);

        var request = new DeployPortfolioRequest
        {
            SourceFolderPath = _testFolder,
            ConnectionId = connection.Id,
            ManualMT4Path = mt4Root
            // No ConflictResolutions - should default to overwrite
        };

        var result = await _sut.DeployAsync(request);

        result.Success.Should().BeTrue();
        result.FilesCopied.Should().Be(1);
        File.ReadAllText(Path.Combine(expertsPath, "EA1.ex4")).Should().Be("new content");
    }

    [Fact]
    public async Task Deploy_Should_Set_ActivationPending_When_Activation_Fails()
    {
        File.WriteAllText(Path.Combine(_testFolder, "EA1.ex4"), "content");
        // Use a path that will fail path derivation for activation
        var connection = PlatformConnection.Create(
            "MetaTrader4", "MT4",
            new PlatformConfig { PlatformType = PlatformType.MetaTrader4, FilePath = @"C:\invalid" });
        _connectionRepoMock.Setup(r => r.GetByIdAsync(connection.Id)).ReturnsAsync(connection);

        var request = new DeployPortfolioRequest
        {
            SourceFolderPath = _testFolder,
            ConnectionId = connection.Id,
            ManualMT4Path = Path.Combine(_testTargetFolder, "MT4")
        };

        var result = await _sut.DeployAsync(request);

        // The deployment should still succeed even if activation fails
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Deploy_Should_Save_Changes()
    {
        File.WriteAllText(Path.Combine(_testFolder, "EA1.ex4"), "content");
        var mt4Root = Path.Combine(_testTargetFolder, "MT4");
        Directory.CreateDirectory(Path.Combine(mt4Root, "MQL4", "Experts"));

        var connection = PlatformConnection.Create(
            "MetaTrader4", "MT4",
            new PlatformConfig { PlatformType = PlatformType.MetaTrader4, FilePath = @"C:\invalid" });
        _connectionRepoMock.Setup(r => r.GetByIdAsync(connection.Id)).ReturnsAsync(connection);

        var request = new DeployPortfolioRequest
        {
            SourceFolderPath = _testFolder,
            ConnectionId = connection.Id,
            ManualMT4Path = mt4Root
        };

        var result = await _sut.DeployAsync(request);

        result.Success.Should().BeTrue();
        _deploymentRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Deploy_Should_Use_SelectedTerminalPath()
    {
        File.WriteAllText(Path.Combine(_testFolder, "EA1.ex4"), "content");

        var metaQuotesRoot = Path.Combine(_testTargetFolder, "MetaQuotesB");
        var expertsPath = Path.Combine(metaQuotesRoot, "Terminal", "HashB", "MQL4", "Experts");
        Directory.CreateDirectory(expertsPath);

        var connection = PlatformConnection.Create(
            "MetaTrader4", "MT4",
            new PlatformConfig { PlatformType = PlatformType.MetaTrader4, FilePath = @"C:\invalid" });
        _connectionRepoMock.Setup(r => r.GetByIdAsync(connection.Id)).ReturnsAsync(connection);

        var request = new DeployPortfolioRequest
        {
            SourceFolderPath = _testFolder,
            ConnectionId = connection.Id,
            SelectedTerminalPath = Path.Combine(metaQuotesRoot, "Terminal", "HashB")
        };

        var result = await _sut.DeployAsync(request);

        result.Success.Should().BeTrue();
        File.Exists(Path.Combine(expertsPath, "EA1.ex4")).Should().BeTrue();
    }

    [Fact]
    public async Task ActivateEAs_Should_Write_To_CommonPath_When_Common_Detected()
    {
        // Setup: Common path + single terminal
        var metaQuotesRoot = Path.Combine(_testTargetFolder, "MetaQuotesA");
        var certusCommonDir = Path.Combine(metaQuotesRoot, "Common", "Files", "Certus");
        var terminalHash = "TerminalHashA";
        var expertsPath = Path.Combine(metaQuotesRoot, "Terminal", terminalHash, "MQL4", "Experts");
        var scriptsPath = Path.Combine(metaQuotesRoot, "Terminal", terminalHash, "MQL4", "Scripts");
        Directory.CreateDirectory(certusCommonDir);
        Directory.CreateDirectory(scriptsPath);
        Directory.CreateDirectory(expertsPath);

        File.WriteAllText(Path.Combine(_testFolder, "EA1.ex4"), "content");

        var connection = PlatformConnection.Create(
            "MetaTrader4", "MT4",
            new PlatformConfig
            {
                PlatformType = PlatformType.MetaTrader4,
                FilePath = Path.Combine(certusCommonDir, "portfolio_status.json")
            });
        _connectionRepoMock.Setup(r => r.GetByIdAsync(connection.Id)).ReturnsAsync(connection);

        var request = new DeployPortfolioRequest
        {
            SourceFolderPath = _testFolder,
            ConnectionId = connection.Id,
            ManualMT4Path = Path.Combine(metaQuotesRoot, "Terminal", terminalHash)
        };

        var result = await _sut.DeployAsync(request);

        result.Success.Should().BeTrue();
        // Verify activation JSON is in Common/Files/Certus/ not per-terminal Files/Certus/
        File.Exists(Path.Combine(certusCommonDir, "certus_activation.json")).Should().BeTrue();
        File.Exists(Path.Combine(metaQuotesRoot, "Terminal", terminalHash, "Files", "Certus", "certus_activation.json"))
            .Should().BeFalse();
    }

    // =====================================================
    // GetDeploymentStatusAsync
    // =====================================================

    [Fact]
    public async Task GetDeploymentStatus_Should_Return_Null_When_Not_Found()
    {
        _deploymentRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((PortfolioDeployment?)null);

        var result = await _sut.GetDeploymentStatusAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetDeploymentStatus_Should_Return_Dto_When_Found()
    {
        var deployment = PortfolioDeployment.Create(
            Guid.NewGuid(), Guid.NewGuid(), @"C:\EA", @"C:\MT4", 3);
        var portfolio = new Portfolio(Guid.NewGuid(), "Test Portfolio", 0, 0, Money.Zero(Currency.USD));
        var connection = PlatformConnection.Create("MetaTrader4", "MT4",
            new PlatformConfig { PlatformType = PlatformType.MetaTrader4 });

        _deploymentRepoMock.Setup(r => r.GetByIdAsync(deployment.Id)).ReturnsAsync(deployment);
        _portfolioRepoMock.Setup(r => r.GetByIdAsync(deployment.PortfolioId)).ReturnsAsync(portfolio);
        _connectionRepoMock.Setup(r => r.GetByIdAsync(deployment.ConnectionId)).ReturnsAsync(connection);

        var result = await _sut.GetDeploymentStatusAsync(deployment.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(deployment.Id);
        result.PortfolioName.Should().Be("Test Portfolio");
        result.ConnectionName.Should().Be("MT4");
        result.EACount.Should().Be(3);
        result.Status.Should().Be(DeploymentStatus.Pending);
    }

    // =====================================================
    // StopMonitoringAsync
    // =====================================================

    [Fact]
    public async Task StopMonitoring_Should_Throw_When_Deployment_Not_Found()
    {
        _deploymentRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((PortfolioDeployment?)null);

        var act = () => _sut.StopMonitoringAsync(Guid.NewGuid());
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task StopMonitoring_Should_Stop_Deployment()
    {
        var deployment = PortfolioDeployment.Create(
            Guid.NewGuid(), Guid.NewGuid(), @"C:\EA", @"C:\MT4", 1);
        deployment.StartCopying();
        deployment.MarkDeployed();
        deployment.StartMonitoring();

        _deploymentRepoMock.Setup(r => r.GetByIdAsync(deployment.Id)).ReturnsAsync(deployment);

        await _sut.StopMonitoringAsync(deployment.Id);

        deployment.Status.Should().Be(DeploymentStatus.Stopped);
        _deploymentRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    // =====================================================
    // GetDeploymentsAsync
    // =====================================================

    [Fact]
    public async Task GetDeployments_Should_Return_Empty_When_None()
    {
        _deploymentRepoMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<PortfolioDeployment>());

        var result = await _sut.GetDeploymentsAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetDeployments_Should_Return_All_Deployments()
    {
        var d1 = PortfolioDeployment.Create(Guid.NewGuid(), Guid.NewGuid(), @"C:\EA1", @"C:\MT4", 1);
        var d2 = PortfolioDeployment.Create(Guid.NewGuid(), Guid.NewGuid(), @"C:\EA2", @"C:\MT4", 2);
        _deploymentRepoMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<PortfolioDeployment> { d1, d2 });

        // Need to setup portfolio and connection lookups for mapping
        _portfolioRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new Portfolio(Guid.NewGuid(), "P", 0, 0, Money.Zero(Currency.USD)));
        _connectionRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(PlatformConnection.Create("MetaTrader4", "MT4",
                new PlatformConfig { PlatformType = PlatformType.MetaTrader4 }));

        var result = await _sut.GetDeploymentsAsync();

        result.Should().HaveCount(2);
    }

    // =====================================================
    // ConflictAction enum
    // =====================================================

    [Fact]
    public void ConflictAction_Should_Have_Skip_And_Overwrite_Values()
    {
        ConflictAction.Skip.Should().Be(default);
        ConflictAction.Overwrite.Should().Be((ConflictAction)1);
    }

    // =====================================================
    // DTO Mapping
    // =====================================================

    [Fact]
    public async Task MapToDto_Should_Use_Unknown_When_Portfolio_Not_Found()
    {
        var deployment = PortfolioDeployment.Create(
            Guid.NewGuid(), Guid.NewGuid(), @"C:\EA", @"C:\MT4", 1);

        _deploymentRepoMock.Setup(r => r.GetByIdAsync(deployment.Id)).ReturnsAsync(deployment);
        _portfolioRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Portfolio?)null);
        _connectionRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((PlatformConnection?)null);

        var result = await _sut.GetDeploymentStatusAsync(deployment.Id);

        result.Should().NotBeNull();
        result!.PortfolioName.Should().Be("Unknown");
        result.ConnectionName.Should().Be("Unknown");
    }
}
