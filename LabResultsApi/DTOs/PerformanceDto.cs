namespace LabResultsApi.DTOs;

public class PerformanceMetricsDto
{
    public TimeSpan Uptime { get; set; }
    public long TotalQueries { get; set; }
    public long TotalEndpointRequests { get; set; }
    public long TotalCacheOperations { get; set; }
    public double CacheHitRate { get; set; }
    public TimeSpan AverageQueryDuration { get; set; }
    public TimeSpan AverageEndpointDuration { get; set; }
    public int SlowQueriesCount { get; set; }
    public int SlowEndpointsCount { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class SlowQueryDto
{
    public string QueryName { get; set; } = string.Empty;
    public TimeSpan MaxDuration { get; set; }
    public TimeSpan AverageDuration { get; set; }
    public long TotalExecutions { get; set; }
    public double SuccessRate { get; set; }
    public DateTime LastExecuted { get; set; }
}

public class CachePerformanceDto
{
    public long TotalOperations { get; set; }
    public long TotalHits { get; set; }
    public long TotalMisses { get; set; }
    public double HitRate { get; set; }
    public double MissRate { get; set; }
    public List<CacheKeyPerformanceDto> CacheKeys { get; set; } = new();
}

public class CacheKeyPerformanceDto
{
    public string Key { get; set; } = string.Empty;
    public long Operations { get; set; }
    public long Hits { get; set; }
    public long Misses { get; set; }
    public double HitRate { get; set; }
    public TimeSpan AverageDuration { get; set; }
    public DateTime LastAccessed { get; set; }
}

public class DatabaseIndexRecommendationDto
{
    public string TableName { get; set; } = string.Empty;
    public string RecommendedIndex { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public int Priority { get; set; }
    public double EstimatedImpact { get; set; }
}

public class QueryOptimizationDto
{
    public string QueryName { get; set; } = string.Empty;
    public string OriginalQuery { get; set; } = string.Empty;
    public string OptimizedQuery { get; set; } = string.Empty;
    public string OptimizationReason { get; set; } = string.Empty;
    public TimeSpan EstimatedImprovement { get; set; }
}