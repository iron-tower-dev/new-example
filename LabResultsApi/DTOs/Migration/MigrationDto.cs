namespace LabResultsApi.DTOs.Migration;

public class MigrationStatusDto
{
    public Guid MigrationId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public double ProgressPercentage { get; set; }
    public TimeSpan? Duration { get; set; }
    public MigrationStatisticsDto Statistics { get; set; } = new();
    public List<MigrationErrorDto> RecentErrors { get; set; } = new();
    public string? CurrentOperation { get; set; }
    public TimeSpan EstimatedTimeRemaining { get; set; }
}

public class MigrationStatisticsDto
{
    public int TotalTables { get; set; }
    public int TablesProcessed { get; set; }
    public int TotalRecords { get; set; }
    public int RecordsProcessed { get; set; }
    public int ErrorCount { get; set; }
    public double ProgressPercentage { get; set; }
}

public class MigrationErrorDto
{
    public DateTime Timestamp { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Component { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? TableName { get; set; }
    public int? RecordNumber { get; set; }
}

public class StartMigrationRequest
{
    public MigrationOptionsDto Options { get; set; } = new();
}

public class MigrationOptionsDto
{
    public bool ClearExistingData { get; set; } = true;
    public bool CreateMissingTables { get; set; } = true;
    public bool ValidateAgainstLegacy { get; set; } = true;
    public bool RemoveAuthentication { get; set; } = false;
    public string[] IncludeTables { get; set; } = Array.Empty<string>();
    public string[] ExcludeTables { get; set; } = Array.Empty<string>();
    public SeedingOptionsDto SeedingOptions { get; set; } = new();
    public ValidationOptionsDto ValidationOptions { get; set; } = new();
    public AuthRemovalOptionsDto AuthRemovalOptions { get; set; } = new();
    public int MaxConcurrentOperations { get; set; } = 4;
    public int OperationTimeoutMinutes { get; set; } = 30;
}

public class SeedingOptionsDto
{
    public bool ClearExistingData { get; set; } = true;
    public bool CreateMissingTables { get; set; } = true;
    public int BatchSize { get; set; } = 1000;
    public bool ContinueOnError { get; set; } = true;
    public bool ValidateBeforeInsert { get; set; } = true;
    public bool UseTransactions { get; set; } = true;
    public int CommandTimeoutMinutes { get; set; } = 5;
}

public class ValidationOptionsDto
{
    public bool CompareQueryResults { get; set; } = true;
    public bool ComparePerformance { get; set; } = true;
    public bool GenerateDetailedReports { get; set; } = true;
    public int MaxDiscrepanciesToReport { get; set; } = 100;
    public double PerformanceThresholdPercent { get; set; } = 20.0;
    public int QueryTimeoutMinutes { get; set; } = 2;
    public string LegacyConnectionString { get; set; } = string.Empty;
    public bool IgnoreMinorDifferences { get; set; } = true;
}

public class AuthRemovalOptionsDto
{
    public bool CreateBackup { get; set; } = true;
    public string BackupDirectory { get; set; } = "auth-backup";
    public bool RemoveFromApi { get; set; } = true;
    public bool RemoveFromFrontend { get; set; } = true;
    public bool UpdateDocumentation { get; set; } = true;
    public List<string> FilesToExclude { get; set; } = new();
}

public class MigrationProgressDto
{
    public Guid MigrationId { get; set; }
    public double ProgressPercentage { get; set; }
    public string CurrentOperation { get; set; } = string.Empty;
    public MigrationStatisticsDto Statistics { get; set; } = new();
    public TimeSpan EstimatedTimeRemaining { get; set; }
    public DateTime StartTime { get; set; }
    public TimeSpan ElapsedTime { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class MigrationStatisticsSummaryDto
{
    public int TotalMigrations { get; set; }
    public int SuccessfulMigrations { get; set; }
    public int FailedMigrations { get; set; }
    public int CancelledMigrations { get; set; }
    public TimeSpan AverageDuration { get; set; }
    public long TotalRecordsProcessed { get; set; }
    public int TotalTablesProcessed { get; set; }
    public int PeriodDays { get; set; }
    public double SuccessRate => TotalMigrations > 0 ? (double)SuccessfulMigrations / TotalMigrations * 100 : 0;
}