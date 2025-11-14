using LabResultsApi.DTOs;

namespace LabResultsApi.Services;

public interface IQualificationService
{
    /// <summary>
    /// Get user qualification level for a specific test
    /// </summary>
    Task<string?> GetUserQualificationAsync(string employeeId, int testId);
    
    /// <summary>
    /// Check if user is qualified to perform a test
    /// </summary>
    Task<bool> IsUserQualifiedAsync(string employeeId, int testId);
    
    /// <summary>
    /// Check if user is qualified to review test results
    /// </summary>
    Task<bool> IsUserQualifiedToReviewAsync(string employeeId, int sampleId, int testId);
    
    /// <summary>
    /// Get all tests a user is qualified to perform
    /// </summary>
    Task<IEnumerable<TestDto>> GetQualifiedTestsAsync(string employeeId);
    
    /// <summary>
    /// Get user qualifications
    /// </summary>
    Task<IEnumerable<QualificationDetailDto>> GetUserQualificationsAsync(string employeeId);
    
    /// <summary>
    /// Check if user can validate results (must not be the same person who entered them)
    /// </summary>
    Task<bool> CanUserValidateResultsAsync(string validatorId, int sampleId, int testId);
    
    /// <summary>
    /// Get required qualification level for a test
    /// </summary>
    Task<string?> GetRequiredQualificationLevelAsync(int testId);
    
    /// <summary>
    /// Check if user has minimum qualification level
    /// </summary>
    Task<bool> HasMinimumQualificationAsync(string employeeId, int testId, string requiredLevel);
}

public class QualificationDetailDto
{
    public string EmployeeId { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public int TestStandId { get; set; }
    public string TestStandName { get; set; } = string.Empty;
    public string QualificationLevel { get; set; } = string.Empty;
    public DateTime? QualificationDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}

public class QualificationCheckResult
{
    public bool IsQualified { get; set; }
    public string QualificationLevel { get; set; } = string.Empty;
    public string RequiredLevel { get; set; } = string.Empty;
    public string? Message { get; set; }
    public bool CanPerformTest { get; set; }
    public bool CanReviewResults { get; set; }
    public bool CanValidateResults { get; set; }
}