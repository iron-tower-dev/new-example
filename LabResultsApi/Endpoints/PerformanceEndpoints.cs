using LabResultsApi.Services;
using Microsoft.AspNetCore.Authorization;

namespace LabResultsApi.Endpoints;

public static class PerformanceEndpoints
{
    public static void MapPerformanceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/performance")
            .WithTags("Performance")
            .RequireAuthorization(); // Require authentication for performance endpoints

        // Performance metrics endpoint
        group.MapGet("/metrics", async (IPerformanceMonitoringService performanceService) =>
        {
            try
            {
                var metrics = await performanceService.GetPerformanceMetricsAsync();
                return Results.Ok(metrics);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error getting performance metrics: {ex.Message}", statusCode: 500);
            }
        })
        .WithName("GetPerformanceMetrics")
        .WithSummary("Get system performance metrics")
        .WithDescription("Returns overall system performance statistics including query times, cache hit rates, and endpoint performance");

        // Slow queries endpoint
        group.MapGet("/slow-queries", async (IPerformanceMonitoringService performanceService, int topCount = 10) =>
        {
            try
            {
                var slowQueries = await performanceService.GetSlowQueriesAsync(topCount);
                return Results.Ok(slowQueries);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error getting slow queries: {ex.Message}", statusCode: 500);
            }
        })
        .WithName("GetSlowQueries")
        .WithSummary("Get slowest database queries")
        .WithDescription("Returns the slowest performing database queries with execution statistics");

        // Cache performance endpoint
        group.MapGet("/cache", async (IPerformanceMonitoringService performanceService) =>
        {
            try
            {
                var cachePerformance = await performanceService.GetCachePerformanceAsync();
                return Results.Ok(cachePerformance);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error getting cache performance: {ex.Message}", statusCode: 500);
            }
        })
        .WithName("GetCachePerformance")
        .WithSummary("Get cache performance statistics")
        .WithDescription("Returns cache hit/miss rates and performance statistics for all cached operations");

        // Database index recommendations endpoint
        group.MapGet("/index-recommendations", async (IDatabaseIndexingService indexingService) =>
        {
            try
            {
                var recommendations = await indexingService.GetIndexRecommendationsAsync();
                return Results.Ok(recommendations);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error getting index recommendations: {ex.Message}", statusCode: 500);
            }
        })
        .WithName("GetIndexRecommendations")
        .WithSummary("Get database indexing recommendations")
        .WithDescription("Returns recommended database indexes to improve query performance");

        // Query optimization suggestions endpoint
        group.MapGet("/query-optimizations", async (IDatabaseIndexingService indexingService) =>
        {
            try
            {
                var optimizations = await indexingService.GetQueryOptimizationsAsync();
                return Results.Ok(optimizations);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error getting query optimizations: {ex.Message}", statusCode: 500);
            }
        })
        .WithName("GetQueryOptimizations")
        .WithSummary("Get query optimization suggestions")
        .WithDescription("Returns suggestions for optimizing slow or inefficient database queries");

        // Index status check endpoint
        group.MapGet("/index-status", async (IDatabaseIndexingService indexingService) =>
        {
            try
            {
                var indexStatus = await indexingService.CheckIndexExistenceAsync();
                return Results.Ok(indexStatus);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error checking index status: {ex.Message}", statusCode: 500);
            }
        })
        .WithName("GetIndexStatus")
        .WithSummary("Check status of recommended indexes")
        .WithDescription("Returns the existence status of recommended database indexes");

        // Reset performance counters endpoint (admin only)
        group.MapPost("/reset-counters", (IPerformanceMonitoringService performanceService) =>
        {
            try
            {
                performanceService.ResetCounters();
                return Results.Ok(new { Message = "Performance counters reset successfully", Timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error resetting performance counters: {ex.Message}", statusCode: 500);
            }
        })
        .WithName("ResetPerformanceCounters")
        .WithSummary("Reset performance monitoring counters")
        .WithDescription("Resets all performance monitoring counters and statistics (admin operation)");

        // System health check with performance data
        group.MapGet("/health", async (
            IPerformanceMonitoringService performanceService,
            IRawSqlService rawSqlService) =>
        {
            try
            {
                var dbConnected = await rawSqlService.TestDatabaseConnectionAsync();
                var metrics = await performanceService.GetPerformanceMetricsAsync();
                
                var health = new
                {
                    Status = dbConnected ? "Healthy" : "Unhealthy",
                    DatabaseConnected = dbConnected,
                    Uptime = metrics.Uptime,
                    TotalQueries = metrics.TotalQueries,
                    AverageQueryDuration = metrics.AverageQueryDuration,
                    CacheHitRate = metrics.CacheHitRate,
                    SlowQueriesCount = metrics.SlowQueriesCount,
                    Timestamp = DateTime.UtcNow
                };

                return dbConnected ? Results.Ok(health) : Results.Problem("Database connection failed", statusCode: 503);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Health check failed: {ex.Message}", statusCode: 503);
            }
        })
        .WithName("GetSystemHealthWithPerformance")
        .WithSummary("Get system health with performance data")
        .WithDescription("Returns system health status including database connectivity and performance metrics");
    }
}