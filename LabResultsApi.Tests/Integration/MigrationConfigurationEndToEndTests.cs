using NUnit.Framework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using LabResultsApi.Services.Migration;
using LabResultsApi.Models.Migration;
using System.Text.Json;

namespace LabResultsApi.Tests.Integration;

[TestFixture]
public class MigrationConfigurationEndToEndTests : TestBase
{
    private IMigrationConfigurationService _configService = null!;
    private IMigrationConfigurationOverrideService _overrideService = null!;
    private IMigrationConfigurationValidationService _validationService = null!;
    private ILogger<MigrationConfigurationEndToEndTests> _logger = null!;

    [SetUp]
    public override async Task SetUp()
    {
        await base.SetUp();
        _configService = GetService<IMigrationConfigurationService>();
        _overrideService = GetService<IMigrationConfigurationOverrideService>();
        _validationService = GetService<IMigrationConfigurationValidationService>();
        _logger = GetService<ILogger<MigrationConfigurationEndToEndTests>>();
    }

    protected override void RegisterServices(IServiceCollection services)
    {
        base.RegisterServices();
        
        // Register configuration services
        services.AddScoped<IMigrationConfigurationService, MigrationConfigurationService>();
        services.AddScoped<IMigrationConfigurationOverrideService, MigrationConfigurationOverrideService>();
        services.AddScoped<IMigrationConfigurationValidationService, MigrationConfigurationValidationService>();
    }

    [Test]
    public async Task ConfigurationLifecycle_CreateSaveLoadValidate_ShouldWorkEndToEnd()
    {
        // Arrange
        var testConfigPath = "TestData/test-migration-config.json";
        Directory.CreateDirectory("TestData");

        var originalOptions = new MigrationOptions
        {
            ClearExistingData = true,
            CreateMissingTables = true,
            ValidateAgainstLegacy = false,
            MaxConcurrentOperations = 6,
            OperationTimeout = TimeSpan.FromMinutes(45),
            SeedingOptions = new SeedingOptions
            {
                BatchSize = 1500,
                ContinueOnError = true,
                ValidateBeforeInsert = true,
                CsvDirectory = "test-csv",
                SqlDirectory = "test-sql",
                CommandTimeout = TimeSpan.FromMinutes(8)
            },
            ValidationOptions = new ValidationOptions
            {
                CompareQueryResults = true,
                MaxDiscrepanciesToReport = 75,
                PerformanceThresholdPercent = 18.5,
                QueryTimeout = TimeSpan.FromMinutes(3)
            },
            LoggingOptions = new LoggingOptions
            {
                MinimumLevel = Models.Migration.LogLevel.Warning,
                LogToFile = true,
                LogToConsole = false,
                MaxLogFileSizeMB = 150
            }
        };

        // Act & Assert - Save configuration
        await _configService.SaveOptionsToFileAsync(originalOptions, testConfigPath);
        Assert.That(File.Exists(testConfigPath), Is.True);

        // Act & Assert - Load configuration
        var loadedOptions = await _configService.LoadOptionsFromFileAsync(testConfigPath);
        Assert.That(loadedOptions, Is.Not.Null);
        Assert.That(loadedOptions.MaxConcurrentOperations, Is.EqualTo(6));
        Assert.That(loadedOptions.SeedingOptions.BatchSize, Is.EqualTo(1500));
        Assert.That(loadedOptions.ValidationOptions.PerformanceThresholdPercent, Is.EqualTo(18.5));
        Assert.That(loadedOptions.LoggingOptions.MinimumLevel, Is.EqualTo(Models.Migration.LogLevel.Warning));

        // Act & Assert - Validate configuration
        var validationResult = await _validationService.ValidateConfigurationAsync(loadedOptions);
        Assert.That(validationResult.IsValid, Is.True);
        Assert.That(validationResult.Errors, Is.Empty);

        _logger.LogInformation("Configuration lifecycle test completed successfully");

        // Cleanup
        File.Delete(testConfigPath);
    }

