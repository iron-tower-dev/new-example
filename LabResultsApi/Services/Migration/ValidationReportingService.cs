using System.Text;
using System.Text.Json;
using LabResultsApi.Models.Migration;
using LabResultsApi.DTOs.Migration;

namespace LabResultsApi.Services.Migration;

public class ValidationReportingService : IValidationReportingService
{
    private readonly ILogger<ValidationReportingService> _logger;

    public ValidationReportingService(ILogger<ValidationReportingService> logger)
    {
        _logger = logger;
    }

    public async Task<ValidationReportDto> GenerateDetailedReportAsync(ValidationResult validationResult)
    {
        try
        {
            _logger.LogInformation("Generating detailed validation report for {QueryCount} queries", validationResult.QueriesValidated);

            var report = new ValidationReportDto
            {
                QueriesValidated = validationResult.QueriesValidated,
                QueriesMatched = validationResult.QueriesMatched,
                QueriesFailed = validationResult.QueriesFailed,
                MatchPercentage = validationResult.Summary.MatchPercentage,
                Duration = validationResult.Duration,
                Success = validationResult.Success,
                Summary = new ValidationSummaryDto
                {
                    MatchPercentage = validationResult.Summary.MatchPercentage,
                    TotalDiscrepancies = validationResult.Summary.TotalDiscrepancies,
                    AverageCurrentExecutionTime = validationResult.Summary.AverageCurrentExecutionTime,
                    AverageLegacyExecutionTime = validationResult.Summary.AverageLegacyExecutionTime,
                    CriticalIssues = validationResult.Summary.CriticalIssues
                }
            };

            // Convert query comparison results
            report.QueryReports = validationResult.Results.Select(r => new QueryComparisonReportDto
            {
                QueryName = r.QueryName,
                DataMatches = r.DataMatches,
                CurrentRowCount = r.CurrentRowCount,
                LegacyRowCount = r.LegacyRowCount,
                DiscrepancyCount = r.Discrepancies.Count,
                CurrentExecutionTime = r.CurrentExecutionTime,
                LegacyExecutionTime = r.LegacyExecutionTime,
                PerformanceRatio = r.LegacyExecutionTime.TotalMilliseconds > 0 
                    ? r.CurrentExecutionTime.TotalMilliseconds / r.LegacyExecutionTime.TotalMilliseconds 
                    : 0,
                Error = r.Error
            }).ToList();

            _logger.LogInformation("Detailed validation report generated successfully");
            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating detailed validation report");
            throw;
        }
    }

