namespace LabResultsApi.DTOs.Migration;

public class MigrationReportDto
{
    public Guid MigrationId { get; set; }
    public DateTime GeneratedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public MigrationSummaryDto Summary { get; set; } = new();
    public SeedingReportDto? SeedingReport { get; set; }
    public ValidationReportDto? ValidationReport { get; set; }
    public AuthRemovalReportDto? AuthRemovalReport { get; set; }
    public List<MigrationErrorDto> Errors { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

public class MigrationSummaryDto
{
    public int TotalTables { get; set; }
    public int TablesProcessed { get; set; }
    public int TotalRecords { get; set; }
    public int RecordsProcessed { get; set; }
    public int ErrorCount { get; set; }
    public bool Success { get; set; }
    public double OverallProgressPercentage { get; set; }
}

public class SeedingReportDto
{
    public int TablesProcessed { get; set; }
    public int TablesCreated { get; set; }
    public int RecordsInserted { get; set; }
    public int RecordsSkipped { get; set; }
    public TimeSpan Duration { get; set; }
    public bool Success { get; set; }
    public List<TableSeedingReportDto> TableReports { get; set; } = new();
}

public class TableSeedingReportDto
{
    public string TableName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public int RecordsProcessed { get; set; }
    public int RecordsInserted { get; set; }
    public int RecordsSkipped { get; set; }
    public TimeSpan Duration { get; set; }
    public List<string> Errors { get; set; } = new();
    public bool TableCreated { get; set; }
}

public class ValidationReportDto
{
    public int QueriesValidated { get; set; }
    public int QueriesMatched { get; set; }
    public int QueriesFailed { get; set; }
    public double MatchPercentage { get; set; }
    public TimeSpan Duration { get; set; }
    public bool Success { get; set; }
    public List<QueryComparisonReportDto> QueryReports { get; set; } = new();
    public ValidationSummaryDto Summary { get; set; } = new();
}

public class QueryComparisonReportDto
{
    public string QueryName { get; set; } = string.Empty;
    public bool DataMatches { get; set; }
    public int CurrentRowCount { get; set; }
    public int LegacyRowCount { get; set; }
    public int DiscrepancyCount { get; set; }
    public TimeSpan CurrentExecutionTime { get; set; }
    public TimeSpan LegacyExecutionTime { get; set; }
    public double PerformanceRatio { get; set; }
    public string? Error { get; set; }
}

public class ValidationSummaryDto
{
    public double MatchPercentage { get; set; }
    public int TotalDiscrepancies { get; set; }
    public TimeSpan AverageCurrentExecutionTime { get; set; }
    public TimeSpan AverageLegacyExecutionTime { get; set; }
    public List<string> CriticalIssues { get; set; } = new();
}

public class AuthRemovalReportDto
{
    public bool Success { get; set; }
    public int RemovedComponentsCount { get; set; }
    public int ModifiedFilesCount { get; set; }
    public int BackupFilesCount { get; set; }
    public string BackupLocation { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public List<string> Errors { get; set; } = new();
}