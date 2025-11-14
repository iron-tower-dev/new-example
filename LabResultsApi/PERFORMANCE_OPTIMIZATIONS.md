# Backend Performance Optimizations

This document outlines the performance optimizations implemented for the Laboratory Test Results API.

## Overview

The performance optimizations focus on four key areas:
1. **Database Query Optimization** - Improved indexing and query patterns
2. **Response Caching** - Memory caching for frequently accessed data
3. **Connection Pooling** - Efficient database connection management
4. **Performance Monitoring** - Real-time performance tracking and alerting

## Database Query Optimizations

### Recommended Indexes

Execute the `Scripts/CreatePerformanceIndexes.sql` script to create the following performance indexes:

#### High Priority Indexes
- `IX_TestReadings_SampleID_TestID` - Composite index for the most common query pattern
- `IX_UsedLubeSamples_TagNumber_Component` - For historical data queries
- `IX_EmSpectro_ID_TestID` - For emission spectroscopy data retrieval

#### Medium Priority Indexes
- `IX_TestReadings_Status_EntryDate` - For status-based filtering and date ordering
- `IX_UsedLubeSamples_SampleDate` - For chronological sample queries
- `IX_Equipment_EquipType` - For equipment selection dropdowns
- `IX_NAS_lookup_Channel` - For particle count NAS calculations

### Query Optimization Patterns

1. **Index Hints**: Added explicit index hints to force optimal index usage
2. **Included Columns**: Indexes include frequently selected columns to reduce key lookups
3. **Filtered Indexes**: Where appropriate, filtered indexes reduce index size
4. **Fill Factor**: Optimized fill factors (90% for transactional, 95% for lookup tables)

## Response Caching Implementation

### Memory Cache Strategy

The system implements a two-tier caching strategy:

1. **In-Memory Dictionary Cache** (Legacy compatibility)
   - Used for NAS and NLGI lookup data
   - Refreshed every hour
   - Thread-safe implementation

2. **IMemoryCache Integration** (New optimized layer)
   - Faster access for frequently requested data
   - Configurable expiration times
   - Automatic cache invalidation on data changes

### Cache Configuration

```json
{
  "Performance": {
    "CacheSettings": {
      "DefaultExpirationMinutes": 5,
      "LookupCacheExpirationHours": 1,
      "MaxCacheSize": 1000
    }
  }
}
```

### Cached Data Types

- **Lookup Data**: NAS values, NLGI grades, equipment lists
- **Sample History**: Last 12 results for equipment/test combinations
- **Emission Spectroscopy**: Test results with 5-minute expiration
- **Equipment Calibration**: MTE equipment calibration values

## Connection Pooling

### DbContext Pooling

Implemented `AddDbContextPool` with optimized settings:

```csharp
builder.Services.AddDbContextPool<LabDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5));
        sqlOptions.CommandTimeout(30);
    });
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
}, poolSize: 128);
```

### Configuration Options

- **Pool Size**: 128 connections (configurable)
- **Command Timeout**: 30 seconds
- **Retry Policy**: 3 retries with exponential backoff
- **Query Tracking**: Disabled for better performance (read-only scenarios)

## Performance Monitoring

### Real-Time Metrics

The `PerformanceMonitoringService` tracks:

- **Query Execution Times**: Min, max, average durations
- **Endpoint Response Times**: HTTP request performance
- **Cache Hit/Miss Rates**: Cache effectiveness metrics
- **Slow Query Detection**: Automatic logging of queries > 1 second
- **Error Rates**: Success/failure ratios

### Monitoring Endpoints

Access performance data via these API endpoints:

- `GET /api/performance/metrics` - Overall system metrics
- `GET /api/performance/slow-queries` - Slowest database queries
- `GET /api/performance/cache` - Cache performance statistics
- `GET /api/performance/index-recommendations` - Database indexing suggestions
- `GET /api/performance/health` - System health with performance data

### Performance Middleware

The `PerformanceMonitoringMiddleware` automatically tracks:
- Request/response times for all endpoints
- HTTP status codes and error rates
- Slow endpoint detection (> 2 seconds)

## Async Patterns

### Implemented Patterns

