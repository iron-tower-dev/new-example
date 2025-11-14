namespace LabResultsApi.Models;

public record HealthCheckResult
{
    public string Status { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}