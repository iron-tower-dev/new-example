using LabResultsApi.Services;
using System.Diagnostics;

namespace LabResultsApi.Middleware;

public class PerformanceMonitoringMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceMonitoringMiddleware> _logger;

    public PerformanceMonitoringMiddleware(RequestDelegate next, ILogger<PerformanceMonitoringMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IPerformanceMonitoringService performanceService)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = context.Request.Path.Value ?? "unknown";
        var method = context.Request.Method;

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            
            try
            {
                performanceService.RecordEndpointExecution(
                    endpoint, 
                    method, 
                    stopwatch.Elapsed, 
                    context.Response.StatusCode);

                // Log slow requests
                if (stopwatch.ElapsedMilliseconds > 2000)
                {
                    _logger.LogWarning("Slow request: {Method} {Endpoint} took {Duration}ms with status {StatusCode}",
                        method, endpoint, stopwatch.ElapsedMilliseconds, context.Response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording performance metrics for {Method} {Endpoint}", method, endpoint);
            }
        }
    }
}