1. **Async/Await Throughout**: All database operations use async patterns
2. **ConfigureAwait(false)**: Used in library code to avoid deadlocks
3. **Parallel Processing**: Where appropriate for independent operations
4. **Cancellation Tokens**: Support for request cancellation

### Example Implementation

```csharp
public async Task<List<TestReading>> GetTestReadingsAsync(int sampleId, int testId)
{
    var stopwatch = Stopwatch.StartNew();
    try
    {
        var results = await _context.TestReadings
            .FromSqlRaw("SELECT * FROM TestReadings WITH (INDEX(IX_TestReadings_SampleID_TestID)) WHERE sampleID = {0} AND testID = {1}", sampleId, testId)
            .ToListAsync();
        
        _performanceService.RecordQueryExecution("GetTestReadings", stopwatch.Elapsed, true);
        return results;
    }
    catch (Exception ex)
    {
        _performanceService.RecordQueryExecution("GetTestReadings", stopwatch.Elapsed, false);
        throw;
    }
}
```

## Configuration

### appsettings.json Performance Section

```json
{
  "Performance": {
    "CacheSettings": {
      "DefaultExpirationMinutes": 5,
      "LookupCacheExpirationHours": 1,
      "MaxCacheSize": 1000
    },
    "DatabaseSettings": {
      "CommandTimeoutSeconds": 30,
      "ConnectionPoolSize": 128,
      "EnableRetryOnFailure": true,
      "MaxRetryCount": 3,
      "MaxRetryDelaySeconds": 5
    },
    "Monitoring": {
      "SlowQueryThresholdMs": 1000,
      "SlowEndpointThresholdMs": 2000,
      "EnableDetailedLogging": false
    }
  }
}
```

## Performance Testing

### Recommended Testing Approach

1. **Load Testing**: Use tools like NBomber or k6 to test concurrent users
2. **Database Profiling**: Monitor query execution plans and index usage
3. **Memory Profiling**: Track cache effectiveness and memory usage
4. **Endpoint Testing**: Measure response times under various loads

### Key Metrics to Monitor

- **Average Response Time**: < 200ms for simple queries, < 1s for complex operations
- **Cache Hit Rate**: > 80% for lookup data
- **Database Connection Pool**: < 50% utilization under normal load
- **Query Execution Time**: < 100ms for indexed queries

## Deployment Considerations

### Database Maintenance

1. **Index Maintenance**: Regular index rebuilding/reorganization
2. **Statistics Updates**: Automatic statistics updates enabled
3. **Query Plan Cache**: Monitor for plan cache pollution

### Application Monitoring

1. **Application Insights**: For production monitoring
2. **Health Checks**: Automated health monitoring endpoints
3. **Alerting**: Set up alerts for slow queries and high error rates

### Scaling Recommendations

1. **Horizontal Scaling**: API can be scaled across multiple instances
2. **Database Scaling**: Consider read replicas for reporting queries
3. **Caching Layer**: Redis for distributed caching if needed

## Troubleshooting

### Common Performance Issues

1. **Slow Queries**: Check index usage and execution plans
2. **High Memory Usage**: Monitor cache size and expiration policies
3. **Connection Pool Exhaustion**: Increase pool size or optimize query patterns
4. **Cache Misses**: Verify cache expiration settings and invalidation logic

### Diagnostic Queries

```sql
-- Check index usage
SELECT * FROM sys.dm_db_index_usage_stats WHERE database_id = DB_ID()

-- Find expensive queries
SELECT TOP 10 * FROM sys.dm_exec_query_stats ORDER BY total_elapsed_time DESC

-- Monitor connection pool
SELECT * FROM sys.dm_exec_connections WHERE session_id > 50
```

## Future Optimizations

### Potential Improvements

1. **Distributed Caching**: Redis implementation for multi-instance deployments
2. **Query Result Caching**: Cache complex query results
3. **Background Processing**: Move heavy operations to background services
4. **Database Partitioning**: For very large datasets
5. **CDN Integration**: For static content and file uploads

### Monitoring Enhancements

1. **Custom Metrics**: Business-specific performance indicators
2. **Predictive Analytics**: Trend analysis for capacity planning
3. **Automated Optimization**: Self-tuning query parameters
4. **Real-time Dashboards**: Live performance visualization