    [Test]
    public async Task EnvironmentSpecificConfigurations_ShouldLoadCorrectSettings()
    {
        // Arrange - Create environment-specific config files
        var environments = new[] { "development", "staging", "production" };
        var configDir = "TestData/Configuration";
        Directory.CreateDirectory(configDir);

        var environmentConfigs = new Dictionary<string, MigrationOptions>
        {
            ["development"] = new MigrationOptions
            {
                MaxConcurrentOperations = 2,
                SeedingOptions = new SeedingOptions { BatchSize = 500 },
                LoggingOptions = new LoggingOptions { MinimumLevel = Models.Migration.LogLevel.Debug }
            },
            ["staging"] = new MigrationOptions
            {
                MaxConcurrentOperations = 4,
                SeedingOptions = new SeedingOptions { BatchSize = 1500 },
                LoggingOptions = new LoggingOptions { MinimumLevel = Models.Migration.LogLevel.Information }
            },
            ["production"] = new MigrationOptions
            {
                MaxConcurrentOperations = 8,
                SeedingOptions = new SeedingOptions { BatchSize = 2000 },
                LoggingOptions = new LoggingOptions { MinimumLevel = Models.Migration.LogLevel.Error }
            }
        };

        // Create config files
        foreach (var env in environments)
        {
            var configPath = Path.Combine(configDir, $"migration-{env}.json");
            await _configService.SaveOptionsToFileAsync(environmentConfigs[env], configPath);
        }

        // Act & Assert - Load each environment configuration
        foreach (var env in environments)
        {
            var configPath = Path.Combine(configDir, $"migration-{env}.json");
            var loadedConfig = await _configService.LoadOptionsFromFileAsync(configPath);
            var expectedConfig = environmentConfigs[env];

            Assert.That(loadedConfig.MaxConcurrentOperations, Is.EqualTo(expectedConfig.MaxConcurrentOperations),
                $"MaxConcurrentOperations mismatch for {env}");
            Assert.That(loadedConfig.SeedingOptions.BatchSize, Is.EqualTo(expectedConfig.SeedingOptions.BatchSize),
                $"BatchSize mismatch for {env}");
            Assert.That(loadedConfig.LoggingOptions.MinimumLevel, Is.EqualTo(expectedConfig.LoggingOptions.MinimumLevel),
                $"MinimumLevel mismatch for {env}");

            _logger.LogInformation("Environment configuration loaded successfully for {Environment}", env);
        }

        // Cleanup
        Directory.Delete(configDir, true);
    }

    [Test]
    public async Task ConfigurationOverrides_ComplexScenarios_ShouldApplyCorrectly()
    {
        // Arrange
        var baseOptions = await _configService.GetDefaultOptionsAsync();
        
        var testScenarios = new[]
        {
            new
            {
                Name = "Performance Optimization",
                Overrides = new Dictionary<string, object>
                {
                    ["MaxConcurrentOperations"] = 16,
                    ["SeedingOptions.BatchSize"] = 5000,
                    ["SeedingOptions.CommandTimeout"] = "00:20:00",
                    ["ValidationOptions.PerformanceThresholdPercent"] = 10.0
                }
            },
            new
            {
                Name = "Debug Configuration",
                Overrides = new Dictionary<string, object>
                {
                    ["MaxConcurrentOperations"] = 1,
                    ["SeedingOptions.BatchSize"] = 100,
                    ["LoggingOptions.MinimumLevel"] = 1, // Debug
                    ["LoggingOptions.IncludeStackTrace"] = true
                }
            },
            new
            {
                Name = "Conservative Settings",
                Overrides = new Dictionary<string, object>
                {
                    ["MaxConcurrentOperations"] = 2,
                    ["SeedingOptions.BatchSize"] = 250,
                    ["SeedingOptions.ContinueOnError"] = false,
                    ["ValidationOptions.MaxDiscrepanciesToReport"] = 25
                }
            }
        };

        foreach (var scenario in testScenarios)
        {
            // Act
            var overriddenOptions = await _overrideService.ApplyOverridesAsync(baseOptions, scenario.Overrides);

            // Assert
            Assert.That(overriddenOptions, Is.Not.Null, $"Failed to apply overrides for {scenario.Name}");
            
            // Validate specific overrides were applied
            if (scenario.Overrides.ContainsKey("MaxConcurrentOperations"))
            {
                Assert.That(overriddenOptions.MaxConcurrentOperations, 
                    Is.EqualTo(scenario.Overrides["MaxConcurrentOperations"]), 
                    $"MaxConcurrentOperations not applied for {scenario.Name}");
            }

            if (scenario.Overrides.ContainsKey("SeedingOptions.BatchSize"))
            {
                Assert.That(overriddenOptions.SeedingOptions.BatchSize, 
                    Is.EqualTo(scenario.Overrides["SeedingOptions.BatchSize"]), 
                    $"BatchSize not applied for {scenario.Name}");
            }

            // Validate configuration is still valid after overrides
            var validationResult = await _validationService.ValidateConfigurationAsync(overriddenOptions);
            Assert.That(validationResult.IsValid, Is.True, 
                $"Configuration invalid after applying overrides for {scenario.Name}");

            _logger.LogInformation("Configuration override scenario '{ScenarioName}' completed successfully", 
                scenario.Name);
        }
    }

