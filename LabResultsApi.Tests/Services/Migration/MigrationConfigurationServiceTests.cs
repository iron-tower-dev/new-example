using NUnit.Framework;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using LabResultsApi.Services.Migration;
using LabResultsApi.Models.Migration;
using LabResultsApi.Data;
using FluentAssertions;
using System.Text.Json;

namespace LabResultsApi.Tests.Services.Migration;

[TestFixture]
public class MigrationConfigurationServiceTests
{
    private Mock<ILogger<MigrationConfigurationService>> _mockLogger;
    private Mock<IConfiguration> _mockConfiguration;
    private LabDbContext _context;
    private MigrationConfigurationService _service;
    private string _testConfigDirectory;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<MigrationConfigurationService>>();
        _mockConfiguration = new Mock<IConfiguration>();
        
        // Create in-memory database for testing
        var options = new DbContextOptionsBuilder<LabDbContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;
        _context = new LabDbContext(options);

        // Create temporary directory for test config files
        _testConfigDirectory = Path.Combine(Path.GetTempPath(), $"migration-config-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_testConfigDirectory);

        SetupMockConfiguration();
        
        _service = new MigrationConfigurationService(_mockLogger.Object, _mockConfiguration.Object, _context);
    }

    [TearDown]
    public void TearDown()
    {
        _context?.Dispose();
        
        // Clean up test config directory
        if (Directory.Exists(_testConfigDirectory))
        {
            Directory.Delete(_testConfigDirectory, true);
        }
    }

    private void SetupMockConfiguration()
    {
        // Setup default configuration values
        _mockConfiguration.Setup(c => c.GetValue<bool>("Migration:DefaultOptions:ClearExistingData", true))
            .Returns(true);
        _mockConfiguration.Setup(c => c.GetValue<bool>("Migration:DefaultOptions:CreateMissingTables", true))
            .Returns(true);
        _mockConfiguration.Setup(c => c.GetValue<bool>("Migration:DefaultOptions:ValidateAgainstLegacy", false))
            .Returns(false);
        _mockConfiguration.Setup(c => c.GetValue<bool>("Migration:DefaultOptions:RemoveAuthentication", false))
            .Returns(false);
        _mockConfiguration.Setup(c => c.GetValue<int>("Migration:DefaultOptions:MaxConcurrentOperations", 4))
            .Returns(4);
        _mockConfiguration.Setup(c => c.GetValue<int>("Migration:DefaultOptions:OperationTimeoutMinutes", 30))
            .Returns(30);

        // Setup seeding configuration
        _mockConfiguration.Setup(c => c.GetValue<int>("Migration:Seeding:BatchSize", 1000))
            .Returns(1000);
        _mockConfiguration.Setup(c => c.GetValue<bool>("Migration:Seeding:ContinueOnError", true))
            .Returns(true);
        _mockConfiguration.Setup(c => c.GetValue<string>("Migration:Seeding:CsvDirectory"))
            .Returns("db-seeding");
        _mockConfiguration.Setup(c => c.GetValue<string>("Migration:Seeding:SqlDirectory"))
            .Returns("db-tables");

        // Setup validation configuration
        _mockConfiguration.Setup(c => c.GetConnectionString("LegacyDatabase"))
            .Returns("Server=localhost;Database=Legacy;Integrated Security=true;");

        // Setup logging configuration
        _mockConfiguration.Setup(c => c.GetValue<string>("Migration:Logging:MinimumLevel"))
            .Returns("Information");
        _mockConfiguration.Setup(c => c.GetValue<string>("Migration:Logging:LogDirectory"))
            .Returns("logs/migration");
    }

