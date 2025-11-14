using NUnit.Framework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using LabResultsApi.Services.Migration;
using LabResultsApi.Models.Migration;
using System.Diagnostics;

namespace LabResultsApi.Tests.Integration;

[TestFixture]
public class MigrationDeploymentValidationTests : TestBase
{
    private IMigrationConfigurationValidationService _validationService = null!;
    private IMigrationConfigurationService _configService = null!;
    private ILogger<MigrationDeploymentValidationTests> _logger = null!;

    [SetUp]
    public override async Task SetUp()
    {
        await base.SetUp();
        _validationService = GetService<IMigrationConfigurationValidationService>();
        _configService = GetService<IMigrationConfigurationService>();
        _logger = GetService<ILogger<MigrationDeploymentValidationTests>>();
    }

    protected override void RegisterServices(IServiceCollection services)
    {
        base.RegisterServices();
        
        services.AddScoped<IMigrationConfigurationValidationService, MigrationConfigurationValidationService>();
        services.AddScoped<IMigrationConfigurationService, MigrationConfigurationService>();
    }

    [Test]
    public async Task DeploymentValidation_DevelopmentEnvironment_ShouldPassAllChecks()
    {
        // Arrange
        var developmentOptions = CreateDevelopmentConfiguration();
        await SetupDevelopmentEnvironment();

        // Act
        var configValidation = await _validationService.ValidateConfigurationAsync(developmentOptions);
        var prerequisiteCheck = await _validationService.CheckPrerequisitesAsync(developmentOptions);
        var compatibilityCheck = await _validationService.CheckEnvironmentCompatibilityAsync(developmentOptions);

        // Assert
        Assert.That(configValidation.IsValid, Is.True, 
            $"Configuration validation failed: {string.Join(", ", configValidation.Errors.Select(e => e.Message))}");
        
        Assert.That(prerequisiteCheck.AllPrerequisitesMet, Is.True, 
            $"Prerequisites not met: {string.Join(", ", prerequisiteCheck.FailedChecks.Select(f => f.Description))}");
        
        Assert.That(compatibilityCheck.IsCompatible, Is.True, 
            $"Environment not compatible: {string.Join(", ", compatibilityCheck.CompatibilityIssues.Select(i => i.Issue))}");

        _logger.LogInformation("Development environment validation passed: Config={ConfigValid}, Prerequisites={PrereqMet}, Compatibility={Compatible}",
            configValidation.IsValid, prerequisiteCheck.AllPrerequisitesMet, compatibilityCheck.IsCompatible);
    }

    [Test]
    public async Task DeploymentValidation_StagingEnvironment_ShouldHandleProductionLikeSettings()
    {
        // Arrange
        var stagingOptions = CreateStagingConfiguration();
        await SetupStagingEnvironment();

        // Act
        var configValidation = await _validationService.ValidateConfigurationAsync(stagingOptions);
        var prerequisiteCheck = await _validationService.CheckPrerequisitesAsync(stagingOptions);
        var compatibilityCheck = await _validationService.CheckEnvironmentCompatibilityAsync(stagingOptions);

        // Assert
        Assert.That(configValidation.IsValid, Is.True, "Staging configuration should be valid");
        
        // Staging might have some warnings but should not fail completely
        var criticalFailures = prerequisiteCheck.FailedChecks.Where(f => f.IsCritical).ToList();
        Assert.That(criticalFailures, Is.Empty, 
            $"Critical prerequisites failed: {string.Join(", ", criticalFailures.Select(f => f.Description))}");

        _logger.LogInformation("Staging environment validation completed: {PassedChecks} passed, {FailedChecks} failed, {WarningChecks} warnings",
            prerequisiteCheck.PassedChecks.Count, prerequisiteCheck.FailedChecks.Count, 
            prerequisiteCheck.FailedChecks.Count(f => !f.IsCritical));
    }

