using LabResultsApi.Models.Migration;
using Microsoft.Data.SqlClient;

namespace LabResultsApi.Services.Migration;

public class MigrationConfigurationValidationService : IMigrationConfigurationValidationService
{
    private readonly ILogger<MigrationConfigurationValidationService> _logger;

    public MigrationConfigurationValidationService(ILogger<MigrationConfigurationValidationService> logger)
    {
        _logger = logger;
    }

    public async Task<ConfigurationValidationResult> ValidateConfigurationAsync(MigrationOptions options)
    {
        var result = new ConfigurationValidationResult
        {
            IsValid = true,
            ValidationStartTime = DateTime.UtcNow
        };

        var validationTasks = new List<Task>
        {
            ValidateGeneralOptionsAsync(options, result),
            ValidateSeedingOptionsAsync(options.SeedingOptions, result),
            ValidateValidationOptionsAsync(options.ValidationOptions, result),
            ValidateAuthRemovalOptionsAsync(options.AuthRemovalOptions, result),
            ValidateLoggingOptionsAsync(options.LoggingOptions, result)
        };

        await Task.WhenAll(validationTasks);

        result.ValidationEndTime = DateTime.UtcNow;
        result.ValidationDuration = result.ValidationEndTime - result.ValidationStartTime;

        if (result.Errors.Any())
        {
            result.IsValid = false;
            _logger.LogWarning("Configuration validation failed with {ErrorCount} errors", result.Errors.Count);
        }
        else
        {
            _logger.LogInformation("Configuration validation passed successfully");
        }

        return result;
    }

    public async Task<PrerequisiteCheckResult> CheckPrerequisitesAsync(MigrationOptions options)
    {
        var result = new PrerequisiteCheckResult
        {
            CheckStartTime = DateTime.UtcNow
        };

        var checkTasks = new List<Task>
        {
            CheckDirectoriesAsync(options, result),
            CheckDatabaseConnectionAsync(options, result),
            CheckLegacyConnectionAsync(options, result),
            CheckDiskSpaceAsync(options, result),
            CheckPermissionsAsync(options, result)
        };

        await Task.WhenAll(checkTasks);

        result.CheckEndTime = DateTime.UtcNow;
        result.CheckDuration = result.CheckEndTime - result.CheckStartTime;
        result.AllPrerequisitesMet = !result.FailedChecks.Any();

        if (result.AllPrerequisitesMet)
        {
            _logger.LogInformation("All prerequisites met for migration");
        }
        else
        {
            _logger.LogWarning("Prerequisites check failed: {FailedChecks}", 
                string.Join(", ", result.FailedChecks.Select(f => f.CheckName)));
        }

        return result;
    }

    public async Task<EnvironmentCompatibilityResult> CheckEnvironmentCompatibilityAsync(MigrationOptions options)
    {
        var result = new EnvironmentCompatibilityResult
        {
            CheckStartTime = DateTime.UtcNow
        };

        await Task.WhenAll(
            CheckOperatingSystemAsync(result),
            CheckDotNetVersionAsync(result),
            CheckSqlServerVersionAsync(options, result),
            CheckAvailableMemoryAsync(result),
            CheckAvailableCpuAsync(result)
        );

        result.CheckEndTime = DateTime.UtcNow;
        result.IsCompatible = !result.CompatibilityIssues.Any();

        return result;
    }

    private async Task ValidateGeneralOptionsAsync(MigrationOptions options, ConfigurationValidationResult result)
    {
        await Task.CompletedTask;

        if (options.MaxConcurrentOperations <= 0)
        {
            result.Errors.Add(new ConfigurationError
            {
                Category = "General",
                Property = nameof(options.MaxConcurrentOperations),
                Message = "Max concurrent operations must be greater than 0",
                Severity = ErrorSeverity.Error
            });
        }

        if (options.MaxConcurrentOperations > Environment.ProcessorCount * 2)
        {
            result.Warnings.Add(new ConfigurationError
            {
                Category = "General",
                Property = nameof(options.MaxConcurrentOperations),
                Message = $"Max concurrent operations ({options.MaxConcurrentOperations}) exceeds recommended limit ({Environment.ProcessorCount * 2})",
                Severity = ErrorSeverity.Warning
            });
        }

        if (options.OperationTimeout <= TimeSpan.Zero)
        {
            result.Errors.Add(new ConfigurationError
            {
                Category = "General",
                Property = nameof(options.OperationTimeout),
                Message = "Operation timeout must be greater than 0",
                Severity = ErrorSeverity.Error
            });
        }
    }

