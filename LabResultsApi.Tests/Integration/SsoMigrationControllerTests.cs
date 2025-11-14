using NUnit.Framework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using System.Net.Http.Json;
using System.Net;
using LabResultsApi.DTOs.Migration;
using LabResultsApi.Services.Migration;
using Moq;

namespace LabResultsApi.Tests.Integration;

[TestFixture]
public class SsoMigrationControllerTests : IntegrationTestBase
{
    private Mock<ISsoMigrationService> _mockSsoMigrationService = null!;

    [SetUp]
    public override async Task SetUp()
    {
        await base.SetUp();
        _mockSsoMigrationService = new Mock<ISsoMigrationService>();
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        
        // Replace the SSO migration service with mock
        services.AddScoped<ISsoMigrationService>(provider => _mockSsoMigrationService.Object);
    }

    [Test]
    public async Task CreateBackup_WithSuccessfulService_ReturnsOk()
    {
        // Arrange
        var expectedResult = new BackupResult
        {
            Success = true,
            BackupId = "test_backup_123",
            BackupLocation = "/test/backup/path",
            BackedUpFiles = { "Program.cs", "appsettings.json" },
            BackupSizeBytes = 12345
        };

        _mockSsoMigrationService.Setup(x => x.BackupCurrentAuthConfigAsync())
            .ReturnsAsync(expectedResult);

        // Act
        var response = await Client.PostAsync("/api/SsoMigration/backup", null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var result = await response.Content.ReadFromJsonAsync<BackupResult>();
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Success, Is.True);
        Assert.That(result.BackupId, Is.EqualTo("test_backup_123"));
        Assert.That(result.BackedUpFiles, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task CreateBackup_WithFailedService_ReturnsBadRequest()
    {
        // Arrange
        var expectedResult = new BackupResult
        {
            Success = false,
            Errors = { "Backup creation failed" }
        };

        _mockSsoMigrationService.Setup(x => x.BackupCurrentAuthConfigAsync())
            .ReturnsAsync(expectedResult);

        // Act
        var response = await Client.PostAsync("/api/SsoMigration/backup", null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        
        var result = await response.Content.ReadFromJsonAsync<BackupResult>();
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Errors, Contains.Item("Backup creation failed"));
    }

    [Test]
    public async Task RemoveJwtAuthentication_WithSuccessfulService_ReturnsOk()
    {
        // Arrange
        var expectedResult = new AuthRemovalResult
        {
            Success = true,
            RemovedComponents = { "JWT Middleware", "Auth Services" },
            ModifiedFiles = { "Program.cs" }
        };

        _mockSsoMigrationService.Setup(x => x.RemoveJwtAuthenticationAsync())
            .ReturnsAsync(expectedResult);

        // Act
        var response = await Client.PostAsync("/api/SsoMigration/remove-jwt-auth", null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var result = await response.Content.ReadFromJsonAsync<AuthRemovalResult>();
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Success, Is.True);
        Assert.That(result.RemovedComponents, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task RemoveJwtAuthentication_WithErrors_ReturnsOkWithErrors()
    {
        // Arrange
        var expectedResult = new AuthRemovalResult
        {
            Success = false,
            Errors = { "Some components failed to remove" },
            RemovedComponents = { "JWT Middleware" }, // Partial success
            ModifiedFiles = { "Program.cs" }
        };

        _mockSsoMigrationService.Setup(x => x.RemoveJwtAuthenticationAsync())
            .ReturnsAsync(expectedResult);

        // Act
        var response = await Client.PostAsync("/api/SsoMigration/remove-jwt-auth", null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK)); // Still returns OK for partial success
        
        var result = await response.Content.ReadFromJsonAsync<AuthRemovalResult>();
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Errors, Contains.Item("Some components failed to remove"));
        Assert.That(result.RemovedComponents, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task CleanupConfiguration_WithSuccessfulService_ReturnsOk()
    {
        // Arrange
        var expectedResult = new ConfigCleanupResult
        {
            Success = true,
            CleanedConfigSections = { "JWT Configuration" },
            ModifiedFiles = { "appsettings.json" }
        };

        _mockSsoMigrationService.Setup(x => x.CleanupAuthConfigurationAsync())
            .ReturnsAsync(expectedResult);

        // Act
        var response = await Client.PostAsync("/api/SsoMigration/cleanup-config", null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var result = await response.Content.ReadFromJsonAsync<ConfigCleanupResult>();
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Success, Is.True);
        Assert.That(result.CleanedConfigSections, Contains.Item("JWT Configuration"));
    }

    [Test]
    public async Task UpdateFrontend_WithSuccessfulService_ReturnsOk()
    {
        // Arrange
        var expectedResult = new FrontendUpdateResult
        {
            Success = true,
            ModifiedComponents = { "Angular Routes" },
            RemovedGuards = { "auth.guard.ts" },
            RemovedInterceptors = { "auth.interceptor.ts" }
        };

        _mockSsoMigrationService.Setup(x => x.UpdateFrontendAuthAsync())
            .ReturnsAsync(expectedResult);

        // Act
        var response = await Client.PostAsync("/api/SsoMigration/update-frontend", null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var result = await response.Content.ReadFromJsonAsync<FrontendUpdateResult>();
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Success, Is.True);
        Assert.That(result.RemovedGuards, Contains.Item("auth.guard.ts"));
        Assert.That(result.RemovedInterceptors, Contains.Item("auth.interceptor.ts"));
    }

    [Test]
    public async Task RollbackAuthentication_WithValidBackupId_ReturnsOk()
    {
        // Arrange
        var backupId = "test_backup_123";
        var expectedResult = new RollbackResult
        {
            Success = true,
            BackupId = backupId,
            RestoredFiles = { "Program.cs", "appsettings.json" }
        };

        _mockSsoMigrationService.Setup(x => x.RollbackAuthenticationAsync(backupId))
            .ReturnsAsync(expectedResult);

        // Act
        var response = await Client.PostAsync($"/api/SsoMigration/rollback/{backupId}", null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var result = await response.Content.ReadFromJsonAsync<RollbackResult>();
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Success, Is.True);
        Assert.That(result.BackupId, Is.EqualTo(backupId));
        Assert.That(result.RestoredFiles, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task RollbackAuthentication_WithEmptyBackupId_ReturnsBadRequest()
    {
        // Act
        var response = await Client.PostAsync("/api/SsoMigration/rollback/", null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound)); // Route not found for empty parameter
    }

    [Test]
    public async Task RollbackAuthentication_WithFailedService_ReturnsBadRequest()
    {
        // Arrange
        var backupId = "invalid_backup";
        var expectedResult = new RollbackResult
        {
            Success = false,
            BackupId = backupId,
            Errors = { "Backup not found" }
        };

        _mockSsoMigrationService.Setup(x => x.RollbackAuthenticationAsync(backupId))
            .ReturnsAsync(expectedResult);

        // Act
        var response = await Client.PostAsync($"/api/SsoMigration/rollback/{backupId}", null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        
        var result = await response.Content.ReadFromJsonAsync<RollbackResult>();
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Errors, Contains.Item("Backup not found"));
    }

    [Test]
    public async Task GetAvailableBackups_WithBackups_ReturnsOk()
    {
        // Arrange
        var expectedBackups = new List<AuthBackupInfo>
        {
            new()
            {
                BackupId = "backup_1",
                CreatedAt = DateTime.UtcNow.AddHours(-1),
                BackupLocation = "/path/backup_1",
                SizeBytes = 12345,
                Description = "First backup"
            },
            new()
            {
                BackupId = "backup_2",
                CreatedAt = DateTime.UtcNow.AddHours(-2),
                BackupLocation = "/path/backup_2",
                SizeBytes = 23456,
                Description = "Second backup"
            }
        };

        _mockSsoMigrationService.Setup(x => x.GetAvailableBackupsAsync())
            .ReturnsAsync(expectedBackups);

        // Act
        var response = await Client.GetAsync("/api/SsoMigration/backups");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var result = await response.Content.ReadFromJsonAsync<List<AuthBackupInfo>>();
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[0].BackupId, Is.EqualTo("backup_1"));
        Assert.That(result[1].BackupId, Is.EqualTo("backup_2"));
    }

    [Test]
    public async Task GetAvailableBackups_WithNoBackups_ReturnsEmptyList()
    {
        // Arrange
        _mockSsoMigrationService.Setup(x => x.GetAvailableBackupsAsync())
            .ReturnsAsync(new List<AuthBackupInfo>());

        // Act
        var response = await Client.GetAsync("/api/SsoMigration/backups");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var result = await response.Content.ReadFromJsonAsync<List<AuthBackupInfo>>();
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task MigrateToSso_WithSuccessfulMigration_ReturnsOk()
    {
        // Arrange
        var backupResult = new BackupResult { Success = true, BackupId = "backup_123" };
        var authRemovalResult = new AuthRemovalResult { Success = true };
        var frontendUpdateResult = new FrontendUpdateResult { Success = true };
        var configCleanupResult = new ConfigCleanupResult { Success = true };

        _mockSsoMigrationService.Setup(x => x.BackupCurrentAuthConfigAsync()).ReturnsAsync(backupResult);
        _mockSsoMigrationService.Setup(x => x.RemoveJwtAuthenticationAsync()).ReturnsAsync(authRemovalResult);
        _mockSsoMigrationService.Setup(x => x.UpdateFrontendAuthAsync()).ReturnsAsync(frontendUpdateResult);
        _mockSsoMigrationService.Setup(x => x.CleanupAuthConfigurationAsync()).ReturnsAsync(configCleanupResult);

        // Act
        var response = await Client.PostAsync("/api/SsoMigration/migrate-to-sso", null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        Assert.That(result, Is.Not.Null);

        // Verify all steps were called
        _mockSsoMigrationService.Verify(x => x.BackupCurrentAuthConfigAsync(), Times.Once);
        _mockSsoMigrationService.Verify(x => x.RemoveJwtAuthenticationAsync(), Times.Once);
        _mockSsoMigrationService.Verify(x => x.UpdateFrontendAuthAsync(), Times.Once);
        _mockSsoMigrationService.Verify(x => x.CleanupAuthConfigurationAsync(), Times.Once);
    }

    [Test]
    public async Task MigrateToSso_WithBackupFailure_ReturnsBadRequest()
    {
        // Arrange
        var backupResult = new BackupResult 
        { 
            Success = false, 
            Errors = { "Backup failed" } 
        };

        _mockSsoMigrationService.Setup(x => x.BackupCurrentAuthConfigAsync()).ReturnsAsync(backupResult);

        // Act
        var response = await Client.PostAsync("/api/SsoMigration/migrate-to-sso", null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        // Verify subsequent steps were not called
        _mockSsoMigrationService.Verify(x => x.BackupCurrentAuthConfigAsync(), Times.Once);
        _mockSsoMigrationService.Verify(x => x.RemoveJwtAuthenticationAsync(), Times.Never);
        _mockSsoMigrationService.Verify(x => x.UpdateFrontendAuthAsync(), Times.Never);
        _mockSsoMigrationService.Verify(x => x.CleanupAuthConfigurationAsync(), Times.Never);
    }
}