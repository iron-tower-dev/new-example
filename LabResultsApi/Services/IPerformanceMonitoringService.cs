using LabResultsApi.DTOs;

namespace LabResultsApi.Services;

public interface IPerformanceMonitoringService
{
    /// <summary>
    /// Record a database query execution time
    /// </summary>
    void RecordQueryExecution(string queryName, TimeSpan duration, bool isSuccessful = true);
    
    /// <summary>
    /// Record an API endpoint execution time
    /// </summary>
    void RecordEndpointExecution(string endpoint, string method, TimeSpan duration, int statusCode);
    
    /// <summary>
    /// Record cache hit/miss statistics
    /// </summary>
    void RecordCacheOperation(string cacheKey, bool isHit, TimeSpan? duration = null);
    
    /// <summary>
    /// Get performance metrics for monitoring dashboard
    /// </summary>
    Task<PerformanceMetricsDto> GetPerformanceMetricsAsync();
    
    /// <summary>
    /// Get slow query report
    /// </summary>
    Task<IEnumerable<SlowQueryDto>> GetSlowQueriesAsync(int topCount = 10);
    
    /// <summary>
    /// Get cache performance statistics
    /// </summary>
    Task<CachePerformanceDto> GetCachePerformanceAsync();
    
    /// <summary>
    /// Reset performance counters
    /// </summary>
    void ResetCounters();
}