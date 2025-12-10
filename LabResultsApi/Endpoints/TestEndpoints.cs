using Microsoft.AspNetCore.Mvc;
using LabResultsApi.Services;
using LabResultsApi.DTOs;

namespace LabResultsApi.Endpoints;

public static class TestEndpoints
{
    public static void MapTestEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tests")
            .WithTags("Tests")
            .WithOpenApi();

        group.MapGet("/", GetTests)
            .WithName("GetTests")
            .WithSummary("Get all available test types");

        group.MapGet("/qualified", GetQualifiedTests)
            .WithName("GetUserQualifiedTests")
            .WithSummary("Get tests that the current user is qualified to perform");

        group.MapGet("/{testId:int}", GetTest)
            .WithName("GetTest")
            .WithSummary("Get specific test information");

        group.MapGet("/{testId:int}/template", GetTestTemplate)
            .WithName("GetTestTemplate")
            .WithSummary("Get test template configuration for a specific test type");

        group.MapGet("/{testId:int}/results/{sampleId:int}", GetTestResults)
            .WithName("GetTestResults")
            .WithSummary("Get test results for a specific sample and test");

        group.MapGet("/{testId:int}/results/{sampleId:int}/history", GetTestResultsHistory)
            .WithName("GetTestResultsHistory")
            .WithSummary("Get historical test results for a sample");

        group.MapPost("/{testId:int}/results", SaveTestResults)
            .WithName("SaveTestResults")
            .WithSummary("Save test results for a sample");

        group.MapPut("/{testId:int}/results/{sampleId:int}", UpdateTestResults)
            .WithName("UpdateTestResults")
            .WithSummary("Update existing test results");

        group.MapDelete("/{testId:int}/results/{sampleId:int}", DeleteTestResults)
            .WithName("DeleteTestResults")
            .WithSummary("Delete test results for a sample");

        group.MapPost("/{testId:int}/calculate", CalculateTestResult)
            .WithName("CalculateTestResult")
            .WithSummary("Calculate test result based on input values");
    }

    private static async Task<IResult> GetTests(ITestResultService testResultService)
    {
        try
        {
            var tests = await testResultService.GetTestsAsync();
            return Results.Ok(tests);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error retrieving tests: {ex.Message}");
        }
    }

    private static async Task<IResult> GetQualifiedTests(
        HttpContext httpContext,
        IQualificationService qualificationService,
        IAuthorizationService authorizationService,
        ILogger<Program> logger)
    {
        try
        {
            var user = httpContext.User;
            
            // Check if user is authenticated
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                logger.LogWarning("Unauthenticated request for qualified tests");
                return Results.Unauthorized();
            }
            
            // Get employee ID from claims
            var employeeId = authorizationService.GetEmployeeId(user);
            if (string.IsNullOrEmpty(employeeId))
            {
                logger.LogWarning("No employee ID found in user claims");
                return Results.Problem("User identity could not be determined", statusCode: 401);
            }
            
            logger.LogInformation("Getting qualified tests for employee {EmployeeId}", employeeId);
            
            // Get tests user is qualified to perform based on LubeTechQualification table
            var qualifiedTests = await qualificationService.GetQualifiedTestsAsync(employeeId);
            var testsList = qualifiedTests.ToList();
            
            logger.LogInformation("Returning {Count} qualified tests for employee {EmployeeId}", testsList.Count, employeeId);
            
            return Results.Ok(testsList);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving qualified tests");
            return Results.Problem($"Error retrieving qualified tests: {ex.Message}");
        }
    }

    private static async Task<IResult> GetTest(int testId, ITestResultService testResultService)
    {
        try
        {
            var test = await testResultService.GetTestAsync(testId);
            if (test == null)
            {
                return Results.NotFound($"Test {testId} not found");
            }
            return Results.Ok(test);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error retrieving test {testId}: {ex.Message}");
        }
    }

    private static async Task<IResult> GetTestTemplate(int testId, ITestResultService testResultService)
    {
        try
        {
            var template = await testResultService.GetTestTemplateAsync(testId);
            if (template == null)
            {
                return Results.NotFound($"Test template for test {testId} not found");
            }
            return Results.Ok(template);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error retrieving test template for test {testId}: {ex.Message}");
        }
    }

    private static async Task<IResult> GetTestResults(int testId, int sampleId, ITestResultService testResultService)
    {
        try
        {
            var results = await testResultService.GetTestResultsAsync(sampleId, testId);
            if (results == null)
            {
                return Results.NotFound($"Test results for sample {sampleId}, test {testId} not found");
            }
            return Results.Ok(results);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error retrieving test results for sample {sampleId}, test {testId}: {ex.Message}");
        }
    }

    private static async Task<IResult> GetTestResultsHistory(
        int testId, 
        int sampleId, 
        ITestResultService testResultService,
        [FromQuery] int count = 12)
    {
        try
        {
            var history = await testResultService.GetTestResultsHistoryAsync(sampleId, testId, count);
            return Results.Ok(history);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error retrieving test results history for sample {sampleId}, test {testId}: {ex.Message}");
        }
    }

    private static async Task<IResult> SaveTestResults(
        int testId, 
        [FromBody] SaveTestResultRequest request, 
        ITestResultService testResultService)
    {
        try
        {
            if (request.TestId != testId)
            {
                return Results.BadRequest("Test ID in URL does not match request body");
            }

            var result = await testResultService.SaveTestResultsAsync(request);
            return Results.Ok(new { Message = $"Saved {result} test result records", RecordsSaved = result });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error saving test results for test {testId}: {ex.Message}");
        }
    }

    private static async Task<IResult> UpdateTestResults(
        int testId, 
        int sampleId, 
        [FromBody] SaveTestResultRequest request, 
        ITestResultService testResultService)
    {
        try
        {
            if (request.TestId != testId || request.SampleId != sampleId)
            {
                return Results.BadRequest("Test ID or Sample ID in URL does not match request body");
            }

            var result = await testResultService.UpdateTestResultsAsync(sampleId, testId, request);
            return Results.Ok(new { Message = $"Updated {result} test result records", RecordsUpdated = result });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error updating test results for sample {sampleId}, test {testId}: {ex.Message}");
        }
    }

    private static async Task<IResult> DeleteTestResults(
        int testId, 
        int sampleId, 
        ITestResultService testResultService)
    {
        try
        {
            var result = await testResultService.DeleteTestResultsAsync(sampleId, testId);
            return Results.Ok(new { Message = $"Deleted {result} test result records", RecordsDeleted = result });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error deleting test results for sample {sampleId}, test {testId}: {ex.Message}");
        }
    }

    private static async Task<IResult> CalculateTestResult(
        int testId, 
        [FromBody] TestCalculationRequest request, 
        ITestResultService testResultService)
    {
        try
        {
            if (request.TestId != testId)
            {
                return Results.BadRequest("Test ID in URL does not match request body");
            }

            var result = await testResultService.CalculateTestResultAsync(request);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error calculating test result for test {testId}: {ex.Message}");
        }
    }
}