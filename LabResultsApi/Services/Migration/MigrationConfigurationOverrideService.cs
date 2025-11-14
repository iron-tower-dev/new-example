using LabResultsApi.Models.Migration;
using System.Text.Json;

namespace LabResultsApi.Services.Migration;

public class MigrationConfigurationOverrideService : IMigrationConfigurationOverrideService
{
    private readonly ILogger<MigrationConfigurationOverrideService> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly IMigrationConfigurationService _baseConfigService;

    public MigrationConfigurationOverrideService(
        ILogger<MigrationConfigurationOverrideService> logger,
        IWebHostEnvironment environment,
        IMigrationConfigurationService baseConfigService)
    {
        _logger = logger;
        _environment = environment;
        _baseConfigService = baseConfigService;
    }

    public async Task<MigrationOptions> GetEnvironmentSpecificOptionsAsync()
    {
        var baseOptions = await _baseConfigService.GetDefaultOptionsAsync();
        var environmentName = _environment.EnvironmentName.ToLowerInvariant();
        
        var configFilePath = Path.Combine("Configuration", $"migration-{environmentName}.json");
        
        if (File.Exists(configFilePath))
        {
            _logger.LogInformation("Loading environment-specific migration configuration from {ConfigFile}", configFilePath);
            var environmentOptions = await _baseConfigService.LoadOptionsFromFileAsync(configFilePath);
            return MergeOptions(baseOptions, environmentOptions);
        }
        
        _logger.LogInformation("No environment-specific configuration found for {Environment}, using base configuration", environmentName);
        return baseOptions;
    }

    public async Task<MigrationOptions> ApplyOverridesAsync(MigrationOptions baseOptions, Dictionary<string, object> overrides)
    {
        var mergedOptions = CloneOptions(baseOptions);
        
        foreach (var kvp in overrides)
        {
            await ApplyOverrideAsync(mergedOptions, kvp.Key, kvp.Value);
        }
        
        return mergedOptions;
    }

    public async Task<Dictionary<string, object>> GetAvailableOverridesAsync()
    {
        await Task.CompletedTask;
        
        return new Dictionary<string, object>
        {
            ["ClearExistingData"] = "boolean - Whether to clear existing data before seeding",
            ["CreateMissingTables"] = "boolean - Whether to create missing tables",
            ["ValidateAgainstLegacy"] = "boolean - Whether to validate against legacy database",
            ["RemoveAuthentication"] = "boolean - Whether to remove authentication system",
            ["MaxConcurrentOperations"] = "integer - Maximum number of concurrent operations",
            ["OperationTimeout"] = "timespan - Timeout for operations (format: HH:MM:SS)",
            ["SeedingOptions.BatchSize"] = "integer - Batch size for seeding operations",
            ["SeedingOptions.ContinueOnError"] = "boolean - Whether to continue on errors",
            ["SeedingOptions.CommandTimeout"] = "timespan - Command timeout for seeding",
            ["ValidationOptions.MaxDiscrepanciesToReport"] = "integer - Maximum discrepancies to report",
            ["ValidationOptions.PerformanceThresholdPercent"] = "double - Performance threshold percentage",
            ["ValidationOptions.QueryTimeout"] = "timespan - Query timeout for validation",
            ["LoggingOptions.MinimumLevel"] = "integer - Minimum log level (0=Trace, 1=Debug, 2=Info, 3=Warning, 4=Error, 5=Critical)",
            ["LoggingOptions.IncludeStackTrace"] = "boolean - Whether to include stack traces in logs"
        };
    }

    public async Task<bool> ValidateOverridesAsync(Dictionary<string, object> overrides)
    {
        var availableOverrides = await GetAvailableOverridesAsync();
        var invalidOverrides = new List<string>();
        
        foreach (var kvp in overrides)
        {
            if (!availableOverrides.ContainsKey(kvp.Key))
            {
                invalidOverrides.Add(kvp.Key);
            }
        }
        
        if (invalidOverrides.Any())
        {
            _logger.LogWarning("Invalid override keys found: {InvalidKeys}", string.Join(", ", invalidOverrides));
            return false;
        }
        
        return true;
    }

