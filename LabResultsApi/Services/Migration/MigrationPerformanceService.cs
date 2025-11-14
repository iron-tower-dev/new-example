using LabResultsApi.Models.Migration;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace LabResultsApi.Services.Migration;

public class MigrationPerformanceService : IMigrationPerformanceService
{
    private readonly ILogger<MigrationPerformanceService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IMigrationLoggingService _loggingService;
    private readonly ConcurrentDictionary<Guid, MigrationPerformanceData> _performanceData;
    private readonly ConcurrentDictionary<Guid, List<PerformanceAlert>> _alerts;
    private readonly ConcurrentDictionary<Guid, Timer> _monitoringTimers;
    private readonly PerformanceThresholds _thresholds;

    public MigrationPerformanceService(
        ILogger<MigrationPerformanceService> logger,
        IConfiguration configuration,
        IMigrationLoggingService loggingService)
    {
        _logger = logger;
        _configuration = configuration;
        _loggingService = loggingService;
        _performanceData = new ConcurrentDictionary<Guid, MigrationPerformanceData>();
        _alerts = new ConcurrentDictionary<Guid, List<PerformanceAlert>>();
        _monitoringTimers = new ConcurrentDictionary<Guid, Timer>();
        _thresholds = _configuration.GetSection("Migration:Performance:Thresholds")
            .Get<PerformanceThresholds>() ?? new PerformanceThresholds();
    }

    public async Task RecordDatabaseOperationAsync(Guid migrationId, string operation, string tableName, 
        TimeSpan duration, int recordsAffected, bool isSuccessful = true)
    {
        var data = _performanceData.GetOrAdd(migrationId, _ => new MigrationPerformanceData());
        
        var operationRecord = new DatabaseOperationRecord
        {
            Timestamp = DateTime.UtcNow,
            Operation = operation,
            TableName = tableName,
            Duration = duration,
            RecordsAffected = recordsAffected,
            IsSuccessful = isSuccessful
        };

        data.DatabaseOperations.Add(operationRecord);

        // Log structured performance data
        var properties = new Dictionary<string, object>
        {
            ["Operation"] = operation,
            ["TableName"] = tableName,
            ["Duration"] = duration.TotalMilliseconds,
            ["RecordsAffected"] = recordsAffected,
            ["RecordsPerSecond"] = recordsAffected / Math.Max(duration.TotalSeconds, 0.001),
            ["IsSuccessful"] = isSuccessful
        };

        await _loggingService.LogStructuredAsync(migrationId, LabResultsApi.Models.Migration.LogLevel.Information, 
            "DatabaseOperation", $"Database operation {operation} on {tableName}", properties);

        // Check for performance issues
        await CheckDatabaseOperationThresholds(migrationId, operationRecord);
    }

    public async Task RecordResourceUtilizationAsync(Guid migrationId, ResourceUtilizationMetrics metrics)
    {
        var data = _performanceData.GetOrAdd(migrationId, _ => new MigrationPerformanceData());
        data.ResourceMetrics.Add(metrics);

        // Keep only the last 1000 resource metrics to prevent memory issues
        if (data.ResourceMetrics.Count > 1000)
        {
            data.ResourceMetrics.RemoveRange(0, data.ResourceMetrics.Count - 1000);
        }

        // Check resource utilization thresholds
        await CheckResourceThresholds(migrationId, metrics);
    }

    public async Task RecordPhasePerformanceAsync(Guid migrationId, string phaseName, TimeSpan duration, 
        int recordsProcessed, bool isSuccessful = true)
    {
        var data = _performanceData.GetOrAdd(migrationId, _ => new MigrationPerformanceData());
        
        var phaseRecord = new PhasePerformanceRecord
        {
            PhaseName = phaseName,
            StartTime = DateTime.UtcNow.Subtract(duration),
            EndTime = DateTime.UtcNow,
            Duration = duration,
            RecordsProcessed = recordsProcessed,
            IsSuccessful = isSuccessful
        };

        data.PhasePerformances.Add(phaseRecord);

        var properties = new Dictionary<string, object>
        {
            ["PhaseName"] = phaseName,
            ["Duration"] = duration.TotalMilliseconds,
            ["RecordsProcessed"] = recordsProcessed,
            ["RecordsPerSecond"] = recordsProcessed / Math.Max(duration.TotalSeconds, 0.001),
            ["IsSuccessful"] = isSuccessful
        };

        await _loggingService.LogStructuredAsync(migrationId, LabResultsApi.Models.Migration.LogLevel.Information, 
            "PhasePerformance", $"Phase {phaseName} completed", properties);
    }

