using Microsoft.AspNetCore.Mvc;
using LabResultsApi.Services;
using LabResultsApi.DTOs;

namespace LabResultsApi.Endpoints;

public static class ParticleAnalysisEndpoints
{
    public static void MapParticleAnalysisEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/particle-analysis")
            .WithTags("Particle Analysis");

        group.MapGet("/particle-types", GetParticleTypes)
            .WithName("GetParticleTypes")
            .WithSummary("Get all active particle type definitions")
            .Produces<IEnumerable<ParticleTypeDto>>(200)
            .Produces(500);

        group.MapGet("/sub-type-categories", GetSubTypeCategories)
            .WithName("GetSubTypeCategories")
            .WithSummary("Get all active particle sub-type categories with their definitions")
            .Produces<IEnumerable<ParticleSubTypeCategoryDto>>(200)
            .Produces(500);
    }

    private static async Task<IResult> GetParticleTypes([FromServices] IParticleAnalysisService particleAnalysisService)
    {
        try
        {
            var particleTypes = await particleAnalysisService.GetParticleTypesAsync();
            return Results.Ok(particleTypes);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error retrieving particle types: {ex.Message}");
        }
    }

    private static async Task<IResult> GetSubTypeCategories([FromServices] IParticleAnalysisService particleAnalysisService)
    {
        try
        {
            var categories = await particleAnalysisService.GetSubTypeCategoriesAsync();
            return Results.Ok(categories);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error retrieving sub-type categories: {ex.Message}");
        }
    }
}