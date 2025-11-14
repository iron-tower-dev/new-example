using LabResultsApi.Models.Migration;
using LabResultsApi.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using ValidationOptions = LabResultsApi.Models.Migration.ValidationOptions;

namespace LabResultsApi.Services.Migration;

public class MigrationConfigurationService : IMigrationConfigurationService
{
    private readonly ILogger<MigrationConfigurationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly LabDbContext _dbContext;

    public MigrationConfigurationService(
        ILogger<MigrationConfigurationService> logger,
        IConfiguration configuration,
        LabDbContext dbContext)
    {
        _logger = logger;
        _configuration = configuration;
        _dbContext = dbContext;
    }

    public async Task<MigrationOptions> GetDefaultOptionsAsync()
    {
        await Task.CompletedTask;
        
        return new MigrationOptions
        {
            ClearExistingData = _configuration.GetValue<bool>("Migration:DefaultOptions:ClearExistingData", true),
            CreateMissingTables = _configuration.GetValue<bool>("Migration:DefaultOptions:CreateMissingTables", true),
            ValidateAgainstLegacy = _configuration.GetValue<bool>("Migration:DefaultOptions:ValidateAgainstLegacy", false),
            RemoveAuthentication = _configuration.GetValue<bool>("Migration:DefaultOptions:RemoveAuthentication", false),
            MaxConcurrentOperations = _configuration.GetValue<int>("Migration:DefaultOptions:MaxConcurrentOperations", 4),
            OperationTimeout = TimeSpan.FromMinutes(_configuration.GetValue<int>("Migration:DefaultOptions:OperationTimeoutMinutes", 30)),
            
            SeedingOptions = new Models.Migration.SeedingOptions
            {
                BatchSize = _configuration.GetValue<int>("Migration:Seeding:BatchSize", 1000),
                ContinueOnError = _configuration.GetValue<bool>("Migration:Seeding:ContinueOnError", true),
                ValidateBeforeInsert = _configuration.GetValue<bool>("Migration:Seeding:ValidateBeforeInsert", true),
                CsvDirectory = _configuration.GetValue<string>("Migration:Seeding:CsvDirectory") ?? "db-seeding",
                SqlDirectory = _configuration.GetValue<string>("Migration:Seeding:SqlDirectory") ?? "db-tables",
                UseTransactions = _configuration.GetValue<bool>("Migration:Seeding:UseTransactions", true),
                CommandTimeout = TimeSpan.FromMinutes(_configuration.GetValue<int>("Migration:Seeding:CommandTimeoutMinutes", 5))
            },
            
            ValidationOptions = new ValidationOptions
            {
                CompareQueryResults = _configuration.GetValue<bool>("Migration:Validation:CompareQueryResults", true),
                ComparePerformance = _configuration.GetValue<bool>("Migration:Validation:ComparePerformance", true),
                GenerateDetailedReports = _configuration.GetValue<bool>("Migration:Validation:GenerateDetailedReports", true),
                MaxDiscrepanciesToReport = _configuration.GetValue<int>("Migration:Validation:MaxDiscrepanciesToReport", 100),
                PerformanceThresholdPercent = _configuration.GetValue<double>("Migration:Validation:PerformanceThresholdPercent", 20.0),
                QueryTimeout = TimeSpan.FromMinutes(_configuration.GetValue<int>("Migration:Validation:QueryTimeoutMinutes", 2)),
                LegacyConnectionString = _configuration.GetConnectionString("LegacyDatabase") ?? string.Empty,
                IgnoreMinorDifferences = _configuration.GetValue<bool>("Migration:Validation:IgnoreMinorDifferences", true)
            },
            
            AuthRemovalOptions = new AuthRemovalOptions
            {
                CreateBackup = _configuration.GetValue<bool>("Migration:AuthRemoval:CreateBackup", true),
                BackupDirectory = _configuration.GetValue<string>("Migration:AuthRemoval:BackupDirectory") ?? "auth-backup",
                RemoveFromApi = _configuration.GetValue<bool>("Migration:AuthRemoval:RemoveFromApi", true),
                RemoveFromFrontend = _configuration.GetValue<bool>("Migration:AuthRemoval:RemoveFromFrontend", true),
                UpdateDocumentation = _configuration.GetValue<bool>("Migration:AuthRemoval:UpdateDocumentation", true)
            },
            
            LoggingOptions = new LoggingOptions
            {
                MinimumLevel = Enum.Parse<Models.Migration.LogLevel>(_configuration.GetValue<string>("Migration:Logging:MinimumLevel") ?? "Information"),
                LogToFile = _configuration.GetValue<bool>("Migration:Logging:LogToFile", true),
                LogToConsole = _configuration.GetValue<bool>("Migration:Logging:LogToConsole", true),
                LogDirectory = _configuration.GetValue<string>("Migration:Logging:LogDirectory") ?? "logs/migration",
                IncludeStackTrace = _configuration.GetValue<bool>("Migration:Logging:IncludeStackTrace", false),
                MaxLogFileSizeMB = _configuration.GetValue<int>("Migration:Logging:MaxLogFileSizeMB", 100),
                MaxLogFiles = _configuration.GetValue<int>("Migration:Logging:MaxLogFiles", 10)
            }
        };
    }

