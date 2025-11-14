using NUnit.Framework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using LabResultsApi.Services.Migration;
using LabResultsApi.Models.Migration;
using LabResultsApi.Data;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace LabResultsApi.Tests.Integration;

[TestFixture]
public class MigrationEndToEndTests : TestBase
{
    private IMigrationControlService _migrationService = null!;
    private IMigrationConfigurationService _configService = null!;
    private IMigrationConfigurationValidationService _validationService = null!;
    private ILogger<MigrationEndToEndTests> _logger = null!;

    [SetUp]
    public override async Task SetUp()
    {
        await base.SetUp();
        _migrationService = GetService<IMigrationControlService>();
        _configService = GetService<IMigrationConfigurationService>();
        _validationService = GetService<IMigrationConfigurationValidationService>();
        _logger = GetService<ILogger<MigrationEndToEndTests>>();
    }

    protected override void RegisterServices(IServiceCollection services)
    {
        base.RegisterServices();
        
        // Register migration services
        services.AddScoped<IMigrationControlService, MigrationControlService>();
        services.AddScoped<IMigrationConfigurationService, MigrationConfigurationService>();
        services.AddScoped<IMigrationConfigurationValidationService, MigrationConfigurationValidationService>();
        services.AddScoped<IDatabaseSeedingService, DatabaseSeedingService>();
        services.AddScoped<ISqlValidationService, SqlValidationService>();
        services.AddScoped<ISsoMigrationService, SsoMigrationService>();
        services.AddScoped<IMigrationLoggingService, MigrationLoggingService>();
        services.AddScoped<IMigrationPerformanceService, MigrationPerformanceService>();
        services.AddScoped<IMigrationNotificationService, MigrationNotificationService>();
    }

    [Test]
    public async Task FullMigrationWorkflow_WithDefaultOptions_ShouldCompleteSuccessfully()
    {
        // Arrange
        var options = await _configService.GetDefaultOptionsAsync();
        options.ValidateAgainstLegacy = false; // Skip legacy validation for test
        options.RemoveAuthentication = false; // Skip auth removal for test
        options.SeedingOptions.CsvDirectory = "TestData/csv";
        options.SeedingOptions.SqlDirectory = "TestData/sql";

        // Create test directories and files
        await CreateTestDataStructure();

        // Act
        var result = await _migrationService.ExecuteFullMigrationAsync(options);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Status, Is.EqualTo(MigrationStatus.Completed));
        Assert.That(result.EndTime, Is.Not.Null);
        Assert.That(result.Statistics, Is.Not.Null);
        Assert.That(result.Statistics.TablesProcessed, Is.GreaterThan(0));

