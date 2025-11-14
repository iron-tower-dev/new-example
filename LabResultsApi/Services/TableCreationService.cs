using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using LabResultsApi.Data;
using LabResultsApi.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LabResultsApi.Services;

public class TableCreationService : ITableCreationService
{
    private readonly LabDbContext _context;
    private readonly ILogger<TableCreationService> _logger;
    private readonly string _schemaDirectory;
    private readonly Dictionary<string, TableDependency> _tableDependencies;

    public TableCreationService(LabDbContext context, ILogger<TableCreationService> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _schemaDirectory = Path.Combine(Directory.GetCurrentDirectory(), "../../db-tables");
        _tableDependencies = new Dictionary<string, TableDependency>();
    }

    public async Task<TableCreationResult> CreateAllMissingTablesAsync()
    {
        var result = new TableCreationResult();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Starting creation of all missing tables");

            // Get available schema files
            var schemaFiles = await GetAvailableSchemaFilesAsync();
            result.TotalTables = schemaFiles.Count;

            if (schemaFiles.Count == 0)
            {
                result.Errors.Add(new TableCreationError
                {
                    ErrorMessage = "No SQL schema files found in directory",
                    ErrorType = "NoSchemaFiles"
                });
                return result;
            }

            // Resolve dependencies
            var dependencies = await ResolveDependenciesAsync();
            
            // Get creation order
            var creationOrder = await GetTableCreationOrderAsync();

            // Create tables in dependency order
            foreach (var tableName in creationOrder)
            {
                try
                {
                    var tableExists = await TableExistsAsync(tableName);
                    if (tableExists)
                    {
                        _logger.LogInformation("Table {TableName} already exists, skipping", tableName);
                        result.SkippedTables.Add(tableName);
                        continue;
                    }

                    var createResult = await CreateTableAsync(tableName);
                    result.ExecutionResults[tableName] = createResult.ExecutionResults.FirstOrDefault().Value ?? new SqlExecutionResult();

                    if (createResult.Success)
                    {
                        result.CreatedTables.Add(tableName);
                        _logger.LogInformation("Successfully created table: {TableName}", tableName);
                    }
                    else
                    {
                        result.FailedTables.Add(tableName);
                        result.Errors.AddRange(createResult.Errors);
                        _logger.LogError("Failed to create table: {TableName}", tableName);
                    }
                }
                catch (Exception ex)
                {
                    result.FailedTables.Add(tableName);
                    result.Errors.Add(new TableCreationError
                    {
                        TableName = tableName,
                        ErrorMessage = ex.Message,
                        ErrorType = "UnexpectedError"
                    });
                    _logger.LogError(ex, "Unexpected error creating table: {TableName}", tableName);
                }
            }

            result.Success = result.FailedTables.Count == 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateAllMissingTablesAsync");
            result.Errors.Add(new TableCreationError
            {
                ErrorMessage = $"General error: {ex.Message}",
                ErrorType = "GeneralError"
            });
        }
        finally
        {
            stopwatch.Stop();
            result.Duration = stopwatch.Elapsed;
        }