    [Test]
    public async Task ConfigurationValidation_ComprehensiveErrorDetection_ShouldCatchAllIssues()
    {
        // Arrange - Create configurations with various types of errors
        var invalidConfigurations = new[]
        {
            new
            {
                Name = "Negative Values",
                Config = new MigrationOptions
                {
                    MaxConcurrentOperations = -1,
                    SeedingOptions = new SeedingOptions { BatchSize = -100 },
                    ValidationOptions = new ValidationOptions { MaxDiscrepanciesToReport = -50 }
                },
                ExpectedErrorCount = 3
            },
            new
            {
                Name = "Zero Timeouts",
                Config = new MigrationOptions
                {
                    OperationTimeout = TimeSpan.Zero,
                    SeedingOptions = new SeedingOptions { CommandTimeout = TimeSpan.Zero },
                    ValidationOptions = new ValidationOptions { QueryTimeout = TimeSpan.Zero }
                },
                ExpectedErrorCount = 3
            },
            new
            {
                Name = "Empty Paths",
                Config = new MigrationOptions
                {
                    SeedingOptions = new SeedingOptions 
                    { 
                        CsvDirectory = "", 
                        SqlDirectory = "" 
                    },
                    AuthRemovalOptions = new AuthRemovalOptions 
                    { 
                        BackupDirectory = "" 
                    },
                    LoggingOptions = new LoggingOptions 
                    { 
                        LogDirectory = "" 
                    }
                },
                ExpectedErrorCount = 4
            },
            new
            {
                Name = "Invalid Ranges",
                Config = new MigrationOptions
                {
                    ValidationOptions = new ValidationOptions 
                    { 
                        PerformanceThresholdPercent = 150.0 // > 100%
                    },
                    LoggingOptions = new LoggingOptions 
                    { 
                        MaxLogFileSizeMB = -10,
                        MaxLogFiles = 0
                    }
                },
                ExpectedErrorCount = 3
            }
        };

        foreach (var testCase in invalidConfigurations)
        {
            // Act
            var validationResult = await _validationService.ValidateConfigurationAsync(testCase.Config);

            // Assert
            Assert.That(validationResult.IsValid, Is.False, 
                $"Configuration should be invalid for {testCase.Name}");
            Assert.That(validationResult.Errors.Count, Is.GreaterThanOrEqualTo(testCase.ExpectedErrorCount), 
                $"Expected at least {testCase.ExpectedErrorCount} errors for {testCase.Name}, got {validationResult.Errors.Count}");

            _logger.LogInformation("Configuration validation correctly detected {ErrorCount} errors for {TestCase}", 
                validationResult.Errors.Count, testCase.Name);
        }
    }

