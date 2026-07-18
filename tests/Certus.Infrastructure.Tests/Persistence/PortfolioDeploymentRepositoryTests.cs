using Certus.Domain.Platform.Aggregates;
using Certus.Domain.Platform.Enums;
using Certus.Infrastructure.Persistence;
using Certus.Infrastructure.Persistence.Repositories.Platform;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Certus.Infrastructure.Tests.Persistence;

public class PortfolioDeploymentRepositoryTests : IDisposable
{
    private readonly CertusDbContext _db;
    private readonly PortfolioDeploymentRepository _sut;

    public PortfolioDeploymentRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<CertusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new CertusDbContext(options);
        _sut = new PortfolioDeploymentRepository(_db);
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    [Fact]
    public async Task AddAsync_Should_Persist_Deployment()
    {
        // Arrange
        var deployment = PortfolioDeployment.Create(
            Guid.NewGuid(), Guid.NewGuid(), @"C:\EA", @"C:\MT4", 3);

        // Act
        await _sut.AddAsync(deployment);
        await _sut.SaveChangesAsync();

        // Assert
        var result = await _sut.GetByIdAsync(deployment.Id);
        result.Should().NotBeNull();
        result!.SourceFolderPath.Should().Be(@"C:\EA");
        result.TargetMT4Path.Should().Be(@"C:\MT4");
        result.EACount.Should().Be(3);
        result.Status.Should().Be(DeploymentStatus.Pending);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Null_When_Not_Found()
    {
        // Arrange & Act
        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_All_Deployments()
    {
        // Arrange
        var deployment1 = PortfolioDeployment.Create(Guid.NewGuid(), Guid.NewGuid(), @"C:\EA1", @"C:\MT4", 1);
        var deployment2 = PortfolioDeployment.Create(Guid.NewGuid(), Guid.NewGuid(), @"C:\EA2", @"C:\MT4", 2);

        await _sut.AddAsync(deployment1);
        await _sut.AddAsync(deployment2);
        await _sut.SaveChangesAsync();

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByPortfolioIdAsync_Should_Return_Correct_Deployment()
    {
        // Arrange
        var portfolioId = Guid.NewGuid();
        var deployment = PortfolioDeployment.Create(portfolioId, Guid.NewGuid(), @"C:\EA", @"C:\MT4", 1);
        await _sut.AddAsync(deployment);
        await _sut.SaveChangesAsync();

        // Act
        var result = await _sut.GetByPortfolioIdAsync(portfolioId);

        // Assert
        result.Should().NotBeNull();
        result!.PortfolioId.Should().Be(portfolioId);
    }

    [Fact]
    public async Task GetByConnectionIdAsync_Should_Return_Correct_Deployment()
    {
        // Arrange
        var connectionId = Guid.NewGuid();
        var deployment = PortfolioDeployment.Create(Guid.NewGuid(), connectionId, @"C:\EA", @"C:\MT4", 1);
        await _sut.AddAsync(deployment);
        await _sut.SaveChangesAsync();

        // Act
        var result = await _sut.GetByConnectionIdAsync(connectionId);

        // Assert
        result.Should().NotBeNull();
        result!.ConnectionId.Should().Be(connectionId);
    }

    [Fact]
    public async Task Update_Should_Persist_Changes()
    {
        // Arrange
        var deployment = PortfolioDeployment.Create(Guid.NewGuid(), Guid.NewGuid(), @"C:\EA", @"C:\MT4", 1);
        await _sut.AddAsync(deployment);
        await _sut.SaveChangesAsync();

        // Act
        deployment.StartCopying();
        _sut.Update(deployment);
        await _sut.SaveChangesAsync();

        // Assert
        var result = await _sut.GetByIdAsync(deployment.Id);
        result!.Status.Should().Be(DeploymentStatus.Copying);
    }
}
