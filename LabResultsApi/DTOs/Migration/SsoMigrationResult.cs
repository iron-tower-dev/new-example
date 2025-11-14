namespace LabResultsApi.DTOs.Migration;

public class SsoMigrationResult
{
    public bool Success { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public BackupResult? BackupResult { get; set; }
    public AuthRemovalResult? AuthRemovalResult { get; set; }
    public ConfigCleanupResult? ConfigCleanupResult { get; set; }
    public FrontendUpdateResult? FrontendUpdateResult { get; set; }
    public List<string> Errors { get; set; } = new();
    public TimeSpan Duration => EndTime?.Subtract(StartTime) ?? TimeSpan.Zero;
}