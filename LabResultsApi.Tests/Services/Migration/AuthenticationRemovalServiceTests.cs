using NUnit.Framework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Moq;
using LabResultsApi.Services.Migration;
using LabResultsApi.DTOs.Migration;
using System.Text.Json;

namespace LabResultsApi.Tests.Services.Migration;

[TestFixture]
public class AuthenticationRemovalServiceTests : TestBase
{
    private IAuthenticationRemovalService _service = null!;
    private Mock<IWebHostEnvironment> _mockEnvironment = null!;
    private Mock<IMigrationLoggingService> _mockLoggingService = null!;
    private string _testDirectory = null!;
    private string _testProgramFile = null!;
    private string _testAppSettingsFile = null!;

    [SetUp]
    public override async Task SetUp()
    {
        await base.SetUp();
        
        // Create test directory
        _testDirectory = Path.Combine(Path.GetTempPath(), $"AuthRemovalTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
        
        _testProgramFile = Path.Combine(_testDirectory, "Program.cs");
        _testAppSettingsFile = Path.Combine(_testDirectory, "appsettings.json");
        
        // Setup mock environment
        _mockEnvironment = new Mock<IWebHostEnvironment>();
        _mockEnvironment.Setup(x => x.ContentRootPath).Returns(_testDirectory);
        
        // Setup mock logging service
        _mockLoggingService = new Mock<IMigrationLoggingService>();
        
        // Create service with mocked dependencies
        var logger = GetService<ILogger<AuthenticationRemovalService>>();
        _service = new AuthenticationRemovalService(logger, _mockLoggingService.Object, _mockEnvironment.Object);
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
    public async Task RemoveJwtMiddlewareAsync_WithValidProgramFile_RemovesJwtConfiguration()
    {
        // Arrange
        var programContent = @"
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(""test-key"")),
            ValidateIssuer = true,
            ValidIssuer = ""TestIssuer"",
            ValidateAudience = true,
            ValidAudience = ""TestAudience"",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
";
        await File.WriteAllTextAsync(_testProgramFile, programContent);

        // Act
        var result = await _service.RemoveJwtMiddlewareAsync();

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.ModifiedFiles, Contains.Item(_testProgramFile));
        Assert.That(result.RemovedComponents, Contains.Item("JWT Authentication Configuration"));
        Assert.That(result.RemovedComponents, Contains.Item("Authentication Middleware"));
        Assert.That(result.RemovedComponents, Contains.Item("Authorization Middleware"));

        var modifiedContent = await File.ReadAllTextAsync(_testProgramFile);
        Assert.That(modifiedContent, Does.Not.Contain("AddJwtBearer"));
        Assert.That(modifiedContent, Does.Not.Contain("UseAuthentication"));
        Assert.That(modifiedContent, Does.Not.Contain("UseAuthorization"));
        Assert.That(modifiedContent, Does.Not.Contain("Microsoft.AspNetCore.Authentication.JwtBearer"));
    }