    public async Task<MigrationOptions> LoadOptionsFromFileAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Migration options file not found: {FilePath}. Using default options.", filePath);
                return await GetDefaultOptionsAsync();
            }

            var json = await File.ReadAllTextAsync(filePath);
            var options = JsonSerializer.Deserialize<MigrationOptions>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true
            });

            if (options == null)
            {
                _logger.LogWarning("Failed to deserialize migration options from {FilePath}. Using default options.", filePath);
                return await GetDefaultOptionsAsync();
            }

            _logger.LogInformation("Loaded migration options from {FilePath}", filePath);
            return options;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading migration options from {FilePath}. Using default options.", filePath);
            return await GetDefaultOptionsAsync();
        }
    }

    public async Task SaveOptionsToFileAsync(MigrationOptions options, string filePath)
    {
        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(options, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await File.WriteAllTextAsync(filePath, json);
            _logger.LogInformation("Saved migration options to {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving migration options to {FilePath}", filePath);
            throw;
        }
    }

    public async Task<bool> ValidateOptionsAsync(MigrationOptions options)
    {
        var isValid = true;
        var validationErrors = new List<string>();

        // Validate seeding options
        if (options.SeedingOptions.BatchSize <= 0)
        {
            validationErrors.Add("Seeding batch size must be greater than 0");
            isValid = false;
        }

        if (options.SeedingOptions.CommandTimeout <= TimeSpan.Zero)
        {
            validationErrors.Add("Seeding command timeout must be greater than 0");
            isValid = false;
        }

        if (string.IsNullOrWhiteSpace(options.SeedingOptions.CsvDirectory))
        {
            validationErrors.Add("CSV directory path cannot be empty");
            isValid = false;
        }

        if (string.IsNullOrWhiteSpace(options.SeedingOptions.SqlDirectory))
        {
            validationErrors.Add("SQL directory path cannot be empty");
            isValid = false;
        }

        // Validate validation options
        if (options.ValidateAgainstLegacy)
        {
            if (string.IsNullOrWhiteSpace(options.ValidationOptions.LegacyConnectionString))
            {
                validationErrors.Add("Legacy connection string is required when validation is enabled");
                isValid = false;
            }
            else
            {
                var connectionResult = await ValidateLegacyConnectionAsync(options.ValidationOptions.LegacyConnectionString);
                if (!connectionResult.IsValid)
                {
                    validationErrors.Add($"Legacy connection string is invalid: {connectionResult.Error}");
                    isValid = false;
                }
            }

            if (options.ValidationOptions.QueryTimeout <= TimeSpan.Zero)
            {
                validationErrors.Add("Query timeout must be greater than 0");
                isValid = false;
            }
        }

        // Validate auth removal options
        if (options.RemoveAuthentication)
        {
            if (string.IsNullOrWhiteSpace(options.AuthRemovalOptions.BackupDirectory))
            {
                validationErrors.Add("Backup directory is required when authentication removal is enabled");
                isValid = false;
            }
        }

        // Validate general options
        if (options.MaxConcurrentOperations <= 0)
        {
            validationErrors.Add("Max concurrent operations must be greater than 0");
            isValid = false;
        }

        if (options.OperationTimeout <= TimeSpan.Zero)
        {
            validationErrors.Add("Operation timeout must be greater than 0");
            isValid = false;
        }

        if (!isValid)
        {
            _logger.LogWarning("Migration options validation failed: {Errors}", string.Join(", ", validationErrors));
        }

        await Task.CompletedTask;
        return isValid;
    }

    public async Task<List<string>> GetAvailableTablesAsync()
    {
        try
        {
            var tables = await _dbContext.Database.SqlQueryRaw<string>(
                "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME")
                .ToListAsync();

            return tables;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available tables");
            return new List<string>();
        }
    }

    public async Task<ConnectionStringValidationResult> ValidateLegacyConnectionAsync(string connectionString)
    {
        var result = new ConnectionStringValidationResult();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            
            // Try to get database version
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT @@VERSION";
            command.CommandTimeout = 10; // 10 second timeout for validation
            
            var version = await command.ExecuteScalarAsync();
            result.DatabaseVersion = version?.ToString();
            
            result.IsValid = true;
            _logger.LogInformation("Legacy database connection validated successfully");
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.Error = ex.Message;
            _logger.LogWarning(ex, "Legacy database connection validation failed");
        }
        finally
        {
            stopwatch.Stop();
            result.ResponseTime = stopwatch.Elapsed;
        }

        return result;
    }
}