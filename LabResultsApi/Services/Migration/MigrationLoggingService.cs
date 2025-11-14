using LabResultsApi.Models.Migration;
using System.Text.Json;
using System.Collections.Concurrent;
using System.Text;

namespace LabResultsApi.Services.Migration;

public class MigrationLoggingService : IMigrationLoggingService
{
    private readonly ILogger<MigrationLoggingService> _logger;
    private readonly IConfiguration _configuration;
    private readonly ConcurrentDictionary<Guid, List<MigrationError>> _migrationLogs;
    private readonly ConcurrentDictionary<Guid, List<StructuredLogEntry>> _structuredLogs;
    private readonly string _logDirectory;
    private readonly LoggingOptions _loggingOptions;
    private readonly SemaphoreSlim _fileLock;

    public MigrationLoggingService(ILogger<MigrationLoggingService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _migrationLogs = new ConcurrentDictionary<Guid, List<MigrationError>>();
        _structuredLogs = new ConcurrentDictionary<Guid, List<StructuredLogEntry>>();
        _logDirectory = _configuration.GetValue<string>("Migration:LogDirectory") ?? "logs/migration";
        _loggingOptions = _configuration.GetSection("Migration:Logging").Get<LoggingOptions>() ?? new LoggingOptions();
        _fileLock = new SemaphoreSlim(1, 1);
        
        // Ensure log directory exists
        Directory.CreateDirectory(_logDirectory);
        
        // Initialize log rotation if needed
        InitializeLogRotation();
    }

    public async Task LogMigrationStartAsync(Guid migrationId, MigrationOptions options)
    {
        var logEntry = new MigrationError
        {
            Timestamp = DateTime.UtcNow,
            Level = ErrorLevel.Info,
            Component = "MigrationControl",
            Message = "Migration started",
            Details = JsonSerializer.Serialize(options, new JsonSerializerOptions { WriteIndented = true })
        };

        await LogToMemoryAsync(migrationId, logEntry);
        await LogToFileAsync(migrationId, logEntry);
        
        _logger.LogInformation("Migration {MigrationId} started with options: {@Options}", migrationId, options);
    }

    public async Task LogMigrationEndAsync(Guid migrationId, MigrationStatus status, TimeSpan duration)
    {
        var logEntry = new MigrationError
        {
            Timestamp = DateTime.UtcNow,
            Level = status == MigrationStatus.Completed ? ErrorLevel.Info : ErrorLevel.Error,
            Component = "MigrationControl",
            Message = $"Migration ended with status: {status}",
            Details = $"Duration: {duration}"
        };

        await LogToMemoryAsync(migrationId, logEntry);
        await LogToFileAsync(migrationId, logEntry);
        
        _logger.LogInformation("Migration {MigrationId} ended with status {Status} after {Duration}", 
            migrationId, status, duration);
    }

    public async Task LogProgressAsync(Guid migrationId, double progressPercentage, string operation)
    {
        var logEntry = new MigrationError
        {
            Timestamp = DateTime.UtcNow,
            Level = ErrorLevel.Info,
            Component = "MigrationControl",
            Message = $"Progress: {progressPercentage:F1}%",
            Details = operation
        };

        await LogToMemoryAsync(migrationId, logEntry);
        
        // Only log progress to file every 10% to avoid spam
        if (progressPercentage % 10 == 0 || progressPercentage == 100)
        {
            await LogToFileAsync(migrationId, logEntry);
        }
        
        _logger.LogDebug("Migration {MigrationId} progress: {Progress}% - {Operation}", 
            migrationId, progressPercentage, operation);
    }

    public async Task LogErrorAsync(Guid migrationId, MigrationError error)
    {
        await LogToMemoryAsync(migrationId, error);
        await LogToFileAsync(migrationId, error);
        
        _logger.LogError("Migration {MigrationId} error in {Component}: {Message} - {Details}", 
            migrationId, error.Component, error.Message, error.Details);
    }