        _logger.LogInformation("Full migration completed successfully with {TablesProcessed} tables processed", 
            result.Statistics.TablesProcessed);
    }

    [Test]
    public async Task MigrationWithDifferentConfigurations_ShouldRespectSettings()
    {
        // Arrange - Test with different batch sizes
        var configurations = new[]
        {
            new { BatchSize = 100, MaxConcurrent = 1, Description = "Small batch, single thread" },
            new { BatchSize = 500, MaxConcurrent = 2, Description = "Medium batch, dual thread" },
            new { BatchSize = 1000, MaxConcurrent = 4, Description = "Large batch, quad thread" }
        };

        await CreateTestDataStructure();

        foreach (var config in configurations)
        {
            // Arrange
            var options = await _configService.GetDefaultOptionsAsync();
            options.ValidateAgainstLegacy = false;
            options.RemoveAuthentication = false;
            options.SeedingOptions.BatchSize = config.BatchSize;
            options.MaxConcurrentOperations = config.MaxConcurrent;
            options.SeedingOptions.CsvDirectory = "TestData/csv";
            options.SeedingOptions.SqlDirectory = "TestData/sql";

            // Act
            var result = await _migrationService.ExecuteFullMigrationAsync(options);

            // Assert
            Assert.That(result.Status, Is.EqualTo(MigrationStatus.Completed), 
                $"Migration failed for configuration: {config.Description}");
            Assert.That(result.Statistics.TablesProcessed, Is.GreaterThan(0), 
                $"No tables processed for configuration: {config.Description}");

            _logger.LogInformation("Migration completed for {Description}: {TablesProcessed} tables, {RecordsProcessed} records", 
                config.Description, result.Statistics.TablesProcessed, result.Statistics.RecordsProcessed);

            // Clean up for next iteration
            await CleanupMigrationData();
        }
    }

    [Test]
    public async Task MigrationWithErrorRecovery_ShouldHandleErrorsGracefully()
    {
        // Arrange
        var options = await _configService.GetDefaultOptionsAsync();
        options.ValidateAgainstLegacy = false;
        options.RemoveAuthentication = false;
        options.SeedingOptions.ContinueOnError = true;
        options.SeedingOptions.CsvDirectory = "TestData/csv-with-errors";
        options.SeedingOptions.SqlDirectory = "TestData/sql";

        // Create test data with intentional errors
        await CreateTestDataWithErrors();

        // Act
        var result = await _migrationService.ExecuteFullMigrationAsync(options);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Status, Is.EqualTo(MigrationStatus.CompletedWithErrors));
        Assert.That(result.Errors, Is.Not.Empty);
        Assert.That(result.Statistics.RecordsSkipped, Is.GreaterThan(0));

        _logger.LogInformation("Migration with errors completed: {ErrorCount} errors, {RecordsSkipped} records skipped", 
            result.Errors.Count, result.Statistics.RecordsSkipped);
    }

    [Test]
    public async Task MigrationCancellation_ShouldStopGracefully()
    {
        // Arrange
        var options = await _configService.GetDefaultOptionsAsync();
        options.ValidateAgainstLegacy = false;
        options.RemoveAuthentication = false;
        options.SeedingOptions.BatchSize = 10; // Small batches for easier cancellation
        options.SeedingOptions.CsvDirectory = "TestData/csv-large";
        options.SeedingOptions.SqlDirectory = "TestData/sql";

        await CreateLargeTestDataSet();

        // Act
        var migrationTask = _migrationService.ExecuteFullMigrationAsync(options);
        
        // Wait a bit then cancel
        await Task.Delay(100);
        var cancelResult = await _migrationService.CancelMigrationAsync();

        var result = await migrationTask;

        // Assert
        Assert.That(cancelResult, Is.True);
        Assert.That(result.Status, Is.EqualTo(MigrationStatus.Cancelled));

        _logger.LogInformation("Migration cancelled successfully after processing {TablesProcessed} tables", 
            result.Statistics.TablesProcessed);
    }

    [Test]
    public async Task ConfigurationValidation_WithInvalidSettings_ShouldFailValidation()
    {
        // Arrange
        var invalidOptions = new MigrationOptions
        {
            SeedingOptions = new SeedingOptions
            {
                BatchSize = -1, // Invalid
                CommandTimeout = TimeSpan.Zero, // Invalid
                CsvDirectory = "", // Invalid
                SqlDirectory = "" // Invalid
            },
            ValidationOptions = new ValidationOptions
            {
                MaxDiscrepanciesToReport = -1, // Invalid
                PerformanceThresholdPercent = 150.0, // Invalid
                QueryTimeout = TimeSpan.Zero // Invalid
            },
            MaxConcurrentOperations = 0, // Invalid
            OperationTimeout = TimeSpan.Zero // Invalid
        };

        // Act
        var validationResult = await _validationService.ValidateConfigurationAsync(invalidOptions);

        // Assert
        Assert.That(validationResult.IsValid, Is.False);
        Assert.That(validationResult.Errors, Is.Not.Empty);
        Assert.That(validationResult.Errors.Count, Is.GreaterThanOrEqualTo(7)); // At least 7 validation errors

        _logger.LogInformation("Configuration validation correctly identified {ErrorCount} errors", 
            validationResult.Errors.Count);
    }

    [Test]
    public async Task PrerequisiteChecks_WithValidEnvironment_ShouldPass()
    {
        // Arrange
        var options = await _configService.GetDefaultOptionsAsync();
        options.SeedingOptions.CsvDirectory = "TestData/csv";
        options.SeedingOptions.SqlDirectory = "TestData/sql";
        options.AuthRemovalOptions.BackupDirectory = "TestData/backup";
        options.LoggingOptions.LogDirectory = "TestData/logs";

        await CreateTestDataStructure();

        // Act
        var prerequisiteResult = await _validationService.CheckPrerequisitesAsync(options);

        // Assert
        Assert.That(prerequisiteResult.AllPrerequisitesMet, Is.True);
        Assert.That(prerequisiteResult.PassedChecks, Is.Not.Empty);
        Assert.That(prerequisiteResult.FailedChecks, Is.Empty);

        _logger.LogInformation("Prerequisite checks passed: {PassedCount} checks successful", 
            prerequisiteResult.PassedChecks.Count);
    }

    [Test]
    public async Task EnvironmentCompatibility_ShouldDetectSystemCapabilities()
    {
        // Arrange
        var options = await _configService.GetDefaultOptionsAsync();

        // Act
        var compatibilityResult = await _validationService.CheckEnvironmentCompatibilityAsync(options);

        // Assert
        Assert.That(compatibilityResult, Is.Not.Null);
        Assert.That(compatibilityResult.EnvironmentInfo, Is.Not.Null);
        Assert.That(compatibilityResult.CompatibilityChecks, Is.Not.Empty);
        Assert.That(compatibilityResult.EnvironmentInfo.ProcessorCount, Is.GreaterThan(0));

        _logger.LogInformation("Environment compatibility check completed: {CheckCount} checks performed", 
            compatibilityResult.CompatibilityChecks.Count);
    }

    [Test]
    public async Task MigrationProgress_ShouldProvideRealTimeUpdates()
    {
        // Arrange
        var options = await _configService.GetDefaultOptionsAsync();
        options.ValidateAgainstLegacy = false;
        options.RemoveAuthentication = false;
        options.SeedingOptions.BatchSize = 50; // Smaller batches for more progress updates
        options.SeedingOptions.CsvDirectory = "TestData/csv";
        options.SeedingOptions.SqlDirectory = "TestData/sql";

        await CreateTestDataStructure();

        var progressUpdates = new List<MigrationResult>();

        // Act
        var migrationTask = _migrationService.ExecuteFullMigrationAsync(options);

        // Monitor progress
        while (!migrationTask.IsCompleted)
        {
            var currentMigration = await _migrationService.GetCurrentMigrationAsync();
            if (currentMigration != null)
            {
                progressUpdates.Add(currentMigration);
            }
            await Task.Delay(50);
        }

        var result = await migrationTask;

        // Assert
        Assert.That(result.Status, Is.EqualTo(MigrationStatus.Completed));
        Assert.That(progressUpdates, Is.Not.Empty);
        Assert.That(progressUpdates.Any(p => p.Statistics.ProgressPercentage > 0), Is.True);
        Assert.That(progressUpdates.Any(p => p.Statistics.ProgressPercentage < 100), Is.True);

        _logger.LogInformation("Migration progress tracked: {UpdateCount} progress updates captured", 
            progressUpdates.Count);
    }

    [Test]
    public async Task MigrationHistory_ShouldTrackMultipleMigrations()
    {
        // Arrange
        await CreateTestDataStructure();

        var migrations = new List<MigrationResult>();

        // Act - Run multiple migrations
        for (int i = 0; i < 3; i++)
        {
            var options = await _configService.GetDefaultOptionsAsync();
            options.ValidateAgainstLegacy = false;
            options.RemoveAuthentication = false;
            options.SeedingOptions.CsvDirectory = "TestData/csv";
            options.SeedingOptions.SqlDirectory = "TestData/sql";

            var result = await _migrationService.ExecuteFullMigrationAsync(options);
            migrations.Add(result);

            await CleanupMigrationData();
        }

        // Get migration history
        var history = await _migrationService.GetMigrationHistoryAsync(10);

        // Assert
        Assert.That(history, Is.Not.Empty);
        Assert.That(history.Count, Is.GreaterThanOrEqualTo(3));
        Assert.That(history.All(h => h.Status == MigrationStatus.Completed), Is.True);

        _logger.LogInformation("Migration history tracked: {HistoryCount} migrations in history", 
            history.Count);
    }

    [Test]
    public async Task ConfigurationOverrides_ShouldApplyCorrectly()
    {
        // Arrange
        var baseOptions = await _configService.GetDefaultOptionsAsync();
        var overrides = new Dictionary<string, object>
        {
            ["MaxConcurrentOperations"] = 8,
            ["SeedingOptions.BatchSize"] = 2000,
            ["ValidationOptions.MaxDiscrepanciesToReport"] = 50,
            ["LoggingOptions.MinimumLevel"] = 4 // Error level
        };

        var overrideService = GetService<IMigrationConfigurationOverrideService>();

        // Act
        var overriddenOptions = await overrideService.ApplyOverridesAsync(baseOptions, overrides);

        // Assert
        Assert.That(overriddenOptions.MaxConcurrentOperations, Is.EqualTo(8));
        Assert.That(overriddenOptions.SeedingOptions.BatchSize, Is.EqualTo(2000));
        Assert.That(overriddenOptions.ValidationOptions.MaxDiscrepanciesToReport, Is.EqualTo(50));
        Assert.That(overriddenOptions.LoggingOptions.MinimumLevel, Is.EqualTo(Models.Migration.LogLevel.Error));

        _logger.LogInformation("Configuration overrides applied successfully: {OverrideCount} overrides", 
            overrides.Count);
    }

    [Test]
    public async Task MigrationRollback_ShouldRestorePreviousState()
    {
        // Arrange
        await CreateTestDataStructure();

        // Create initial state
        var initialOptions = await _configService.GetDefaultOptionsAsync();
        initialOptions.ValidateAgainstLegacy = false;
        initialOptions.RemoveAuthentication = false;
        initialOptions.SeedingOptions.CsvDirectory = "TestData/csv";
        initialOptions.SeedingOptions.SqlDirectory = "TestData/sql";

        var initialResult = await _migrationService.ExecuteFullMigrationAsync(initialOptions);
        var initialRecordCount = await GetTotalRecordCount();

        // Perform a second migration that adds more data
        await CreateAdditionalTestData();
        var secondResult = await _migrationService.ExecuteFullMigrationAsync(initialOptions);
        var secondRecordCount = await GetTotalRecordCount();

        // Act - Simulate rollback by restoring to initial state
        await RestoreToInitialState(initialResult);
        var rolledBackRecordCount = await GetTotalRecordCount();

        // Assert
        Assert.That(initialResult.Status, Is.EqualTo(MigrationStatus.Completed));
        Assert.That(secondResult.Status, Is.EqualTo(MigrationStatus.Completed));
        Assert.That(secondRecordCount, Is.GreaterThan(initialRecordCount));
        Assert.That(rolledBackRecordCount, Is.EqualTo(initialRecordCount));

        _logger.LogInformation("Migration rollback successful: {InitialCount} -> {SecondCount} -> {RolledBackCount} records", 
            initialRecordCount, secondRecordCount, rolledBackRecordCount);
    }

    #region Helper Methods

    private async Task CreateTestDataStructure()
    {
        var directories = new[] { "TestData/csv", "TestData/sql", "TestData/backup", "TestData/logs" };
        
        foreach (var dir in directories)
        {
            Directory.CreateDirectory(dir);
        }

        // Create sample CSV files
        await File.WriteAllTextAsync("TestData/csv/TestTable1.csv", 
            "Id,Name,Value\n1,Test1,100\n2,Test2,200\n3,Test3,300");
        
        await File.WriteAllTextAsync("TestData/csv/TestTable2.csv", 
            "Id,Description,Amount\n1,Description1,50.5\n2,Description2,75.25");

        // Create sample SQL files
        await File.WriteAllTextAsync("TestData/sql/TestTable1.sql", 
            "CREATE TABLE TestTable1 (Id INT PRIMARY KEY, Name NVARCHAR(50), Value INT)");
        
        await File.WriteAllTextAsync("TestData/sql/TestTable2.sql", 
            "CREATE TABLE TestTable2 (Id INT PRIMARY KEY, Description NVARCHAR(100), Amount DECIMAL(10,2))");
    }

    private async Task CreateTestDataWithErrors()
    {
        Directory.CreateDirectory("TestData/csv-with-errors");
        Directory.CreateDirectory("TestData/sql");

        // Create CSV with invalid data
        await File.WriteAllTextAsync("TestData/csv-with-errors/TestTable1.csv", 
            "Id,Name,Value\n1,Test1,100\nINVALID,Test2,NotANumber\n3,Test3,300");

        await File.WriteAllTextAsync("TestData/sql/TestTable1.sql", 
            "CREATE TABLE TestTable1 (Id INT PRIMARY KEY, Name NVARCHAR(50), Value INT)");
    }

    private async Task CreateLargeTestDataSet()
    {
        Directory.CreateDirectory("TestData/csv-large");
        Directory.CreateDirectory("TestData/sql");

        var csvContent = "Id,Name,Value\n";
        for (int i = 1; i <= 1000; i++)
        {
            csvContent += $"{i},Test{i},{i * 10}\n";
        }

        await File.WriteAllTextAsync("TestData/csv-large/LargeTable.csv", csvContent);
        await File.WriteAllTextAsync("TestData/sql/LargeTable.sql", 
            "CREATE TABLE LargeTable (Id INT PRIMARY KEY, Name NVARCHAR(50), Value INT)");
    }

    private async Task CreateAdditionalTestData()
    {
        await File.WriteAllTextAsync("TestData/csv/TestTable3.csv", 
            "Id,Category,Score\n1,A,95\n2,B,87\n3,C,92");
        
        await File.WriteAllTextAsync("TestData/sql/TestTable3.sql", 
            "CREATE TABLE TestTable3 (Id INT PRIMARY KEY, Category NVARCHAR(10), Score INT)");
    }

    private async Task CleanupMigrationData()
    {
        // Clear test tables from in-memory database
        var tableNames = new[] { "TestTable1", "TestTable2", "TestTable3", "LargeTable" };
        
        foreach (var tableName in tableNames)
        {
            try
            {
                await Context.Database.ExecuteSqlRawAsync($"DROP TABLE IF EXISTS {tableName}");
            }
            catch
            {
                // Ignore errors for non-existent tables
            }
        }
    }

    private async Task<int> GetTotalRecordCount()
    {
        var count = 0;
        var tableNames = new[] { "TestTable1", "TestTable2", "TestTable3" };
        
        foreach (var tableName in tableNames)
        {
            try
            {
                var tableCount = await Context.Database.SqlQueryRaw<int>($"SELECT COUNT(*) FROM {tableName}").FirstOrDefaultAsync();
                count += tableCount;
            }
            catch
            {
                // Table doesn't exist, skip
            }
        }
        
        return count;
    }

    private async Task RestoreToInitialState(MigrationResult initialResult)
    {
        // Simulate rollback by dropping additional tables
        try
        {
            await Context.Database.ExecuteSqlRawAsync("DROP TABLE IF EXISTS TestTable3");
        }
        catch
        {
            // Ignore errors
        }
    }

    #endregion

    [OneTimeTearDown]
    public override async Task OneTimeTearDown()
    {
        // Clean up test directories
        var testDirectories = new[] { "TestData" };
        
        foreach (var dir in testDirectories)
        {
            if (Directory.Exists(dir))
            {
                try
                {
                    Directory.Delete(dir, true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        await base.OneTimeTearDown();
    }
}