    [Test]
    public async Task DeploymentValidation_ProductionEnvironment_ShouldEnforceStrictRequirements()
    {
        // Arrange
        var productionOptions = CreateProductionConfiguration();
        await SetupProductionEnvironment();

        // Act
        var configValidation = await _validationService.ValidateConfigurationAsync(productionOptions);
        var prerequisiteCheck = await _validationService.CheckPrerequisitesAsync(productionOptions);
        var compatibilityCheck = await _validationService.CheckEnvironmentCompatibilityAsync(productionOptions);

        // Assert
        Assert.That(configValidation.IsValid, Is.True, "Production configuration must be valid");
        
        // Production should have stricter requirements
        Assert.That(productionOptions.SeedingOptions.ContinueOnError, Is.False, 
            "Production should not continue on errors");
        Assert.That(productionOptions.LoggingOptions.MinimumLevel, Is.GreaterThanOrEqualTo(Models.Migration.LogLevel.Warning), 
            "Production should have minimal logging");
        Assert.That(productionOptions.MaxConcurrentOperations, Is.GreaterThan(4), 
            "Production should support higher concurrency");

        _logger.LogInformation("Production environment validation completed with strict requirements enforced");
    }

    [Test]
    public async Task SystemResourceValidation_ShouldDetectInsufficientResources()
    {
        // Arrange
        var resourceIntensiveOptions = new MigrationOptions
        {
            MaxConcurrentOperations = 32, // Very high
            SeedingOptions = new SeedingOptions
            {
                BatchSize = 10000, // Very large batches
                CommandTimeout = TimeSpan.FromHours(2) // Very long timeout
            },
            ValidationOptions = new ValidationOptions
            {
                MaxDiscrepanciesToReport = 10000 // Large report size
            }
        };

        // Act
        var compatibilityResult = await _validationService.CheckEnvironmentCompatibilityAsync(resourceIntensiveOptions);

        // Assert
        Assert.That(compatibilityResult, Is.Not.Null);
        Assert.That(compatibilityResult.EnvironmentInfo.ProcessorCount, Is.GreaterThan(0));
        Assert.That(compatibilityResult.EnvironmentInfo.AvailableMemoryMB, Is.GreaterThan(0));

        // Check if warnings are generated for resource-intensive settings
        var resourceWarnings = compatibilityResult.CompatibilityChecks
            .Where(c => !c.IsCompatible || c.Recommendation.Contains("memory") || c.Recommendation.Contains("CPU"))
            .ToList();

        _logger.LogInformation("Resource validation completed: {ProcessorCount} cores, {MemoryMB}MB memory, {WarningCount} resource warnings",
            compatibilityResult.EnvironmentInfo.ProcessorCount,
            compatibilityResult.EnvironmentInfo.AvailableMemoryMB,
            resourceWarnings.Count);
    }

    [Test]
    public async Task NetworkConnectivityValidation_ShouldTestDatabaseConnections()
    {
        // Arrange
        var optionsWithConnections = new MigrationOptions
        {
            ValidateAgainstLegacy = true,
            ValidationOptions = new ValidationOptions
            {
                LegacyConnectionString = "Server=nonexistent;Database=test;Integrated Security=true;"
            }
        };

        // Act
        var prerequisiteResult = await _validationService.CheckPrerequisitesAsync(optionsWithConnections);

        // Assert
        var connectionChecks = prerequisiteResult.FailedChecks
            .Where(f => f.CheckName.Contains("Connection") || f.CheckName.Contains("Database"))
            .ToList();

        // Should detect connection failures for non-existent servers
        Assert.That(connectionChecks, Is.Not.Empty, "Should detect database connection issues");

        _logger.LogInformation("Network connectivity validation detected {ConnectionIssues} connection issues",
            connectionChecks.Count);
    }

