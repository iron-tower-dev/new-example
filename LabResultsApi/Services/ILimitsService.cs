using LabResultsApi.DTOs;

namespace LabResultsApi.Services;

public interface ILimitsService
{
    /// <summary>
    /// Check if test results are within acceptable limits
    /// </summary>
    Task<LimitsCheckResult> CheckLimitsAsync(int sampleId, int testId, Dictionary<string, double> results);
    
    /// <summary>
    /// Check a single result value against limits
    /// </summary>
    Task<bool> IsResultWithinLimitsAsync(int sampleId, int testId, string parameter, double result);
    
    /// <summary>
    /// Get limits for a specific test and equipment combination
    /// </summary>
    Task<IEnumerable<TestLimitDto>> GetLimitsAsync(string tagNumber, string component, string location, int testId);
    
    /// <summary>
    /// Get limits cross-reference data
    /// </summary>
    Task<IEnumerable<LimitsCrossReferenceDto>> GetLimitsCrossReferenceAsync(int testId);
    
    /// <summary>
    /// Evaluate test results and determine status
    /// </summary>
    Task<TestEvaluationResult> EvaluateTestResultsAsync(int sampleId, int testId, Dictionary<string, double> results);
    
    /// <summary>
    /// Get LCDE (Lubrication Condition Data Evaluation) limits
    /// </summary>
    Task<IEnumerable<LcdeLimitDto>> GetLcdeLimitsAsync(string tagNumber, string component, string location);
    
    /// <summary>
    /// Check if results exceed alert thresholds
    /// </summary>
    Task<AlertResult> CheckAlertThresholdsAsync(int sampleId, int testId, Dictionary<string, double> results);
}

public class LimitsCheckResult
{
    public bool IsWithinLimits { get; set; }
    public List<LimitViolation> Violations { get; set; } = new();
    public List<LimitWarning> Warnings { get; set; } = new();
    public string OverallStatus { get; set; } = "NORMAL"; // NORMAL, WARNING, CRITICAL
    public string? Message { get; set; }
}

public class LimitViolation
{
    public string Parameter { get; set; } = string.Empty;
    public double ActualValue { get; set; }
    public double LimitValue { get; set; }
    public string LimitType { get; set; } = string.Empty; // HIGH, LOW
    public string Severity { get; set; } = string.Empty; // WARNING, CRITICAL
    public string Message { get; set; } = string.Empty;
}

public class LimitWarning
{
    public string Parameter { get; set; } = string.Empty;
    public double ActualValue { get; set; }
    public double WarningValue { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class TestLimitDto
{
    public int LimitId { get; set; }
    public int TestId { get; set; }
    public string Parameter { get; set; } = string.Empty;
    public double? HighLimit { get; set; }
    public double? LowLimit { get; set; }
    public double? HighWarning { get; set; }
    public double? LowWarning { get; set; }
    public string LimitType { get; set; } = string.Empty;
    public string QualityClass { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class LimitsCrossReferenceDto
{
    public int CrossRefId { get; set; }
    public int TestId { get; set; }
    public int LimitId { get; set; }
    public string Parameter { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class LcdeLimitDto
{
    public int LcdeId { get; set; }
    public string TagNumber { get; set; } = string.Empty;
    public string Component { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Parameter { get; set; } = string.Empty;
    public double? NormalHigh { get; set; }
    public double? NormalLow { get; set; }
    public double? CautionHigh { get; set; }
    public double? CautionLow { get; set; }
    public double? CriticalHigh { get; set; }
    public double? CriticalLow { get; set; }
    public DateTime? EffectiveDate { get; set; }
}

public class TestEvaluationResult
{
    public string Status { get; set; } = "NORMAL"; // NORMAL, CAUTION, CRITICAL
    public string Recommendation { get; set; } = string.Empty;
    public List<string> Alerts { get; set; } = new();
    public bool RequiresReview { get; set; }
    public string? ReviewerLevel { get; set; }
}

public class AlertResult
{
    public bool HasAlerts { get; set; }
    public List<Alert> Alerts { get; set; } = new();
}

public class Alert
{
    public string Type { get; set; } = string.Empty; // TREND, LIMIT, CRITICAL
    public string Parameter { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty; // LOW, MEDIUM, HIGH
    public DateTime AlertDate { get; set; }
}