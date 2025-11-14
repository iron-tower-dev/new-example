namespace LabResultsApi.Models.Migration;

public class MigrationResult
{
    public Guid MigrationId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public MigrationStatus Status { get; set; }
    public SeedingResult? SeedingResult { get; set; }
    public ValidationResult? ValidationResult { get; set; }
    public AuthRemovalResult? AuthRemovalResult { get; set; }
    public List<MigrationError> Errors { get; set; } = new();
    public MigrationStatistics Statistics { get; set; } = new();
    public string? CurrentOperation { get; set; }
    public TimeSpan EstimatedTimeRemaining { get; set; }
    
    public TimeSpan? Duration => EndTime.HasValue ? EndTime.Value - StartTime : null;
    public bool IsCompleted => Status == MigrationStatus.Completed || Status == MigrationStatus.Failed || Status == MigrationStatus.Cancelled;
}

public enum MigrationStatus
{
    NotStarted,
    InProgress,
    Completed,
    Failed,
    Cancelled,
    Paused
}

public class MigrationStatistics
{
    public int TotalTables { get; set; }
    public int TablesProcessed { get; set; }
    public int TotalRecords { get; set; }
    public int RecordsProcessed { get; set; }
    public int ErrorCount { get; set; }
    public TimeSpan EstimatedTimeRemaining { get; set; }
    public double ProgressPercentage => TotalTables > 0 ? (double)TablesProcessed / TotalTables * 100 : 0;
}

public class MigrationError
{
    public DateTime Timestamp { get; set; }
    public ErrorLevel Level { get; set; }
    public string Component { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? StackTrace { get; set; }
    public string? TableName { get; set; }
    public int? RecordNumber { get; set; }
}

public enum ErrorLevel
{
    Info,
    Warning,
    Error,
    Critical
}