        return result;
    }

    public async Task<TableCreationResult> CreateTableAsync(string tableName)
    {
        var result = new TableCreationResult { TotalTables = 1 };
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Creating table: {TableName}", tableName);

            // Find the SQL script file
            var scriptPath = Path.Combine(_schemaDirectory, $"{tableName}.sql");
            if (!File.Exists(scriptPath))
            {
                result.Errors.Add(new TableCreationError
                {
                    TableName = tableName,
                    ScriptPath = scriptPath,
                    ErrorMessage = $"SQL script file not found: {scriptPath}",
                    ErrorType = "ScriptNotFound"
                });
                result.FailedTables.Add(tableName);
                return result;
            }

            // Validate script
            var validationResult = await ValidateSqlScriptAsync(scriptPath);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    result.Errors.Add(new TableCreationError
                    {
                        TableName = tableName,
                        ScriptPath = scriptPath,
                        ErrorMessage = error.ErrorMessage,
                        ErrorType = "ValidationError",
                        LineNumber = error.LineNumber
                    });
                }
                result.FailedTables.Add(tableName);
                return result;
            }

            // Check if table already exists
            var tableExists = await TableExistsAsync(tableName);
            if (tableExists)
            {
                _logger.LogInformation("Table {TableName} already exists", tableName);
                result.SkippedTables.Add(tableName);
                result.Success = true;
                return result;
            }

            // Execute the SQL script
            var executionResult = await ExecuteSqlScriptAsync(scriptPath);
            result.ExecutionResults[tableName] = executionResult;

            if (executionResult.Success)
            {
                result.CreatedTables.Add(tableName);
                result.Success = true;
                _logger.LogInformation("Successfully created table: {TableName}", tableName);
            }
            else
            {
                result.FailedTables.Add(tableName);
                result.Errors.Add(new TableCreationError
                {
                    TableName = tableName,
                    ScriptPath = scriptPath,
                    ErrorMessage = executionResult.ErrorMessage ?? "Unknown execution error",
                    ErrorType = "ExecutionError"
                });
                _logger.LogError("Failed to create table {TableName}: {Error}", tableName, executionResult.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            result.FailedTables.Add(tableName);
            result.Errors.Add(new TableCreationError
            {
                TableName = tableName,
                ErrorMessage = ex.Message,
                ErrorType = "UnexpectedError"
            });
            _logger.LogError(ex, "Unexpected error creating table: {TableName}", tableName);
        }
        finally
        {
            stopwatch.Stop();
            result.Duration = stopwatch.Elapsed;
        }

        return result;
    }

    public async Task<bool> TableExistsAsync(string tableName)
    {
        try
        {
            var sql = @"
                SELECT COUNT(*) 
                FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_NAME = @tableName 
                AND TABLE_TYPE = 'BASE TABLE'";

            using var connection = new SqlConnection(_context.Database.GetConnectionString());
            await connection.OpenAsync();
            
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@tableName", tableName);
            
            var count = (int)await command.ExecuteScalarAsync();
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if table exists: {TableName}", tableName);
            return false;
        }
    }

    public async Task<List<string>> GetTableCreationOrderAsync()
    {
        var dependencies = await ResolveDependenciesAsync();
        var ordered = new List<string>();
        var visited = new HashSet<string>();
        var visiting = new HashSet<string>();

        foreach (var tableName in dependencies.Keys)
        {
            await VisitTable(tableName, dependencies, ordered, visited, visiting);
        }

        return ordered;
    }

    public async Task<Dictionary<string, List<string>>> ResolveDependenciesAsync()
    {
        var dependencies = new Dictionary<string, List<string>>();
        var schemaFiles = await GetAvailableSchemaFilesAsync();

        foreach (var schemaFile in schemaFiles)
        {
            var tableName = Path.GetFileNameWithoutExtension(schemaFile);
            var scriptPath = Path.Combine(_schemaDirectory, schemaFile);
            
            try
            {
                var scriptContent = await File.ReadAllTextAsync(scriptPath);
                var tableDependencies = ExtractTableDependencies(scriptContent);
                dependencies[tableName] = tableDependencies;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Error reading schema file {SchemaFile}: {Error}", schemaFile, ex.Message);
                dependencies[tableName] = new List<string>();
            }
        }

        return dependencies;
    }

    public async Task<SqlExecutionResult> ExecuteSqlScriptAsync(string scriptPath)
    {
        var result = new SqlExecutionResult
        {
            ScriptPath = scriptPath,
            TableName = Path.GetFileNameWithoutExtension(scriptPath)
        };

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var scriptContent = await File.ReadAllTextAsync(scriptPath);
            
            // Clean up the script (remove USE statements, GO statements, etc.)
            var cleanedScript = CleanSqlScript(scriptContent);
            
            // Split into individual statements
            var statements = SplitSqlStatements(cleanedScript);

            using var connection = new SqlConnection(_context.Database.GetConnectionString());
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();
            
            try
            {
                int totalAffectedRows = 0;

                foreach (var statement in statements)
                {
                    if (string.IsNullOrWhiteSpace(statement))
                        continue;

                    using var command = new SqlCommand(statement, connection, transaction);
                    command.CommandTimeout = 300; // 5 minutes timeout
                    
                    var affectedRows = await command.ExecuteNonQueryAsync();
                    totalAffectedRows += affectedRows;
                    result.ExecutedStatements.Add(statement.Trim());
                }

                await transaction.CommitAsync();
                result.Success = true;
                result.AffectedRows = totalAffectedRows;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                result.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Error executing SQL script: {ScriptPath}", scriptPath);
            }
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Error reading SQL script: {ScriptPath}", scriptPath);
        }
        finally
        {
            stopwatch.Stop();
            result.ExecutionTime = stopwatch.Elapsed;
        }

        return result;
    }

    public async Task<ConflictResolutionResult> ResolveTableConflictAsync(string tableName, ConflictResolutionStrategy strategy)
    {
        var result = new ConflictResolutionResult
        {
            TableName = tableName,
            Strategy = strategy
        };

        try
        {
            var tableExists = await TableExistsAsync(tableName);
            if (!tableExists)
            {
                result.Success = true;
                result.Action = "NoConflict";
                return result;
            }

            switch (strategy)
            {
                case ConflictResolutionStrategy.Skip:
                    result.Success = true;
                    result.Action = "Skipped";
                    break;

                case ConflictResolutionStrategy.DropAndRecreate:
                    await DropTableAsync(tableName);
                    result.Success = true;
                    result.Action = "DroppedAndWillRecreate";
                    break;

                case ConflictResolutionStrategy.BackupAndRecreate:
                    var backupResult = await BackupTableAsync(tableName);
                    result.BackupResult = backupResult;
                    
                    if (backupResult.Success)
                    {
                        await DropTableAsync(tableName);
                        result.Success = true;
                        result.Action = "BackedUpAndWillRecreate";
                    }
                    else
                    {
                        result.Success = false;
                        result.ErrorMessage = backupResult.ErrorMessage;
                    }
                    break;

                case ConflictResolutionStrategy.Fail:
                    result.Success = false;
                    result.ErrorMessage = $"Table {tableName} already exists and strategy is set to Fail";
                    break;

                default:
                    result.Success = false;
                    result.ErrorMessage = $"Unknown conflict resolution strategy: {strategy}";
                    break;
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Error resolving table conflict for: {TableName}", tableName);
        }

        return result;
    }

    public async Task<SqlValidationResult> ValidateSqlScriptAsync(string scriptPath)
    {
        var result = new SqlValidationResult
        {
            ScriptPath = scriptPath
        };

        try
        {
            if (!File.Exists(scriptPath))
            {
                result.Errors.Add(new SqlValidationError
                {
                    ErrorMessage = "Script file does not exist",
                    ErrorType = "FileNotFound"
                });
                return result;
            }

            var scriptContent = await File.ReadAllTextAsync(scriptPath);
            var lines = scriptContent.Split('\n');

            // Basic validation
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                
                // Check for dangerous operations
                if (line.ToUpper().Contains("DROP DATABASE") || 
                    line.ToUpper().Contains("DELETE FROM") && !line.ToUpper().Contains("WHERE"))
                {
                    result.Warnings.Add(new SqlValidationWarning
                    {
                        LineNumber = i + 1,
                        Statement = line,
                        Message = "Potentially dangerous operation detected",
                        WarningType = "DangerousOperation"
                    });
                }

                // Extract table names from CREATE TABLE statements
                if (line.ToUpper().StartsWith("CREATE TABLE"))
                {
                    var tableName = ExtractTableNameFromCreateStatement(line);
                    if (!string.IsNullOrEmpty(tableName))
                    {
                        result.DetectedTables.Add(tableName);
                    }
                }
            }

            // Extract dependencies
            result.Dependencies = ExtractTableDependencies(scriptContent);
            result.IsValid = result.Errors.Count == 0;
        }
        catch (Exception ex)
        {
            result.Errors.Add(new SqlValidationError
            {
                ErrorMessage = ex.Message,
                ErrorType = "ValidationException"
            });
            _logger.LogError(ex, "Error validating SQL script: {ScriptPath}", scriptPath);
        }

        return result;
    }

    public async Task<List<string>> GetAvailableSchemaFilesAsync()
    {
        try
        {
            if (!Directory.Exists(_schemaDirectory))
            {
                _logger.LogWarning("Schema directory does not exist: {SchemaDirectory}", _schemaDirectory);
                return new List<string>();
            }

            var files = Directory.GetFiles(_schemaDirectory, "*.sql")
                .Select(Path.GetFileName)
                .Where(f => f != null)
                .Cast<string>()
                .ToList();

            return files;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available schema files from: {SchemaDirectory}", _schemaDirectory);
            return new List<string>();
        }
    }

    public async Task<BackupResult> BackupTableAsync(string tableName)
    {
        var result = new BackupResult
        {
            TableName = tableName,
            BackupTimestamp = DateTime.UtcNow
        };

        try
        {
            var backupTableName = $"{tableName}_backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}";
            result.BackupTableName = backupTableName;

            var sql = $@"
                SELECT * 
                INTO [{backupTableName}] 
                FROM [{tableName}]";

            using var connection = new SqlConnection(_context.Database.GetConnectionString());
            await connection.OpenAsync();
            
            using var command = new SqlCommand(sql, connection);
            result.BackedUpRows = await command.ExecuteNonQueryAsync();
            
            result.Success = true;
            result.BackupLocation = "Same database";
            
            _logger.LogInformation("Successfully backed up table {TableName} to {BackupTableName} with {RowCount} rows", 
                tableName, backupTableName, result.BackedUpRows);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Error backing up table: {TableName}", tableName);
        }

        return result;
    }

    private async Task DropTableAsync(string tableName)
    {
        try
        {
            var sql = $"DROP TABLE IF EXISTS [{tableName}]";
            
            using var connection = new SqlConnection(_context.Database.GetConnectionString());
            await connection.OpenAsync();
            
            using var command = new SqlCommand(sql, connection);
            await command.ExecuteNonQueryAsync();
            
            _logger.LogInformation("Successfully dropped table: {TableName}", tableName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dropping table: {TableName}", tableName);
            throw;
        }
    }

    private async Task VisitTable(string tableName, Dictionary<string, List<string>> dependencies, 
        List<string> ordered, HashSet<string> visited, HashSet<string> visiting)
    {
        if (visited.Contains(tableName))
            return;

        if (visiting.Contains(tableName))
        {
            _logger.LogWarning("Circular dependency detected involving table: {TableName}", tableName);
            return;
        }

        visiting.Add(tableName);

        if (dependencies.ContainsKey(tableName))
        {
            foreach (var dependency in dependencies[tableName])
            {
                await VisitTable(dependency, dependencies, ordered, visited, visiting);
            }
        }

        visiting.Remove(tableName);
        visited.Add(tableName);
        ordered.Add(tableName);
    }

    private List<string> ExtractTableDependencies(string scriptContent)
    {
        var dependencies = new List<string>();
        
        // Look for FOREIGN KEY references
        var foreignKeyPattern = @"REFERENCES\s+\[?(\w+)\]?\s*\(";
        var matches = Regex.Matches(scriptContent, foreignKeyPattern, RegexOptions.IgnoreCase);
        
        foreach (Match match in matches)
        {
            if (match.Groups.Count > 1)
            {
                var referencedTable = match.Groups[1].Value;
                if (!dependencies.Contains(referencedTable))
                {
                    dependencies.Add(referencedTable);
                }
            }
        }

        return dependencies;
    }

    private string ExtractTableNameFromCreateStatement(string createStatement)
    {
        var pattern = @"CREATE\s+TABLE\s+\[?(\w+)\]?";
        var match = Regex.Match(createStatement, pattern, RegexOptions.IgnoreCase);
        
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    private string CleanSqlScript(string scriptContent)
    {
        // Remove USE statements
        scriptContent = Regex.Replace(scriptContent, @"USE\s+\[.*?\]", "", RegexOptions.IgnoreCase);
        
        // Remove GO statements
        scriptContent = Regex.Replace(scriptContent, @"^\s*GO\s*$", "", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        
        // Remove SET statements that might cause issues
        scriptContent = Regex.Replace(scriptContent, @"SET\s+ANSI_NULLS\s+(ON|OFF)", "", RegexOptions.IgnoreCase);
        scriptContent = Regex.Replace(scriptContent, @"SET\s+QUOTED_IDENTIFIER\s+(ON|OFF)", "", RegexOptions.IgnoreCase);
        
        // Remove comments
        scriptContent = Regex.Replace(scriptContent, @"/\*.*?\*/", "", RegexOptions.Singleline);
        scriptContent = Regex.Replace(scriptContent, @"--.*$", "", RegexOptions.Multiline);
        
        return scriptContent.Trim();
    }

    private List<string> SplitSqlStatements(string scriptContent)
    {
        // Split on semicolons, but be careful about semicolons within strings
        var statements = new List<string>();
        var currentStatement = new StringBuilder();
        bool inString = false;
        char stringDelimiter = '\0';

        for (int i = 0; i < scriptContent.Length; i++)
        {
            char c = scriptContent[i];

            if (!inString && (c == '\'' || c == '"'))
            {
                inString = true;
                stringDelimiter = c;
                currentStatement.Append(c);
            }
            else if (inString && c == stringDelimiter)
            {
                // Check for escaped quotes
                if (i + 1 < scriptContent.Length && scriptContent[i + 1] == stringDelimiter)
                {
                    currentStatement.Append(c);
                    currentStatement.Append(scriptContent[i + 1]);
                    i++; // Skip the next character
                }
                else
                {
                    inString = false;
                    currentStatement.Append(c);
                }
            }
            else if (!inString && c == ';')
            {
                var statement = currentStatement.ToString().Trim();
                if (!string.IsNullOrEmpty(statement))
                {
                    statements.Add(statement);
                }
                currentStatement.Clear();
            }
            else
            {
                currentStatement.Append(c);
            }
        }

        // Add the last statement if it doesn't end with semicolon
        var finalStatement = currentStatement.ToString().Trim();
        if (!string.IsNullOrEmpty(finalStatement))
        {
            statements.Add(finalStatement);
        }

        return statements;
    }
}