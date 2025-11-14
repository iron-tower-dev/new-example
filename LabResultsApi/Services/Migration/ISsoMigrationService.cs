using LabResultsApi.DTOs.Migration;

namespace LabResultsApi.Services.Migration;

public interface ISsoMigrationService
{
    Task<AuthRemovalResult> RemoveJwtAuthenticationAsync();
    Task<ConfigCleanupResult> CleanupAuthConfigurationAsync();
    Task<FrontendUpdateResult> UpdateFrontendAuthAsync();
    Task<DTOs.Migration.BackupResult> BackupCurrentAuthConfigAsync();
    Task<DTOs.Migration.RollbackResult> RollbackAuthenticationAsync(string backupId);
    Task<List<AuthBackupInfo>> GetAvailableBackupsAsync();
}