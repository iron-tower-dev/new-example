namespace LabResultsApi.Models.Migration;

public class LogAggregationResult
{
    public Dictionary<LogLevel, int> CountsByLevel { get; set; } = new();
    public Dictionary<string, int> CountsByComponent { get; set; } = new();
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int TotalEntries { get; set; }
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
}