    public async Task<string> GenerateHtmlReportAsync(ValidationResult validationResult)
    {
        try
        {
            _logger.LogInformation("Generating HTML validation report");

            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("<title>SQL Validation Report</title>");
            html.AppendLine("<style>");
            html.AppendLine(GetReportCss());
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");

            // Header
            html.AppendLine("<div class='header'>");
            html.AppendLine("<h1>SQL Validation Report</h1>");
            html.AppendLine($"<p>Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");
            html.AppendLine($"<p>Duration: {validationResult.Duration.TotalMinutes:F2} minutes</p>");
            html.AppendLine("</div>");

            // Summary
            html.AppendLine("<div class='summary'>");
            html.AppendLine("<h2>Summary</h2>");
            html.AppendLine("<table>");
            html.AppendLine($"<tr><td>Queries Validated</td><td>{validationResult.QueriesValidated}</td></tr>");
            html.AppendLine($"<tr><td>Queries Matched</td><td>{validationResult.QueriesMatched}</td></tr>");
            html.AppendLine($"<tr><td>Queries Failed</td><td>{validationResult.QueriesFailed}</td></tr>");
            html.AppendLine($"<tr><td>Match Percentage</td><td>{validationResult.Summary.MatchPercentage:F1}%</td></tr>");
            html.AppendLine($"<tr><td>Total Discrepancies</td><td>{validationResult.Summary.TotalDiscrepancies}</td></tr>");
            html.AppendLine("</table>");
            html.AppendLine("</div>");

            // Performance Summary
            html.AppendLine("<div class='performance'>");
            html.AppendLine("<h2>Performance Summary</h2>");
            html.AppendLine("<table>");
            html.AppendLine($"<tr><td>Average Current Execution Time</td><td>{validationResult.Summary.AverageCurrentExecutionTime.TotalMilliseconds:F0}ms</td></tr>");
            html.AppendLine($"<tr><td>Average Legacy Execution Time</td><td>{validationResult.Summary.AverageLegacyExecutionTime.TotalMilliseconds:F0}ms</td></tr>");
            html.AppendLine("</table>");
            html.AppendLine("</div>");

            // Critical Issues
            if (validationResult.Summary.CriticalIssues.Any())
            {
                html.AppendLine("<div class='critical-issues'>");
                html.AppendLine("<h2>Critical Issues</h2>");
                html.AppendLine("<ul>");
                foreach (var issue in validationResult.Summary.CriticalIssues)
                {
                    html.AppendLine($"<li>{issue}</li>");
                }
                html.AppendLine("</ul>");
                html.AppendLine("</div>");
            }

            // Query Details
            html.AppendLine("<div class='query-details'>");
            html.AppendLine("<h2>Query Details</h2>");
            html.AppendLine("<table>");
            html.AppendLine("<tr><th>Query Name</th><th>Status</th><th>Current Rows</th><th>Legacy Rows</th><th>Discrepancies</th><th>Current Time</th><th>Legacy Time</th><th>Performance</th></tr>");

            foreach (var result in validationResult.Results)
            {
                var status = string.IsNullOrEmpty(result.Error) ? (result.DataMatches ? "✓ Match" : "✗ Mismatch") : "✗ Error";
                var statusClass = string.IsNullOrEmpty(result.Error) ? (result.DataMatches ? "success" : "warning") : "error";
                var performanceRatio = result.LegacyExecutionTime.TotalMilliseconds > 0 
                    ? result.CurrentExecutionTime.TotalMilliseconds / result.LegacyExecutionTime.TotalMilliseconds 
                    : 0;
                var performanceText = performanceRatio > 0 ? $"{performanceRatio:F2}x" : "N/A";

                html.AppendLine($"<tr class='{statusClass}'>");
                html.AppendLine($"<td>{result.QueryName}</td>");
                html.AppendLine($"<td>{status}</td>");
                html.AppendLine($"<td>{result.CurrentRowCount}</td>");
                html.AppendLine($"<td>{result.LegacyRowCount}</td>");
                html.AppendLine($"<td>{result.Discrepancies.Count}</td>");
                html.AppendLine($"<td>{result.CurrentExecutionTime.TotalMilliseconds:F0}ms</td>");
                html.AppendLine($"<td>{result.LegacyExecutionTime.TotalMilliseconds:F0}ms</td>");
                html.AppendLine($"<td>{performanceText}</td>");
                html.AppendLine("</tr>");

                // Add error details if present
                if (!string.IsNullOrEmpty(result.Error))
                {
                    html.AppendLine($"<tr class='error-detail'><td colspan='8'>Error: {result.Error}</td></tr>");
                }
            }

            html.AppendLine("</table>");
            html.AppendLine("</div>");

            html.AppendLine("</body>");
            html.AppendLine("</html>");

            _logger.LogInformation("HTML validation report generated successfully");
            return html.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating HTML validation report");
            throw;
        }
    }

    public async Task<string> GenerateCsvReportAsync(ValidationResult validationResult)
    {
        try
        {
            _logger.LogInformation("Generating CSV validation report");

            var csv = new StringBuilder();
            
            // Header
            csv.AppendLine("Query Name,Status,Data Matches,Current Rows,Legacy Rows,Discrepancies,Current Time (ms),Legacy Time (ms),Performance Ratio,Error");

            // Data rows
            foreach (var result in validationResult.Results)
            {
                var status = string.IsNullOrEmpty(result.Error) ? (result.DataMatches ? "Match" : "Mismatch") : "Error";
                var performanceRatio = result.LegacyExecutionTime.TotalMilliseconds > 0 
                    ? (result.CurrentExecutionTime.TotalMilliseconds / result.LegacyExecutionTime.TotalMilliseconds).ToString("F2")
                    : "N/A";

                csv.AppendLine($"\"{result.QueryName}\",\"{status}\",{result.DataMatches},{result.CurrentRowCount},{result.LegacyRowCount},{result.Discrepancies.Count},{result.CurrentExecutionTime.TotalMilliseconds:F0},{result.LegacyExecutionTime.TotalMilliseconds:F0},{performanceRatio},\"{result.Error?.Replace("\"", "\"\"")}\"");
            }

