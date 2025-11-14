using LabResultsApi.DTOs;

namespace LabResultsApi.Services;

public interface ITestSchedulingService
{
    /// <summary>
    /// Automatically schedule tests based on completed test results and business rules
    /// </summary>
    Task<TestSchedulingResult> AutoScheduleTestsAsync(int sampleId, int completedTestId, string tagNumber, string component, string location);
    
    /// <summary>
    /// Check if a test should be scheduled based on business rules
    /// </summary>
    Task<bool> ShouldScheduleTestAsync(int sampleId, int testId, string tagNumber, string component, string location);
    
    /// <summary>
    /// Remove tests that no longer meet scheduling criteria
    /// </summary>
    Task<TestSchedulingResult> RemoveUnneededTestsAsync(int sampleId, string tagNumber, string component, string location);
    
    /// <summary>
    /// Get scheduled tests for a sample
    /// </summary>
    Task<IEnumerable<ScheduledTestDto>> GetScheduledTestsAsync(int sampleId);
    
    /// <summary>
    /// Check minimum interval requirements for test scheduling
    /// </summary>
    Task<bool> CheckMinimumIntervalAsync(string tagNumber, string component, string location, int testId, DateTime sampleDate);
    
    /// <summary>
    /// Get test scheduling rules for equipment
    /// </summary>
    Task<IEnumerable<TestScheduleRuleDto>> GetTestRulesAsync(string tagNumber, string component, string location);
    
    /// <summary>
    /// Schedule Ferrography test when Large Spectroscopy is completed
    /// </summary>
    Task<bool> ScheduleFerrographyAsync(int sampleId);
    
    /// <summary>
    /// Set sample schedule type based on test results
    /// </summary>
    Task<bool> SetSampleScheduleTypeAsync(int sampleId, string scheduleType);
}

public class TestSchedulingResult
{
    public bool Success { get; set; }
    public List<int> TestsAdded { get; set; } = new();
    public List<int> TestsRemoved { get; set; } = new();
    public List<string> Messages { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

public class ScheduledTestDto
{
    public int TestId { get; set; }
    public string TestName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? ScheduledDate { get; set; }
    public string ScheduleType { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

public class TestScheduleRuleDto
{
    public int TestId { get; set; }
    public string TestName { get; set; } = string.Empty;
    public string RuleType { get; set; } = string.Empty; // ADD, REMOVE
    public string Condition { get; set; } = string.Empty;
    public double? ThresholdValue { get; set; }
    public string? ThresholdOperator { get; set; } // >, <, >=, <=, =
    public int? TriggerTestId { get; set; }
    public int? MinimumInterval { get; set; }
    public bool IsActive { get; set; }
}