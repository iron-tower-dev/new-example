using LabResultsApi.Models.Migration;

namespace LabResultsApi.Services.Migration;

public interface IMigrationConfigurationOverrideService
{
    /// <summary>
    /// Gets migration options specific to the current environment
    /// </summary>
    Task<MigrationOptions> GetEnvironmentSpecificOptionsAsync();

    /// <summary>
    /// Applies runtime overrides to migration options
    /// </summary>
    Task<MigrationOptions> ApplyOverridesAsync(MigrationOptions baseOptions, Dictionary<string, object> overrides);

    /// <summary>
    /// Gets available override keys and their descriptions
    /// </summary>
    Task<Dictionary<string, object>> GetAvailableOverridesAsync();

    /// <summary>
    /// Validates that override keys are valid
    /// </summary>
    Task<bool> ValidateOverridesAsync(Dictionary<string, object> overrides);
}