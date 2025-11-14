using System.Collections.Concurrent;
using LabResultsApi.DTOs;

namespace LabResultsApi.Services;

public class PerformanceMonitoringService : IPerformanceMonitoringService
{
    private readonly ILogger<PerformanceMonitoringService> _logger;
    private readonly ConcurrentDictionary<string, QueryMetrics> _queryMetrics = new();
    private readonly ConcurrentDictionary<string, EndpointMetrics> _endpointMetrics = new();
    private readonly ConcurrentDictionary<string, CacheMetrics> _cacheMetrics = new();
    private readonly object _lockObject = new();
    private DateTime _startTime = DateTime.UtcNow;

    public PerformanceMonitoringService(ILogger<PerformanceMonitoringService> logger)
    {
        _logger = logger;
    }

    public void RecordQueryExecution(string queryName, TimeSpan duration, bool isSuccessful = true)
    {
        try
        {
            _queryMetrics.AddOrUpdate(queryName, 
                new QueryMetrics 
                { 
                    QueryName = queryName,
                    TotalExecutions = 1,
                    SuccessfulExecutions = isSuccessful ? 1 : 0,
                    TotalDuration = duration,
                    MinDuration = duration,
                    MaxDuration = duration,
                    LastExecuted = DateTime.UtcNow
                },
                (key, existing) =>
                {
                    existing.TotalExecutions++;
                    if (isSuccessful) existing.SuccessfulExecutions++;
                    existing.TotalDuration = existing.TotalDuration.Add(duration);
                    existing.MinDuration = duration < existing.MinDuration ? duration : existing.MinDuration;
                    existing.MaxDuration = duration > existing.MaxDuration ? duration : existing.MaxDuration;
                    existing.LastExecuted = DateTime.UtcNow;
                    return existing;
                });

            // Log slow queries (over 1 second)
            if (duration.TotalSeconds > 1)
            {
                _logger.LogWarning("Slow query detected: {QueryName} took {Duration}ms", 
                    queryName, duration.TotalMilliseconds);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording query execution metrics for {QueryName}", queryName);
        }
    }

    public void RecordEndpointExecution(string endpoint, string method, TimeSpan duration, int statusCode)
    {
        try
        {
            var key = $"{method} {endpoint}";
            _endpointMetrics.AddOrUpdate(key,
                new EndpointMetrics
                {
                    Endpoint = endpoint,
                    Method = method,
                    TotalRequests = 1,
                    SuccessfulRequests = statusCode < 400 ? 1 : 0,
                    TotalDuration = duration,
                    MinDuration = duration,
                    MaxDuration = duration,
                    LastAccessed = DateTime.UtcNow
                },
                (key, existing) =>
                {
                    existing.TotalRequests++;
                    if (statusCode < 400) existing.SuccessfulRequests++;
                    existing.TotalDuration = existing.TotalDuration.Add(duration);
                    existing.MinDuration = duration < existing.MinDuration ? duration : existing.MinDuration;
                    existing.MaxDuration = duration > existing.MaxDuration ? duration : existing.MaxDuration;
                    existing.LastAccessed = DateTime.UtcNow;
                    return existing;
                });

            // Log slow endpoints (over 2 seconds)
            if (duration.TotalSeconds > 2)
            {
                _logger.LogWarning("Slow endpoint detected: {Method} {Endpoint} took {Duration}ms", 
                    method, endpoint, duration.TotalMilliseconds);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording endpoint execution metrics for {Method} {Endpoint}", method, endpoint);
        }
    }

    public void RecordCacheOperation(string cacheKey, bool isHit, TimeSpan? duration = null)
    {
        try
        {
            _cacheMetrics.AddOrUpdate(cacheKey,
                new CacheMetrics
                {
                    CacheKey = cacheKey,
                    TotalOperations = 1,
                    Hits = isHit ? 1 : 0,
                    Misses = isHit ? 0 : 1,
                    TotalDuration = duration ?? TimeSpan.Zero,
                    LastAccessed = DateTime.UtcNow
                },
                (key, existing) =>
                {
                    existing.TotalOperations++;
                    if (isHit) existing.Hits++;
                    else existing.Misses++;
                    if (duration.HasValue)
                        existing.TotalDuration = existing.TotalDuration.Add(duration.Value);
                    existing.LastAccessed = DateTime.UtcNow;
                    return existing;
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording cache operation metrics for {CacheKey}", cacheKey);
        }
    }

    public async Task<PerformanceMetricsDto> GetPerformanceMetricsAsync()
    {
        try
        {
            var uptime = DateTime.UtcNow - _startTime;
            var totalQueries = _queryMetrics.Values.Sum(q => q.TotalExecutions);
            var totalEndpointRequests = _endpointMetrics.Values.Sum(e => e.TotalRequests);
            var totalCacheOperations = _cacheMetrics.Values.Sum(c => c.TotalOperations);
            var totalCacheHits = _cacheMetrics.Values.Sum(c => c.Hits);

            return new PerformanceMetricsDto
            {
                Uptime = uptime,
                TotalQueries = totalQueries,
                TotalEndpointRequests = totalEndpointRequests,
                TotalCacheOperations = totalCacheOperations,
                CacheHitRate = totalCacheOperations > 0 ? (double)totalCacheHits / totalCacheOperations : 0,
                AverageQueryDuration = totalQueries > 0 
                    ? TimeSpan.FromTicks(_queryMetrics.Values.Sum(q => q.TotalDuration.Ticks) / totalQueries)
                    : TimeSpan.Zero,
                AverageEndpointDuration = totalEndpointRequests > 0
                    ? TimeSpan.FromTicks(_endpointMetrics.Values.Sum(e => e.TotalDuration.Ticks) / totalEndpointRequests)
                    : TimeSpan.Zero,
                SlowQueriesCount = _queryMetrics.Values.Count(q => q.MaxDuration.TotalSeconds > 1),
                SlowEndpointsCount = _endpointMetrics.Values.Count(e => e.MaxDuration.TotalSeconds > 2),
                LastUpdated = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance metrics");
            throw;
        }
    }

    public async Task<IEnumerable<SlowQueryDto>> GetSlowQueriesAsync(int topCount = 10)
    {
        try
        {
            return _queryMetrics.Values
                .OrderByDescending(q => q.MaxDuration)
                .Take(topCount)
                .Select(q => new SlowQueryDto
                {
                    QueryName = q.QueryName,
                    MaxDuration = q.MaxDuration,
                    AverageDuration = TimeSpan.FromTicks(q.TotalDuration.Ticks / q.TotalExecutions),
                    TotalExecutions = q.TotalExecutions,
                    SuccessRate = q.TotalExecutions > 0 ? (double)q.SuccessfulExecutions / q.TotalExecutions : 0,
                    LastExecuted = q.LastExecuted
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting slow queries");
            throw;
        }
    }

    public async Task<CachePerformanceDto> GetCachePerformanceAsync()
    {
        try
        {
            var totalOperations = _cacheMetrics.Values.Sum(c => c.TotalOperations);
            var totalHits = _cacheMetrics.Values.Sum(c => c.Hits);
            var totalMisses = _cacheMetrics.Values.Sum(c => c.Misses);

            return new CachePerformanceDto
            {
                TotalOperations = totalOperations,
                TotalHits = totalHits,
                TotalMisses = totalMisses,
                HitRate = totalOperations > 0 ? (double)totalHits / totalOperations : 0,
                MissRate = totalOperations > 0 ? (double)totalMisses / totalOperations : 0,
                CacheKeys = _cacheMetrics.Values.Select(c => new CacheKeyPerformanceDto
                {
                    Key = c.CacheKey,
                    Operations = c.TotalOperations,
                    Hits = c.Hits,
                    Misses = c.Misses,
                    HitRate = c.TotalOperations > 0 ? (double)c.Hits / c.TotalOperations : 0,
                    AverageDuration = c.TotalOperations > 0 
                        ? TimeSpan.FromTicks(c.TotalDuration.Ticks / c.TotalOperations)
                        : TimeSpan.Zero,
                    LastAccessed = c.LastAccessed
                }).OrderByDescending(c => c.Operations).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache performance");
            throw;
        }
    }

    public void ResetCounters()
    {
        try
        {
            lock (_lockObject)
            {
                _queryMetrics.Clear();
                _endpointMetrics.Clear();
                _cacheMetrics.Clear();
                _startTime = DateTime.UtcNow;
                _logger.LogInformation("Performance counters reset");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting performance counters");
        }
    }

    private class QueryMetrics
    {
        public string QueryName { get; set; } = string.Empty;
        public long TotalExecutions { get; set; }
        public long SuccessfulExecutions { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public TimeSpan MinDuration { get; set; }
        public TimeSpan MaxDuration { get; set; }
        public DateTime LastExecuted { get; set; }
    }

    private class EndpointMetrics
    {
        public string Endpoint { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public long TotalRequests { get; set; }
        public long SuccessfulRequests { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public TimeSpan MinDuration { get; set; }
        public TimeSpan MaxDuration { get; set; }
        public DateTime LastAccessed { get; set; }
    }

    private class CacheMetrics
    {
        public string CacheKey { get; set; } = string.Empty;
        public long TotalOperations { get; set; }
        public long Hits { get; set; }
        public long Misses { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public DateTime LastAccessed { get; set; }
    }
}