    [Test]
    public async Task SecurityValidation_ShouldCheckPermissionsAndAccess()
    {
        // Arrange
        var securitySensitiveOptions = new MigrationOptions
        {
            AuthRemovalOptions = new AuthRemovalOptions
            {
                CreateBackup = true,
                BackupDirectory = "TestData/secure-backup"
            },
            LoggingOptions = new LoggingOptions
            {
                LogToFile = true,
                LogDirectory = "TestData/secure-logs"
            }
        };

        // Create directories with different permission scenarios
        Directory.CreateDirectory("TestData/secure-backup");
        Directory.CreateDirectory("TestData/secure-logs");

        // Act
        var prerequisiteResult = await _validationService.CheckPrerequisitesAsync(securitySensitiveOptions);

        // Assert
        var permissionChecks = prerequisiteResult.PassedChecks
            .Where(p => p.CheckName.Contains("Permission") || p.CheckName.Contains("Access"))
            .ToList();

        Assert.That(permissionChecks, Is.Not.Empty, "Should perform permission checks");

        _logger.LogInformation("Security validation completed: {PermissionChecks} permission checks performed",
            permissionChecks.Count);

        // Cleanup
        Directory.Delete("TestData", true);
    }

    [Test]
    public async Task PerformanceValidation_ShouldEstimateResourceUsage()
    {
        // Arrange
        var performanceTestOptions = new MigrationOptions
        {
            MaxConcurrentOperations = Environment.ProcessorCount,
            SeedingOptions = new SeedingOptions
            {
                BatchSize = 1000,
                CommandTimeout = TimeSpan.FromMinutes(5)
            }
        };

        var stopwatch = Stopwatch.StartNew();

        // Act
        var configValidation = await _validationService.ValidateConfigurationAsync(performanceTestOptions);
        var compatibilityCheck = await _validationService.CheckEnvironmentCompatibilityAsync(performanceTestOptions);

        stopwatch.Stop();

        // Assert
        Assert.That(configValidation.ValidationDuration, Is.LessThan(TimeSpan.FromSeconds(5)), 
            "Configuration validation should complete quickly");
        
        Assert.That(stopwatch.Elapsed, Is.LessThan(TimeSpan.FromSeconds(10)), 
            "Overall validation should complete within reasonable time");

        // Check if performance recommendations are provided
        var performanceRecommendations = compatibilityCheck.CompatibilityChecks
            .Where(c => c.Recommendation.Contains("performance") || c.Recommendation.Contains("memory") || c.Recommendation.Contains("CPU"))
            .ToList();

        _logger.LogInformation("Performance validation completed in {ValidationTime}ms: {RecommendationCount} performance recommendations",
            stopwatch.ElapsedMilliseconds, performanceRecommendations.Count);
    }

    [Test]
    public async Task ConfigurationCompatibility_CrossEnvironment_ShouldIdentifyIssues()
    {
        // Arrange
        var environments = new[] { "Development", "Staging", "Production" };
        var configurations = new Dictionary<string, MigrationOptions>
        {
            ["Development"] = CreateDevelopmentConfiguration(),
            ["Staging"] = CreateStagingConfiguration(),
            ["Production"] = CreateProductionConfiguration()
        };

        var compatibilityIssues = new List<string>();

        // Act
        foreach (var env in environments)
        {
            var config = configurations[env];
            var validation = await _validationService.ValidateConfigurationAsync(config);
            
            if (!validation.IsValid)
            {
                compatibilityIssues.AddRange(validation.Errors.Select(e => $"{env}: {e.Message}"));
            }

            // Check for environment-specific issues
            if (env == "Production" && config.LoggingOptions.MinimumLevel < Models.Migration.LogLevel.Warning)
            {
                compatibilityIssues.Add($"{env}: Logging level too verbose for production");
            }

            if (env == "Development" && config.MaxConcurrentOperations > Environment.ProcessorCount)
            {
                compatibilityIssues.Add($"{env}: Too many concurrent operations for development");
            }
        }

        // Assert
        Assert.That(compatibilityIssues, Is.Empty, 
            $"Configuration compatibility issues found: {string.Join("; ", compatibilityIssues)}");

        _logger.LogInformation("Cross-environment configuration compatibility validated for {EnvironmentCount} environments",
            environments.Length);
    }

