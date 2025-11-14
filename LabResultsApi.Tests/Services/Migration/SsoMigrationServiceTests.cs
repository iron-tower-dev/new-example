using NUnit.Framework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Moq;
using LabResultsApi.Services.Migration;
using LabResultsApi.DTOs.Migration;

namespace LabResultsApi.Tests.Services.Migration;

[TestFixture]
public class SsoMigrationServiceTests : TestBase
{
    private ISsoMigrationService _service = null!;
    private Mock<IAuthenticationRemovalService> _mockAuthRemovalService = null!;
    private Mock<IWebHostEnvironment> _mockEnvironment = null!;
    private Mock<IMigrationLoggingService> _mockLoggingService = null!;
    private Mock<IConfiguration> _mockConfiguration = null!;
    private string _testDirectory = null!;

    [SetUp]
    public override async Task SetUp()
    {
        await base.SetUp();
        
        // Create test directory
        _testDirectory = Path.Combine(Path.GetTempPath(), $"SsoMigrationTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
        
        // Setup mocks
        _mockAuthRemovalService = new Mock<IAuthenticationRemovalService>();
        _mockEnvironment = new Mock<IWebHostEnvironment>();
        _mockLoggingService = new Mock<IMigrationLoggingService>();
        _mockConfiguration = new Mock<IConfiguration>();
        
        _mockEnvironment.Setup(x => x.ContentRootPath).Returns(_testDirectory);
        _mockConfiguration.Setup(x => x["Frontend:Path"]).Returns(Path.Combine(_testDirectory, "frontend"));
        
        // Create service with mocked dependencies
        var logger = GetService<ILogger<SsoMigrationService>>();
        _service = new SsoMigrationService(
            _mockAuthRemovalService.Object,
            logger,
            _mockLoggingService.Object,
            _mockEnvironment.Object,
            _mockConfiguration.Object);
    }

    [TearDown]
    public override async Task TearDown()
    {
        // Clean up test directory
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
        
        await base.TearDown();
    }

    [Test]
    public async Task RemoveJwtAuthenticationAsync_WithSuccessfulSteps_ReturnsSuccess()
    {
        // Arrange
        var middlewareResult = new AuthRemovalResult
        {
            Success = true,
            RemovedComponents = { "JWT Middleware" },
            ModifiedFiles = { "Program.cs" }
        };

        var servicesResult = new AuthRemovalResult
        {
            Success = true,
            RemovedComponents = { "Auth Services" },
            ModifiedFiles = { "Program.cs" }
        };

        var cleanupResult = new AuthRemovalResult
        {
            Success = true,
            RemovedComponents = { "JWT Config" },
            ModifiedFiles = { "appsettings.json" }
        };

        _mockAuthRemovalService.Setup(x => x.RemoveJwtMiddlewareAsync())
            .ReturnsAsync(middlewareResult);
        _mockAuthRemovalService.Setup(x => x.RemoveAuthenticationServicesAsync())
            .ReturnsAsync(servicesResult);
        _mockAuthRemovalService.Setup(x => x.CleanupAuthDependenciesAsync())
            .ReturnsAsync(cleanupResult);

        // Act
        var result = await _service.RemoveJwtAuthenticationAsync();

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.RemovedComponents, Has.Count.EqualTo(3));
        Assert.That(result.ModifiedFiles, Has.Count.EqualTo(3));
        Assert.That(result.Errors, Is.Empty);

        // Verify all steps were called
        _mockAuthRemovalService.Verify(x => x.RemoveJwtMiddlewareAsync(), Times.Once);
        _mockAuthRemovalService.Verify(x => x.RemoveAuthenticationServicesAsync(), Times.Once);
        _mockAuthRemovalService.Verify(x => x.CleanupAuthDependenciesAsync(), Times.Once);
    }

    [Test]
    public async Task RemoveJwtAuthenticationAsync_WithMiddlewareFailure_StopsAndReturnsError()
    {
        // Arrange
        var middlewareResult = new AuthRemovalResult
        {
            Success = false,
            Errors = { "Middleware removal failed" }
        };

        _mockAuthRemovalService.Setup(x => x.RemoveJwtMiddlewareAsync())
            .ReturnsAsync(middlewareResult);

        // Act
        var result = await _service.RemoveJwtAuthenticationAsync();

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Errors, Contains.Item("Failed to remove JWT middleware"));

