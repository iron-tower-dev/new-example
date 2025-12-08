using Microsoft.AspNetCore.Mvc;
using LabResultsApi.Services;
using LabResultsApi.Models;

namespace LabResultsApi.Endpoints;

public static class ParticleTestEndpoints
{
    public static void MapParticleTestEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/particle-tests")
            .WithTags("Particle Tests");

        // Filter Residue endpoints (Test ID 180)
        group.MapGet("/filter-residue/{sampleId}", GetFilterResidue)
            .WithName("GetFilterResidue")
            .WithSummary("Get Filter Residue test results for a sample")
            .Produces<FilterResidueResult>(200)
            .Produces(404)
            .Produces(500);

        group.MapPost("/filter-residue", SaveFilterResidue)
            .WithName("SaveFilterResidue")
            .WithSummary("Save Filter Residue test results")
            .Produces<SaveTestResultResponse>(200)
            .Produces(400)
            .Produces(500);

        group.MapDelete("/filter-residue/{sampleId}/{testId}", DeleteFilterResidue)
            .WithName("DeleteFilterResidue")
            .WithSummary("Delete Filter Residue test results")
            .Produces<DeleteTestResultResponse>(200)
            .Produces(404)
            .Produces(500);

        // Debris Identification endpoints (Test ID 240)
        group.MapGet("/debris-identification/{sampleId}", GetDebrisIdentification)
            .WithName("GetDebrisIdentification")
            .WithSummary("Get Debris Identification test results for a sample")
            .Produces<DebrisIdentificationResult>(200)
            .Produces(404)
            .Produces(500);

        group.MapPost("/debris-identification", SaveDebrisIdentification)
            .WithName("SaveDebrisIdentification")
            .WithSummary("Save Debris Identification test results")
            .Produces<SaveTestResultResponse>(200)
            .Produces(400)
            .Produces(500);

        group.MapDelete("/debris-identification/{sampleId}/{testId}", DeleteDebrisIdentification)
            .WithName("DeleteDebrisIdentification")
            .WithSummary("Delete Debris Identification test results")
            .Produces<DeleteTestResultResponse>(200)
            .Produces(404)
            .Produces(500);
    }

    #region Filter Residue Handlers

    private static async Task<IResult> GetFilterResidue(
        [FromRoute] int sampleId,
        [FromQuery] int testId,
        [FromServices] IParticleTestService particleTestService)
    {
        try
        {
            // Default to test ID 180 if not specified
            if (testId == 0) testId = 180;

            var result = await particleTestService.GetFilterResidueAsync(sampleId, testId);
            
            if (result == null)
            {
                return Results.NotFound(new { message = $"No Filter Residue test results found for sample {sampleId}" });
            }

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error retrieving Filter Residue test results: {ex.Message}");
        }
    }

    private static async Task<IResult> SaveFilterResidue(
        [FromBody] FilterResidueResult filterResidue,
        [FromServices] IParticleTestService particleTestService)
    {
        try
        {
            if (filterResidue == null)
            {
                return Results.BadRequest(new { message = "Filter Residue data is required" });
            }

            if (filterResidue.SampleId <= 0)
            {
                return Results.BadRequest(new { message = "Valid Sample ID is required" });
            }

            // Default to test ID 180 if not set
            if (filterResidue.TestId == 0) filterResidue.TestId = 180;

            var rowsAffected = await particleTestService.SaveFilterResidueAsync(filterResidue);

            return Results.Ok(new SaveTestResultResponse
            {
                Success = true,
                Message = $"Filter Residue test results saved successfully for sample {filterResidue.SampleId}",
                SampleId = filterResidue.SampleId,
                TestId = filterResidue.TestId,
                RowsAffected = rowsAffected
            });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error saving Filter Residue test results: {ex.Message}");
        }
    }

    private static async Task<IResult> DeleteFilterResidue(
        [FromRoute] int sampleId,
        [FromRoute] int testId,
        [FromServices] IParticleTestService particleTestService)
    {
        try
        {
            if (sampleId <= 0 || testId <= 0)
            {
                return Results.BadRequest(new { message = "Valid Sample ID and Test ID are required" });
            }

            var rowsAffected = await particleTestService.DeleteFilterResidueAsync(sampleId, testId);

            if (rowsAffected == 0)
            {
                return Results.NotFound(new { message = $"No Filter Residue test results found for sample {sampleId}" });
            }

            return Results.Ok(new DeleteTestResultResponse
            {
                Success = true,
                Message = $"Filter Residue test results deleted successfully for sample {sampleId}",
                SampleId = sampleId,
                TestId = testId,
                RowsAffected = rowsAffected
            });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error deleting Filter Residue test results: {ex.Message}");
        }
    }

    #endregion

    #region Debris Identification Handlers

    private static async Task<IResult> GetDebrisIdentification(
        [FromRoute] int sampleId,
        [FromQuery] int testId,
        [FromServices] IParticleTestService particleTestService)
    {
        try
        {
            // Default to test ID 240 if not specified
            if (testId == 0) testId = 240;

            var result = await particleTestService.GetDebrisIdentificationAsync(sampleId, testId);
            
            if (result == null)
            {
                return Results.NotFound(new { message = $"No Debris Identification test results found for sample {sampleId}" });
            }

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error retrieving Debris Identification test results: {ex.Message}");
        }
    }

    private static async Task<IResult> SaveDebrisIdentification(
        [FromBody] DebrisIdentificationResult debrisId,
        [FromServices] IParticleTestService particleTestService)
    {
        try
        {
            if (debrisId == null)
            {
                return Results.BadRequest(new { message = "Debris Identification data is required" });
            }

            if (debrisId.SampleId <= 0)
            {
                return Results.BadRequest(new { message = "Valid Sample ID is required" });
            }

            // Default to test ID 240 if not set
            if (debrisId.TestId == 0) debrisId.TestId = 240;

            var rowsAffected = await particleTestService.SaveDebrisIdentificationAsync(debrisId);

            return Results.Ok(new SaveTestResultResponse
            {
                Success = true,
                Message = $"Debris Identification test results saved successfully for sample {debrisId.SampleId}",
                SampleId = debrisId.SampleId,
                TestId = debrisId.TestId,
                RowsAffected = rowsAffected
            });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error saving Debris Identification test results: {ex.Message}");
        }
    }

    private static async Task<IResult> DeleteDebrisIdentification(
        [FromRoute] int sampleId,
        [FromRoute] int testId,
        [FromServices] IParticleTestService particleTestService)
    {
        try
        {
            if (sampleId <= 0 || testId <= 0)
            {
                return Results.BadRequest(new { message = "Valid Sample ID and Test ID are required" });
            }

            var rowsAffected = await particleTestService.DeleteDebrisIdentificationAsync(sampleId, testId);

            if (rowsAffected == 0)
            {
                return Results.NotFound(new { message = $"No Debris Identification test results found for sample {sampleId}" });
            }

            return Results.Ok(new DeleteTestResultResponse
            {
                Success = true,
                Message = $"Debris Identification test results deleted successfully for sample {sampleId}",
                SampleId = sampleId,
                TestId = testId,
                RowsAffected = rowsAffected
            });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error deleting Debris Identification test results: {ex.Message}");
        }
    }

    #endregion
}

#region Response Models

public class SaveTestResultResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int SampleId { get; set; }
    public int TestId { get; set; }
    public int RowsAffected { get; set; }
}

public class DeleteTestResultResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int SampleId { get; set; }
    public int TestId { get; set; }
    public int RowsAffected { get; set; }
}

#endregion