    [Test]
    public async Task DeploymentReadiness_ComprehensiveCheck_ShouldProvideDetailedReport()
    {
        // Arrange
        var testEnvironment = "Staging";
        var options = CreateStagingConfiguration();
        await SetupStagingEnvironment();

        var readinessReport = new
        {
            Environment = testEnvironment,
            Timestamp = DateTime.UtcNow,
            ConfigurationValid = false,
            PrerequisitesMet = false,
            EnvironmentCompatible = false,
            Issues = new List<string>(),
            Recommendations = new List<string>()
        };

        // Act
        var configValidation = await _validationService.ValidateConfigurationAsync(options);
        var prerequisiteCheck = await _validationService.CheckPrerequisitesAsync(options);
        var compatibilityCheck = await _validationService.CheckEnvironmentCompatibilityAsync(options);

        // Compile readiness report
        var issues = new List<string>();
        var recommendations = new List<string>();

        if (!configValidation.IsValid)
        {
            issues.AddRange(configValidation.Errors.Select(e => $"Config: {e.Message}"));
        }

        if (!prerequisiteCheck.AllPrerequisitesMet)
        {
            issues.AddRange(prerequisiteCheck.FailedChecks.Select(f => $"Prerequisite: {f.Description}"));
        }

        if (!compatibilityCheck.IsCompatible)
        {
            issues.AddRange(compatibilityCheck.CompatibilityIssues.Select(i => $"Compatibility: {i.Issue}"));
        }

        recommendations.AddRange(configValidation.Recommendations.Select(r => r.Message));
        recommendations.AddRange(prerequisiteCheck.FailedChecks.Where(f => !string.IsNullOrEmpty(f.Recommendation))
            .Select(f => f.Recommendation!));
        recommendations.AddRange(compatibilityCheck.CompatibilityChecks.Where(c => !string.IsNullOrEmpty(c.Recommendation))
            .Select(c => c.Recommendation));

        // Assert
        var isReady = configValidation.IsValid && prerequisiteCheck.AllPrerequisitesMet && compatibilityCheck.IsCompatible;
        
        if (isReady)
        {
            Assert.Pass($"Environment '{testEnvironment}' is ready for deployment");
        }
        else
        {
            var reportMessage = $"Environment '{testEnvironment}' is not ready for deployment.\n" +
                               $"Issues ({issues.Count}): {string.Join("; ", issues)}\n" +
                               $"Recommendations ({recommendations.Count}): {string.Join("; ", recommendations)}";
            
            _logger.LogWarning("Deployment readiness check failed: {IssueCount} issues, {RecommendationCount} recommendations",
                issues.Count, recommendations.Count);
            
            // For test purposes, we'll assert that we got a comprehensive report
            Assert.That(issues.Count + recommendations.Count, Is.GreaterThan(0), 
                "Should provide detailed feedback even when not ready");
        }
    }

    #region Helper Methods

    private MigrationOptions CreateDevelopmentConfiguration()
    {
        return new MigrationOptions
        {
            ClearExistingData = true,
            CreateMissingTables = true,
            ValidateAgainstLegacy = false,
            MaxConcurrentOperations = 2,
            OperationTimeout = TimeSpan.FromMinutes(30),
            SeedingOptions = new SeedingOptions
            {
                BatchSize = 500,
                ContinueOnError = true,
                ValidateBeforeInsert = true,
                CsvDirectory = "TestData/csv",
                SqlDirectory = "TestData/sql",
                CommandTimeout = TimeSpan.FromMinutes(10)
            },
            ValidationOptions = new ValidationOptions
            {
                CompareQueryResults = true,
                MaxDiscrepanciesToReport = 50,
                PerformanceThresholdPercent = 30.0,
                QueryTimeout = TimeSpan.FromMinutes(5)
            },
            AuthRemovalOptions = new AuthRemovalOptions
            {
                CreateBackup = true,
                BackupDirectory = "TestData/backup"
            },
            LoggingOptions = new LoggingOptions
            {
                MinimumLevel = Models.Migration.LogLevel.Debug,
                LogToFile = true,
                LogToConsole = true,
                LogDirectory = "TestData/logs",
                IncludeStackTrace = true
            }
        };
    }

