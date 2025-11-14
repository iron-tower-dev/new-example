namespace LabResultsApi.Models.Migration;

public class ValidationResult
{
    public int QueriesValidated { get; set; }
    public int QueriesMatched { get; set; }
    public int QueriesFailed { get; set; }
    public List<QueryComparisonResult> Results { get; set; } = new();
    public ValidationSummary Summary { get; set; } = new();
    public TimeSpan Duration { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool Success => QueriesFailed == 0;
}

public class QueryComparisonResult
{
    public string QueryName { get; set; } = string.Empty;
    public bool DataMatches { get; set; }
    public int CurrentRowCount { get; set; }
    public int LegacyRowCount { get; set; }
    public List<DataDiscrepancy> Discrepancies { get; set; } = new();
    public TimeSpan CurrentExecutionTime { get; set; }
    public TimeSpan LegacyExecutionTime { get; set; }
    public string? CurrentQuery { get; set; }
    public string? LegacyQuery { get; set; }
    public string? Error { get; set; }
}

public class DataDiscrepancy
{
    public string FieldName { get; set; } = string.Empty;
    public object? CurrentValue { get; set; }
    public object? LegacyValue { get; set; }
    public string RowIdentifier { get; set; } = string.Empty;
    public DiscrepancyType Type { get; set; }
    public string? Description { get; set; }
}

public enum DiscrepancyType
{
    ValueMismatch,
    MissingInCurrent,
    MissingInLegacy,
    TypeMismatch,
    FormatDifference
}

public class ValidationSummary
{
    public double MatchPercentage => QueriesValidated > 0 ? (double)QueriesMatched / QueriesValidated * 100 : 0;
    public int QueriesValidated { get; set; }
    public int QueriesMatched { get; set; }
    public int QueriesFailed { get; set; }
    public int TotalDiscrepancies { get; set; }
    public TimeSpan AverageCurrentExecutionTime { get; set; }
    public TimeSpan AverageLegacyExecutionTime { get; set; }
    public List<string> CriticalIssues { get; set; } = new();
}

public class PerformanceComparisonResult
{
    public string QueryName { get; set; } = string.Empty;
    public TimeSpan CurrentExecutionTime { get; set; }
    public TimeSpan LegacyExecutionTime { get; set; }
    public double PerformanceRatio => LegacyExecutionTime.TotalMilliseconds > 0 
        ? CurrentExecutionTime.TotalMilliseconds / LegacyExecutionTime.TotalMilliseconds 
        : 0;
    public bool CurrentIsFaster => CurrentExecutionTime < LegacyExecutionTime;
    public TimeSpan TimeDifference => CurrentExecutionTime - LegacyExecutionTime;
}