    private async Task ValidateSeedingOptionsAsync(LabResultsApi.Models.Migration.SeedingOptions options, ConfigurationValidationResult result)
    {
        await Task.CompletedTask;

        if (options.BatchSize <= 0)
        {
            result.Errors.Add(new ConfigurationError
            {
                Category = "Seeding",
                Property = nameof(options.BatchSize),
                Message = "Batch size must be greater than 0",
                Severity = ErrorSeverity.Error
            });
        }

        if (options.BatchSize > 10000)
        {
            result.Warnings.Add(new ConfigurationError
            {
                Category = "Seeding",
                Property = nameof(options.BatchSize),
                Message = $"Large batch size ({options.BatchSize}) may cause memory issues",
                Severity = ErrorSeverity.Warning
            });
        }

        if (string.IsNullOrWhiteSpace(options.CsvDirectory))
        {
            result.Errors.Add(new ConfigurationError
            {
                Category = "Seeding",
                Property = nameof(options.CsvDirectory),
                Message = "CSV directory cannot be empty",
                Severity = ErrorSeverity.Error
            });
        }

        if (string.IsNullOrWhiteSpace(options.SqlDirectory))
        {
            result.Errors.Add(new ConfigurationError
            {
                Category = "Seeding",
                Property = nameof(options.SqlDirectory),
                Message = "SQL directory cannot be empty",
                Severity = ErrorSeverity.Error
            });
        }

        if (options.CommandTimeout <= TimeSpan.Zero)
        {
            result.Errors.Add(new ConfigurationError
            {
                Category = "Seeding",
                Property = nameof(options.CommandTimeout),
                Message = "Command timeout must be greater than 0",
                Severity = ErrorSeverity.Error
            });
        }
    }

    private async Task ValidateValidationOptionsAsync(ValidationOptions options, ConfigurationValidationResult result)
    {
        await Task.CompletedTask;

        if (options.MaxDiscrepanciesToReport <= 0)
        {
            result.Errors.Add(new ConfigurationError
            {
                Category = "Validation",
                Property = nameof(options.MaxDiscrepanciesToReport),
                Message = "Max discrepancies to report must be greater than 0",
                Severity = ErrorSeverity.Error
            });
        }

        if (options.PerformanceThresholdPercent < 0 || options.PerformanceThresholdPercent > 100)
        {
            result.Errors.Add(new ConfigurationError
            {
                Category = "Validation",
                Property = nameof(options.PerformanceThresholdPercent),
                Message = "Performance threshold percent must be between 0 and 100",
                Severity = ErrorSeverity.Error
            });
        }

        if (options.QueryTimeout <= TimeSpan.Zero)
        {
            result.Errors.Add(new ConfigurationError
            {
                Category = "Validation",
                Property = nameof(options.QueryTimeout),
                Message = "Query timeout must be greater than 0",
                Severity = ErrorSeverity.Error
            });
        }
    }

    private async Task ValidateAuthRemovalOptionsAsync(AuthRemovalOptions options, ConfigurationValidationResult result)
    {
        await Task.CompletedTask;

        if (string.IsNullOrWhiteSpace(options.BackupDirectory))
        {
            result.Errors.Add(new ConfigurationError
            {
                Category = "AuthRemoval",
                Property = nameof(options.BackupDirectory),
                Message = "Backup directory cannot be empty",
                Severity = ErrorSeverity.Error
            });
        }
    }

    private async Task ValidateLoggingOptionsAsync(LoggingOptions options, ConfigurationValidationResult result)
    {
        await Task.CompletedTask;

        if (string.IsNullOrWhiteSpace(options.LogDirectory))
        {
            result.Errors.Add(new ConfigurationError
            {
                Category = "Logging",
                Property = nameof(options.LogDirectory),
                Message = "Log directory cannot be empty",
                Severity = ErrorSeverity.Error
            });
        }

        if (options.MaxLogFileSizeMB <= 0)
        {
            result.Errors.Add(new ConfigurationError
            {
                Category = "Logging",
                Property = nameof(options.MaxLogFileSizeMB),
                Message = "Max log file size must be greater than 0",
                Severity = ErrorSeverity.Error
            });
        }

        if (options.MaxLogFiles <= 0)
        {
            result.Errors.Add(new ConfigurationError
            {
                Category = "Logging",
                Property = nameof(options.MaxLogFiles),
                Message = "Max log files must be greater than 0",
                Severity = ErrorSeverity.Error
            });
        }
    }

