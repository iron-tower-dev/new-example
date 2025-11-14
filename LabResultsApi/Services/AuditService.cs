using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using LabResultsApi.Data;
using LabResultsApi.Models;

namespace LabResultsApi.Services;

public class AuditService : IAuditService
{
    private readonly LabDbContext _context;
    private readonly ILogger<AuditService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(LabDbContext context, ILogger<AuditService> logger, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogActionAsync(string employeeId, string action, string entityType, string? entityId = null, 
        object? oldValues = null, object? newValues = null, string? additionalInfo = null)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            
            var auditLog = new AuditLog
            {
                EmployeeId = employeeId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
                NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
                Timestamp = DateTime.UtcNow,
                IpAddress = httpContext?.Connection?.RemoteIpAddress?.ToString(),
                UserAgent = httpContext?.Request?.Headers["User-Agent"].ToString(),
                AdditionalInfo = additionalInfo
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Audit log created: {Action} on {EntityType} by {EmployeeId}", 
                action, entityType, employeeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create audit log for action {Action} by {EmployeeId}", action, employeeId);
            // Don't throw - audit failures shouldn't break the main operation
        }
    }

    public async Task LogTestResultEntryAsync(string employeeId, int sampleId, int testId, int trialNumber, 
        object? oldValues = null, object? newValues = null)
    {
        await LogActionAsync(employeeId, "CREATE", "TestResult", $"{sampleId}_{testId}_{trialNumber}", 
            oldValues, newValues, $"Sample: {sampleId}, Test: {testId}, Trial: {trialNumber}");
    }

    public async Task LogTestResultUpdateAsync(string employeeId, int sampleId, int testId, int trialNumber, 
        object oldValues, object newValues)
    {
        await LogActionAsync(employeeId, "UPDATE", "TestResult", $"{sampleId}_{testId}_{trialNumber}", 
            oldValues, newValues, $"Sample: {sampleId}, Test: {testId}, Trial: {trialNumber}");
    }

    public async Task LogTestResultDeleteAsync(string employeeId, int sampleId, int testId, int? trialNumber = null)
    {
        var entityId = trialNumber.HasValue ? $"{sampleId}_{testId}_{trialNumber}" : $"{sampleId}_{testId}";
        var additionalInfo = trialNumber.HasValue 
            ? $"Sample: {sampleId}, Test: {testId}, Trial: {trialNumber}"
            : $"Sample: {sampleId}, Test: {testId} (All Trials)";
            
        await LogActionAsync(employeeId, "DELETE", "TestResult", entityId, null, null, additionalInfo);
    }

    public async Task LogLoginAsync(string employeeId, bool success, string? additionalInfo = null)
    {
        var action = success ? "LOGIN_SUCCESS" : "LOGIN_FAILED";
        await LogActionAsync(employeeId, action, "Authentication", employeeId, null, null, additionalInfo);
    }

    public async Task LogLogoutAsync(string employeeId)
    {
        await LogActionAsync(employeeId, "LOGOUT", "Authentication", employeeId);
    }

    public async Task<List<AuditLog>> GetAuditLogsAsync(string? employeeId = null, string? entityType = null, 
        DateTime? fromDate = null, DateTime? toDate = null, int page = 1, int pageSize = 50)
    {
        try
        {
            var query = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrEmpty(employeeId))
            {
                query = query.Where(a => a.EmployeeId == employeeId);
            }

            if (!string.IsNullOrEmpty(entityType))
            {
                query = query.Where(a => a.EntityType == entityType);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(a => a.Timestamp >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(a => a.Timestamp <= toDate.Value);
            }

            return await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs");
            return new List<AuditLog>();
        }
    }

    public async Task LogFileUploadAsync(string employeeId, int sampleId, int testId, string fileName, long fileSize)
    {
        await LogActionAsync(employeeId, "FILE_UPLOAD", "File", fileName, null, 
            new { fileName, fileSize, sampleId, testId }, 
            $"Sample: {sampleId}, Test: {testId}, Size: {fileSize} bytes");
    }

    public async Task LogFileDeleteAsync(string employeeId, int sampleId, int testId, string fileName)
    {
        await LogActionAsync(employeeId, "FILE_DELETE", "File", fileName, 
            new { fileName, sampleId, testId }, null, 
            $"Sample: {sampleId}, Test: {testId}");
    }

    public async Task LogCalculationAsync(string employeeId, int sampleId, int testId, string calculationType, 
        object inputValues, object result)
    {
        await LogActionAsync(employeeId, "CALCULATION", "TestResult", $"{sampleId}_{testId}", 
            inputValues, result, 
            $"Sample: {sampleId}, Test: {testId}, Type: {calculationType}");
    }

    public async Task LogValidationErrorAsync(string employeeId, string entityType, string? entityId, 
        string validationErrors)
    {
        await LogActionAsync(employeeId, "VALIDATION_ERROR", entityType, entityId, null, null, 
            $"Validation errors: {validationErrors}");
    }

    public async Task LogUnauthorizedAccessAsync(string? employeeId, string resource, string requiredPermission)
    {
        await LogActionAsync(employeeId ?? "ANONYMOUS", "UNAUTHORIZED_ACCESS", "Security", resource, null, null, 
            $"Required permission: {requiredPermission}");
    }
}