    [Test]
    public async Task PrerequisiteChecks_VariousEnvironmentStates_ShouldDetectIssues()
    {
        // Arrange - Test different prerequisite scenarios
        var testScenarios = new[]
        {
            new
            {
                Name = "Missing Directories",
                Config = new MigrationOptions
                {
                    SeedingOptions = new SeedingOptions 
                    { 
                        CsvDirectory = "NonExistent/csv",
                        SqlDirectory = "NonExistent/sql"
                    },
                    AuthRemovalOptions = new AuthRemovalOptions 
                    { 
                        BackupDirectory = "NonExistent/backup" 
                    },
                    LoggingOptions = new LoggingOptions 
                    { 
                        LogDirectory = "NonExistent/logs" 
                    }
                },
                ExpectedFailures = 4
            },
            new
            {
                Name = "Valid Directories",
                Config = new MigrationOptions
                {
                    SeedingOptions = new SeedingOptions 
                    { 
                        CsvDirectory = "TestData/csv",
                        SqlDirectory = "TestData/sql"
                    },
                    AuthRemovalOptions = new AuthRemovalOptions 
                    { 
                        BackupDirectory = "TestData/backup" 
                    },
                    LoggingOptions = new LoggingOptions 
                    { 
                        LogDirectory = "TestData/logs" 
                    }
                },
                ExpectedFailures = 0
            }
        };

        foreach (var scenario in testScenarios)
        {
            // Setup
            if (scenario.ExpectedFailures == 0)
            {
                // Create directories for valid scenario
                Directory.CreateDirectory("TestData/csv");
                Directory.CreateDirectory("TestData/sql");
                Directory.CreateDirectory("TestData/backup");
                Directory.CreateDirectory("TestData/logs");
            }

            // Act
            var prerequisiteResult = await _validationService.CheckPrerequisitesAsync(scenario.Config);

            // Assert
            if (scenario.ExpectedFailures > 0)
            {
                Assert.That(prerequisiteResult.AllPrerequisitesMet, Is.False, 
                    $"Prerequisites should fail for {scenario.Name}");
                Assert.That(prerequisiteResult.FailedChecks.Count, Is.GreaterThanOrEqualTo(scenario.ExpectedFailures), 
                    $"Expected at least {scenario.ExpectedFailures} failed checks for {scenario.Name}");
            }
            else
            {
                Assert.That(prerequisiteResult.AllPrerequisitesMet, Is.True, 
                    $"Prerequisites should pass for {scenario.Name}");
                Assert.That(prerequisiteResult.FailedChecks, Is.Empty, 
                    $"No failed checks expected for {scenario.Name}");
            }

            _logger.LogInformation("Prerequisite check for '{ScenarioName}': {PassedCount} passed, {FailedCount} failed", 
                scenario.Name, prerequisiteResult.PassedChecks.Count, prerequisiteResult.FailedChecks.Count);
        }

        // Cleanup
        if (Directory.Exists("TestData"))
        {
            Directory.Delete("TestData", true);
        }
    }

    [Test]
    public async Task ConfigurationMerging_MultipleOverrideLayers_ShouldApplyInCorrectOrder()
    {
        // Arrange
        var baseOptions = new MigrationOptions
        {
            MaxConcurrentOperations = 2,
            SeedingOptions = new SeedingOptions { BatchSize = 500 },
            LoggingOptions = new LoggingOptions { MinimumLevel = Models.Migration.LogLevel.Information }
        };

        var firstOverrides = new Dictionary<string, object>
        {
            ["MaxConcurrentOperations"] = 4,
            ["SeedingOptions.BatchSize"] = 1000
        };

        var secondOverrides = new Dictionary<string, object>
        {
            ["MaxConcurrentOperations"] = 8, // Should override first override
            ["LoggingOptions.MinimumLevel"] = 4 // Error level
        };

        // Act
        var firstOverridden = await _overrideService.ApplyOverridesAsync(baseOptions, firstOverrides);
        var finalOverridden = await _overrideService.ApplyOverridesAsync(firstOverridden, secondOverrides);

        // Assert
        Assert.That(finalOverridden.MaxConcurrentOperations, Is.EqualTo(8), 
            "Second override should take precedence");
        Assert.That(finalOverridden.SeedingOptions.BatchSize, Is.EqualTo(1000), 
            "First override should be preserved when not overridden");
        Assert.That(finalOverridden.LoggingOptions.MinimumLevel, Is.EqualTo(Models.Migration.LogLevel.Error), 
            "Second override should be applied");

        _logger.LogInformation("Configuration merging completed: Final MaxConcurrent={MaxConcurrent}, BatchSize={BatchSize}, LogLevel={LogLevel}", 
            finalOverridden.MaxConcurrentOperations, 
            finalOverridden.SeedingOptions.BatchSize, 
            finalOverridden.LoggingOptions.MinimumLevel);
    }

