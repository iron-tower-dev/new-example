using Microsoft.AspNetCore.Mvc;
using LabResultsApi.Services;
using LabResultsApi.DTOs;

namespace LabResultsApi.Endpoints;

public static class SampleEndpoints
{
    public static void MapSampleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/samples")
            .WithTags("Samples")
            .WithOpenApi();

        group.MapGet("/by-test/{testId:int}", GetSamplesByTest)
            .WithName("GetSamplesByTest")
            .WithSummary("Get samples available for a specific test type");

        group.MapGet("/", GetSamples)
            .WithName("GetSamples")
            .WithSummary("Get samples with optional filtering");

        group.MapGet("/{sampleId:int}", GetSample)
            .WithName("GetSample")
            .WithSummary("Get detailed information for a specific sample");

        group.MapGet("/{sampleId:int}/history/{testId:int}", GetSampleHistory)
            .WithName("GetSampleHistory")
            .WithSummary("Get historical test results for a sample");
    }

    private static async Task<IResult> GetSamplesByTest(int testId, ISampleService sampleService)
    {
        try
        {
            var samples = await sampleService.GetSamplesByTestAsync(testId);
            return Results.Ok(samples);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error retrieving samples for test {testId}: {ex.Message}");
        }
    }

    private static async Task<IResult> GetSamples(
        ISampleService sampleService,
        [FromQuery] string? tagNumber = null,
        [FromQuery] string? component = null,
        [FromQuery] string? location = null,
        [FromQuery] string? lubeType = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int? status = null)
    {
        try
        {
            var filter = new SampleFilterDto
            {
                TagNumber = tagNumber,
                Component = component,
                Location = location,
                LubeType = lubeType,
                FromDate = fromDate,
                ToDate = toDate,
                Status = status
            };

            var samples = await sampleService.GetSamplesAsync(filter);
            return Results.Ok(samples);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error retrieving samples: {ex.Message}");
        }
    }

    private static async Task<IResult> GetSample(int sampleId, ISampleService sampleService)
    {
        try
        {
            var sample = await sampleService.GetSampleAsync(sampleId);
            if (sample == null)
            {
                return Results.NotFound($"Sample {sampleId} not found");
            }
            return Results.Ok(sample);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error retrieving sample {sampleId}: {ex.Message}");
        }
    }

    private static async Task<IResult> GetSampleHistory(int sampleId, int testId, ISampleService sampleService)
    {
        try
        {
            var history = await sampleService.GetSampleHistoryAsync(sampleId, testId);
            return Results.Ok(history);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error retrieving history for sample {sampleId}, test {testId}: {ex.Message}");
        }
    }
}