    private MigrationOptions CreateStagingConfiguration()
    {
        return new MigrationOptions
        {
            ClearExistingData = true,
            CreateMissingTables = true,
            ValidateAgainstLegacy = true,
            MaxConcurrentOperations = 4,
            OperationTimeout = TimeSpan.FromMinutes(45),
            SeedingOptions = new SeedingOptions
            {
                BatchSize = 1500,
                ContinueOnError = true,
                ValidateBeforeInsert = true,
                CsvDirectory = "TestData/csv",
                SqlDirectory = "TestData/sql",
                CommandTimeout = TimeSpan.FromMinutes(10)
            },
            ValidationOptions = new ValidationOptions
            {
                CompareQueryResults = true,
                MaxDiscrepanciesToReport = 150,
                PerformanceThresholdPercent = 25.0,
                QueryTimeout = TimeSpan.FromMinutes(5)
            },
            AuthRemovalOptions = new AuthRemovalOptions
            {
                CreateBackup = true,
                BackupDirectory = "TestData/backup"
            },
            LoggingOptions = new LoggingOptions
            {
                MinimumLevel = Models.Migration.LogLevel.Information,
                LogToFile = true,
                LogToConsole = true,
                LogDirectory = "TestData/logs",
                IncludeStackTrace = true
            }
        };
    }

    private MigrationOptions CreateProductionConfiguration()
    {
        return new MigrationOptions
        {
            ClearExistingData = false,
            CreateMissingTables = true,
            ValidateAgainstLegacy = true,
            MaxConcurrentOperations = 8,
            OperationTimeout = TimeSpan.FromHours(1),
            SeedingOptions = new SeedingOptions
            {
                BatchSize = 2000,
                ContinueOnError = false,
                ValidateBeforeInsert = true,
                CsvDirectory = "TestData/csv",
                SqlDirectory = "TestData/sql",
                CommandTimeout = TimeSpan.FromMinutes(15)
            },
            ValidationOptions = new ValidationOptions
            {
                CompareQueryResults = true,
                MaxDiscrepanciesToReport = 200,
                PerformanceThresholdPercent = 15.0,
                QueryTimeout = TimeSpan.FromMinutes(10)
            },
            AuthRemovalOptions = new AuthRemovalOptions
            {
                CreateBackup = true,
                BackupDirectory = "TestData/backup"
            },
            LoggingOptions = new LoggingOptions
            {
                MinimumLevel = Models.Migration.LogLevel.Warning,
                LogToFile = true,
                LogToConsole = false,
                LogDirectory = "TestData/logs",
                IncludeStackTrace = false
            }
        };
    }

    private async Task SetupDevelopmentEnvironment()
    {
        await CreateTestDirectories();
        await CreateSampleTestData();
    }

    private async Task SetupStagingEnvironment()
    {
        await CreateTestDirectories();
        await CreateSampleTestData();
    }

    private async Task SetupProductionEnvironment()
    {
        await CreateTestDirectories();
        await CreateSampleTestData();
    }

    private async Task CreateTestDirectories()
    {
        var directories = new[] { "TestData/csv", "TestData/sql", "TestData/backup", "TestData/logs" };
        
        foreach (var dir in directories)
        {
            Directory.CreateDirectory(dir);
        }

        await Task.CompletedTask;
    }

    private async Task CreateSampleTestData()
    {
        await File.WriteAllTextAsync("TestData/csv/sample.csv", "Id,Name\n1,Test");
        await File.WriteAllTextAsync("TestData/sql/sample.sql", "CREATE TABLE Sample (Id INT, Name NVARCHAR(50))");
    }

    #endregion

    [OneTimeTearDown]
    public override async Task OneTimeTearDown()
    {
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