using Microsoft.AspNetCore.Mvc;
using LabResultsApi.Services;
using LabResultsApi.DTOs;

namespace LabResultsApi.Endpoints;

public static class LookupEndpoints
{
    public static void MapLookupEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/lookups")
            .WithTags("Lookups")
            .WithOpenApi();

        // NAS Lookup endpoints
        group.MapGet("/nas", GetNASLookupTable)
            .WithName("GetNASLookupTable")
            .WithSummary("Get the complete NAS lookup table");

        group.MapPost("/nas/calculate", CalculateNAS)
            .WithName("CalculateNAS")
            .WithSummary("Calculate NAS values from particle counts");

        group.MapGet("/nas/channel/{channel:int}/count/{count:int}", GetNASForParticleCount)
            .WithName("GetNASForParticleCount")
            .WithSummary("Get NAS value for specific channel and particle count");


        // NLGI Lookup endpoints
        group.MapGet("/nlgi", GetNLGILookupTable)
            .WithName("GetNLGILookupTable")
            .WithSummary("Get the complete NLGI lookup table");

        group.MapGet("/nlgi/penetration/{penetrationValue:int}", GetNLGIForPenetration)
            .WithName("GetNLGIForPenetration")
            .WithSummary("Get NLGI value for specific penetration value");



        // Particle Type Lookup endpoints
        group.MapGet("/particle-types", GetParticleTypeDefinitions)
            .WithName("GetParticleTypeDefinitions")
            .WithSummary("Get all particle type definitions");

        group.MapGet("/particle-subtypes/category/{categoryId:int}", GetParticleSubTypeDefinitions)
            .WithName("GetParticleSubTypeDefinitions")
            .WithSummary("Get particle sub-type definitions by category");

        group.MapGet("/particle-categories", GetParticleSubTypeCategories)
            .WithName("GetParticleSubTypeCategories")
            .WithSummary("Get all particle sub-type categories");



    }

    private static async Task<IResult> GetNASLookupTable(ILookupService lookupService)
    {
        try
        {
            var nasTable = await lookupService.GetNASLookupTableAsync();
            return Results.Ok(nasTable);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error retrieving NAS lookup table: {ex.Message}");
        }
    }

    private static async Task<IResult> CalculateNAS(
        [FromBody] NasLookupRequest request, 
        ILookupService lookupService)
    {
        try
        {
            if (request.ParticleCounts == null || !request.ParticleCounts.Any())
            {
                return Results.BadRequest("Particle counts are required");
            }

            var result = await lookupService.CalculateNASAsync(request);
            
            if (!result.IsValid)
            {
                return Results.BadRequest(result.ErrorMessage);
            }

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error calculating NAS: {ex.Message}");
        }
    }

    private static async Task<IResult> GetNASForParticleCount(
        int channel, 
        int count, 
        ILookupService lookupService)
    {
        try
        {
            if (channel < 1 || channel > 6)
            {
                return Results.BadRequest("Channel must be between 1 and 6");
            }

            if (count < 0)
            {
                return Results.BadRequest("Particle count cannot be negative");
            }

            var nasValue = await lookupService.GetNASForParticleCountAsync(channel, count);
            
            if (!nasValue.HasValue)
            {
                return Results.NotFound($"No NAS value found for channel {channel} with count {count}");
            }

            return Results.Ok(new { Channel = channel, Count = count, NAS = nasValue.Value });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error getting NAS for channel {channel} and count {count}: {ex.Message}");
        }
    }


    private static async Task<IResult> GetNLGILookupTable(ILookupService lookupService)
    {
        try
        {
            var nlgiTable = await lookupService.GetNLGILookupTableAsync();
            return Results.Ok(nlgiTable);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error retrieving NLGI lookup table: {ex.Message}");
        }
    }

    private static async Task<IResult> GetNLGIForPenetration(
        int penetrationValue, 
        ILookupService lookupService)
    {
        try
        {
            if (penetrationValue < 0)
            {
                return Results.BadRequest("Penetration value cannot be negative");
            }

            var nlgiValue = await lookupService.GetNLGIForPenetrationAsync(penetrationValue);
            
            if (nlgiValue == null)
            {
                return Results.NotFound($"No NLGI value found for penetration value {penetrationValue}");
            }

            return Results.Ok(new { PenetrationValue = penetrationValue, NLGI = nlgiValue });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error getting NLGI for penetration value {penetrationValue}: {ex.Message}");
        }
    }



    // Particle Type Lookup endpoint implementations
    private static async Task<IResult> GetParticleTypeDefinitions(ILookupService lookupService)
    {
        try
        {
            var particleTypes = await lookupService.GetParticleTypeDefinitionsAsync();
            return Results.Ok(particleTypes);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error retrieving particle type definitions: {ex.Message}");
        }
    }

    private static async Task<IResult> GetParticleSubTypeDefinitions(
        int categoryId,
        ILookupService lookupService)
    {
        try
        {
            if (categoryId <= 0)
            {
                return Results.BadRequest("Valid category ID is required");
            }

            var particleSubTypes = await lookupService.GetParticleSubTypeDefinitionsAsync(categoryId);
            return Results.Ok(particleSubTypes);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error retrieving particle sub-type definitions for category {categoryId}: {ex.Message}");
        }
    }

    private static async Task<IResult> GetParticleSubTypeCategories(ILookupService lookupService)
    {
        try
        {
            var categories = await lookupService.GetParticleSubTypeCategoriesAsync();
            return Results.Ok(categories);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error retrieving particle sub-type categories: {ex.Message}");
        }
    }



}