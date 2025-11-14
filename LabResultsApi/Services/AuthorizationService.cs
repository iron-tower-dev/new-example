using System.Security.Claims;
using LabResultsApi.Models;

namespace LabResultsApi.Services;

public class AuthorizationService : IAuthorizationService
{
    private readonly IAuthenticationService _authService;
    private readonly IAuditService _auditService;
    private readonly ILogger<AuthorizationService> _logger;

    public AuthorizationService(
        IAuthenticationService authService, 
        IAuditService auditService,
        ILogger<AuthorizationService> logger)
    {
        _authService = authService;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<bool> CanAccessTestAsync(ClaimsPrincipal user, short testStandId, string requiredLevel = "TRAIN")
    {
        var employeeId = GetEmployeeId(user);
        if (string.IsNullOrEmpty(employeeId))
        {
            await _auditService.LogUnauthorizedAccessAsync(null, $"Test_{testStandId}", requiredLevel);
            return false;
        }

        // Reviewers have access to all tests for review purposes
        if (IsReviewer(user))
        {
            return true;
        }

        // Check technician qualifications
        if (IsTechnician(user))
        {
            var accessResult = await _authService.CheckTestAccessAsync(employeeId, testStandId, requiredLevel);
            
            if (!accessResult.HasAccess)
            {
                await _auditService.LogUnauthorizedAccessAsync(employeeId, $"Test_{testStandId}", 
                    $"{requiredLevel} (User has: {accessResult.UserQualificationLevel})");
            }
            
            return accessResult.HasAccess;
        }

        await _auditService.LogUnauthorizedAccessAsync(employeeId, $"Test_{testStandId}", "Valid role required");
        return false;
    }

    public async Task<bool> CanModifyTestResultAsync(ClaimsPrincipal user, int sampleId, int testId)
    {
        var employeeId = GetEmployeeId(user);
        if (string.IsNullOrEmpty(employeeId))
        {
            await _auditService.LogUnauthorizedAccessAsync(null, $"TestResult_{sampleId}_{testId}", "Modify");
            return false;
        }

        // Reviewers can modify test results
        if (IsReviewer(user))
        {
            return true;
        }

        // Technicians can modify test results if they have appropriate qualifications
        // This would need to be enhanced based on business rules about who can modify what
        if (IsTechnician(user))
        {
            // For now, allow technicians to modify results they have access to
            // In a real system, you might want to check if they created the result originally
            return true;
        }

        await _auditService.LogUnauthorizedAccessAsync(employeeId, $"TestResult_{sampleId}_{testId}", "Modify");
        return false;
    }

    public async Task<bool> CanDeleteTestResultAsync(ClaimsPrincipal user, int sampleId, int testId)
    {
        var employeeId = GetEmployeeId(user);
        if (string.IsNullOrEmpty(employeeId))
        {
            await _auditService.LogUnauthorizedAccessAsync(null, $"TestResult_{sampleId}_{testId}", "Delete");
            return false;
        }

        // Only reviewers can delete test results (more restrictive than modify)
        if (IsReviewer(user))
        {
            return true;
        }

        await _auditService.LogUnauthorizedAccessAsync(employeeId, $"TestResult_{sampleId}_{testId}", "Delete (Reviewer required)");
        return false;
    }

    public async Task<bool> CanViewAuditLogsAsync(ClaimsPrincipal user, string? targetEmployeeId = null)
    {
        var employeeId = GetEmployeeId(user);
        if (string.IsNullOrEmpty(employeeId))
        {
            await _auditService.LogUnauthorizedAccessAsync(null, "AuditLogs", "View");
            return false;
        }

        // Reviewers can view all audit logs
        if (IsReviewer(user))
        {
            return true;
        }

        // Users can view their own audit logs
        if (!string.IsNullOrEmpty(targetEmployeeId) && targetEmployeeId == employeeId)
        {
            return true;
        }

        // If no specific target, users can only see their own logs
        if (string.IsNullOrEmpty(targetEmployeeId))
        {
            return true; // Will be filtered to their own logs in the service
        }

        await _auditService.LogUnauthorizedAccessAsync(employeeId, "AuditLogs", "View others' logs");
        return false;
    }

    public async Task<bool> CanUploadFileAsync(ClaimsPrincipal user, int sampleId, int testId)
    {
        var employeeId = GetEmployeeId(user);
        if (string.IsNullOrEmpty(employeeId))
        {
            await _auditService.LogUnauthorizedAccessAsync(null, $"FileUpload_{sampleId}_{testId}", "Upload");
            return false;
        }

        // Both reviewers and technicians can upload files
        if (IsReviewer(user) || IsTechnician(user))
        {
            return true;
        }

        await _auditService.LogUnauthorizedAccessAsync(employeeId, $"FileUpload_{sampleId}_{testId}", "Upload");
        return false;
    }

    public async Task<bool> CanReviewTestResultAsync(ClaimsPrincipal user, int sampleId, int testId)
    {
        var employeeId = GetEmployeeId(user);
        if (string.IsNullOrEmpty(employeeId))
        {
            await _auditService.LogUnauthorizedAccessAsync(null, $"TestResult_{sampleId}_{testId}", "Review");
            return false;
        }

        // Only reviewers can perform official reviews
        if (IsReviewer(user))
        {
            return true;
        }

        await _auditService.LogUnauthorizedAccessAsync(employeeId, $"TestResult_{sampleId}_{testId}", "Review (Reviewer required)");
        return false;
    }

    public bool IsReviewer(ClaimsPrincipal user)
    {
        return user.IsInRole("Reviewer");
    }

    public bool IsTechnician(ClaimsPrincipal user)
    {
        return user.IsInRole("Technician");
    }

    public string? GetEmployeeId(ClaimsPrincipal user)
    {
        return user.FindFirst("employee_id")?.Value;
    }

    public string? GetUserRole(ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Role)?.Value;
    }

    public List<string> GetUserQualifications(ClaimsPrincipal user)
    {
        var qualifications = new List<string>();
        
        foreach (var claim in user.Claims)
        {
            if (claim.Type.StartsWith("qualification_"))
            {
                var testStandId = claim.Type.Substring("qualification_".Length);
                qualifications.Add($"TestStand_{testStandId}:{claim.Value}");
            }
        }
        
        return qualifications;
    }
}