    [Test]
    public async Task RemoveJwtMiddlewareAsync_WithMissingProgramFile_ReturnsError()
    {
        // Act
        var result = await _service.RemoveJwtMiddlewareAsync();

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Errors, Has.Count.EqualTo(1));
        Assert.That(result.Errors[0], Does.Contain("Program.cs not found"));
    }

    [Test]
    public async Task RemoveAuthenticationServicesAsync_WithValidProgramFile_RemovesServiceRegistrations()
    {
        // Arrange
        var programContent = @"
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();

app.MapAuthenticationEndpoints();
";
        await File.WriteAllTextAsync(_testProgramFile, programContent);

        // Act
        var result = await _service.RemoveAuthenticationServicesAsync();

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.ModifiedFiles, Contains.Item(_testProgramFile));
        Assert.That(result.RemovedComponents, Contains.Item("Authentication Service Registration"));
        Assert.That(result.RemovedComponents, Contains.Item("Authorization Service Registration"));
        Assert.That(result.RemovedComponents, Contains.Item("Authentication Endpoints Mapping"));

        var modifiedContent = await File.ReadAllTextAsync(_testProgramFile);
        Assert.That(modifiedContent, Does.Not.Contain("IAuthenticationService"));
        Assert.That(modifiedContent, Does.Not.Contain("IAuthorizationService"));
        Assert.That(modifiedContent, Does.Not.Contain("MapAuthenticationEndpoints"));
    }

    [Test]
    public async Task CleanupAuthDependenciesAsync_WithJwtConfig_RemovesJwtSection()
    {
        // Arrange
        var appSettingsContent = new
        {
            Logging = new { LogLevel = new { Default = "Information" } },
            Jwt = new
            {
                Key = "test-key",
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                ExpirationHours = 8
            },
            ConnectionStrings = new { DefaultConnection = "test-connection" }
        };

        await File.WriteAllTextAsync(_testAppSettingsFile, JsonSerializer.Serialize(appSettingsContent, new JsonSerializerOptions { WriteIndented = true }));

        // Act
        var result = await _service.CleanupAuthDependenciesAsync();

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.ModifiedFiles, Contains.Item(_testAppSettingsFile));
        Assert.That(result.RemovedComponents, Contains.Item($"JWT Configuration from {Path.GetFileName(_testAppSettingsFile)}"));

        var modifiedContent = await File.ReadAllTextAsync(_testAppSettingsFile);
        Assert.That(modifiedContent, Does.Not.Contain("Jwt"));
        Assert.That(modifiedContent, Does.Contain("Logging"));
        Assert.That(modifiedContent, Does.Contain("ConnectionStrings"));
    }

    [Test]
    public async Task BackupProgramConfigurationAsync_CreatesBackupWithMetadata()
    {
        // Arrange
        await File.WriteAllTextAsync(_testProgramFile, "test program content");
        await File.WriteAllTextAsync(_testAppSettingsFile, "{ \"test\": \"config\" }");

        // Act
        var result = await _service.BackupProgramConfigurationAsync();

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.BackupId, Is.Not.Null);
        Assert.That(result.BackupLocation, Is.Not.Null);
        Assert.That(result.BackedUpFiles, Has.Count.GreaterThan(0));

        // Verify backup directory exists
        Assert.That(Directory.Exists(result.BackupLocation), Is.True);

        // Verify metadata file exists
        var metadataPath = Path.Combine(result.BackupLocation!, "backup_metadata.json");
        Assert.That(File.Exists(metadataPath), Is.True);

        // Verify backup files exist
        foreach (var originalFile in result.BackedUpFiles)
        {
            var fileName = Path.GetFileName(originalFile);
            var backupFile = Path.Combine(result.BackupLocation!, fileName);
            Assert.That(File.Exists(backupFile), Is.True);
        }
    }

    [Test]
    public async Task RestoreProgramConfigurationAsync_WithValidBackup_RestoresFiles()
    {
        // Arrange
        var originalContent = "original program content";
        await File.WriteAllTextAsync(_testProgramFile, originalContent);

        // Create backup
        var backupResult = await _service.BackupProgramConfigurationAsync();
        Assert.That(backupResult.Success, Is.True);

        // Modify original file
        await File.WriteAllTextAsync(_testProgramFile, "modified content");

        // Act
        var restoreResult = await _service.RestoreProgramConfigurationAsync(backupResult.BackupId!);

        // Assert
        Assert.That(restoreResult.Success, Is.True);
        Assert.That(restoreResult.RestoredFiles, Contains.Item(_testProgramFile));

        var restoredContent = await File.ReadAllTextAsync(_testProgramFile);
        Assert.That(restoredContent, Is.EqualTo(originalContent));
    }

    [Test]
    public async Task RestoreProgramConfigurationAsync_WithInvalidBackupId_ReturnsError()
    {
        // Act
        var result = await _service.RestoreProgramConfigurationAsync("invalid_backup_id");

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Errors, Has.Count.EqualTo(1));
        Assert.That(result.Errors[0], Does.Contain("Backup directory not found"));
    }

    protected override void RegisterServices(IServiceCollection services)
    {
        base.RegisterServices(services);
        
        // Register migration services
        services.AddScoped<IMigrationLoggingService>(provider => _mockLoggingService?.Object ?? Mock.Of<IMigrationLoggingService>());
    }
}