        // Verify subsequent steps were not called
        _mockAuthRemovalService.Verify(x => x.RemoveJwtMiddlewareAsync(), Times.Once);
        _mockAuthRemovalService.Verify(x => x.RemoveAuthenticationServicesAsync(), Times.Never);
        _mockAuthRemovalService.Verify(x => x.CleanupAuthDependenciesAsync(), Times.Never);
    }

    [Test]
    public async Task CleanupAuthConfigurationAsync_ReturnsSuccess()
    {
        // Act
        var result = await _service.CleanupAuthConfigurationAsync();

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.CleanedConfigSections, Contains.Item("JWT Configuration"));
        Assert.That(result.Errors, Is.Empty);
    }

    [Test]
    public async Task UpdateFrontendAuthAsync_WithMissingFrontendDirectory_ReturnsError()
    {
        // Act
        var result = await _service.UpdateFrontendAuthAsync();

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Errors, Has.Count.EqualTo(1));
        Assert.That(result.Errors[0], Does.Contain("Frontend directory not found"));
    }

    [Test]
    public async Task UpdateFrontendAuthAsync_WithValidFrontendDirectory_ProcessesFiles()
    {
        // Arrange
        var frontendPath = Path.Combine(_testDirectory, "frontend");
        var srcPath = Path.Combine(frontendPath, "src", "app");
        Directory.CreateDirectory(srcPath);

        // Create test routing file
        var routesPath = Path.Combine(srcPath, "app.routes.ts");
        var routesContent = @"
export const routes: Routes = [
    {
        path: 'test',
        component: TestComponent,
        canActivate: [AuthGuard]
    }
];";
        await File.WriteAllTextAsync(routesPath, routesContent);

        // Create guards directory
        var guardsPath = Path.Combine(srcPath, "guards");
        Directory.CreateDirectory(guardsPath);
        var guardFile = Path.Combine(guardsPath, "auth.guard.ts");
        await File.WriteAllTextAsync(guardFile, "// auth guard content");

        // Create interceptors directory
        var interceptorsPath = Path.Combine(srcPath, "interceptors");
        Directory.CreateDirectory(interceptorsPath);
        var interceptorFile = Path.Combine(interceptorsPath, "auth.interceptor.ts");
        await File.WriteAllTextAsync(interceptorFile, "// auth interceptor content");

        // Create services directory
        var servicesPath = Path.Combine(srcPath, "services");
        Directory.CreateDirectory(servicesPath);
        var serviceFile = Path.Combine(servicesPath, "auth.service.ts");
        await File.WriteAllTextAsync(serviceFile, "export class AuthService { }");

        // Act
        var result = await _service.UpdateFrontendAuthAsync();

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.UpdatedRoutes, Contains.Item(routesPath));
        Assert.That(result.RemovedGuards, Contains.Item(guardFile));
        Assert.That(result.RemovedInterceptors, Contains.Item(interceptorFile));

        // Verify files were processed
        var modifiedRoutes = await File.ReadAllTextAsync(routesPath);
        Assert.That(modifiedRoutes, Does.Not.Contain("canActivate: [AuthGuard]"));

        // Verify guard and interceptor files were renamed to .bak
        Assert.That(File.Exists(guardFile + ".bak"), Is.True);
        Assert.That(File.Exists(interceptorFile + ".bak"), Is.True);
    }

    [Test]
    public async Task BackupCurrentAuthConfigAsync_CallsAuthRemovalService()
    {
        // Arrange
        var expectedResult = new BackupResult
        {
            Success = true,
            BackupId = "test_backup_123",
            BackupLocation = "/test/backup/path"
        };

        _mockAuthRemovalService.Setup(x => x.BackupProgramConfigurationAsync())
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _service.BackupCurrentAuthConfigAsync();

        // Assert
        Assert.That(result, Is.EqualTo(expectedResult));
        _mockAuthRemovalService.Verify(x => x.BackupProgramConfigurationAsync(), Times.Once);
    }

    [Test]
    public async Task RollbackAuthenticationAsync_CallsAuthRemovalService()
    {
        // Arrange
        var backupId = "test_backup_123";
        var expectedResult = new RollbackResult
        {
            Success = true,
            BackupId = backupId,
            RestoredFiles = { "Program.cs", "appsettings.json" }
        };

        _mockAuthRemovalService.Setup(x => x.RestoreProgramConfigurationAsync(backupId))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _service.RollbackAuthenticationAsync(backupId);

        // Assert
        Assert.That(result, Is.EqualTo(expectedResult));
        _mockAuthRemovalService.Verify(x => x.RestoreProgramConfigurationAsync(backupId), Times.Once);
    }

    [Test]
    public async Task GetAvailableBackupsAsync_WithNoBackups_ReturnsEmptyList()
    {
        // Act
        var result = await _service.GetAvailableBackupsAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetAvailableBackupsAsync_WithValidBackups_ReturnsBackupInfo()
    {
        // Arrange
        var backupDir = Path.Combine(_testDirectory, "Backups", "Authentication");
        Directory.CreateDirectory(backupDir);

        var backupId = "auth_backup_20231113_143022";
        var backupPath = Path.Combine(backupDir, backupId);
        Directory.CreateDirectory(backupPath);

        var metadata = new
        {
            BackupId = backupId,
            CreatedAt = DateTime.UtcNow,
            Description = "Test backup",
            BackedUpFiles = new[] { "Program.cs", "appsettings.json" },
            TotalSizeBytes = 12345L
        };

        var metadataPath = Path.Combine(backupPath, "backup_metadata.json");
        await File.WriteAllTextAsync(metadataPath, System.Text.Json.JsonSerializer.Serialize(metadata));

        // Act
        var result = await _service.GetAvailableBackupsAsync();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].BackupId, Is.EqualTo(backupId));
        Assert.That(result[0].Description, Is.EqualTo("Test backup"));
        Assert.That(result[0].SizeBytes, Is.EqualTo(12345L));
        Assert.That(result[0].BackedUpFiles, Has.Count.EqualTo(2));
    }

    protected override void RegisterServices(IServiceCollection services)
    {
        base.RegisterServices(services);
        
        // Register migration services
        services.AddScoped<IAuthenticationRemovalService>(provider => _mockAuthRemovalService?.Object ?? Mock.Of<IAuthenticationRemovalService>());
        services.AddScoped<IMigrationLoggingService>(provider => _mockLoggingService?.Object ?? Mock.Of<IMigrationLoggingService>());
    }
}