    public async Task LogPhaseStartAsync(Guid migrationId, string phaseName)
    {
        var logEntry = new MigrationError
        {
            Timestamp = DateTime.UtcNow,
            Level = ErrorLevel.Info,
            Component = phaseName,
            Message = $"Phase started: {phaseName}",
            Details = null
        };

        await LogToMemoryAsync(migrationId, logEntry);
        await LogToFileAsync(migrationId, logEntry);
        
        _logger.LogInformation("Migration {MigrationId} phase started: {PhaseName}", migrationId, phaseName);
    }

    public async Task LogPhaseEndAsync(Guid migrationId, string phaseName, bool success, TimeSpan duration)
    {
        var logEntry = new MigrationError
        {
            Timestamp = DateTime.UtcNow,
            Level = success ? ErrorLevel.Info : ErrorLevel.Error,
            Component = phaseName,
            Message = $"Phase ended: {phaseName} - {(success ? "Success" : "Failed")}",
            Details = $"Duration: {duration}"
        };

        await LogToMemoryAsync(migrationId, logEntry);
        await LogToFileAsync(migrationId, logEntry);
        
        _logger.LogInformation("Migration {MigrationId} phase ended: {PhaseName} - {Success} after {Duration}", 
            migrationId, phaseName, success, duration);
    }

    public async Task<List<MigrationError>> GetMigrationLogsAsync(Guid migrationId)
    {
        await Task.CompletedTask;
        return _migrationLogs.GetValueOrDefault(migrationId, new List<MigrationError>());
    }

    public async Task<string> GenerateLogReportAsync(Guid migrationId)
    {
        var logs = await GetMigrationLogsAsync(migrationId);
        
        var report = new
        {
            MigrationId = migrationId,
            GeneratedAt = DateTime.UtcNow,
            TotalEntries = logs.Count,
            ErrorCount = logs.Count(l => l.Level == ErrorLevel.Error || l.Level == ErrorLevel.Critical),
            WarningCount = logs.Count(l => l.Level == ErrorLevel.Warning),
            InfoCount = logs.Count(l => l.Level == ErrorLevel.Info),
            Logs = logs.OrderBy(l => l.Timestamp)
        };

        return JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
    }

    public async Task ArchiveLogsAsync(DateTime olderThan)
    {
        var archiveDirectory = Path.Combine(_logDirectory, "archive");
        Directory.CreateDirectory(archiveDirectory);

        var logFiles = Directory.GetFiles(_logDirectory, "migration-*.log")
            .Where(f => File.GetCreationTime(f) < olderThan);

        foreach (var logFile in logFiles)
        {
            var fileName = Path.GetFileName(logFile);
            var archivePath = Path.Combine(archiveDirectory, fileName);
            
            File.Move(logFile, archivePath);
            _logger.LogInformation("Archived log file: {FileName}", fileName);
        }

        // Clean up in-memory logs for completed migrations older than the threshold
        var expiredMigrations = _migrationLogs.Keys
            .Where(id => _migrationLogs[id].Any() && _migrationLogs[id].Max(l => l.Timestamp) < olderThan)
            .ToList();

        foreach (var migrationId in expiredMigrations)
        {
            _migrationLogs.TryRemove(migrationId, out _);
        }

        await Task.CompletedTask;
    }

    private async Task LogToMemoryAsync(Guid migrationId, MigrationError logEntry)
    {
        _migrationLogs.AddOrUpdate(migrationId, 
            new List<MigrationError> { logEntry },
            (key, existing) => 
            {
                existing.Add(logEntry);
                // Keep only the last 1000 entries per migration to prevent memory issues
                if (existing.Count > 1000)
                {
                    existing.RemoveRange(0, existing.Count - 1000);
                }
                return existing;
            });

        await Task.CompletedTask;
    }