    private MigrationOptions MergeOptions(MigrationOptions baseOptions, MigrationOptions overrideOptions)
    {
        var merged = CloneOptions(baseOptions);
        
        // Merge top-level properties
        merged.ClearExistingData = overrideOptions.ClearExistingData;
        merged.CreateMissingTables = overrideOptions.CreateMissingTables;
        merged.ValidateAgainstLegacy = overrideOptions.ValidateAgainstLegacy;
        merged.RemoveAuthentication = overrideOptions.RemoveAuthentication;
        merged.MaxConcurrentOperations = overrideOptions.MaxConcurrentOperations;
        merged.OperationTimeout = overrideOptions.OperationTimeout;
        
        if (overrideOptions.IncludeTables.Any())
            merged.IncludeTables = overrideOptions.IncludeTables;
            
        if (overrideOptions.ExcludeTables.Any())
            merged.ExcludeTables = overrideOptions.ExcludeTables;
        
        // Merge seeding options
        MergeSeedingOptions(merged.SeedingOptions, overrideOptions.SeedingOptions);
        
        // Merge validation options
        MergeValidationOptions(merged.ValidationOptions, overrideOptions.ValidationOptions);
        
        // Merge auth removal options
        MergeAuthRemovalOptions(merged.AuthRemovalOptions, overrideOptions.AuthRemovalOptions);
        
        // Merge logging options
        MergeLoggingOptions(merged.LoggingOptions, overrideOptions.LoggingOptions);
        
        return merged;
    }

    private void MergeSeedingOptions(LabResultsApi.Models.Migration.SeedingOptions target, LabResultsApi.Models.Migration.SeedingOptions source)
    {
        target.ClearExistingData = source.ClearExistingData;
        target.CreateMissingTables = source.CreateMissingTables;
        target.BatchSize = source.BatchSize;
        target.ContinueOnError = source.ContinueOnError;
        target.ValidateBeforeInsert = source.ValidateBeforeInsert;
        target.UseTransactions = source.UseTransactions;
        target.CommandTimeout = source.CommandTimeout;
        
        if (!string.IsNullOrEmpty(source.CsvDirectory))
            target.CsvDirectory = source.CsvDirectory;
            
        if (!string.IsNullOrEmpty(source.SqlDirectory))
            target.SqlDirectory = source.SqlDirectory;
    }

    private void MergeValidationOptions(ValidationOptions target, ValidationOptions source)
    {
        target.CompareQueryResults = source.CompareQueryResults;
        target.ComparePerformance = source.ComparePerformance;
        target.GenerateDetailedReports = source.GenerateDetailedReports;
        target.MaxDiscrepanciesToReport = source.MaxDiscrepanciesToReport;
        target.PerformanceThresholdPercent = source.PerformanceThresholdPercent;
        target.QueryTimeout = source.QueryTimeout;
        target.IgnoreMinorDifferences = source.IgnoreMinorDifferences;
        
        if (!string.IsNullOrEmpty(source.LegacyConnectionString))
            target.LegacyConnectionString = source.LegacyConnectionString;
            
        if (source.IncludeQueries.Any())
            target.IncludeQueries = source.IncludeQueries;
            
        if (source.ExcludeQueries.Any())
            target.ExcludeQueries = source.ExcludeQueries;
    }

    private void MergeAuthRemovalOptions(AuthRemovalOptions target, AuthRemovalOptions source)
    {
        target.CreateBackup = source.CreateBackup;
        target.RemoveFromApi = source.RemoveFromApi;
        target.RemoveFromFrontend = source.RemoveFromFrontend;
        target.UpdateDocumentation = source.UpdateDocumentation;
        
        if (!string.IsNullOrEmpty(source.BackupDirectory))
            target.BackupDirectory = source.BackupDirectory;
            
        if (source.FilesToExclude.Any())
            target.FilesToExclude = source.FilesToExclude;
    }

