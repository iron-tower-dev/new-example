namespace LabResultsApi.Models.Migration;

public class StructuredLogEntry
{
    public Guid Id { get; set; }
    public Guid MigrationId { get; set; }
    public DateTime Timestamp { get; set; }
    public LogLevel Level { get; set; }
    public string Component { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object>? Properties { get; set; }
    public string? ExceptionDetails { get; set; }
}