using System.Security.Claims;

namespace LabResultsApi.Services;

public interface IAuthorizationService
{
    Task<bool> CanAccessTestAsync(ClaimsPrincipal user, short testStandId, string requiredLevel = "TRAIN");
    Task<bool> CanModifyTestResultAsync(ClaimsPrincipal user, int sampleId, int testId);
    Task<bool> CanDeleteTestResultAsync(ClaimsPrincipal user, int sampleId, int testId);
    Task<bool> CanViewAuditLogsAsync(ClaimsPrincipal user, string? targetEmployeeId = null);
    Task<bool> CanUploadFileAsync(ClaimsPrincipal user, int sampleId, int testId);
    Task<bool> CanReviewTestResultAsync(ClaimsPrincipal user, int sampleId, int testId);
    bool IsReviewer(ClaimsPrincipal user);
    bool IsTechnician(ClaimsPrincipal user);
    string? GetEmployeeId(ClaimsPrincipal user);
    string? GetUserRole(ClaimsPrincipal user);
    List<string> GetUserQualifications(ClaimsPrincipal user);
}