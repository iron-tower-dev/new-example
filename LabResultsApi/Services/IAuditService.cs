using LabResultsApi.Models;

namespace LabResultsApi.Services;

public interface IAuditService
{
    Task LogActionAsync(string employeeId, string action, string entityType, string? entityId = null, 
        object? oldValues = null, object? newValues = null, string? additionalInfo = null);
    
    Task LogTestResultEntryAsync(string employeeId, int sampleId, int testId, int trialNumber, 
        object? oldValues = null, object? newValues = null);
    
    Task LogTestResultUpdateAsync(string employeeId, int sampleId, int testId, int trialNumber, 
        object oldValues, object newValues);
    
    Task LogTestResultDeleteAsync(string employeeId, int sampleId, int testId, int? trialNumber = null);
    
    Task LogLoginAsync(string employeeId, bool success, string? additionalInfo = null);
    
    Task LogLogoutAsync(string employeeId);
    
    Task<List<AuditLog>> GetAuditLogsAsync(string? employeeId = null, string? entityType = null, 
        DateTime? fromDate = null, DateTime? toDate = null, int page = 1, int pageSize = 50);
    
    Task LogFileUploadAsync(string employeeId, int sampleId, int testId, string fileName, long fileSize);
    
    Task LogFileDeleteAsync(string employeeId, int sampleId, int testId, string fileName);
    
    Task LogCalculationAsync(string employeeId, int sampleId, int testId, string calculationType, 
        object inputValues, object result);
    
    Task LogValidationErrorAsync(string employeeId, string entityType, string? entityId, 
        string validationErrors);
    
    Task LogUnauthorizedAccessAsync(string? employeeId, string resource, string requiredPermission);
}