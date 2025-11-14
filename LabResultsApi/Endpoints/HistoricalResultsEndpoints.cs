using Microsoft.AspNetCore.Mvc;
using LabResultsApi.Services;
using LabResultsApi.DTOs;

namespace LabResultsApi.Endpoints;

public static class HistoricalResultsEndpoints
{
    public static void MapHistoricalResultsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/historical-results")
            .WithTags("Historical Results")
            .WithOpenApi();

        // Get last N results for a specific test and equipment/component combination
        group.MapGet("/samples/{sampleId}/tests/{testId}", GetHistoricalResults)
            .WithName("GetHistoricalResults")
            .WithSummary("Get historical test results for a sample and test combination")
            .WithDescription("Returns the last 12 (or specified count) historical test results for the same equipment/component combination");

        // Get extended historical results with date range filtering
        group.MapGet("/samples/{sampleId}/tests/{testId}/extended", GetExtendedHistoricalResults)
            .WithName("GetExtendedHistoricalResults")
            .WithSummary("Get extended historical test results with filtering")
            .WithDescription("Returns historical test results with date range filtering and pagination");

        // Get historical results summary (just basic info for display)
        group.MapGet("/samples/{sampleId}/tests/{testId}/summary", GetHistoricalResultsSummary)
            .WithName("GetHistoricalResultsSummary")
            .WithSummary("Get historical results summary")
            .WithDescription("Returns a summary of historical results for quick display");

        // Get detailed historical result for a specific sample
        group.MapGet("/samples/{sampleId}/tests/{testId}/details/{historicalSampleId}", GetHistoricalResultDetails)
            .WithName("GetHistoricalResultDetails")
            .WithSummary("Get detailed historical result")
            .WithDescription("Returns detailed test results for a specific historical sample");
    }

    private static async Task<IResult> GetHistoricalResults(
        int sampleId,
        int testId,
        ITestResultService testResultService,
        ILogger<Program> logger,
        [FromQuery] int count = 12)
    {
        try
        {
            logger.LogInformation("Getting historical results for sample {SampleId}, test {TestId}, count {Count}", 
                sampleId, testId, count);

            var results = await testResultService.GetTestResultsHistoryAsync(sampleId, testId, count);
            
            return Results.Ok(new
            {
                SampleId = sampleId,
                TestId = testId,
                Count = results.Count(),
                Results = results
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting historical results for sample {SampleId}, test {TestId}", 
                sampleId, testId);
            return Results.Problem(
                title: "Error retrieving historical results",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> GetExtendedHistoricalResults(
        int sampleId,
        int testId,
        IRawSqlService rawSqlService,
        ITestResultService testResultService,
        ILogger<Program> logger,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? status = null)
    {
        try
        {
            logger.LogInformation("Getting extended historical results for sample {SampleId}, test {TestId}", 
                sampleId, testId);

            // Get extended history with filtering
            var extendedHistory = await rawSqlService.GetExtendedSampleHistoryAsync(
                sampleId, testId, fromDate, toDate, page, pageSize, status);

            var results = new List<TestResultDto>();
            foreach (var historyItem in extendedHistory.Results)
            {
                var testResult = await testResultService.GetTestResultsAsync(historyItem.SampleId, testId);
                if (testResult != null)
                {
                    results.Add(testResult);
                }
            }

            return Results.Ok(new
            {
                SampleId = sampleId,
                TestId = testId,
                TotalCount = extendedHistory.TotalCount,
                Page = extendedHistory.Page,
                PageSize = extendedHistory.PageSize,
                TotalPages = extendedHistory.TotalPages,
                Results = results
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting extended historical results for sample {SampleId}, test {TestId}", 
                sampleId, testId);
            return Results.Problem(
                title: "Error retrieving extended historical results",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> GetHistoricalResultsSummary(
        int sampleId,
        int testId,
        IRawSqlService rawSqlService,
        ILogger<Program> logger,
        [FromQuery] int count = 12)
    {
        try
        {
            logger.LogInformation("Getting historical results summary for sample {SampleId}, test {TestId}", 
                sampleId, testId);

            var history = await rawSqlService.GetSampleHistoryAsync(sampleId, testId);
            var summary = history.Take(count).Select(h => new HistoricalResultSummaryDto
            {
                SampleId = h.SampleId,
                TagNumber = h.TagNumber,
                SampleDate = h.SampleDate,
                Status = h.Status,
                EntryDate = h.EntryDate,
                TestName = h.TestName
            }).ToList();

            return Results.Ok(new
            {
                SampleId = sampleId,
                TestId = testId,
                Count = summary.Count,
                Summary = summary
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting historical results summary for sample {SampleId}, test {TestId}", 
                sampleId, testId);
            return Results.Problem(
                title: "Error retrieving historical results summary",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> GetHistoricalResultDetails(
        int sampleId,
        int testId,
        int historicalSampleId,
        ITestResultService testResultService,
        ILogger<Program> logger)
    {
        try
        {
            logger.LogInformation("Getting historical result details for sample {SampleId}, test {TestId}, historical sample {HistoricalSampleId}", 
                sampleId, testId, historicalSampleId);

            var result = await testResultService.GetTestResultsAsync(historicalSampleId, testId);
            
            if (result == null)
            {
                return Results.NotFound($"Historical result not found for sample {historicalSampleId}, test {testId}");
            }

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting historical result details for sample {SampleId}, test {TestId}, historical sample {HistoricalSampleId}", 
                sampleId, testId, historicalSampleId);
            return Results.Problem(
                title: "Error retrieving historical result details",
                detail: ex.Message,
                statusCode: 500);
        }
    }

}