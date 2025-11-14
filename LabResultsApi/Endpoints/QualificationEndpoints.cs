using Microsoft.AspNetCore.Mvc;
using LabResultsApi.Services;
using LabResultsApi.DTOs;

namespace LabResultsApi.Endpoints;

public static class QualificationEndpoints
{
    public static void MapQualificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/qualifications")
            .WithTags("User Qualifications")
            .WithOpenApi();

        group.MapGet("/user/{employeeId}/test/{testId:int}", GetUserQualification)
            .WithName("GetUserQualification")
            .WithSummary("Get user qualification level for a specific test");

        group.MapGet("/user/{employeeId}/qualified-tests", GetQualifiedTests)
            .WithName("GetQualifiedTests")
            .WithSummary("Get all tests a user is qualified to perform");

        group.MapGet("/user/{employeeId}", GetUserQualifications)
            .WithName("GetUserQualifications")
            .WithSummary("Get all qualifications for a user");

        group.MapGet("/check/{employeeId}/test/{testId:int}", CheckQualification)
            .WithName("CheckQualification")
            .WithSummary("Check if user is qualified to perform a test");

        group.MapGet("/check-review/{employeeId}/sample/{sampleId:int}/test/{testId:int}", CheckReviewQualification)
            .WithName("CheckReviewQualification")
            .WithSummary("Check if user is qualified to review test results");

        group.MapGet("/check-validate/{validatorId}/sample/{sampleId:int}/test/{testId:int}", CheckValidationQualification)
            .WithName("CheckValidationQualification")
            .WithSummary("Check if user can validate test results (not their own work)");

        group.MapGet("/required-level/test/{testId:int}", GetRequiredQualificationLevel)
            .WithName("GetRequiredQualificationLevel")
            .WithSummary("Get required qualification level for a test");

        group.MapPost("/check-minimum", CheckMinimumQualification)
            .WithName("CheckMinimumQualification")
            .WithSummary("Check if user has minimum qualification level");
    }

    private static async Task<IResult> GetUserQualification(
        string employeeId,
        int testId,
        IQualificationService qualificationService)
    {
        try
        {
            var qualification = await qualificationService.GetUserQualificationAsync(employeeId, testId);
            
            if (qualification != null)
            {
                return Results.Ok(new { EmployeeId = employeeId, TestId = testId, QualificationLevel = qualification });
            }
            else
            {
                return Results.NotFound(new { Message = "No qualification found", EmployeeId = employeeId, TestId = testId });
            }
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error getting user qualification: {ex.Message}");
        }
    }

    private static async Task<IResult> GetQualifiedTests(string employeeId, IQualificationService qualificationService)
    {
        try
        {
            var tests = await qualificationService.GetQualifiedTestsAsync(employeeId);
            return Results.Ok(tests);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error getting qualified tests: {ex.Message}");
        }
    }

    private static async Task<IResult> GetUserQualifications(string employeeId, IQualificationService qualificationService)
    {
        try
        {
            var qualifications = await qualificationService.GetUserQualificationsAsync(employeeId);
            return Results.Ok(qualifications);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error getting user qualifications: {ex.Message}");
        }
    }

    private static async Task<IResult> CheckQualification(
        string employeeId,
        int testId,
        IQualificationService qualificationService)
    {
        try
        {
            var isQualified = await qualificationService.IsUserQualifiedAsync(employeeId, testId);
            return Results.Ok(new { EmployeeId = employeeId, TestId = testId, IsQualified = isQualified });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error checking qualification: {ex.Message}");
        }
    }

    private static async Task<IResult> CheckReviewQualification(
        string employeeId,
        int sampleId,
        int testId,
        IQualificationService qualificationService)
    {
        try
        {
            var canReview = await qualificationService.IsUserQualifiedToReviewAsync(employeeId, sampleId, testId);
            return Results.Ok(new 
            { 
                EmployeeId = employeeId, 
                SampleId = sampleId, 
                TestId = testId, 
                CanReview = canReview 
            });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error checking review qualification: {ex.Message}");
        }
    }

    private static async Task<IResult> CheckValidationQualification(
        string validatorId,
        int sampleId,
        int testId,
        IQualificationService qualificationService)
    {
        try
        {
            var canValidate = await qualificationService.CanUserValidateResultsAsync(validatorId, sampleId, testId);
            return Results.Ok(new 
            { 
                ValidatorId = validatorId, 
                SampleId = sampleId, 
                TestId = testId, 
                CanValidate = canValidate 
            });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error checking validation qualification: {ex.Message}");
        }
    }

    private static async Task<IResult> GetRequiredQualificationLevel(int testId, IQualificationService qualificationService)
    {
        try
        {
            var requiredLevel = await qualificationService.GetRequiredQualificationLevelAsync(testId);
            return Results.Ok(new { TestId = testId, RequiredLevel = requiredLevel });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error getting required qualification level: {ex.Message}");
        }
    }

    private static async Task<IResult> CheckMinimumQualification(
        [FromBody] MinimumQualificationRequest request,
        IQualificationService qualificationService)
    {
        try
        {
            var hasMinimum = await qualificationService.HasMinimumQualificationAsync(
                request.EmployeeId, 
                request.TestId, 
                request.RequiredLevel);
            
            return Results.Ok(new 
            { 
                EmployeeId = request.EmployeeId, 
                TestId = request.TestId, 
                RequiredLevel = request.RequiredLevel,
                HasMinimumQualification = hasMinimum 
            });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error checking minimum qualification: {ex.Message}");
        }
    }
}

public class MinimumQualificationRequest
{
    public string EmployeeId { get; set; } = string.Empty;
    public int TestId { get; set; }
    public string RequiredLevel { get; set; } = string.Empty;
}