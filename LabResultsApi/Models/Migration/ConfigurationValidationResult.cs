namespace LabResultsApi.Models.Migration;

public class ConfigurationValidationResult
{
    public bool IsValid { get; set; }
    public DateTime ValidationStartTime { get; set; }
    public DateTime ValidationEndTime { get; set; }
    public TimeSpan ValidationDuration { get; set; }
    public List<ConfigurationError> Errors { get; set; } = new();
    public List<ConfigurationError> Warnings { get; set; } = new();
    public List<ConfigurationError> Recommendations { get; set; } = new();
}

public class ConfigurationError
{
    public string Category { get; set; } = string.Empty;
    public string Property { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public ErrorSeverity Severity { get; set; }
    public string? SuggestedFix { get; set; }
}

public enum ErrorSeverity
{
    Information,
    Warning,
    Error,
    Critical
}

public class PrerequisiteCheckResult
{
    public bool AllPrerequisitesMet { get; set; }
    public DateTime CheckStartTime { get; set; }
    public DateTime CheckEndTime { get; set; }
    public TimeSpan CheckDuration { get; set; }
    public List<PrerequisiteCheck> PassedChecks { get; set; } = new();
    public List<PrerequisiteCheck> FailedChecks { get; set; } = new();
    public List<PrerequisiteCheck> SkippedChecks { get; set; } = new();
}

public class PrerequisiteCheck
{
    public string CheckName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool CheckResult { get; set; }
    public bool IsCritical { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Recommendation { get; set; }
}

public class EnvironmentCompatibilityResult
{
    public bool IsCompatible { get; set; }
    public DateTime CheckStartTime { get; set; }
    public DateTime CheckEndTime { get; set; }
    public EnvironmentInfo EnvironmentInfo { get; set; } = new();
    public List<CompatibilityCheck> CompatibilityChecks { get; set; } = new();
    public List<CompatibilityIssue> CompatibilityIssues { get; set; } = new();
}

public class EnvironmentInfo
{
    public string OperatingSystem { get; set; } = string.Empty;
    public string DotNetVersion { get; set; } = string.Empty;
    public string SqlServerVersion { get; set; } = string.Empty;
    public int ProcessorCount { get; set; }
    public long AvailableMemoryMB { get; set; }
    public long AvailableDiskSpaceGB { get; set; }
}

public class CompatibilityCheck
{
    public string CheckName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsCompatible { get; set; }
    public string Recommendation { get; set; } = string.Empty;
}

public class CompatibilityIssue
{
    public string Component { get; set; } = string.Empty;
    public string Issue { get; set; } = string.Empty;
    public string Impact { get; set; } = string.Empty;
    public string Resolution { get; set; } = string.Empty;
    public IssueSeverity Severity { get; set; }
}

public enum IssueSeverity
{
    Low,
    Medium,
    High,
    Critical
}