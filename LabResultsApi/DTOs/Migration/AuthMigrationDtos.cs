namespace LabResultsApi.DTOs.Migration;

public class AuthRemovalResult
{
    public bool Success { get; set; }
    public List<string> RemovedComponents { get; set; } = new();
    public List<string> ModifiedFiles { get; set; } = new();
    public List<string> BackupFiles { get; set; } = new();
    public string? BackupLocation { get; set; }
    public List<string> Errors { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class ConfigCleanupResult
{
    public bool Success { get; set; }
    public List<string> CleanedConfigSections { get; set; } = new();
    public List<string> ModifiedFiles { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class FrontendUpdateResult
{
    public bool Success { get; set; }
    public List<string> ModifiedComponents { get; set; } = new();
    public List<string> RemovedGuards { get; set; } = new();
    public List<string> RemovedInterceptors { get; set; } = new();
    public List<string> UpdatedRoutes { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class BackupResult
{
    public bool Success { get; set; }
    public string? BackupId { get; set; }
    public string? BackupLocation { get; set; }
    public List<string> BackedUpFiles { get; set; } = new();
    public long BackupSizeBytes { get; set; }
    public List<string> Errors { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class RollbackResult
{
    public bool Success { get; set; }
    public string? BackupId { get; set; }
    public List<string> RestoredFiles { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class AuthBackupInfo
{
    public string BackupId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string BackupLocation { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public List<string> BackedUpFiles { get; set; } = new();
    public string Description { get; set; } = string.Empty;
}