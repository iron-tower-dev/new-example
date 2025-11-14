using LabResultsApi.Models.Migration;
using LabResultsApi.DTOs.Migration;

namespace LabResultsApi.Services.Migration;

public interface IValidationReportingService
{
    Task<ValidationReportDto> GenerateDetailedReportAsync(ValidationResult validationResult);
    Task<string> GenerateHtmlReportAsync(ValidationResult validationResult);
    Task<string> GenerateCsvReportAsync(ValidationResult validationResult);
    Task<string> GenerateJsonReportAsync(ValidationResult validationResult);
    Task<ValidationSummaryDto> GenerateSummaryAsync(ValidationResult validationResult);
    Task<List<TrendAnalysisDto>> AnalyzeTrendsAsync(List<ValidationResult> historicalResults);
    Task<string> ExportReportAsync(ValidationResult validationResult, ReportFormat format, string outputPath);
    Task<ValidationReportDto> GenerateComparisonReportAsync(ValidationResult current, ValidationResult previous);
}

public enum ReportFormat
{
    Html,
    Csv,
    Json,
    Pdf,
    Excel
}

public class TrendAnalysisDto
{
    public string MetricName { get; set; } = string.Empty;
    public List<TrendDataPoint> DataPoints { get; set; } = new();
    public TrendDirection Direction { get; set; }
    public double ChangePercentage { get; set; }
    public string Analysis { get; set; } = string.Empty;
}

public class TrendDataPoint
{
    public DateTime Timestamp { get; set; }
    public double Value { get; set; }
    public string Label { get; set; } = string.Empty;
}

public enum TrendDirection
{
    Improving,
    Declining,
    Stable,
    Volatile
}