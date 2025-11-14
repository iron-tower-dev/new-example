using LabResultsApi.DTOs.Migration;

namespace LabResultsApi.Services.Migration;

public interface IAuthenticationRemovalService
{
    Task<AuthRemovalResult> RemoveJwtMiddlewareAsync();
    Task<AuthRemovalResult> RemoveAuthenticationServicesAsync();
    Task<AuthRemovalResult> CleanupAuthDependenciesAsync();
    Task<DTOs.Migration.BackupResult> BackupProgramConfigurationAsync();
    Task<DTOs.Migration.RollbackResult> RestoreProgramConfigurationAsync(string backupId);
}