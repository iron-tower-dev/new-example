using System.Text;
using System.Text.Json;
using LabResultsApi.Services;

namespace LabResultsApi.Middleware;

public class AuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditMiddleware> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _next = next;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip audit for health checks and static files
        if (ShouldSkipAudit(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var startTime = DateTime.UtcNow;
        var requestBody = await CaptureRequestBodyAsync(context.Request);
        
        // Store original response body stream
        var originalBodyStream = context.Response.Body;
        
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);
        }
        finally
        {
            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;
            
            // Capture response
            var responseBodyText = await CaptureResponseBodyAsync(responseBody);
            
            // Copy response back to original stream
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
            
            // Log the audit entry
            await LogAuditEntryAsync(context, requestBody, responseBodyText, startTime, duration);
        }
    }

    private bool ShouldSkipAudit(PathString path)
    {
        var pathValue = path.Value?.ToLowerInvariant();
        
        return pathValue switch
        {
            null => true,
            var p when p.StartsWith("/health") => true,
            var p when p.StartsWith("/swagger") => true,
            var p when p.StartsWith("/_framework") => true,
            var p when p.Contains("/css/") => true,
            var p when p.Contains("/js/") => true,
            var p when p.Contains("/images/") => true,
            var p when p.EndsWith(".ico") => true,
            _ => false
        };
    }

    private async Task<string> CaptureRequestBodyAsync(HttpRequest request)
    {
        try
        {
            if (request.ContentLength == null || request.ContentLength == 0)
                return string.Empty;

            request.EnableBuffering();
            
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
            
            return body;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to capture request body");
            return "[Error capturing request body]";
        }
    }

    private async Task<string> CaptureResponseBodyAsync(MemoryStream responseBody)
    {
        try
        {
            responseBody.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(responseBody, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            responseBody.Seek(0, SeekOrigin.Begin);
            
            return body;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to capture response body");
            return "[Error capturing response body]";
        }
    }

    private async Task LogAuditEntryAsync(HttpContext context, string requestBody, string responseBody, 
        DateTime startTime, TimeSpan duration)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var auditService = scope.ServiceProvider.GetRequiredService<IAuditService>();
            var authorizationService = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
            
            var employeeId = authorizationService.GetEmployeeId(context.User) ?? "ANONYMOUS";
            var method = context.Request.Method;
            var path = context.Request.Path.Value ?? "";
            var statusCode = context.Response.StatusCode;
            
            // Determine action type based on HTTP method and path
            var action = DetermineAction(method, path, statusCode);
            var entityType = DetermineEntityType(path);
            var entityId = ExtractEntityId(path);
            
            // Create audit info
            var auditInfo = new
            {
                Method = method,
                Path = path,
                StatusCode = statusCode,
                Duration = duration.TotalMilliseconds,
                UserAgent = context.Request.Headers["User-Agent"].ToString(),
                RequestSize = requestBody.Length,
                ResponseSize = responseBody.Length
            };

            // Sanitize sensitive data
            var sanitizedRequestBody = SanitizeRequestBody(requestBody);
            var sanitizedResponseBody = SanitizeResponseBody(responseBody);

            await auditService.LogActionAsync(
                employeeId,
                action,
                entityType,
                entityId,
                string.IsNullOrEmpty(sanitizedRequestBody) ? null : JsonSerializer.Deserialize<object>(sanitizedRequestBody),
                string.IsNullOrEmpty(sanitizedResponseBody) ? null : TryDeserializeJson(sanitizedResponseBody),
                JsonSerializer.Serialize(auditInfo)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log audit entry for {Method} {Path}", 
                context.Request.Method, context.Request.Path);
        }
    }

    private string DetermineAction(string method, string path, int statusCode)
    {
        if (statusCode >= 400)
        {
            return $"API_ERROR_{statusCode}";
        }

        return method.ToUpperInvariant() switch
        {
            "GET" => "API_READ",
            "POST" => "API_CREATE",
            "PUT" => "API_UPDATE",
            "PATCH" => "API_UPDATE",
            "DELETE" => "API_DELETE",
            _ => "API_REQUEST"
        };
    }

    private string DetermineEntityType(string path)
    {
        var pathLower = path.ToLowerInvariant();
        
        return pathLower switch
        {
            var p when p.Contains("/samples") => "Sample",
            var p when p.Contains("/tests") => "Test",
            var p when p.Contains("/equipment") => "Equipment",
            var p when p.Contains("/files") => "File",
            var p when p.Contains("/auth") => "Authentication",
            var p when p.Contains("/lookups") => "Lookup",
            var p when p.Contains("/particle") => "ParticleAnalysis",
            var p when p.Contains("/emission") => "EmissionSpectroscopy",
            var p when p.Contains("/historical") => "HistoricalResults",
            _ => "API"
        };
    }

    private string? ExtractEntityId(string path)
    {
        try
        {
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            
            // Look for numeric IDs in the path
            for (int i = 0; i < segments.Length; i++)
            {
                if (int.TryParse(segments[i], out _))
                {
                    return segments[i];
                }
            }
            
            return null;
        }
        catch
        {
            return null;
        }
    }

    private string SanitizeRequestBody(string requestBody)
    {
        if (string.IsNullOrEmpty(requestBody))
            return requestBody;

        try
        {
            // Remove sensitive fields like passwords
            var json = JsonSerializer.Deserialize<JsonElement>(requestBody);
            var sanitized = SanitizeJsonElement(json);
            return JsonSerializer.Serialize(sanitized);
        }
        catch
        {
            return "[Invalid JSON]";
        }
    }

    private string SanitizeResponseBody(string responseBody)
    {
        if (string.IsNullOrEmpty(responseBody))
            return responseBody;

        try
        {
            // Limit response body size for audit logs
            if (responseBody.Length > 5000)
            {
                return responseBody.Substring(0, 5000) + "... [truncated]";
            }
            
            var json = JsonSerializer.Deserialize<JsonElement>(responseBody);
            var sanitized = SanitizeJsonElement(json);
            return JsonSerializer.Serialize(sanitized);
        }
        catch
        {
            return responseBody.Length > 1000 ? responseBody.Substring(0, 1000) + "... [truncated]" : responseBody;
        }
    }

    private JsonElement SanitizeJsonElement(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var dict = new Dictionary<string, object?>();
            
            foreach (var property in element.EnumerateObject())
            {
                var key = property.Name.ToLowerInvariant();
                
                if (key.Contains("password") || key.Contains("token") || key.Contains("secret"))
                {
                    dict[property.Name] = "[REDACTED]";
                }
                else
                {
                    dict[property.Name] = SanitizeJsonElement(property.Value);
                }
            }
            
            return JsonSerializer.SerializeToElement(dict);
        }
        
        return element;
    }

    private object? TryDeserializeJson(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<object>(json);
        }
        catch
        {
            return json;
        }
    }
}