using Microsoft.AspNetCore.Mvc;
using LabResultsApi.Services;
using LabResultsApi.DTOs;

namespace LabResultsApi.Endpoints;

public static class TestSchedulingEndpoints
{
    public static void MapTestSchedulingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/test-scheduling")
            .WithTags("Test Scheduling")
            .WithOpenApi();

        group.MapPost("/auto-schedule/{sampleId:int}/{completedTestId:int}", AutoScheduleTests)
            .WithName("AutoScheduleTests")
            .WithSummary("Automatically schedule tests based on completed test and business rules");

        group.MapGet("/scheduled/{sampleId:int}", GetScheduledTests)
            .WithName("GetScheduledTests")
            .WithSummary("Get all scheduled tests for a sample");

        group.MapPost("/schedule-ferrography/{sampleId:int}", ScheduleFerrography)
            .WithName("ScheduleFerrography")
            .WithSummary("Schedule Ferrography test for a sample");

        group.MapGet("/rules/{tagNumber}/{component}/{location}", GetTestRules)
            .WithName("GetTestRules")
            .WithSummary("Get test scheduling rules for specific equipment");

        group.MapPost("/remove-unneeded/{sampleId:int}", RemoveUnneededTests)
            .WithName("RemoveUnneededTests")
            .WithSummary("Remove tests that no longer meet scheduling criteria");

        group.MapPost("/set-schedule-type/{sampleId:int}", SetSampleScheduleType)
            .WithName("SetSampleScheduleType")
            .WithSummary("Set the schedule type for a sample");
    }

    private static async Task<IResult> AutoScheduleTests(
        int sampleId,
        int completedTestId,
        [FromQuery] string tagNumber,
        [FromQuery] string component,
        [FromQuery] string location,
        ITestSchedulingService schedulingService)
    {
        try
        {
            var result = await schedulingService.AutoScheduleTestsAsync(sampleId, completedTestId, tagNumber, component, location);
            
            if (result.Success)
            {
                return Results.Ok(result);
            }
            else
            {
                return Results.BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error auto-scheduling tests: {ex.Message}");
        }
    }

    private static async Task<IResult> GetScheduledTests(int sampleId, ITestSchedulingService schedulingService)
    {
        try
        {
            var tests = await schedulingService.GetScheduledTestsAsync(sampleId);
            return Results.Ok(tests);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error getting scheduled tests: {ex.Message}");
        }
    }

    private static async Task<IResult> ScheduleFerrography(int sampleId, ITestSchedulingService schedulingService)
    {
        try
        {
            var success = await schedulingService.ScheduleFerrographyAsync(sampleId);
            
            if (success)
            {
                return Results.Ok(new { Message = "Ferrography scheduled successfully", SampleId = sampleId });
            }
            else
            {
                return Results.BadRequest(new { Message = "Failed to schedule Ferrography", SampleId = sampleId });
            }
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error scheduling Ferrography: {ex.Message}");
        }
    }

    private static async Task<IResult> GetTestRules(
        string tagNumber,
        string component,
        string location,
        ITestSchedulingService schedulingService)
    {
        try
        {
            var rules = await schedulingService.GetTestRulesAsync(tagNumber, component, location);
            return Results.Ok(rules);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error getting test rules: {ex.Message}");
        }
    }

    private static async Task<IResult> RemoveUnneededTests(
        int sampleId,
        [FromQuery] string tagNumber,
        [FromQuery] string component,
        [FromQuery] string location,
        ITestSchedulingService schedulingService)
    {
        try
        {
            var result = await schedulingService.RemoveUnneededTestsAsync(sampleId, tagNumber, component, location);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error removing unneeded tests: {ex.Message}");
        }
    }

    private static async Task<IResult> SetSampleScheduleType(
        int sampleId,
        [FromBody] SetScheduleTypeRequest request,
        ITestSchedulingService schedulingService)
    {
        try
        {
            var success = await schedulingService.SetSampleScheduleTypeAsync(sampleId, request.ScheduleType);
            
            if (success)
            {
                return Results.Ok(new { Message = "Schedule type updated successfully", SampleId = sampleId, ScheduleType = request.ScheduleType });
            }
            else
            {
                return Results.BadRequest(new { Message = "Failed to update schedule type", SampleId = sampleId });
            }
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error setting schedule type: {ex.Message}");
        }
    }
}

public class SetScheduleTypeRequest
{
    public string ScheduleType { get; set; } = string.Empty;
}