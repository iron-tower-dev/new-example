namespace LabResultsApi.Models.Migration;

public class MigrationOptions
{
    public bool ClearExistingData { get; set; } = true;
    public bool CreateMissingTables { get; set; } = true;
    public bool ValidateAgainstLegacy { get; set; } = true;
    public bool RemoveAuthentication { get; set; } = false;
    public string[] IncludeTables { get; set; } = Array.Empty<string>();
    public string[] ExcludeTables { get; set; } = Array.Empty<string>();
    public SeedingOptions SeedingOptions { get; set; } = new();
    public ValidationOptions ValidationOptions { get; set; } = new();
    public AuthRemovalOptions AuthRemovalOptions { get; set; } = new();
    public LoggingOptions LoggingOptions { get; set; } = new();
    public int MaxConcurrentOperations { get; set; } = 4;
    public TimeSpan OperationTimeout { get; set; } = TimeSpan.FromMinutes(30);
}

public class SeedingOptions
{
    public bool ClearExistingData { get; set; } = true;
    public bool CreateMissingTables { get; set; } = true;
    public int BatchSize { get; set; } = 1000;
    public bool ContinueOnError { get; set; } = true;
    public bool ValidateBeforeInsert { get; set; } = true;
    public string CsvDirectory { get; set; } = "db-seeding";
    public string SqlDirectory { get; set; } = "db-tables";
    public bool UseTransactions { get; set; } = true;
    public TimeSpan CommandTimeout { get; set; } = TimeSpan.FromMinutes(5);
}

public class ValidationOptions
{
    public bool CompareQueryResults { get; set; } = true;
    public bool ComparePerformance { get; set; } = true;
    public bool GenerateDetailedReports { get; set; } = true;
    public int MaxDiscrepanciesToReport { get; set; } = 100;
    public double PerformanceThresholdPercent { get; set; } = 20.0; // 20% difference threshold
    public int QueryTimeoutMinutes { get; set; } = 2;
    public TimeSpan QueryTimeout { get; set; } = TimeSpan.FromMinutes(2);
    public string LegacyConnectionString { get; set; } = string.Empty;
    public bool IgnoreMinorDifferences { get; set; } = true;
    public List<string> IncludeQueries { get; set; } = new();
    public List<string> ExcludeQueries { get; set; } = new();
}

public class AuthRemovalOptions
{
    public bool CreateBackup { get; set; } = true;
    public string BackupDirectory { get; set; } = "auth-backup";
    public bool RemoveFromApi { get; set; } = true;
    public bool RemoveFromFrontend { get; set; } = true;
    public bool UpdateDocumentation { get; set; } = true;
    public List<string> FilesToExclude { get; set; } = new();
}

public class LoggingOptions
{
    public Models.Migration.LogLevel MinimumLevel { get; set; } = Models.Migration.LogLevel.Information;
    public bool LogToFile { get; set; } = true;
    public bool LogToConsole { get; set; } = true;
    public string LogDirectory { get; set; } = "logs/migration";
    public bool IncludeStackTrace { get; set; } = false;
    public int MaxLogFileSizeMB { get; set; } = 100;
    public int MaxLogFiles { get; set; } = 10;
}

public enum LogLevel
{
    Trace,
    Debug,
    Information,
    Warning,
    Error,
    Critical
}