    private async Task CheckDirectoriesAsync(MigrationOptions options, PrerequisiteCheckResult result)
    {
        await Task.CompletedTask;

        var directoriesToCheck = new[]
        {
            options.SeedingOptions.CsvDirectory,
            options.SeedingOptions.SqlDirectory,
            options.AuthRemovalOptions.BackupDirectory,
            options.LoggingOptions.LogDirectory
        };

        foreach (var directory in directoriesToCheck.Where(d => !string.IsNullOrWhiteSpace(d)))
        {
            if (!Directory.Exists(directory))
            {
                result.FailedChecks.Add(new PrerequisiteCheck
                {
                    CheckName = "Directory Existence",
                    Description = $"Directory '{directory}' does not exist",
                    IsCritical = true,
                    CheckResult = false
                });
            }
            else
            {
                result.PassedChecks.Add(new PrerequisiteCheck
                {
                    CheckName = "Directory Existence",
                    Description = $"Directory '{directory}' exists",
                    IsCritical = true,
                    CheckResult = true
                });
            }
        }
    }

    private async Task CheckDatabaseConnectionAsync(MigrationOptions options, PrerequisiteCheckResult result)
    {
        // This would require access to the connection string from configuration
        // For now, we'll add a placeholder check
        await Task.CompletedTask;

        result.PassedChecks.Add(new PrerequisiteCheck
        {
            CheckName = "Database Connection",
            Description = "Database connection check (placeholder)",
            IsCritical = true,
            CheckResult = true
        });
    }

    private async Task CheckLegacyConnectionAsync(MigrationOptions options, PrerequisiteCheckResult result)
    {
        if (!options.ValidateAgainstLegacy)
        {
            result.SkippedChecks.Add(new PrerequisiteCheck
            {
                CheckName = "Legacy Database Connection",
                Description = "Legacy validation disabled, skipping connection check",
                IsCritical = false,
                CheckResult = true
            });
            return;
        }

        if (string.IsNullOrWhiteSpace(options.ValidationOptions.LegacyConnectionString))
        {
            result.FailedChecks.Add(new PrerequisiteCheck
            {
                CheckName = "Legacy Database Connection",
                Description = "Legacy connection string is required when validation is enabled",
                IsCritical = true,
                CheckResult = false
            });
            return;
        }

        try
        {
            using var connection = new SqlConnection(options.ValidationOptions.LegacyConnectionString);
            await connection.OpenAsync();
            
            result.PassedChecks.Add(new PrerequisiteCheck
            {
                CheckName = "Legacy Database Connection",
                Description = "Legacy database connection successful",
                IsCritical = true,
                CheckResult = true
            });
        }
        catch (Exception ex)
        {
            result.FailedChecks.Add(new PrerequisiteCheck
            {
                CheckName = "Legacy Database Connection",
                Description = $"Legacy database connection failed: {ex.Message}",
                IsCritical = true,
                CheckResult = false
            });
        }
    }

    private async Task CheckDiskSpaceAsync(MigrationOptions options, PrerequisiteCheckResult result)
    {
        await Task.CompletedTask;

        try
        {
            var drive = new DriveInfo(Path.GetPathRoot(Environment.CurrentDirectory) ?? "C:");
            var availableSpaceGB = drive.AvailableFreeSpace / (1024 * 1024 * 1024);
            
            if (availableSpaceGB < 1) // Less than 1GB
            {
                result.FailedChecks.Add(new PrerequisiteCheck
                {
                    CheckName = "Disk Space",
                    Description = $"Low disk space: {availableSpaceGB:F1}GB available",
                    IsCritical = true,
                    CheckResult = false
                });
            }
            else
            {
                result.PassedChecks.Add(new PrerequisiteCheck
                {
                    CheckName = "Disk Space",
                    Description = $"Sufficient disk space: {availableSpaceGB:F1}GB available",
                    IsCritical = false,
                    CheckResult = true
                });
            }
        }
        catch (Exception ex)
        {
            result.FailedChecks.Add(new PrerequisiteCheck
            {
                CheckName = "Disk Space",
                Description = $"Unable to check disk space: {ex.Message}",
                IsCritical = false,
                CheckResult = false
            });
        }
    }