    public async Task CheckPerformanceThresholdsAsync(Guid migrationId)
    {
        var data = _performanceData.GetValueOrDefault(migrationId);
        if (data == null) return;

        var alerts = new List<PerformanceAlert>();

        // Check recent resource utilization
        var recentMetrics = data.ResourceMetrics
            .Where(m => m.Timestamp > DateTime.UtcNow.AddMinutes(-5))
            .ToList();

        if (recentMetrics.Any())
        {
            var avgCpu = recentMetrics.Average(m => m.CpuUsagePercent);
            var avgMemory = recentMetrics.Average(m => m.MemoryUsageBytes);
            var maxConnections = recentMetrics.Max(m => m.ActiveDatabaseConnections);

            if (avgCpu > _thresholds.CpuUsageThreshold)
            {
                alerts.Add(CreateAlert(migrationId, AlertSeverity.Warning, "HighCpuUsage",
                    $"High CPU usage detected: {avgCpu:F1}%", "ResourceMonitoring",
                    new Dictionary<string, object> { ["CpuUsage"] = avgCpu }));
            }

            if (avgMemory > _thresholds.MemoryUsageThreshold)
            {
                alerts.Add(CreateAlert(migrationId, AlertSeverity.Warning, "HighMemoryUsage",
                    $"High memory usage detected: {avgMemory / 1024 / 1024:F0} MB", "ResourceMonitoring",
                    new Dictionary<string, object> { ["MemoryUsage"] = avgMemory }));
            }

            if (maxConnections > _thresholds.DatabaseConnectionThreshold)
            {
                alerts.Add(CreateAlert(migrationId, AlertSeverity.Critical, "HighDatabaseConnections",
                    $"High database connection count: {maxConnections}", "DatabaseMonitoring",
                    new Dictionary<string, object> { ["ConnectionCount"] = maxConnections }));
            }
        }

        // Check slow operations
        var recentOperations = data.DatabaseOperations
            .Where(op => op.Timestamp > DateTime.UtcNow.AddMinutes(-10))
            .ToList();

        var slowOperations = recentOperations
            .Where(op => op.Duration.TotalSeconds > _thresholds.SlowOperationThreshold)
            .ToList();

        foreach (var slowOp in slowOperations.Take(5)) // Limit to 5 alerts
        {
            alerts.Add(CreateAlert(migrationId, AlertSeverity.Warning, "SlowDatabaseOperation",
                $"Slow operation detected: {slowOp.Operation} on {slowOp.TableName} took {slowOp.Duration.TotalSeconds:F1}s",
                "DatabaseMonitoring",
                new Dictionary<string, object> 
                { 
                    ["Operation"] = slowOp.Operation,
                    ["TableName"] = slowOp.TableName,
                    ["Duration"] = slowOp.Duration.TotalSeconds
                }));
        }

        // Add alerts to collection
        if (alerts.Any())
        {
            _alerts.AddOrUpdate(migrationId, alerts, (key, existing) =>
            {
                existing.AddRange(alerts);
                return existing;
            });

            // Log alerts
            foreach (var alert in alerts)
            {
                await _loggingService.LogStructuredAsync(migrationId, 
                    alert.Severity == AlertSeverity.Critical ? LabResultsApi.Models.Migration.LogLevel.Error : LabResultsApi.Models.Migration.LogLevel.Warning,
                    "PerformanceAlert", alert.Message, alert.Metrics);
            }
        }
    }

    public async Task<List<PerformanceAlert>> GetActiveAlertsAsync(Guid migrationId)
    {
        var alerts = _alerts.GetValueOrDefault(migrationId, new List<PerformanceAlert>());
        return await Task.FromResult(alerts.Where(a => !a.IsResolved).ToList());
    }