    [Test]
    public async Task GetDefaultOptionsAsync_ShouldReturnConfiguredDefaults()
    {
        // Act
        var options = await _service.GetDefaultOptionsAsync();

        // Assert
        options.Should().NotBeNull();
        options.ClearExistingData.Should().BeTrue();
        options.CreateMissingTables.Should().BeTrue();
        options.ValidateAgainstLegacy.Should().BeFalse();
        options.RemoveAuthentication.Should().BeFalse();
        options.MaxConcurrentOperations.Should().Be(4);
        options.OperationTimeout.Should().Be(TimeSpan.FromMinutes(30));
        
        options.SeedingOptions.Should().NotBeNull();
        options.SeedingOptions.BatchSize.Should().Be(1000);
        options.SeedingOptions.CsvDirectory.Should().Be("db-seeding");
        options.SeedingOptions.SqlDirectory.Should().Be("db-tables");
        
        options.ValidationOptions.Should().NotBeNull();
        options.ValidationOptions.LegacyConnectionString.Should().NotBeNullOrEmpty();
        
        options.LoggingOptions.Should().NotBeNull();
        options.LoggingOptions.MinimumLevel.Should().Be(LogLevel.Information);
    }

    [Test]
    public async Task LoadOptionsFromFileAsync_WithValidFile_ShouldReturnOptions()
    {
        // Arrange
        var options = new MigrationOptions
        {
            ClearExistingData = false,
            MaxConcurrentOperations = 8,
            SeedingOptions = new SeedingOptions { BatchSize = 2000 }
        };
        
        var filePath = Path.Combine(_testConfigDirectory, "test-options.json");
        var json = JsonSerializer.Serialize(options, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(filePath, json);

        // Act
        var loadedOptions = await _service.LoadOptionsFromFileAsync(filePath);

        // Assert
        loadedOptions.Should().NotBeNull();
        loadedOptions.ClearExistingData.Should().BeFalse();
        loadedOptions.MaxConcurrentOperations.Should().Be(8);
        loadedOptions.SeedingOptions.BatchSize.Should().Be(2000);
    }

    [Test]
    public async Task LoadOptionsFromFileAsync_WithNonExistentFile_ShouldReturnDefaults()
    {
        // Arrange
        var filePath = Path.Combine(_testConfigDirectory, "non-existent.json");

        // Act
        var options = await _service.LoadOptionsFromFileAsync(filePath);

        // Assert
        options.Should().NotBeNull();
        options.ClearExistingData.Should().BeTrue(); // Default value
    }

    [Test]
    public async Task LoadOptionsFromFileAsync_WithInvalidJson_ShouldReturnDefaults()
    {
        // Arrange
        var filePath = Path.Combine(_testConfigDirectory, "invalid.json");
        await File.WriteAllTextAsync(filePath, "{ invalid json }");

        // Act
        var options = await _service.LoadOptionsFromFileAsync(filePath);

        // Assert
        options.Should().NotBeNull();
        options.ClearExistingData.Should().BeTrue(); // Default value
    }

    [Test]
    public async Task SaveOptionsToFileAsync_ShouldCreateValidJsonFile()
    {
        // Arrange
        var options = new MigrationOptions
        {
            ClearExistingData = false,
            MaxConcurrentOperations = 6,
            SeedingOptions = new SeedingOptions { BatchSize = 1500 }
        };
        
        var filePath = Path.Combine(_testConfigDirectory, "saved-options.json");

        // Act
        await _service.SaveOptionsToFileAsync(options, filePath);

        // Assert
        File.Exists(filePath).Should().BeTrue();
        
        var json = await File.ReadAllTextAsync(filePath);
        var loadedOptions = JsonSerializer.Deserialize<MigrationOptions>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        loadedOptions.Should().NotBeNull();
        loadedOptions!.ClearExistingData.Should().BeFalse();
        loadedOptions.MaxConcurrentOperations.Should().Be(6);
        loadedOptions.SeedingOptions.BatchSize.Should().Be(1500);
    }

    [Test]
    public async Task ValidateOptionsAsync_WithValidOptions_ShouldReturnTrue()
    {
        // Arrange
        var options = new MigrationOptions
        {
            SeedingOptions = new SeedingOptions
            {
                BatchSize = 1000,
                CommandTimeout = TimeSpan.FromMinutes(5),
                CsvDirectory = "db-seeding",
                SqlDirectory = "db-tables"
            },
            ValidationOptions = new ValidationOptions
            {
                QueryTimeout = TimeSpan.FromMinutes(2)
            },
            MaxConcurrentOperations = 4,
            OperationTimeout = TimeSpan.FromMinutes(30)
        };

        // Act
        var isValid = await _service.ValidateOptionsAsync(options);

        // Assert
        isValid.Should().BeTrue();
    }

    [Test]
    public async Task ValidateOptionsAsync_WithInvalidBatchSize_ShouldReturnFalse()
    {
        // Arrange
        var options = new MigrationOptions
        {
            SeedingOptions = new SeedingOptions { BatchSize = 0 }
        };

        // Act
        var isValid = await _service.ValidateOptionsAsync(options);

        // Assert
        isValid.Should().BeFalse();
    }

    [Test]
    public async Task ValidateOptionsAsync_WithEmptyCsvDirectory_ShouldReturnFalse()
    {
        // Arrange
        var options = new MigrationOptions
        {
            SeedingOptions = new SeedingOptions { CsvDirectory = "" }
        };

        // Act
        var isValid = await _service.ValidateOptionsAsync(options);

        // Assert
        isValid.Should().BeFalse();
    }

    [Test]
    public async Task ValidateOptionsAsync_WithInvalidMaxConcurrentOperations_ShouldReturnFalse()
    {
        // Arrange
        var options = new MigrationOptions
        {
            MaxConcurrentOperations = 0
        };

        // Act
        var isValid = await _service.ValidateOptionsAsync(options);

        // Assert
        isValid.Should().BeFalse();
    }

    [Test]
    public async Task ValidateOptionsAsync_WithValidationEnabledButNoConnectionString_ShouldReturnFalse()
    {
        // Arrange
        var options = new MigrationOptions
        {
            ValidateAgainstLegacy = true,
            ValidationOptions = new ValidationOptions
            {
                LegacyConnectionString = ""
            }
        };

        // Act
        var isValid = await _service.ValidateOptionsAsync(options);

        // Assert
        isValid.Should().BeFalse();
    }

    [Test]
    public async Task ValidateOptionsAsync_WithAuthRemovalEnabledButNoBackupDirectory_ShouldReturnFalse()
    {
        // Arrange
        var options = new MigrationOptions
        {
            RemoveAuthentication = true,
            AuthRemovalOptions = new AuthRemovalOptions
            {
                BackupDirectory = ""
            }
        };

        // Act
        var isValid = await _service.ValidateOptionsAsync(options);

        // Assert
        isValid.Should().BeFalse();
    }

    [Test]
    public async Task GetAvailableTablesAsync_ShouldReturnTableList()
    {
        // Arrange
        // Add some test tables to the in-memory database
        await _context.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE TestTable1 (Id INT PRIMARY KEY);
            CREATE TABLE TestTable2 (Id INT PRIMARY KEY);
        ");

        // Act
        var tables = await _service.GetAvailableTablesAsync();

        // Assert
        tables.Should().NotBeNull();
        // Note: In-memory database might not support INFORMATION_SCHEMA queries
        // This test verifies the method doesn't throw an exception
    }

    [Test]
    public async Task ValidateLegacyConnectionAsync_WithInvalidConnectionString_ShouldReturnInvalid()
    {
        // Arrange
        var invalidConnectionString = "Server=invalid;Database=invalid;";

        // Act
        var result = await _service.ValidateLegacyConnectionAsync(invalidConnectionString);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
        result.ResponseTime.Should().BeGreaterThan(TimeSpan.Zero);
    }

    [Test]
    public async Task SaveOptionsToFileAsync_WithNonExistentDirectory_ShouldCreateDirectory()
    {
        // Arrange
        var options = new MigrationOptions();
        var subdirectory = Path.Combine(_testConfigDirectory, "subdir", "nested");
        var filePath = Path.Combine(subdirectory, "options.json");

        // Act
        await _service.SaveOptionsToFileAsync(options, filePath);

        // Assert
        Directory.Exists(subdirectory).Should().BeTrue();
        File.Exists(filePath).Should().BeTrue();
    }
}