    private void MergeLoggingOptions(LoggingOptions target, LoggingOptions source)
    {
        target.MinimumLevel = source.MinimumLevel;
        target.LogToFile = source.LogToFile;
        target.LogToConsole = source.LogToConsole;
        target.IncludeStackTrace = source.IncludeStackTrace;
        target.MaxLogFileSizeMB = source.MaxLogFileSizeMB;
        target.MaxLogFiles = source.MaxLogFiles;
        
        if (!string.IsNullOrEmpty(source.LogDirectory))
            target.LogDirectory = source.LogDirectory;
    }

    private MigrationOptions CloneOptions(MigrationOptions options)
    {
        var json = JsonSerializer.Serialize(options);
        return JsonSerializer.Deserialize<MigrationOptions>(json) ?? new MigrationOptions();
    }

    private async Task ApplyOverrideAsync(MigrationOptions options, string key, object value)
    {
        await Task.CompletedTask;
        
        var parts = key.Split('.');
        
        try
        {
            switch (parts[0])
            {
                case nameof(MigrationOptions.ClearExistingData):
                    options.ClearExistingData = Convert.ToBoolean(value);
                    break;
                case nameof(MigrationOptions.CreateMissingTables):
                    options.CreateMissingTables = Convert.ToBoolean(value);
                    break;
                case nameof(MigrationOptions.ValidateAgainstLegacy):
                    options.ValidateAgainstLegacy = Convert.ToBoolean(value);
                    break;
                case nameof(MigrationOptions.RemoveAuthentication):
                    options.RemoveAuthentication = Convert.ToBoolean(value);
                    break;
                case nameof(MigrationOptions.MaxConcurrentOperations):
                    options.MaxConcurrentOperations = Convert.ToInt32(value);
                    break;
                case nameof(MigrationOptions.OperationTimeout):
                    options.OperationTimeout = TimeSpan.Parse(value.ToString() ?? "00:30:00");
                    break;
                case "SeedingOptions":
                    ApplySeedingOverride(options.SeedingOptions, parts[1], value);
                    break;
                case "ValidationOptions":
                    ApplyValidationOverride(options.ValidationOptions, parts[1], value);
                    break;
                case "LoggingOptions":
                    ApplyLoggingOverride(options.LoggingOptions, parts[1], value);
                    break;
                default:
                    _logger.LogWarning("Unknown override key: {Key}", key);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying override {Key} with value {Value}", key, value);
            throw;
        }
    }

    private void ApplySeedingOverride(LabResultsApi.Models.Migration.SeedingOptions options, string property, object value)
    {
        switch (property)
        {
            case nameof(SeedingOptions.BatchSize):
                options.BatchSize = Convert.ToInt32(value);
                break;
            case nameof(SeedingOptions.ContinueOnError):
                options.ContinueOnError = Convert.ToBoolean(value);
                break;
            case nameof(LabResultsApi.Models.Migration.SeedingOptions.CommandTimeout):
                options.CommandTimeout = TimeSpan.Parse(value.ToString() ?? "00:05:00");
                break;
            default:
                _logger.LogWarning("Unknown seeding override property: {Property}", property);
                break;
        }
    }

    private void ApplyValidationOverride(ValidationOptions options, string property, object value)
    {
        switch (property)
        {
            case nameof(ValidationOptions.MaxDiscrepanciesToReport):
                options.MaxDiscrepanciesToReport = Convert.ToInt32(value);
                break;
            case nameof(ValidationOptions.PerformanceThresholdPercent):
                options.PerformanceThresholdPercent = Convert.ToDouble(value);
                break;
            case nameof(ValidationOptions.QueryTimeout):
                options.QueryTimeout = TimeSpan.Parse(value.ToString() ?? "00:02:00");
                break;
            default:
                _logger.LogWarning("Unknown validation override property: {Property}", property);
                break;
        }
    }

    private void ApplyLoggingOverride(LoggingOptions options, string property, object value)
    {
        switch (property)
        {
            case nameof(LoggingOptions.MinimumLevel):
                options.MinimumLevel = (Models.Migration.LogLevel)Convert.ToInt32(value);
                break;
            case nameof(LoggingOptions.IncludeStackTrace):
                options.IncludeStackTrace = Convert.ToBoolean(value);
                break;
            default:
                _logger.LogWarning("Unknown logging override property: {Property}", property);
                break;
        }
    }
}