    public async Task<MigrationPerformanceReport> GeneratePerformanceReportAsync(Guid migrationId)
    {
        var data = _performanceData.GetValueOrDefault(migrationId);
        if (data == null)
        {
            return new MigrationPerformanceReport { MigrationId = migrationId, GeneratedAt = DateTime.UtcNow };
        }

        var totalDuration = data.PhasePerformances.Any() 
            ? data.PhasePerformances.Max(p => p.EndTime) - data.PhasePerformances.Min(p => p.StartTime)
            : TimeSpan.Zero;

        var totalRecords = data.DatabaseOperations.Sum(op => op.RecordsAffected);
        var recordsPerSecond = totalDuration.TotalSeconds > 0 ? totalRecords / totalDuration.TotalSeconds : 0;

        var report = new MigrationPerformanceReport
        {
            MigrationId = migrationId,
            GeneratedAt = DateTime.UtcNow,
            TotalDuration = totalDuration,
            TotalRecordsProcessed = totalRecords,
            RecordsPerSecond = recordsPerSecond,
            PhasePerformances = data.PhasePerformances.Select(p => new PhasePerformance
            {
                PhaseName = p.PhaseName,
                Duration = p.Duration,
                RecordsProcessed = p.RecordsProcessed,
                RecordsPerSecond = p.Duration.TotalSeconds > 0 ? p.RecordsProcessed / p.Duration.TotalSeconds : 0,
                IsSuccessful = p.IsSuccessful
            }).ToList(),
            DatabaseOperations = GenerateDatabaseOperationSummary(data.DatabaseOperations),
            ResourceUtilization = GenerateResourceUtilizationSummary(data.ResourceMetrics),
            Alerts = _alerts.GetValueOrDefault(migrationId, new List<PerformanceAlert>()),
            Suggestions = await GetOptimizationSuggestionsAsync(migrationId)
        };

        return report;
    }

    public async Task<List<PerformanceMetric>> GetPerformanceMetricsAsync(Guid migrationId, TimeSpan timeWindow)
    {
        var data = _performanceData.GetValueOrDefault(migrationId);
        if (data == null) return new List<PerformanceMetric>();

        var cutoff = DateTime.UtcNow.Subtract(timeWindow);
        var metrics = new List<PerformanceMetric>();

        // Resource metrics
        var recentResourceMetrics = data.ResourceMetrics.Where(m => m.Timestamp >= cutoff).ToList();
        foreach (var metric in recentResourceMetrics)
        {
            metrics.AddRange(new[]
            {
                new PerformanceMetric { Timestamp = metric.Timestamp, MetricName = "CpuUsage", Value = metric.CpuUsagePercent, Unit = "%" },
                new PerformanceMetric { Timestamp = metric.Timestamp, MetricName = "MemoryUsage", Value = metric.MemoryUsageBytes / 1024.0 / 1024.0, Unit = "MB" },
                new PerformanceMetric { Timestamp = metric.Timestamp, MetricName = "DatabaseConnections", Value = metric.ActiveDatabaseConnections, Unit = "count" }
            });
        }

        // Database operation metrics
        var recentOperations = data.DatabaseOperations.Where(op => op.Timestamp >= cutoff).ToList();
        var operationGroups = recentOperations.GroupBy(op => new { op.Timestamp.Hour, op.Timestamp.Minute });
        
        foreach (var group in operationGroups)
        {
            var timestamp = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, group.Key.Hour, group.Key.Minute, 0);
            var avgDuration = group.Average(op => op.Duration.TotalMilliseconds);
            var totalRecords = group.Sum(op => op.RecordsAffected);
            
            metrics.AddRange(new[]
            {
                new PerformanceMetric { Timestamp = timestamp, MetricName = "AvgOperationDuration", Value = avgDuration, Unit = "ms" },
                new PerformanceMetric { Timestamp = timestamp, MetricName = "RecordsProcessed", Value = totalRecords, Unit = "count" }
            });
        }

