using NUnit.Framework;
using Moq;
using Microsoft.Extensions.Logging;
using LabResultsApi.Services.Migration;
using LabResultsApi.Models.Migration;
using FluentAssertions;
using System.Text.Json;

namespace LabResultsApi.Tests.Services.Migration;

[TestFixture]
public class ValidationReportingServiceTests
{
    private Mock<ILogger<ValidationReportingService>> _mockLogger;
    private ValidationReportingService _service;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<ValidationReportingService>>();
        _service = new ValidationReportingService(_mockLogger.Object);
    }

    [Test]
    public async Task GenerateDetailedReportAsync_WithValidationResult_ShouldReturnDetailedReport()
    {
        // Arrange
        var validationResult = CreateSampleValidationResult();

        // Act
        var report = await _service.GenerateDetailedReportAsync(validationResult);

        // Assert
        report.Should().NotBeNull();
        report.QueriesValidated.Should().Be(validationResult.QueriesValidated);
        report.QueriesMatched.Should().Be(validationResult.QueriesMatched);
        report.QueriesFailed.Should().Be(validationResult.QueriesFailed);
        report.MatchPercentage.Should().Be(validationResult.Summary.MatchPercentage);
        report.Duration.Should().Be(validationResult.Duration);
        report.Success.Should().Be(validationResult.Success);
        report.QueryReports.Should().HaveCount(validationResult.Results.Count);
        report.Summary.Should().NotBeNull();
    }

    [Test]
    public async Task GenerateHtmlReportAsync_WithValidationResult_ShouldReturnValidHtml()
    {
        // Arrange
        var validationResult = CreateSampleValidationResult();

        // Act
        var html = await _service.GenerateHtmlReportAsync(validationResult);

        // Assert
        html.Should().NotBeNullOrEmpty();
        html.Should().Contain("<!DOCTYPE html>");
        html.Should().Contain("<html>");
        html.Should().Contain("SQL Validation Report");
        html.Should().Contain("Summary");
        html.Should().Contain("Query Details");
        html.Should().Contain($"Queries Validated</td><td>{validationResult.QueriesValidated}");
        html.Should().Contain($"Queries Matched</td><td>{validationResult.QueriesMatched}");
        html.Should().Contain("</html>");
    }

    [Test]
    public async Task GenerateHtmlReportAsync_WithCriticalIssues_ShouldIncludeCriticalIssuesSection()
    {
        // Arrange
        var validationResult = CreateSampleValidationResult();
        validationResult.Summary.CriticalIssues.Add("Critical issue 1");
        validationResult.Summary.CriticalIssues.Add("Critical issue 2");

        // Act
        var html = await _service.GenerateHtmlReportAsync(validationResult);

        // Assert
        html.Should().Contain("Critical Issues");
        html.Should().Contain("Critical issue 1");
        html.Should().Contain("Critical issue 2");
    }

    [Test]
    public async Task GenerateCsvReportAsync_WithValidationResult_ShouldReturnValidCsv()
    {
        // Arrange
        var validationResult = CreateSampleValidationResult();

        // Act
        var csv = await _service.GenerateCsvReportAsync(validationResult);

        // Assert
        csv.Should().NotBeNullOrEmpty();
        csv.Should().StartWith("Query Name,Status,Data Matches,Current Rows,Legacy Rows,Discrepancies,Current Time (ms),Legacy Time (ms),Performance Ratio,Error");
        
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        lines.Should().HaveCount(validationResult.Results.Count + 1); // Header + data rows
        
        foreach (var result in validationResult.Results)
        {
            csv.Should().Contain(result.QueryName);
        }
    }

    [Test]
    public async Task GenerateJsonReportAsync_WithValidationResult_ShouldReturnValidJson()
    {
        // Arrange
        var validationResult = CreateSampleValidationResult();

        // Act
        var json = await _service.GenerateJsonReportAsync(validationResult);

        // Assert
        json.Should().NotBeNullOrEmpty();
        
        // Verify it's valid JSON by deserializing
        var deserializedReport = JsonSerializer.Deserialize<object>(json);
        deserializedReport.Should().NotBeNull();
        
        json.Should().Contain("queriesValidated");
        json.Should().Contain("queriesMatched");
        json.Should().Contain("queryReports");
    }

    [Test]
    public async Task GenerateSummaryAsync_WithValidationResult_ShouldReturnSummary()
    {
        // Arrange
        var validationResult = CreateSampleValidationResult();

        // Act
        var summary = await _service.GenerateSummaryAsync(validationResult);

        // Assert
        summary.Should().NotBeNull();
        summary.MatchPercentage.Should().Be(validationResult.Summary.MatchPercentage);
        summary.TotalDiscrepancies.Should().Be(validationResult.Summary.TotalDiscrepancies);
        summary.AverageCurrentExecutionTime.Should().Be(validationResult.Summary.AverageCurrentExecutionTime);
        summary.AverageLegacyExecutionTime.Should().Be(validationResult.Summary.AverageLegacyExecutionTime);
        summary.CriticalIssues.Should().BeEquivalentTo(validationResult.Summary.CriticalIssues);
    }

    [Test]
    public async Task AnalyzeTrendsAsync_WithInsufficientData_ShouldReturnEmptyTrends()
    {
        // Arrange
        var historicalResults = new List<ValidationResult>
        {
            CreateSampleValidationResult()
        };

        // Act
        var trends = await _service.AnalyzeTrendsAsync(historicalResults);

        // Assert
        trends.Should().BeEmpty();
    }

    [Test]
    public async Task AnalyzeTrendsAsync_WithSufficientData_ShouldReturnTrends()
    {
        // Arrange
        var historicalResults = new List<ValidationResult>
        {
            CreateSampleValidationResult(DateTime.UtcNow.AddDays(-2), 80.0, 100, 0),
            CreateSampleValidationResult(DateTime.UtcNow.AddDays(-1), 85.0, 110, 1),
            CreateSampleValidationResult(DateTime.UtcNow, 90.0, 120, 2)
        };

        // Act
        var trends = await _service.AnalyzeTrendsAsync(historicalResults);

        // Assert
        trends.Should().NotBeEmpty();
        trends.Should().HaveCount(3); // Match percentage, performance, error count
        
        var matchPercentageTrend = trends.FirstOrDefault(t => t.MetricName == "Match Percentage");
        matchPercentageTrend.Should().NotBeNull();
        matchPercentageTrend!.DataPoints.Should().HaveCount(3);
        matchPercentageTrend.Direction.Should().Be(TrendDirection.Improving);
        
        var performanceTrend = trends.FirstOrDefault(t => t.MetricName == "Average Current Execution Time");
        performanceTrend.Should().NotBeNull();
        performanceTrend!.DataPoints.Should().HaveCount(3);
        
        var errorTrend = trends.FirstOrDefault(t => t.MetricName == "Failed Queries");
        errorTrend.Should().NotBeNull();
        errorTrend!.DataPoints.Should().HaveCount(3);
    }

    [Test]
    public async Task ExportReportAsync_WithHtmlFormat_ShouldCreateHtmlFile()
    {
        // Arrange
        var validationResult = CreateSampleValidationResult();
        var outputPath = Path.GetTempPath();
        var format = ReportFormat.Html;

        // Act
        var filePath = await _service.ExportReportAsync(validationResult, format, outputPath);

        // Assert
        filePath.Should().NotBeNullOrEmpty();
        File.Exists(filePath).Should().BeTrue();
        filePath.Should().EndWith(".html");
        
        var content = await File.ReadAllTextAsync(filePath);
        content.Should().Contain("<!DOCTYPE html>");
        
        // Cleanup
        File.Delete(filePath);
    }

    [Test]
    public async Task ExportReportAsync_WithCsvFormat_ShouldCreateCsvFile()
    {
        // Arrange
        var validationResult = CreateSampleValidationResult();
        var outputPath = Path.GetTempPath();
        var format = ReportFormat.Csv;

        // Act
        var filePath = await _service.ExportReportAsync(validationResult, format, outputPath);

        // Assert
        filePath.Should().NotBeNullOrEmpty();
        File.Exists(filePath).Should().BeTrue();
        filePath.Should().EndWith(".csv");
        
        var content = await File.ReadAllTextAsync(filePath);
        content.Should().StartWith("Query Name,Status");
        
        // Cleanup
        File.Delete(filePath);
    }

    [Test]
    public async Task ExportReportAsync_WithJsonFormat_ShouldCreateJsonFile()
    {
        // Arrange
        var validationResult = CreateSampleValidationResult();
        var outputPath = Path.GetTempPath();
        var format = ReportFormat.Json;

        // Act
        var filePath = await _service.ExportReportAsync(validationResult, format, outputPath);

        // Assert
        filePath.Should().NotBeNullOrEmpty();
        File.Exists(filePath).Should().BeTrue();
        filePath.Should().EndWith(".json");
        
        var content = await File.ReadAllTextAsync(filePath);
        var deserializedReport = JsonSerializer.Deserialize<object>(content);
        deserializedReport.Should().NotBeNull();
        
        // Cleanup
        File.Delete(filePath);
    }

    [Test]
    public async Task ExportReportAsync_WithUnsupportedFormat_ShouldThrowNotSupportedException()
    {
        // Arrange
        var validationResult = CreateSampleValidationResult();
        var outputPath = Path.GetTempPath();
        var format = ReportFormat.Pdf; // Not implemented

        // Act & Assert
        await Assert.ThrowsAsync<NotSupportedException>(
            () => _service.ExportReportAsync(validationResult, format, outputPath));
    }

    [Test]
    public async Task GenerateComparisonReportAsync_WithTwoResults_ShouldReturnComparisonReport()
    {
        // Arrange
        var currentResult = CreateSampleValidationResult(DateTime.UtcNow, 90.0);
        var previousResult = CreateSampleValidationResult(DateTime.UtcNow.AddDays(-1), 85.0);

        // Act
        var report = await _service.GenerateComparisonReportAsync(currentResult, previousResult);

        // Assert
        report.Should().NotBeNull();
        report.Summary.CriticalIssues.Should().Contain(issue => issue.Contains("Previous Match Percentage: 85.0%"));
        report.Summary.CriticalIssues.Should().Contain(issue => issue.Contains("Current Match Percentage: 90.0%"));
        report.Summary.CriticalIssues.Should().Contain(issue => issue.Contains("improved by 5.0%"));
    }

    private ValidationResult CreateSampleValidationResult(DateTime? startTime = null, double matchPercentage = 75.0, int avgCurrentTime = 100, int failedQueries = 1)
    {
        var result = new ValidationResult
        {
            QueriesValidated = 4,
            QueriesMatched = 3,
            QueriesFailed = failedQueries,
            Duration = TimeSpan.FromMinutes(5),
            StartTime = startTime ?? DateTime.UtcNow.AddMinutes(-5),
            EndTime = startTime?.AddMinutes(5) ?? DateTime.UtcNow,
            Results = new List<QueryComparisonResult>
            {
                new()
                {
                    QueryName = "Query1",
                    DataMatches = true,
                    CurrentRowCount = 10,
                    LegacyRowCount = 10,
                    CurrentExecutionTime = TimeSpan.FromMilliseconds(avgCurrentTime),
                    LegacyExecutionTime = TimeSpan.FromMilliseconds(120),
                    Discrepancies = new List<DataDiscrepancy>()
                },
                new()
                {
                    QueryName = "Query2",
                    DataMatches = true,
                    CurrentRowCount = 5,
                    LegacyRowCount = 5,
                    CurrentExecutionTime = TimeSpan.FromMilliseconds(avgCurrentTime + 10),
                    LegacyExecutionTime = TimeSpan.FromMilliseconds(130),
                    Discrepancies = new List<DataDiscrepancy>()
                },
                new()
                {
                    QueryName = "Query3",
                    DataMatches = true,
                    CurrentRowCount = 15,
                    LegacyRowCount = 15,
                    CurrentExecutionTime = TimeSpan.FromMilliseconds(avgCurrentTime + 20),
                    LegacyExecutionTime = TimeSpan.FromMilliseconds(140),
                    Discrepancies = new List<DataDiscrepancy>()
                },
                new()
                {
                    QueryName = "Query4",
                    DataMatches = false,
                    CurrentRowCount = 8,
                    LegacyRowCount = 10,
                    CurrentExecutionTime = TimeSpan.FromMilliseconds(avgCurrentTime + 30),
                    LegacyExecutionTime = TimeSpan.FromMilliseconds(150),
                    Error = failedQueries > 0 ? "Connection timeout" : null,
                    Discrepancies = new List<DataDiscrepancy>
                    {
                        new()
                        {
                            FieldName = "RowCount",
                            CurrentValue = 8,
                            LegacyValue = 10,
                            Type = DiscrepancyType.ValueMismatch,
                            RowIdentifier = "TOTAL"
                        }
                    }
                }
            },
            Summary = new ValidationSummary
            {
                QueriesValidated = 4,
                QueriesMatched = 3,
                QueriesFailed = failedQueries,
                TotalDiscrepancies = 1,
                AverageCurrentExecutionTime = TimeSpan.FromMilliseconds(avgCurrentTime + 15),
                AverageLegacyExecutionTime = TimeSpan.FromMilliseconds(135),
                CriticalIssues = new List<string> { "Query4: Data mismatch" }
            }
        };

        // Override match percentage if specified
        if (Math.Abs(matchPercentage - 75.0) > 0.1)
        {
            // Adjust the matched/failed counts to achieve the desired percentage
            var totalQueries = result.QueriesValidated;
            var targetMatched = (int)Math.Round(totalQueries * matchPercentage / 100.0);
            result.QueriesMatched = targetMatched;
            result.QueriesFailed = totalQueries - targetMatched;
            result.Summary.QueriesMatched = targetMatched;
            result.Summary.QueriesFailed = totalQueries - targetMatched;
        }

        return result;
    }
}