using Microsoft.AspNetCore.Mvc;
using LabResultsApi.Services;
using LabResultsApi.DTOs;

namespace LabResultsApi.Endpoints;

public static class LimitsEndpoints
{
    public static void MapLimitsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/limits")
            .WithTags("Limits & Validation")
            .WithOpenApi();

        group.MapPost("/check/{sampleId:int}/{testId:int}", CheckLimits)
            .WithName("CheckLimits")
            .WithSummary("Check if test results are within acceptable limits");

        group.MapGet("/{tagNumber}/{component}/{location}/{testId:int}", GetLimits)
            .WithName("GetLimits")
            .WithSummary("Get limits for specific equipment and test combination");

        group.MapPost("/evaluate/{sampleId:int}/{testId:int}", EvaluateTestResults)
            .WithName("EvaluateTestResults")
            .WithSummary("Evaluate test results and determine status");

        group.MapGet("/lcde/{tagNumber}/{component}/{location}", GetLcdeLimits)
            .WithName("GetLcdeLimits")
            .WithSummary("Get LCDE (Lubrication Condition Data Evaluation) limits");

        group.MapPost("/alerts/{sampleId:int}/{testId:int}", CheckAlertThresholds)
            .WithName("CheckAlertThresholds")
            .WithSummary("Check if results exceed alert thresholds");

        group.MapGet("/cross-reference/{testId:int}", GetLimitsCrossReference)
            .WithName("GetLimitsCrossReference")
            .WithSummary("Get limits cross-reference data for a test");

        group.MapPost("/validate-single", ValidateSingleResult)
            .WithName("ValidateSingleResult")
            .WithSummary("Validate a single test result against limits");
    }

    private static async Task<IResult> CheckLimits(
        int sampleId,
        int testId,
        [FromBody] Dictionary<string, double> results,
        ILimitsService limitsService)
    {
        try
        {
            var limitsCheck = await limitsService.CheckLimitsAsync(sampleId, testId, results);
            return Results.Ok(limitsCheck);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error checking limits: {ex.Message}");
        }
    }

    private static async Task<IResult> GetLimits(
        string tagNumber,
        string component,
        string location,
        int testId,
        ILimitsService limitsService)
    {
        try
        {
            var limits = await limitsService.GetLimitsAsync(tagNumber, component, location, testId);
            return Results.Ok(limits);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error getting limits: {ex.Message}");
        }
    }

    private static async Task<IResult> EvaluateTestResults(
        int sampleId,
        int testId,
        [FromBody] Dictionary<string, double> results,
        ILimitsService limitsService)
    {
        try
        {
            var evaluation = await limitsService.EvaluateTestResultsAsync(sampleId, testId, results);
            return Results.Ok(evaluation);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error evaluating test results: {ex.Message}");
        }
    }

    private static async Task<IResult> GetLcdeLimits(
        string tagNumber,
        string component,
        string location,
        ILimitsService limitsService)
    {
        try
        {
            var lcdeLimits = await limitsService.GetLcdeLimitsAsync(tagNumber, component, location);
            return Results.Ok(lcdeLimits);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error getting LCDE limits: {ex.Message}");
        }
    }

    private static async Task<IResult> CheckAlertThresholds(
        int sampleId,
        int testId,
        [FromBody] Dictionary<string, double> results,
        ILimitsService limitsService)
    {
        try
        {
            var alerts = await limitsService.CheckAlertThresholdsAsync(sampleId, testId, results);
            return Results.Ok(alerts);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error checking alert thresholds: {ex.Message}");
        }
    }

    private static async Task<IResult> GetLimitsCrossReference(int testId, ILimitsService limitsService)
    {
        try
        {
            var crossRef = await limitsService.GetLimitsCrossReferenceAsync(testId);
            return Results.Ok(crossRef);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error getting limits cross-reference: {ex.Message}");
        }
    }

    private static async Task<IResult> ValidateSingleResult(
        [FromBody] SingleResultValidationRequest request,
        ILimitsService limitsService)
    {
        try
        {
            var isValid = await limitsService.IsResultWithinLimitsAsync(
                request.SampleId, 
                request.TestId, 
                request.Parameter, 
                request.Value);
            
            return Results.Ok(new { IsValid = isValid, Parameter = request.Parameter, Value = request.Value });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error validating single result: {ex.Message}");
        }
    }
}

public class SingleResultValidationRequest
{
    public int SampleId { get; set; }
    public int TestId { get; set; }
    public string Parameter { get; set; } = string.Empty;
    public double Value { get; set; }
}