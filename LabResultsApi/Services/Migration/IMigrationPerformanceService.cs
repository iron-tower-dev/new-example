using LabResultsApi.Models.Migration;

namespace LabResultsApi.Services.Migration;

public interface IMigrationPerformanceService
{
    // Database operation metrics
    Task RecordDatabaseOperationAsync(Guid migrationId, string operation, string tableName, 
        TimeSpan duration, int recordsAffected, bool isSuccessful = true);
    
    // Resource utilization monitoring
    Task RecordResourceUtilizationAsync(Guid migrationId, ResourceUtilizationMetrics metrics);
    
    // Migration phase performance
    Task RecordPhasePerformanceAsync(Guid migrationId, string phaseName, TimeSpan duration, 
        int recordsProcessed, bool isSuccessful = true);
    
    // Performance alerts and thresholds
    Task CheckPerformanceThresholdsAsync(Guid migrationId);
    Task<List<PerformanceAlert>> GetActiveAlertsAsync(Guid migrationId);
    
    // Performance reporting
    Task<MigrationPerformanceReport> GeneratePerformanceReportAsync(Guid migrationId);
    Task<List<PerformanceMetric>> GetPerformanceMetricsAsync(Guid migrationId, TimeSpan timeWindow);
    Task<ResourceUtilizationTrend> GetResourceTrendAsync(Guid migrationId, TimeSpan timeWindow);
    
    // Performance optimization suggestions
    Task<List<PerformanceOptimizationSuggestion>> GetOptimizationSuggestionsAsync(Guid migrationId);
    
    // Real-time monitoring
    Task StartRealTimeMonitoringAsync(Guid migrationId);
    Task StopRealTimeMonitoringAsync(Guid migrationId);
    Task<RealTimePerformanceData> GetRealTimeDataAsync(Guid migrationId);
}

public class ResourceUtilizationMetrics
{
    public DateTime Timestamp { get; set; }
    public double CpuUsagePercent { get; set; }
    public long MemoryUsageBytes { get; set; }
    public long AvailableMemoryBytes { get; set; }
    public double DiskUsagePercent { get; set; }
    public long DiskIOReadBytes { get; set; }
    public long DiskIOWriteBytes { get; set; }
    public int ActiveDatabaseConnections { get; set; }
    public double DatabaseCpuPercent { get; set; }
    public long DatabaseMemoryUsageBytes { get; set; }
}

public class PerformanceAlert
{
    public Guid AlertId { get; set; }
    public Guid MigrationId { get; set; }
    public DateTime Timestamp { get; set; }
    public AlertSeverity Severity { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Component { get; set; } = string.Empty;
    public Dictionary<string, object> Metrics { get; set; } = new();
    public bool IsResolved { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public enum AlertSeverity
{
    Info,
    Warning,
    Critical
}

public class MigrationPerformanceReport
{
    public Guid MigrationId { get; set; }
    public DateTime GeneratedAt { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public int TotalRecordsProcessed { get; set; }
    public double RecordsPerSecond { get; set; }
    public List<PhasePerformance> PhasePerformances { get; set; } = new();
    public List<DatabaseOperationSummary> DatabaseOperations { get; set; } = new();
    public ResourceUtilizationSummary ResourceUtilization { get; set; } = new();
    public List<PerformanceAlert> Alerts { get; set; } = new();
    public List<PerformanceOptimizationSuggestion> Suggestions { get; set; } = new();
}

public class PhasePerformance
{
    public string PhaseName { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public int RecordsProcessed { get; set; }
    public double RecordsPerSecond { get; set; }
    public bool IsSuccessful { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class DatabaseOperationSummary
{
    public string Operation { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public int TotalOperations { get; set; }
    public int SuccessfulOperations { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public TimeSpan AverageDuration { get; set; }
    public TimeSpan MinDuration { get; set; }
    public TimeSpan MaxDuration { get; set; }
    public int TotalRecordsAffected { get; set; }
    public double RecordsPerSecond { get; set; }
}

public class ResourceUtilizationSummary
{
    public double AverageCpuUsage { get; set; }
    public double PeakCpuUsage { get; set; }
    public long AverageMemoryUsage { get; set; }
    public long PeakMemoryUsage { get; set; }
    public double AverageDiskUsage { get; set; }
    public long TotalDiskIORead { get; set; }
    public long TotalDiskIOWrite { get; set; }
    public int PeakDatabaseConnections { get; set; }
    public double AverageDatabaseCpu { get; set; }
}

public class PerformanceMetric
{
    public DateTime Timestamp { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public Dictionary<string, string> Tags { get; set; } = new();
}

public class ResourceUtilizationTrend
{
    public List<DataPoint> CpuUsage { get; set; } = new();
    public List<DataPoint> MemoryUsage { get; set; } = new();
    public List<DataPoint> DiskUsage { get; set; } = new();
    public List<DataPoint> DatabaseConnections { get; set; } = new();
}

public class DataPoint
{
    public DateTime Timestamp { get; set; }
    public double Value { get; set; }
}

public class PerformanceOptimizationSuggestion
{
    public string Category { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Impact { get; set; } = string.Empty;
    public string Implementation { get; set; } = string.Empty;
    public int Priority { get; set; }
}

public class RealTimePerformanceData
{
    public DateTime Timestamp { get; set; }
    public ResourceUtilizationMetrics CurrentUtilization { get; set; } = new();
    public List<ActiveOperation> ActiveOperations { get; set; } = new();
    public double CurrentThroughput { get; set; }
    public TimeSpan EstimatedTimeRemaining { get; set; }
}

public class ActiveOperation
{
    public string OperationType { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public int RecordsProcessed { get; set; }
    public int TotalRecords { get; set; }
    public double ProgressPercent { get; set; }
}