namespace LabResultsApi.Models.Migration;

public class LogSearchCriteria
{
    public LogLevel? MinLevel { get; set; }
    public LogLevel? MaxLevel { get; set; }
    public string? Component { get; set; }
    public string? MessageContains { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public bool IncludeExceptions { get; set; } = true;
}