    private async Task CheckPermissionsAsync(MigrationOptions options, PrerequisiteCheckResult result)
    {
        await Task.CompletedTask;

        var directoriesToCheck = new[]
        {
            options.AuthRemovalOptions.BackupDirectory,
            options.LoggingOptions.LogDirectory
        };

        foreach (var directory in directoriesToCheck.Where(d => !string.IsNullOrWhiteSpace(d)))
        {
            try
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var testFile = Path.Combine(directory, $"test_{Guid.NewGuid()}.tmp");
                await File.WriteAllTextAsync(testFile, "test");
                File.Delete(testFile);

                result.PassedChecks.Add(new PrerequisiteCheck
                {
                    CheckName = "Directory Permissions",
                    Description = $"Write permissions verified for '{directory}'",
                    IsCritical = true,
                    CheckResult = true
                });
            }
            catch (Exception ex)
            {
                result.FailedChecks.Add(new PrerequisiteCheck
                {
                    CheckName = "Directory Permissions",
                    Description = $"No write permissions for '{directory}': {ex.Message}",
                    IsCritical = true,
                    CheckResult = false
                });
            }
        }
    }

    private async Task CheckOperatingSystemAsync(EnvironmentCompatibilityResult result)
    {
        await Task.CompletedTask;

        result.EnvironmentInfo.OperatingSystem = Environment.OSVersion.ToString();
        result.EnvironmentInfo.ProcessorCount = Environment.ProcessorCount;
        
        // Add OS-specific compatibility checks if needed
        result.CompatibilityChecks.Add(new CompatibilityCheck
        {
            CheckName = "Operating System",
            Description = $"Running on {Environment.OSVersion}",
            IsCompatible = true,
            Recommendation = "No issues detected"
        });
    }

    private async Task CheckDotNetVersionAsync(EnvironmentCompatibilityResult result)
    {
        await Task.CompletedTask;

        var version = Environment.Version;
        result.EnvironmentInfo.DotNetVersion = version.ToString();
        
        result.CompatibilityChecks.Add(new CompatibilityCheck
        {
            CheckName = ".NET Version",
            Description = $".NET version {version}",
            IsCompatible = version.Major >= 6,
            Recommendation = version.Major < 6 ? "Upgrade to .NET 6 or later" : "Version is compatible"
        });
    }

    private async Task CheckSqlServerVersionAsync(MigrationOptions options, EnvironmentCompatibilityResult result)
    {
        await Task.CompletedTask;

        // Placeholder for SQL Server version check
        result.CompatibilityChecks.Add(new CompatibilityCheck
        {
            CheckName = "SQL Server Version",
            Description = "SQL Server version check (placeholder)",
            IsCompatible = true,
            Recommendation = "Version compatibility not verified"
        });
    }

    private async Task CheckAvailableMemoryAsync(EnvironmentCompatibilityResult result)
    {
        await Task.CompletedTask;

        var workingSet = Environment.WorkingSet / (1024 * 1024); // MB
        result.EnvironmentInfo.AvailableMemoryMB = workingSet;
        
        result.CompatibilityChecks.Add(new CompatibilityCheck
        {
            CheckName = "Available Memory",
            Description = $"Working set: {workingSet}MB",
            IsCompatible = workingSet > 100,
            Recommendation = workingSet <= 100 ? "Consider increasing available memory" : "Memory is sufficient"
        });
    }

    private async Task CheckAvailableCpuAsync(EnvironmentCompatibilityResult result)
    {
        await Task.CompletedTask;

        var processorCount = Environment.ProcessorCount;
        result.EnvironmentInfo.ProcessorCount = processorCount;
        
        result.CompatibilityChecks.Add(new CompatibilityCheck
        {
            CheckName = "CPU Cores",
            Description = $"Processor count: {processorCount}",
            IsCompatible = processorCount >= 2,
            Recommendation = processorCount < 2 ? "Consider running on a system with more CPU cores" : "CPU is sufficient"
        });
    }
}