            _logger.LogInformation("CSV validation report generated successfully");
            return csv.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating CSV validation report");
            throw;
        }
    }

    public async Task<string> GenerateJsonReportAsync(ValidationResult validationResult)
    {
        try
        {
            _logger.LogInformation("Generating JSON validation report");

            var report = await GenerateDetailedReportAsync(validationResult);
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(report, options);
            
            _logger.LogInformation("JSON validation report generated successfully");
            return json;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating JSON validation report");
            throw;
        }
    }

    public async Task<ValidationSummaryDto> GenerateSummaryAsync(ValidationResult validationResult)
    {
        try
        {
            _logger.LogInformation("Generating validation summary");

            return new ValidationSummaryDto
            {
                MatchPercentage = validationResult.Summary.MatchPercentage,
                TotalDiscrepancies = validationResult.Summary.TotalDiscrepancies,
                AverageCurrentExecutionTime = validationResult.Summary.AverageCurrentExecutionTime,
                AverageLegacyExecutionTime = validationResult.Summary.AverageLegacyExecutionTime,
                CriticalIssues = validationResult.Summary.CriticalIssues
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating validation summary");
            throw;
        }
    }

    public async Task<List<TrendAnalysisDto>> AnalyzeTrendsAsync(List<ValidationResult> historicalResults)
    {
        try
        {
            _logger.LogInformation("Analyzing trends for {ResultCount} historical results", historicalResults.Count);

            var trends = new List<TrendAnalysisDto>();

            if (historicalResults.Count < 2)
            {
                _logger.LogWarning("Insufficient data for trend analysis");
                return trends;
            }

            // Sort by start time
            var sortedResults = historicalResults.OrderBy(r => r.StartTime).ToList();

            // Match percentage trend
            var matchPercentageTrend = new TrendAnalysisDto
            {
                MetricName = "Match Percentage",
                DataPoints = sortedResults.Select(r => new TrendDataPoint
                {
                    Timestamp = r.StartTime,
                    Value = r.Summary.MatchPercentage,
                    Label = $"{r.Summary.MatchPercentage:F1}%"
                }).ToList()
            };

            matchPercentageTrend.Direction = AnalyzeTrendDirection(matchPercentageTrend.DataPoints);
            matchPercentageTrend.ChangePercentage = CalculateChangePercentage(matchPercentageTrend.DataPoints);
            matchPercentageTrend.Analysis = GenerateTrendAnalysis(matchPercentageTrend);
            trends.Add(matchPercentageTrend);

            // Performance trend (current execution time)
            var performanceTrend = new TrendAnalysisDto
            {
                MetricName = "Average Current Execution Time",
                DataPoints = sortedResults.Select(r => new TrendDataPoint
                {
                    Timestamp = r.StartTime,
                    Value = r.Summary.AverageCurrentExecutionTime.TotalMilliseconds,
                    Label = $"{r.Summary.AverageCurrentExecutionTime.TotalMilliseconds:F0}ms"
                }).ToList()
            };

            performanceTrend.Direction = AnalyzeTrendDirection(performanceTrend.DataPoints, isLowerBetter: true);
            performanceTrend.ChangePercentage = CalculateChangePercentage(performanceTrend.DataPoints);
            performanceTrend.Analysis = GenerateTrendAnalysis(performanceTrend);
            trends.Add(performanceTrend);

            // Error count trend
            var errorTrend = new TrendAnalysisDto
            {
                MetricName = "Failed Queries",
                DataPoints = sortedResults.Select(r => new TrendDataPoint
                {
                    Timestamp = r.StartTime,
                    Value = r.QueriesFailed,
                    Label = r.QueriesFailed.ToString()
                }).ToList()
            };

            errorTrend.Direction = AnalyzeTrendDirection(errorTrend.DataPoints, isLowerBetter: true);
            errorTrend.ChangePercentage = CalculateChangePercentage(errorTrend.DataPoints);
            errorTrend.Analysis = GenerateTrendAnalysis(errorTrend);
            trends.Add(errorTrend);

            _logger.LogInformation("Trend analysis completed for {TrendCount} metrics", trends.Count);
            return trends;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing trends");
            throw;
        }
    }

    public async Task<string> ExportReportAsync(ValidationResult validationResult, ReportFormat format, string outputPath)
    {
        try
        {
            _logger.LogInformation("Exporting validation report in {Format} format to {OutputPath}", format, outputPath);

            string content;
            string fileName;

            switch (format)
            {
                case ReportFormat.Html:
                    content = await GenerateHtmlReportAsync(validationResult);
                    fileName = $"validation-report-{DateTime.Now:yyyyMMdd-HHmmss}.html";
                    break;
                case ReportFormat.Csv:
                    content = await GenerateCsvReportAsync(validationResult);
                    fileName = $"validation-report-{DateTime.Now:yyyyMMdd-HHmmss}.csv";
                    break;
                case ReportFormat.Json:
                    content = await GenerateJsonReportAsync(validationResult);
                    fileName = $"validation-report-{DateTime.Now:yyyyMMdd-HHmmss}.json";
                    break;
                default:
                    throw new NotSupportedException($"Report format {format} is not supported");
            }

            var fullPath = Path.Combine(outputPath, fileName);
            Directory.CreateDirectory(outputPath);
            await File.WriteAllTextAsync(fullPath, content);

            _logger.LogInformation("Validation report exported successfully to {FullPath}", fullPath);
            return fullPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting validation report");
            throw;
        }
    }

    public async Task<ValidationReportDto> GenerateComparisonReportAsync(ValidationResult current, ValidationResult previous)
    {
        try
        {
            _logger.LogInformation("Generating comparison report between current and previous validation results");

            var report = await GenerateDetailedReportAsync(current);

            // Add comparison data
            report.Summary.CriticalIssues.Add($"Previous Match Percentage: {previous.Summary.MatchPercentage:F1}%");
            report.Summary.CriticalIssues.Add($"Current Match Percentage: {current.Summary.MatchPercentage:F1}%");
            
            var matchPercentageChange = current.Summary.MatchPercentage - previous.Summary.MatchPercentage;
            if (Math.Abs(matchPercentageChange) > 0.1)
            {
                var direction = matchPercentageChange > 0 ? "improved" : "declined";
                report.Summary.CriticalIssues.Add($"Match percentage {direction} by {Math.Abs(matchPercentageChange):F1}%");
            }

            _logger.LogInformation("Comparison report generated successfully");
            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating comparison report");
            throw;
        }
    }

    private string GetReportCss()
    {
        return @"
            body { font-family: Arial, sans-serif; margin: 20px; }
            .header { background-color: #f5f5f5; padding: 20px; border-radius: 5px; margin-bottom: 20px; }
            .summary, .performance, .critical-issues, .query-details { margin-bottom: 30px; }
            h1 { color: #333; margin: 0; }
            h2 { color: #666; border-bottom: 2px solid #ddd; padding-bottom: 5px; }
            table { width: 100%; border-collapse: collapse; margin-top: 10px; }
            th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
            th { background-color: #f2f2f2; font-weight: bold; }
            .success { background-color: #d4edda; }
            .warning { background-color: #fff3cd; }
            .error { background-color: #f8d7da; }
            .error-detail { background-color: #f8f9fa; font-style: italic; }
            .critical-issues ul { color: #721c24; }
        ";
    }

    private TrendDirection AnalyzeTrendDirection(List<TrendDataPoint> dataPoints, bool isLowerBetter = false)
    {
        if (dataPoints.Count < 2) return TrendDirection.Stable;

        var values = dataPoints.Select(dp => dp.Value).ToList();
        var firstHalf = values.Take(values.Count / 2).Average();
        var secondHalf = values.Skip(values.Count / 2).Average();

        var changePercent = Math.Abs((secondHalf - firstHalf) / firstHalf * 100);
        
        if (changePercent < 5) return TrendDirection.Stable;
        if (changePercent > 20) return TrendDirection.Volatile;

        var isImproving = isLowerBetter ? secondHalf < firstHalf : secondHalf > firstHalf;
        return isImproving ? TrendDirection.Improving : TrendDirection.Declining;
    }

    private double CalculateChangePercentage(List<TrendDataPoint> dataPoints)
    {
        if (dataPoints.Count < 2) return 0;

        var first = dataPoints.First().Value;
        var last = dataPoints.Last().Value;

        return first != 0 ? ((last - first) / first * 100) : 0;
    }

    private string GenerateTrendAnalysis(TrendAnalysisDto trend)
    {
        return trend.Direction switch
        {
            TrendDirection.Improving => $"{trend.MetricName} is showing improvement with a {Math.Abs(trend.ChangePercentage):F1}% positive change.",
            TrendDirection.Declining => $"{trend.MetricName} is declining with a {Math.Abs(trend.ChangePercentage):F1}% negative change.",
            TrendDirection.Stable => $"{trend.MetricName} remains stable with minimal variation.",
            TrendDirection.Volatile => $"{trend.MetricName} shows high volatility with significant fluctuations.",
            _ => $"{trend.MetricName} trend analysis inconclusive."
        };
    }
}