using Certus.Domain.Platform.Aggregates;
using Certus.Domain.Platform.Enums;
using FluentAssertions;

namespace Certus.Domain.Tests.Platform;

public class PortfolioDeploymentTests
{
    [Fact]
    public void Create_Should_SetProperties_Correctly()
    {
        // Arrange
        var portfolioId = Guid.NewGuid();
        var connectionId = Guid.NewGuid();
        var sourcePath = @"C:\EA\Source";
        var targetPath = @"C:\MT4\MQL4\Experts";
        var eaCount = 5;

        // Act
        var deployment = PortfolioDeployment.Create(portfolioId, connectionId, sourcePath, targetPath, eaCount);

        // Assert
        deployment.Id.Should().NotBe(Guid.Empty);
        deployment.PortfolioId.Should().Be(portfolioId);
        deployment.ConnectionId.Should().Be(connectionId);
        deployment.SourceFolderPath.Should().Be(sourcePath);
        deployment.TargetMT4Path.Should().Be(targetPath);
        deployment.EACount.Should().Be(eaCount);
        deployment.Status.Should().Be(DeploymentStatus.Pending);
        deployment.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        deployment.DeployedAt.Should().BeNull();
        deployment.MonitoringStartedAt.Should().BeNull();
        deployment.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Create_Should_Throw_When_SourcePath_Is_Empty()
    {
        var act = () => PortfolioDeployment.Create(Guid.NewGuid(), Guid.NewGuid(), "", @"C:\MT4", 1);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_Should_Throw_When_SourcePath_Is_Whitespace()
    {
        var act = () => PortfolioDeployment.Create(Guid.NewGuid(), Guid.NewGuid(), "   ", @"C:\MT4", 1);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_Should_Throw_When_TargetPath_Is_Empty()
    {
        var act = () => PortfolioDeployment.Create(Guid.NewGuid(), Guid.NewGuid(), @"C:\EA", "", 1);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_Should_Throw_When_TargetPath_Is_Whitespace()
    {
        var act = () => PortfolioDeployment.Create(Guid.NewGuid(), Guid.NewGuid(), @"C:\EA", "  ", 1);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_Should_Allow_Zero_EACount()
    {
        var deployment = PortfolioDeployment.Create(Guid.NewGuid(), Guid.NewGuid(), @"C:\EA", @"C:\MT4", 0);
        deployment.EACount.Should().Be(0);
    }

    [Fact]
    public void Create_Should_Generate_Unique_Id()
    {
        var d1 = PortfolioDeployment.Create(Guid.NewGuid(), Guid.NewGuid(), @"C:\EA", @"C:\MT4", 1);
        var d2 = PortfolioDeployment.Create(Guid.NewGuid(), Guid.NewGuid(), @"C:\EA", @"C:\MT4", 1);
        d1.Id.Should().NotBe(d2.Id);
    }

    // --- StartCopying ---

    [Fact]
    public void StartCopying_Should_Change_Status_To_Copying()
    {
        var deployment = PortfolioDeployment.Create(Guid.NewGuid(), Guid.NewGuid(), @"C:\EA", @"C:\MT4", 1);
        deployment.StartCopying();
        deployment.Status.Should().Be(DeploymentStatus.Copying);
    }

    [Fact]
    public void StartCopying_Should_Throw_When_Status_Is_Copying()
    {
        var deployment = PortfolioDeployment.Create(Guid.NewGuid(), Guid.NewGuid(), @"C:\EA", @"C:\MT4", 1);
        deployment.StartCopying();
        var act = () => deployment.StartCopying();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void StartCopying_Should_Throw_When_Status_Is_Deployed()
    {
        var deployment = PortfolioDeployment.Create(Guid.NewGuid(), Guid.NewGuid(), @"C:\EA", @"C:\MT4", 1);
        deployment.StartCopying();
        deployment.MarkDeployed();
        var act = () => deployment.StartCopying();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void StartCopying_Should_Throw_When_Status_Is_Monitoring()
    {
        var deployment = PortfolioDeployment.Create(Guid.NewGuid(), Guid.NewGuid(), @"C:\EA", @"C:\MT4", 1);
        deployment.StartCopying();
        deployment.MarkDeployed();
        deployment.StartMonitoring();
        var act = () => deployment.StartCopying();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void StartCopying_Should_Throw_When_Status_Is_Error()
    {
        var deployment = PortfolioDeployment.Create(Guid.NewGuid(), Guid.NewGuid(), @"C:\EA", @"C:\MT4", 1);
        deployment.MarkError("failed");
        var act = () => deployment.StartCopying();
        act.Should().Throw<InvalidOperationException>();
    }

    // --- MarkDeployed ---

    [Fact]
    public void MarkDeployed_Should_Change_Status_To_Deployed()
    {
        var deployment = PortfolioDeployment.Create(Guid.NewGuid(), Guid.NewGuid(), @"C:\EA", @"C:\MT4", 1);
        deployment.StartCopying();
        deployment.MarkDeployed();
        deployment.Status.Should().Be(DeploymentStatus.Deployed);
        deployment.DeployedAt.Should().NotBeNull();
        deployment.DeployedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void MarkDeployed_Should_Throw_When_Status_Is_Pending()
    {
        var deployment = PortfolioDeployment.Create(Guid.NewGuid(), Guid.NewGuid(), @"C:\EA", @"C:\MT4", 1);
        var act = () => deployment.MarkDeployed();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkDeployed_Should_Throw_When_Status_Is_Monitoring()
    {
        var deployment = PortfolioDeployment.Create(Guid.NewGuid(), Guid.NewGuid(), @"C:\EA", @"C:\MT4", 1);
        deployment.StartCopying();
        deployment.MarkDeployed();
        deployment.StartMonitoring();
        var act = () => deployment.MarkDeployed();
        act.Should().Throw<InvalidOperationException>();
    }

    // --- StartMonitoring ---

    [Fact]
    public void StartMonitoring_Should_Change_Status_To_Monitoring()
    {
        var deployment = PortfolioDeployment.Create(Guid.NewGuid(), Guid.NewGuid(), @"C:\EA", @"C:\MT4", 1);
        deployment.StartCopying();
        deployment.MarkDeployed();
        deployment.StartMonitoring();
        deployment.Status.Should().Be(DeploymentStatus.Monitoring);
        deployment.MonitoringStartedAt.Should().NotBeNull();
        deployment.MonitoringStartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void StartMonitoring_Should_Throw_When_Status_Is_Pending()
    {
        var deployment = PortfolioDeployment.Create(Guid.NewGuid(), Guid.NewGuid(), @"C:\EA", @"C:\MT4", 1);
        var act = () => deployment.StartMonitoring();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void StartMonitoring_Should_Throw_When_Status_Is_Copying()
    {
        var deployment = PortfolioDeployment.Create(Guid.NewGuid(), Guid.NewGuid(), @"C:\EA", @"C:\MT4", 1);
        deployment.StartCopying();
        var act = () => deployment.StartMonitoring();
        act.Should().Throw<InvalidOperationException>();
    }

    // --- MarkError ---

    [Fact]
    public void MarkError_Should_Change_Status_To_Error_From_Pending()
    {
        var deployment = PortfolioDeployment.Create(Guid.NewGuid(), Guid.NewGuid(), @"C:\EA", @"C:\MT4", 1);
        deployment.MarkError("failed");
        deployment.Status.Should().Be(DeploymentStatus.Error);
        deployment.ErrorMessage.Should().Be("failed");
    }

    [Fact]
    public void MarkError_Should_Change_Status_To_Error_From_Copying()
    {
        var deployment = PortfolioDeployment.Create(Guid.NewGuid(), Guid.NewGuid(), @"C:\EA", @"C:\MT4", 1);
        deployment.StartCopying();
        deployment.MarkError("file locked");
        deployment.Status.Should().Be(DeploymentStatus.Error);
        deployment.ErrorMessage.Should().Be("file locked");
    }

    [Fact]
    public void MarkError_Should_Overwrite_Previous_Error_Message()
    {
        var deployment = PortfolioDeployment.Create(Guid.NewGuid(), Guid.NewGuid(), @"C:\EA", @"C:\MT4", 1);
        deployment.MarkError("first error");
        deployment.MarkError("second error");
        deployment.ErrorMessage.Should().Be("second error");
    }

    // --- Stop ---

    [Fact]
    public void Stop_Should_Change_Status_To_Stopped()
    {
        var deployment = PortfolioDeployment.Create(Guid.NewGuid(), Guid.NewGuid(), @"C:\EA", @"C:\MT4", 1);
        deployment.StartCopying();
        deployment.MarkDeployed();
        deployment.StartMonitoring();
        deployment.Stop();
        deployment.Status.Should().Be(DeploymentStatus.Stopped);
    }

    [Fact]
    public void Stop_Should_Throw_When_Status_Is_Pending()
    {
        var deployment = PortfolioDeployment.Create(Guid.NewGuid(), Guid.NewGuid(), @"C:\EA", @"C:\MT4", 1);
        var act = () => deployment.Stop();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Stop_Should_Throw_When_Status_Is_Deployed()
    {
        var deployment = PortfolioDeployment.Create(Guid.NewGuid(), Guid.NewGuid(), @"C:\EA", @"C:\MT4", 1);
        deployment.StartCopying();
        deployment.MarkDeployed();
        var act = () => deployment.Stop();
        act.Should().Throw<InvalidOperationException>();
    }

    // --- IsMonitoring ---

    [Fact]
    public void IsMonitoring_Should_Return_True_When_Status_Is_Monitoring()
    {
        var deployment = PortfolioDeployment.Create(Guid.NewGuid(), Guid.NewGuid(), @"C:\EA", @"C:\MT4", 1);
        deployment.StartCopying();
        deployment.MarkDeployed();
        deployment.StartMonitoring();
        deployment.IsMonitoring.Should().BeTrue();
    }

    [Theory]
    [InlineData(DeploymentStatus.Pending)]
    [InlineData(DeploymentStatus.Copying)]
    [InlineData(DeploymentStatus.Deployed)]
    [InlineData(DeploymentStatus.Error)]
    [InlineData(DeploymentStatus.Stopped)]
    public void IsMonitoring_Should_Return_False_For_All_Other_Statuses(DeploymentStatus status)
    {
        var deployment = PortfolioDeployment.Create(Guid.NewGuid(), Guid.NewGuid(), @"C:\EA", @"C:\MT4", 1);
        // Set to desired status via valid transitions or error
        switch (status)
        {
            case DeploymentStatus.Copying:
                deployment.StartCopying();
                break;
            case DeploymentStatus.Deployed:
                deployment.StartCopying();
                deployment.MarkDeployed();
                break;
            case DeploymentStatus.Monitoring:
                deployment.StartCopying();
                deployment.MarkDeployed();
                deployment.StartMonitoring();
                break;
            case DeploymentStatus.Error:
                deployment.MarkError("fail");
                break;
            case DeploymentStatus.Stopped:
                deployment.StartCopying();
                deployment.MarkDeployed();
                deployment.StartMonitoring();
                deployment.Stop();
                break;
        }
        // If status is Monitoring, IsMonitoring should be true; otherwise false
        if (status == DeploymentStatus.Monitoring)
            deployment.IsMonitoring.Should().BeTrue();
        else
            deployment.IsMonitoring.Should().BeFalse();
    }

    // --- Full Lifecycle ---

    [Fact]
    public void FullLifecycle_Should_Follow_Happy_Path()
    {
        var deployment = PortfolioDeployment.Create(Guid.NewGuid(), Guid.NewGuid(), @"C:\EA", @"C:\MT4", 3);

        deployment.Status.Should().Be(DeploymentStatus.Pending);

        deployment.StartCopying();
        deployment.Status.Should().Be(DeploymentStatus.Copying);

        deployment.MarkDeployed();
        deployment.Status.Should().Be(DeploymentStatus.Deployed);
        deployment.DeployedAt.Should().NotBeNull();

        deployment.StartMonitoring();
        deployment.Status.Should().Be(DeploymentStatus.Monitoring);
        deployment.MonitoringStartedAt.Should().NotBeNull();

        deployment.Stop();
        deployment.Status.Should().Be(DeploymentStatus.Stopped);
    }

    [Fact]
    public void ErrorPath_Should_Allow_Error_From_Any_State()
    {
        // Pending -> Error
        var d1 = PortfolioDeployment.Create(Guid.NewGuid(), Guid.NewGuid(), @"C:\EA", @"C:\MT4", 1);
        d1.MarkError("validation failed");
        d1.Status.Should().Be(DeploymentStatus.Error);

        // Copying -> Error
        var d2 = PortfolioDeployment.Create(Guid.NewGuid(), Guid.NewGuid(), @"C:\EA", @"C:\MT4", 1);
        d2.StartCopying();
        d2.MarkError("file locked");
        d2.Status.Should().Be(DeploymentStatus.Error);

        // Deployed -> Error
        var d3 = PortfolioDeployment.Create(Guid.NewGuid(), Guid.NewGuid(), @"C:\EA", @"C:\MT4", 1);
        d3.StartCopying();
        d3.MarkDeployed();
        d3.MarkError("activation failed");
        d3.Status.Should().Be(DeploymentStatus.Error);
    }

    [Fact]
    public void CannotTransition_From_Stopped()
    {
        var deployment = PortfolioDeployment.Create(Guid.NewGuid(), Guid.NewGuid(), @"C:\EA", @"C:\MT4", 1);
        deployment.StartCopying();
        deployment.MarkDeployed();
        deployment.StartMonitoring();
        deployment.Stop();

        // Stopped is terminal - no further transitions allowed
        var act1 = () => deployment.StartCopying();
        act1.Should().Throw<InvalidOperationException>();

        var act2 = () => deployment.MarkDeployed();
        act2.Should().Throw<InvalidOperationException>();

        var act3 = () => deployment.StartMonitoring();
        act3.Should().Throw<InvalidOperationException>();

        var act4 = () => deployment.Stop();
        act4.Should().Throw<InvalidOperationException>();
    }
}
