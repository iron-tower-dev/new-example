namespace LabResultsApi.Models.Migration;

public class AuthRemovalResult
{
    public bool Success { get; set; }
    public List<string> RemovedComponents { get; set; } = new();
    public List<string> ModifiedFiles { get; set; } = new();
    public List<string> BackupFiles { get; set; } = new();
    public string BackupLocation { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class ConfigCleanupResult
{
    public bool Success { get; set; }
    public List<string> CleanedConfigFiles { get; set; } = new();
    public List<string> RemovedSettings { get; set; } = new();
    public string? BackupLocation { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class FrontendUpdateResult
{
    public bool Success { get; set; }
    public List<string> ModifiedComponents { get; set; } = new();
    public List<string> RemovedGuards { get; set; } = new();
    public List<string> RemovedInterceptors { get; set; } = new();
    public List<string> UpdatedRoutes { get; set; } = new();
    public string? BackupLocation { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class BackupResult
{
    public bool Success { get; set; }
    public string BackupLocation { get; set; } = string.Empty;
    public List<string> BackedUpFiles { get; set; } = new();
    public long BackupSizeBytes { get; set; }
    public DateTime BackupTimestamp { get; set; }
    public string? Error { get; set; }
}