using LabResultsApi.Models.Migration;

namespace LabResultsApi.Services.Migration;

public interface IMigrationConfigurationValidationService
{
    /// <summary>
    /// Validates migration configuration for correctness
    /// </summary>
    Task<ConfigurationValidationResult> ValidateConfigurationAsync(MigrationOptions options);

    /// <summary>
    /// Checks system prerequisites for migration
    /// </summary>
    Task<PrerequisiteCheckResult> CheckPrerequisitesAsync(MigrationOptions options);

    /// <summary>
    /// Checks environment compatibility for migration
    /// </summary>
    Task<EnvironmentCompatibilityResult> CheckEnvironmentCompatibilityAsync(MigrationOptions options);
}