    [Test]
    public async Task ConfigurationSerialization_ComplexObjects_ShouldPreserveAllData()
    {
        // Arrange
        var complexOptions = new MigrationOptions
        {
            ClearExistingData = true,
            CreateMissingTables = false,
            ValidateAgainstLegacy = true,
            RemoveAuthentication = false,
            IncludeTables = new[] { "Table1", "Table2", "Table3" },
            ExcludeTables = new[] { "AuditLog", "SystemLog" },
            MaxConcurrentOperations = 6,
            OperationTimeout = TimeSpan.FromMinutes(45),
            SeedingOptions = new SeedingOptions
            {
                ClearExistingData = false,
                CreateMissingTables = true,
                BatchSize = 2500,
                ContinueOnError = true,
                ValidateBeforeInsert = false,
                CsvDirectory = "production/csv",
                SqlDirectory = "production/sql",
                UseTransactions = true,
                CommandTimeout = TimeSpan.FromMinutes(15)
            },
            ValidationOptions = new ValidationOptions
            {
                CompareQueryResults = true,
                ComparePerformance = false,
                GenerateDetailedReports = true,
                MaxDiscrepanciesToReport = 150,
                PerformanceThresholdPercent = 12.5,
                QueryTimeout = TimeSpan.FromMinutes(8),
                LegacyConnectionString = "Server=legacy;Database=OldDb;Integrated Security=true;",
                IgnoreMinorDifferences = false,
                IncludeQueries = new List<string> { "Query1", "Query2" },
                ExcludeQueries = new List<string> { "SlowQuery", "DebugQuery" }
            },
            AuthRemovalOptions = new AuthRemovalOptions
            {
                CreateBackup = true,
                BackupDirectory = "auth-backups/production",
                RemoveFromApi = true,
                RemoveFromFrontend = false,
                UpdateDocumentation = true,
                FilesToExclude = new List<string> { "config.json", "secrets.json" }
            },
            LoggingOptions = new LoggingOptions
            {
                MinimumLevel = Models.Migration.LogLevel.Warning,
                LogToFile = true,
                LogToConsole = false,
                LogDirectory = "logs/migration/production",
                IncludeStackTrace = false,
                MaxLogFileSizeMB = 250,
                MaxLogFiles = 50
            }
        };

        var testPath = "TestData/complex-config.json";
        Directory.CreateDirectory("TestData");

        // Act
        await _configService.SaveOptionsToFileAsync(complexOptions, testPath);
        var deserializedOptions = await _configService.LoadOptionsFromFileAsync(testPath);

        // Assert - Verify all properties are preserved
        Assert.That(deserializedOptions.ClearExistingData, Is.EqualTo(complexOptions.ClearExistingData));
        Assert.That(deserializedOptions.CreateMissingTables, Is.EqualTo(complexOptions.CreateMissingTables));
        Assert.That(deserializedOptions.ValidateAgainstLegacy, Is.EqualTo(complexOptions.ValidateAgainstLegacy));
        Assert.That(deserializedOptions.RemoveAuthentication, Is.EqualTo(complexOptions.RemoveAuthentication));
        Assert.That(deserializedOptions.IncludeTables, Is.EqualTo(complexOptions.IncludeTables));
        Assert.That(deserializedOptions.ExcludeTables, Is.EqualTo(complexOptions.ExcludeTables));
        Assert.That(deserializedOptions.MaxConcurrentOperations, Is.EqualTo(complexOptions.MaxConcurrentOperations));
        Assert.That(deserializedOptions.OperationTimeout, Is.EqualTo(complexOptions.OperationTimeout));

        // Verify nested objects
        Assert.That(deserializedOptions.SeedingOptions.BatchSize, Is.EqualTo(complexOptions.SeedingOptions.BatchSize));
        Assert.That(deserializedOptions.SeedingOptions.CsvDirectory, Is.EqualTo(complexOptions.SeedingOptions.CsvDirectory));
        Assert.That(deserializedOptions.ValidationOptions.PerformanceThresholdPercent, Is.EqualTo(complexOptions.ValidationOptions.PerformanceThresholdPercent));
        Assert.That(deserializedOptions.ValidationOptions.LegacyConnectionString, Is.EqualTo(complexOptions.ValidationOptions.LegacyConnectionString));
        Assert.That(deserializedOptions.AuthRemovalOptions.BackupDirectory, Is.EqualTo(complexOptions.AuthRemovalOptions.BackupDirectory));
        Assert.That(deserializedOptions.LoggingOptions.MinimumLevel, Is.EqualTo(complexOptions.LoggingOptions.MinimumLevel));

        // Verify collections
        Assert.That(deserializedOptions.ValidationOptions.IncludeQueries, Is.EqualTo(complexOptions.ValidationOptions.IncludeQueries));
        Assert.That(deserializedOptions.ValidationOptions.ExcludeQueries, Is.EqualTo(complexOptions.ValidationOptions.ExcludeQueries));
        Assert.That(deserializedOptions.AuthRemovalOptions.FilesToExclude, Is.EqualTo(complexOptions.AuthRemovalOptions.FilesToExclude));

        _logger.LogInformation("Complex configuration serialization test completed successfully");

        // Cleanup
        File.Delete(testPath);
        Directory.Delete("TestData");
    }

    [OneTimeTearDown]
    public override async Task OneTimeTearDown()
    {
        // Clean up any remaining test files
        if (Directory.Exists("TestData"))
        {
            try
            {
                Directory.Delete("TestData", true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        await base.OneTimeTearDown();
    }
}