        return await Task.FromResult(metrics.OrderBy(m => m.Timestamp).ToList());
    }

    public async Task<ResourceUtilizationTrend> GetResourceTrendAsync(Guid migrationId, TimeSpan timeWindow)
    {
        var data = _performanceData.GetValueOrDefault(migrationId);
        if (data == null) return new ResourceUtilizationTrend();

        var cutoff = DateTime.UtcNow.Subtract(timeWindow);
        var recentMetrics = data.ResourceMetrics.Where(m => m.Timestamp >= cutoff).OrderBy(m => m.Timestamp).ToList();

        var trend = new ResourceUtilizationTrend
        {
            CpuUsage = recentMetrics.Select(m => new DataPoint { Timestamp = m.Timestamp, Value = m.CpuUsagePercent }).ToList(),
            MemoryUsage = recentMetrics.Select(m => new DataPoint { Timestamp = m.Timestamp, Value = m.MemoryUsageBytes / 1024.0 / 1024.0 }).ToList(),
            DiskUsage = recentMetrics.Select(m => new DataPoint { Timestamp = m.Timestamp, Value = m.DiskUsagePercent }).ToList(),
            DatabaseConnections = recentMetrics.Select(m => new DataPoint { Timestamp = m.Timestamp, Value = m.ActiveDatabaseConnections }).ToList()
        };

        return await Task.FromResult(trend);
    }

    public async Task<List<PerformanceOptimizationSuggestion>> GetOptimizationSuggestionsAsync(Guid migrationId)
    {
        var data = _performanceData.GetValueOrDefault(migrationId);
        if (data == null) return new List<PerformanceOptimizationSuggestion>();

        var suggestions = new List<PerformanceOptimizationSuggestion>();

        // Analyze database operations for optimization opportunities
        var operationGroups = data.DatabaseOperations.GroupBy(op => op.TableName).ToList();
        
        foreach (var group in operationGroups)
        {
            var avgDuration = group.Average(op => op.Duration.TotalMilliseconds);
            var totalRecords = group.Sum(op => op.RecordsAffected);
            var recordsPerSecond = group.Average(op => op.RecordsAffected / Math.Max(op.Duration.TotalSeconds, 0.001));

            if (avgDuration > 5000) // Operations taking more than 5 seconds
            {
                suggestions.Add(new PerformanceOptimizationSuggestion
                {
                    Category = "Database",
                    Title = $"Optimize operations on {group.Key}",
                    Description = $"Operations on table {group.Key} are taking an average of {avgDuration:F0}ms",
                    Impact = "High",
                    Implementation = "Consider adding indexes, optimizing batch sizes, or using bulk operations",
                    Priority = 1
                });
            }

            if (recordsPerSecond < 100) // Less than 100 records per second
            {
                suggestions.Add(new PerformanceOptimizationSuggestion
                {
                    Category = "Throughput",
                    Title = $"Improve throughput for {group.Key}",
                    Description = $"Processing only {recordsPerSecond:F0} records per second for {group.Key}",
                    Impact = "Medium",
                    Implementation = "Increase batch size or implement parallel processing",
                    Priority = 2
                });
            }
        }

        // Analyze resource utilization
        if (data.ResourceMetrics.Any())
        {
            var avgCpu = data.ResourceMetrics.Average(m => m.CpuUsagePercent);
            var avgMemory = data.ResourceMetrics.Average(m => m.MemoryUsageBytes);

            if (avgCpu < 30)
            {
                suggestions.Add(new PerformanceOptimizationSuggestion
                {
                    Category = "Resource Utilization",
                    Title = "Increase parallelism",
                    Description = $"CPU utilization is low ({avgCpu:F1}%), indicating potential for increased parallelism",
                    Impact = "Medium",
                    Implementation = "Increase the number of concurrent operations or batch processing threads",
                    Priority = 3
                });
            }

            if (avgMemory > _thresholds.MemoryUsageThreshold * 0.8)
            {
                suggestions.Add(new PerformanceOptimizationSuggestion
                {
                    Category = "Memory Management",
                    Title = "Optimize memory usage",
                    Description = $"Memory usage is high ({avgMemory / 1024 / 1024:F0} MB)",
                    Impact = "High",
                    Implementation = "Reduce batch sizes, implement streaming for large datasets, or increase available memory",
                    Priority = 1
                });
            }
        }

        return await Task.FromResult(suggestions.OrderBy(s => s.Priority).ToList());
    }

    public async Task StartRealTimeMonitoringAsync(Guid migrationId)
    {
        if (_monitoringTimers.ContainsKey(migrationId))
        {
            await StopRealTimeMonitoringAsync(migrationId);
        }

        var timer = new Timer(async _ => await CollectRealTimeMetrics(migrationId), 
            null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        
        _monitoringTimers.TryAdd(migrationId, timer);
        
        await _loggingService.LogStructuredAsync(migrationId, LabResultsApi.Models.Migration.LogLevel.Information, 
            "PerformanceMonitoring", "Real-time performance monitoring started");
    }

    public async Task StopRealTimeMonitoringAsync(Guid migrationId)
    {
        if (_monitoringTimers.TryRemove(migrationId, out var timer))
        {
            timer.Dispose();
            await _loggingService.LogStructuredAsync(migrationId, LabResultsApi.Models.Migration.LogLevel.Information, 
                "PerformanceMonitoring", "Real-time performance monitoring stopped");
        }
    }

    public async Task<RealTimePerformanceData> GetRealTimeDataAsync(Guid migrationId)
    {
        var data = _performanceData.GetValueOrDefault(migrationId);
        if (data == null)
        {
            return new RealTimePerformanceData { Timestamp = DateTime.UtcNow };
        }

        var latestMetrics = data.ResourceMetrics.LastOrDefault() ?? new ResourceUtilizationMetrics();
        var recentOperations = data.DatabaseOperations
            .Where(op => op.Timestamp > DateTime.UtcNow.AddMinutes(-1))
            .ToList();

        var currentThroughput = recentOperations.Any() 
            ? recentOperations.Sum(op => op.RecordsAffected) / 60.0 // records per second over last minute
            : 0;

        return await Task.FromResult(new RealTimePerformanceData
        {
            Timestamp = DateTime.UtcNow,
            CurrentUtilization = latestMetrics,
            ActiveOperations = new List<ActiveOperation>(), // Would be populated with actual active operations
            CurrentThroughput = currentThroughput,
            EstimatedTimeRemaining = TimeSpan.Zero // Would be calculated based on progress
        });
    }

    private async Task CollectRealTimeMetrics(Guid migrationId)
    {
        try
        {
            var process = Process.GetCurrentProcess();
            var metrics = new ResourceUtilizationMetrics
            {
                Timestamp = DateTime.UtcNow,
                CpuUsagePercent = GetCpuUsage(),
                MemoryUsageBytes = process.WorkingSet64,
                AvailableMemoryBytes = GC.GetTotalMemory(false),
                DiskUsagePercent = GetDiskUsage(),
                DiskIOReadBytes = 0, // Would need performance counters
                DiskIOWriteBytes = 0, // Would need performance counters
                ActiveDatabaseConnections = 0, // Would query from connection pool
                DatabaseCpuPercent = 0, // Would query from database
                DatabaseMemoryUsageBytes = 0 // Would query from database
            };

            await RecordResourceUtilizationAsync(migrationId, metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting real-time metrics for migration {MigrationId}", migrationId);
        }
    }

    private double GetCpuUsage()
    {
        // Simplified CPU usage calculation
        // In production, would use PerformanceCounter or similar
        return Environment.ProcessorCount * 10; // Placeholder
    }

    private double GetDiskUsage()
    {
        try
        {
            var drive = DriveInfo.GetDrives().FirstOrDefault(d => d.IsReady);
            if (drive != null)
            {
                var usedSpace = drive.TotalSize - drive.AvailableFreeSpace;
                return (double)usedSpace / drive.TotalSize * 100;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get disk usage");
        }
        return 0;
    }

    private async Task CheckDatabaseOperationThresholds(Guid migrationId, DatabaseOperationRecord operation)
    {
        if (operation.Duration.TotalSeconds > _thresholds.SlowOperationThreshold)
        {
            var alert = CreateAlert(migrationId, AlertSeverity.Warning, "SlowDatabaseOperation",
                $"Slow operation: {operation.Operation} on {operation.TableName} took {operation.Duration.TotalSeconds:F1}s",
                "DatabaseMonitoring",
                new Dictionary<string, object>
                {
                    ["Operation"] = operation.Operation,
                    ["TableName"] = operation.TableName,
                    ["Duration"] = operation.Duration.TotalSeconds,
                    ["RecordsAffected"] = operation.RecordsAffected
                });

            _alerts.AddOrUpdate(migrationId, new List<PerformanceAlert> { alert }, 
                (key, existing) => { existing.Add(alert); return existing; });
        }
    }

    private async Task CheckResourceThresholds(Guid migrationId, ResourceUtilizationMetrics metrics)
    {
        var alerts = new List<PerformanceAlert>();

        if (metrics.CpuUsagePercent > _thresholds.CpuUsageThreshold)
        {
            alerts.Add(CreateAlert(migrationId, AlertSeverity.Warning, "HighCpuUsage",
                $"High CPU usage: {metrics.CpuUsagePercent:F1}%", "ResourceMonitoring",
                new Dictionary<string, object> { ["CpuUsage"] = metrics.CpuUsagePercent }));
        }

        if (metrics.MemoryUsageBytes > _thresholds.MemoryUsageThreshold)
        {
            alerts.Add(CreateAlert(migrationId, AlertSeverity.Warning, "HighMemoryUsage",
                $"High memory usage: {metrics.MemoryUsageBytes / 1024 / 1024:F0} MB", "ResourceMonitoring",
                new Dictionary<string, object> { ["MemoryUsage"] = metrics.MemoryUsageBytes }));
        }

        if (alerts.Any())
        {
            _alerts.AddOrUpdate(migrationId, alerts, (key, existing) =>
            {
                existing.AddRange(alerts);
                return existing;
            });
        }
    }

    private PerformanceAlert CreateAlert(Guid migrationId, AlertSeverity severity, string alertType, 
        string message, string component, Dictionary<string, object> metrics)
    {
        return new PerformanceAlert
        {
            AlertId = Guid.NewGuid(),
            MigrationId = migrationId,
            Timestamp = DateTime.UtcNow,
            Severity = severity,
            AlertType = alertType,
            Message = message,
            Component = component,
            Metrics = metrics,
            IsResolved = false
        };
    }

    private List<DatabaseOperationSummary> GenerateDatabaseOperationSummary(List<DatabaseOperationRecord> operations)
    {
        return operations
            .GroupBy(op => new { op.Operation, op.TableName })
            .Select(group => new DatabaseOperationSummary
            {
                Operation = group.Key.Operation,
                TableName = group.Key.TableName,
                TotalOperations = group.Count(),
                SuccessfulOperations = group.Count(op => op.IsSuccessful),
                TotalDuration = TimeSpan.FromTicks(group.Sum(op => op.Duration.Ticks)),
                AverageDuration = TimeSpan.FromTicks((long)group.Average(op => op.Duration.Ticks)),
                MinDuration = TimeSpan.FromTicks(group.Min(op => op.Duration.Ticks)),
                MaxDuration = TimeSpan.FromTicks(group.Max(op => op.Duration.Ticks)),
                TotalRecordsAffected = group.Sum(op => op.RecordsAffected),
                RecordsPerSecond = group.Sum(op => op.RecordsAffected) / Math.Max(group.Sum(op => op.Duration.TotalSeconds), 0.001)
            })
            .OrderByDescending(s => s.TotalDuration)
            .ToList();
    }

    private ResourceUtilizationSummary GenerateResourceUtilizationSummary(List<ResourceUtilizationMetrics> metrics)
    {
        if (!metrics.Any()) return new ResourceUtilizationSummary();

        return new ResourceUtilizationSummary
        {
            AverageCpuUsage = metrics.Average(m => m.CpuUsagePercent),
            PeakCpuUsage = metrics.Max(m => m.CpuUsagePercent),
            AverageMemoryUsage = (long)metrics.Average(m => m.MemoryUsageBytes),
            PeakMemoryUsage = metrics.Max(m => m.MemoryUsageBytes),
            AverageDiskUsage = metrics.Average(m => m.DiskUsagePercent),
            TotalDiskIORead = metrics.Sum(m => m.DiskIOReadBytes),
            TotalDiskIOWrite = metrics.Sum(m => m.DiskIOWriteBytes),
            PeakDatabaseConnections = metrics.Max(m => m.ActiveDatabaseConnections),
            AverageDatabaseCpu = metrics.Average(m => m.DatabaseCpuPercent)
        };
    }
}

// Supporting classes
internal class MigrationPerformanceData
{
    public List<DatabaseOperationRecord> DatabaseOperations { get; set; } = new();
    public List<ResourceUtilizationMetrics> ResourceMetrics { get; set; } = new();
    public List<PhasePerformanceRecord> PhasePerformances { get; set; } = new();
}

internal class DatabaseOperationRecord
{
    public DateTime Timestamp { get; set; }
    public string Operation { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public int RecordsAffected { get; set; }
    public bool IsSuccessful { get; set; }
}

internal class PhasePerformanceRecord
{
    public string PhaseName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration { get; set; }
    public int RecordsProcessed { get; set; }
    public bool IsSuccessful { get; set; }
}

public class PerformanceThresholds
{
    public double CpuUsageThreshold { get; set; } = 80.0; // 80%
    public long MemoryUsageThreshold { get; set; } = 2L * 1024 * 1024 * 1024; // 2GB
    public int DatabaseConnectionThreshold { get; set; } = 50;
    public double SlowOperationThreshold { get; set; } = 5.0; // 5 seconds
    public double DiskUsageThreshold { get; set; } = 90.0; // 90%
}