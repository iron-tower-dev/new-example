using LabResultsApi.Models.Migration;

namespace LabResultsApi.Services.Migration;

public interface IMigrationLoggingService
{
    // Existing methods
    Task LogMigrationStartAsync(Guid migrationId, MigrationOptions options);
    Task LogMigrationEndAsync(Guid migrationId, MigrationStatus status, TimeSpan duration);
    Task LogProgressAsync(Guid migrationId, double progressPercentage, string operation);
    Task LogErrorAsync(Guid migrationId, MigrationError error);
    Task LogPhaseStartAsync(Guid migrationId, string phaseName);
    Task LogPhaseEndAsync(Guid migrationId, string phaseName, bool success, TimeSpan duration);
    Task<List<MigrationError>> GetMigrationLogsAsync(Guid migrationId);
    Task<string> GenerateLogReportAsync(Guid migrationId);
    Task ArchiveLogsAsync(DateTime olderThan);
    
    // Enhanced structured logging methods
    Task LogStructuredAsync(Guid migrationId, LabResultsApi.Models.Migration.LogLevel level, string component, string message, 
        Dictionary<string, object>? properties = null, Exception? exception = null);
    Task LogAsync(Guid migrationId, LabResultsApi.Models.Migration.LogLevel level, string message, Exception? exception = null);
    Task<List<StructuredLogEntry>> SearchLogsAsync(Guid migrationId, LogSearchCriteria criteria);
    Task<LogAggregationResult> GetLogAggregationAsync(Guid migrationId, TimeSpan timeWindow);
}

