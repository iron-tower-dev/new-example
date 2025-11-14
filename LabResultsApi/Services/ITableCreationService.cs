using LabResultsApi.Models;

namespace LabResultsApi.Services;

public interface ITableCreationService
{
    /// <summary>
    /// Create all missing tables from SQL schema files
    /// </summary>
    Task<TableCreationResult> CreateAllMissingTablesAsync();

    /// <summary>
    /// Create a specific table from its SQL schema file
    /// </summary>
    Task<TableCreationResult> CreateTableAsync(string tableName);

    /// <summary>
    /// Check if a table exists in the database
    /// </summary>
    Task<bool> TableExistsAsync(string tableName);

    /// <summary>
    /// Get table creation order based on dependencies
    /// </summary>
    Task<List<string>> GetTableCreationOrderAsync();

    /// <summary>
    /// Resolve table dependencies from SQL scripts
    /// </summary>
    Task<Dictionary<string, List<string>>> ResolveDependenciesAsync();

    /// <summary>
    /// Execute SQL script from file
    /// </summary>
    Task<SqlExecutionResult> ExecuteSqlScriptAsync(string scriptPath);

    /// <summary>
    /// Handle table conflicts (existing tables)
    /// </summary>
    Task<ConflictResolutionResult> ResolveTableConflictAsync(string tableName, ConflictResolutionStrategy strategy);

    /// <summary>
    /// Validate SQL script before execution
    /// </summary>
    Task<SqlValidationResult> ValidateSqlScriptAsync(string scriptPath);

    /// <summary>
    /// Get available SQL schema files
    /// </summary>
    Task<List<string>> GetAvailableSchemaFilesAsync();

    /// <summary>
    /// Backup existing table before recreation
    /// </summary>
    Task<BackupResult> BackupTableAsync(string tableName);
}

public class TableCreationResult
{
    public bool Success { get; set; }
    public List<string> CreatedTables { get; set; } = new();
    public List<string> SkippedTables { get; set; } = new();
    public List<string> FailedTables { get; set; } = new();
    public List<TableCreationError> Errors { get; set; } = new();
    public TimeSpan Duration { get; set; }
    public int TotalTables { get; set; }
    public Dictionary<string, SqlExecutionResult> ExecutionResults { get; set; } = new();
}

public class TableCreationError
{
    public string TableName { get; set; } = string.Empty;
    public string ScriptPath { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string ErrorType { get; set; } = string.Empty;
    public string SqlStatement { get; set; } = string.Empty;
    public int? LineNumber { get; set; }
}

public class SqlExecutionResult
{
    public bool Success { get; set; }
    public string ScriptPath { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public int AffectedRows { get; set; }
    public TimeSpan ExecutionTime { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> ExecutedStatements { get; set; } = new();
}

public class ConflictResolutionResult
{
    public bool Success { get; set; }
    public string TableName { get; set; } = string.Empty;
    public ConflictResolutionStrategy Strategy { get; set; }
    public string Action { get; set; } = string.Empty;
    public BackupResult? BackupResult { get; set; }
    public string? ErrorMessage { get; set; }
}

public class SqlValidationResult
{
    public bool IsValid { get; set; }
    public List<SqlValidationError> Errors { get; set; } = new();
    public List<SqlValidationWarning> Warnings { get; set; } = new();
    public string ScriptPath { get; set; } = string.Empty;
    public List<string> DetectedTables { get; set; } = new();
    public List<string> Dependencies { get; set; } = new();
}

public class SqlValidationError
{
    public int LineNumber { get; set; }
    public string Statement { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string ErrorType { get; set; } = string.Empty;
}

public class SqlValidationWarning
{
    public int LineNumber { get; set; }
    public string Statement { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string WarningType { get; set; } = string.Empty;
}

public class BackupResult
{
    public bool Success { get; set; }
    public string TableName { get; set; } = string.Empty;
    public string BackupTableName { get; set; } = string.Empty;
    public int BackedUpRows { get; set; }
    public string BackupLocation { get; set; } = string.Empty;
    public DateTime BackupTimestamp { get; set; }
    public string? ErrorMessage { get; set; }
    public Guid BackupId { get; set; } = Guid.NewGuid();
    public List<string> BackedUpFiles { get; set; } = new List<string>();
    public long BackupSizeBytes { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
}

public enum ConflictResolutionStrategy
{
    Skip,
    DropAndRecreate,
    BackupAndRecreate,
    Prompt,
    Fail
}

public class TableDependency
{
    public string TableName { get; set; } = string.Empty;
    public List<string> DependsOn { get; set; } = new();
    public int DependencyLevel { get; set; }
    public string ScriptPath { get; set; } = string.Empty;
}