    private async Task LogToFileAsync(Guid migrationId, MigrationError logEntry)
    {
        if (!_loggingOptions.LogToFile)
            return;

        await _fileLock.WaitAsync();
        try
        {
            var logFileName = $"migration-{migrationId}.log";
            var logFilePath = Path.Combine(_logDirectory, logFileName);
            
            // Check file size and rotate if necessary
            await RotateLogFileIfNeeded(logFilePath);
            
            var logLine = FormatLogEntry(logEntry);
            await File.AppendAllTextAsync(logFilePath, logLine);
            
            // Also write structured log entry
            await WriteStructuredLogAsync(migrationId, logEntry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write migration log to file for {MigrationId}", migrationId);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    private async Task WriteStructuredLogAsync(Guid migrationId, MigrationError logEntry)
    {
        var structuredEntry = new StructuredLogEntry
        {
            Timestamp = logEntry.Timestamp,
            MigrationId = migrationId,
            Level = logEntry.Level.ToString(),
            Component = logEntry.Component,
            Message = logEntry.Message,
            Details = logEntry.Details,
            StackTrace = logEntry.StackTrace,
            TableName = logEntry.TableName,
            RecordNumber = logEntry.RecordNumber,
            Properties = new Dictionary<string, object>()
        };

        // Add to structured logs collection
        _structuredLogs.AddOrUpdate(migrationId,
            new List<StructuredLogEntry> { structuredEntry },
            (key, existing) =>
            {
                existing.Add(structuredEntry);
                // Keep only the last 5000 structured entries per migration
                if (existing.Count > 5000)
                {
                    existing.RemoveRange(0, existing.Count - 5000);
                }
                return existing;
            });

        // Write to structured log file (JSON format)
        var structuredLogFileName = $"migration-{migrationId}-structured.jsonl";
        var structuredLogPath = Path.Combine(_logDirectory, structuredLogFileName);
        
        var jsonLine = JsonSerializer.Serialize(structuredEntry) + Environment.NewLine;
        await File.AppendAllTextAsync(structuredLogPath, jsonLine);
    }

    private string FormatLogEntry(MigrationError logEntry)
    {
        var sb = new StringBuilder();
        sb.Append($"{logEntry.Timestamp:yyyy-MM-dd HH:mm:ss.fff} ");
        sb.Append($"[{logEntry.Level}] ");
        sb.Append($"{logEntry.Component}: ");
        sb.Append(logEntry.Message);
        
        if (!string.IsNullOrEmpty(logEntry.Details))
        {
            sb.Append($" - {logEntry.Details}");
        }
        
        if (!string.IsNullOrEmpty(logEntry.TableName))
        {
            sb.Append($" (Table: {logEntry.TableName}");
            if (logEntry.RecordNumber.HasValue)
            {
                sb.Append($", Record: {logEntry.RecordNumber}");
            }
            sb.Append(")");
        }
        
        if (_loggingOptions.IncludeStackTrace && !string.IsNullOrEmpty(logEntry.StackTrace))
        {
            sb.AppendLine();
            sb.Append($"StackTrace: {logEntry.StackTrace}");
        }
        
        sb.AppendLine();
        return sb.ToString();
    }

    private async Task RotateLogFileIfNeeded(string logFilePath)
    {
        if (!File.Exists(logFilePath))
            return;

        var fileInfo = new FileInfo(logFilePath);
        var maxSizeBytes = _loggingOptions.MaxLogFileSizeMB * 1024 * 1024;
        
        if (fileInfo.Length > maxSizeBytes)
        {
            // Rotate log files
            for (int i = _loggingOptions.MaxLogFiles - 1; i > 0; i--)
            {
                var oldFile = $"{logFilePath}.{i}";
                var newFile = $"{logFilePath}.{i + 1}";
                
                if (File.Exists(oldFile))
                {
                    if (i == _loggingOptions.MaxLogFiles - 1)
                    {
                        File.Delete(oldFile); // Delete oldest file
                    }
                    else
                    {
                        File.Move(oldFile, newFile);
                    }
                }
            }
            
            // Move current file to .1
            File.Move(logFilePath, $"{logFilePath}.1");
        }
        
        await Task.CompletedTask;
    }

    private void InitializeLogRotation()
    {
        // Clean up old log files on startup
        var logFiles = Directory.GetFiles(_logDirectory, "*.log.*")
            .Where(f => File.GetCreationTime(f) < DateTime.Now.AddDays(-30));
            
        foreach (var file in logFiles)
        {
            try
            {
                File.Delete(file);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete old log file: {FileName}", file);
            }
        }
    }

    // Enhanced logging methods for structured logging
    public async Task LogStructuredAsync(Guid migrationId, LabResultsApi.Models.Migration.LogLevel level, string component, string message, 
        Dictionary<string, object>? properties = null, Exception? exception = null)
    {
        if (!ShouldLog(level))
            return;

        var logEntry = new MigrationError
        {
            Timestamp = DateTime.UtcNow,
            Level = ConvertLogLevel(level),
            Component = component,
            Message = message,
            Details = properties != null ? JsonSerializer.Serialize(properties) : null,
            StackTrace = exception?.StackTrace
        };

        await LogToMemoryAsync(migrationId, logEntry);
        await LogToFileAsync(migrationId, logEntry);
        
        // Log to standard logger as well
        LogToStandardLogger(level, component, message, properties, exception);
    }

    public async Task LogAsync(Guid migrationId, LabResultsApi.Models.Migration.LogLevel level, string message, Exception? exception = null)
    {
        await LogStructuredAsync(migrationId, level, "Migration", message, null, exception);
    }

    public async Task<List<StructuredLogEntry>> SearchLogsAsync(Guid migrationId, LogSearchCriteria criteria)
    {
        var logs = _structuredLogs.GetValueOrDefault(migrationId, new List<StructuredLogEntry>());
        
        var filteredLogs = logs.AsQueryable();
        
        if (criteria.StartTime.HasValue)
        {
            filteredLogs = filteredLogs.Where(l => l.Timestamp >= criteria.StartTime.Value);
        }
        
        if (criteria.EndTime.HasValue)
        {
            filteredLogs = filteredLogs.Where(l => l.Timestamp <= criteria.EndTime.Value);
        }
        
        if (!string.IsNullOrEmpty(criteria.Component))
        {
            filteredLogs = filteredLogs.Where(l => l.Component.Contains(criteria.Component, StringComparison.OrdinalIgnoreCase));
        }
        
        if (!string.IsNullOrEmpty(criteria.Message))
        {
            filteredLogs = filteredLogs.Where(l => l.Message.Contains(criteria.Message, StringComparison.OrdinalIgnoreCase));
        }
        
        if (criteria.Levels?.Any() == true)
        {
            var levelStrings = criteria.Levels.Select(l => l.ToString()).ToList();
            filteredLogs = filteredLogs.Where(l => levelStrings.Contains(l.Level));
        }
        
        if (!string.IsNullOrEmpty(criteria.TableName))
        {
            filteredLogs = filteredLogs.Where(l => l.TableName == criteria.TableName);
        }
        
        var result = filteredLogs
            .OrderBy(l => l.Timestamp)
            .Skip(criteria.Skip)
            .Take(criteria.Take)
            .ToList();
            
        return await Task.FromResult(result);
    }

    public async Task<LogAggregationResult> GetLogAggregationAsync(Guid migrationId, TimeSpan timeWindow)
    {
        var logs = _structuredLogs.GetValueOrDefault(migrationId, new List<StructuredLogEntry>());
        var now = DateTime.UtcNow;
        var windowStart = now.Subtract(timeWindow);
        
        var recentLogs = logs.Where(l => l.Timestamp >= windowStart).ToList();
        
        var aggregation = new LogAggregationResult
        {
            TimeWindow = timeWindow,
            TotalEntries = recentLogs.Count,
            ErrorCount = recentLogs.Count(l => l.Level == "Error" || l.Level == "Critical"),
            WarningCount = recentLogs.Count(l => l.Level == "Warning"),
            InfoCount = recentLogs.Count(l => l.Level == "Information" || l.Level == "Info"),
            ComponentBreakdown = recentLogs
                .GroupBy(l => l.Component)
                .ToDictionary(g => g.Key, g => g.Count()),
            HourlyBreakdown = recentLogs
                .GroupBy(l => l.Timestamp.Hour)
                .ToDictionary(g => g.Key, g => g.Count()),
            TopErrors = recentLogs
                .Where(l => l.Level == "Error" || l.Level == "Critical")
                .GroupBy(l => l.Message)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .ToDictionary(g => g.Key, g => g.Count())
        };
        
        return await Task.FromResult(aggregation);
    }

    private bool ShouldLog(LabResultsApi.Models.Migration.LogLevel level)
    {
        return level >= _loggingOptions.MinimumLevel;
    }

    private ErrorLevel ConvertLogLevel(LabResultsApi.Models.Migration.LogLevel level)
    {
        return level switch
        {
            LabResultsApi.Models.Migration.LogLevel.Critical => ErrorLevel.Critical,
            LabResultsApi.Models.Migration.LogLevel.Error => ErrorLevel.Error,
            LabResultsApi.Models.Migration.LogLevel.Warning => ErrorLevel.Warning,
            LabResultsApi.Models.Migration.LogLevel.Information => ErrorLevel.Info,
            LabResultsApi.Models.Migration.LogLevel.Debug => ErrorLevel.Info,
            LabResultsApi.Models.Migration.LogLevel.Trace => ErrorLevel.Info,
            _ => ErrorLevel.Info
        };
    }

    private void LogToStandardLogger(LabResultsApi.Models.Migration.LogLevel level, string component, string message, 
        Dictionary<string, object>? properties, Exception? exception)
    {
        if (!_loggingOptions.LogToConsole)
            return;

        var logMessage = $"[{component}] {message}";
        if (properties?.Any() == true)
        {
            logMessage += $" {@properties}";
        }

        switch (level)
        {
            case LabResultsApi.Models.Migration.LogLevel.Critical:
                _logger.LogCritical(exception, logMessage, properties);
                break;
            case LabResultsApi.Models.Migration.LogLevel.Error:
                _logger.LogError(exception, logMessage, properties);
                break;
            case LabResultsApi.Models.Migration.LogLevel.Warning:
                _logger.LogWarning(exception, logMessage, properties);
                break;
            case LabResultsApi.Models.Migration.LogLevel.Information:
                _logger.LogInformation(logMessage, properties);
                break;
            case LabResultsApi.Models.Migration.LogLevel.Debug:
                _logger.LogDebug(logMessage, properties);
                break;
            case LabResultsApi.Models.Migration.LogLevel.Trace:
                _logger.LogTrace(logMessage, properties);
                break;
        }
    }
}

// Supporting classes for enhanced logging
public class StructuredLogEntry
{
    public DateTime Timestamp { get; set; }
    public Guid MigrationId { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Component { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? StackTrace { get; set; }
    public string? TableName { get; set; }
    public int? RecordNumber { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
}

public class LogSearchCriteria
{
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? Component { get; set; }
    public string? Message { get; set; }
    public List<LabResultsApi.Models.Migration.LogLevel>? Levels { get; set; }
    public string? TableName { get; set; }
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 100;
}

public class LogAggregationResult
{
    public TimeSpan TimeWindow { get; set; }
    public int TotalEntries { get; set; }
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
    public int InfoCount { get; set; }
    public Dictionary<string, int> ComponentBreakdown { get; set; } = new();
    public Dictionary<int, int> HourlyBreakdown { get; set; } = new();
    public Dictionary<string, int> TopErrors { get; set; } = new();
}