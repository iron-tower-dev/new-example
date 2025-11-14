using LabResultsApi.Models.Migration;

namespace LabResultsApi.Services.Migration;

public interface IMigrationConfigurationService
{
    /// <summary>
    /// Gets the default migration options from configuration
    /// </summary>
    Task<MigrationOptions> GetDefaultOptionsAsync();

    /// <summary>
    /// Loads migration options from a JSON file
    /// </summary>
    Task<MigrationOptions> LoadOptionsFromFileAsync(string filePath);

    /// <summary>
    /// Saves migration options to a JSON file
    /// </summary>
    Task SaveOptionsToFileAsync(MigrationOptions options, string filePath);

    /// <summary>
    /// Validates migration options for correctness
    /// </summary>
    Task<bool> ValidateOptionsAsync(MigrationOptions options);

    /// <summary>
    /// Gets list of available tables in the database
    /// </summary>
    Task<List<string>> GetAvailableTablesAsync();

    /// <summary>
    /// Validates legacy database connection string
    /// </summary>
    Task<ConnectionStringValidationResult> ValidateLegacyConnectionAsync(string connectionString);
}

public class ConnectionStringValidationResult
{
    public bool IsValid { get; set; }
    public string Error { get; set; } = string.Empty;
    public string DatabaseVersion { get; set; } = string.Empty;
    public TimeSpan ResponseTime { get; set; }
}