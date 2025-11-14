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

        group.MapPost("/nas/refresh-cache", RefreshNASCache)
            .WithName("RefreshNASCache")
            .WithSummary("Refresh the NAS lookup cache");

        // NLGI Lookup endpoints
        group.MapGet("/nlgi", GetNLGILookupTable)
            .WithName("GetNLGILookupTable")
            .WithSummary("Get the complete NLGI lookup table");

        group.MapGet("/nlgi/penetration/{penetrationValue:int}", GetNLGIForPenetration)
            .WithName("GetNLGIForPenetration")
            .WithSummary("Get NLGI value for specific penetration value");

        group.MapPost("/nlgi/refresh-cache", RefreshNLGICache)
            .WithName("RefreshNLGICache")
            .WithSummary("Refresh the NLGI lookup cache");

        // MTE Equipment Lookup endpoints
        group.MapGet("/equipment/type/{equipType}", GetCachedEquipmentByType)
            .WithName("GetCachedEquipmentByType")
            .WithSummary("Get cached equipment list by type");

        group.MapGet("/equipment/type/{equipType}/test/{testId:int}", GetCachedEquipmentByTypeAndTest)
            .WithName("GetCachedEquipmentByTypeAndTest")
            .WithSummary("Get cached equipment list by type and test ID");

        group.MapGet("/equipment/calibration/{equipmentId:int}", GetCachedEquipmentCalibration)
            .WithName("GetCachedEquipmentCalibration")
            .WithSummary("Get cached equipment calibration data");

        group.MapPost("/equipment/refresh-cache", RefreshEquipmentCache)
            .WithName("RefreshEquipmentCache")
            .WithSummary("Refresh all equipment cache");

        group.MapPost("/equipment/refresh-cache/type/{equipType}", RefreshEquipmentCacheByType)
            .WithName("RefreshEquipmentCacheByType")
            .WithSummary("Refresh equipment cache for specific type");

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

        group.MapPost("/particle-types/refresh-cache", RefreshParticleTypeCache)
            .WithName("RefreshParticleTypeCache")
            .WithSummary("Refresh particle type cache");

        // Comment Lookup endpoints
        group.MapGet("/comments/area/{area}", GetCommentsByArea)
            .WithName("GetCommentsByArea")
            .WithSummary("Get comments by area");

        group.MapGet("/comments/area/{area}/type/{type}", GetCommentsByAreaAndType)
            .WithName("GetCommentsByAreaAndType")
            .WithSummary("Get comments by area and type");

        group.MapGet("/comments/areas", GetCommentAreas)
            .WithName("GetCommentAreas")
            .WithSummary("Get all comment areas");

        group.MapGet("/comments/types/{area}", GetCommentTypes)
            .WithName("GetCommentTypes")
            .WithSummary("Get comment types for an area");

        group.MapPost("/comments/refresh-cache", RefreshCommentCache)
            .WithName("RefreshCommentCache")
            .WithSummary("Refresh comment cache");

        // Cache Management endpoints
        group.MapPost("/refresh-all-caches", RefreshAllCaches)
            .WithName("RefreshAllCaches")
            .WithSummary("Refresh all lookup caches");

        group.MapGet("/cache-status", GetCacheStatus)
            .WithName("GetCacheStatus")
            .WithSummary("Get status of all caches");
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

    private static async Task<IResult> RefreshNASCache(ILookupService lookupService)
    {
        try
        {
            var success = await lookupService.RefreshNASCacheAsync();
            
            if (success)
            {
                return Results.Ok(new { Message = "NAS cache refreshed successfully" });
            }
            else
            {
                return Results.Problem("Failed to refresh NAS cache");
            }
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error refreshing NAS cache: {ex.Message}");
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

    private static async Task<IResult> RefreshNLGICache(ILookupService lookupService)
    {
        try
        {
            var success = await lookupService.RefreshNLGICacheAsync();
            
            if (success)
            {
                return Results.Ok(new { Message = "NLGI cache refreshed successfully" });
            }
            else
            {
                return Results.Problem("Failed to refresh NLGI cache");
            }
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error refreshing NLGI cache: {ex.Message}");
        }
    }

    // MTE Equipment Lookup endpoint implementations
    private static async Task<IResult> GetCachedEquipmentByType(
        string equipType,
        ILookupService lookupService)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(equipType))
            {
                return Results.BadRequest("Equipment type is required");
            }

            var equipment = await lookupService.GetCachedEquipmentByTypeAsync(equipType);
            return Results.Ok(equipment);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error retrieving equipment by type {equipType}: {ex.Message}");
        }
    }

    private static async Task<IResult> GetCachedEquipmentByTypeAndTest(
        string equipType,
        int testId,
        ILookupService lookupService)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(equipType))
            {
                return Results.BadRequest("Equipment type is required");
            }

            var equipment = await lookupService.GetCachedEquipmentByTypeAsync(equipType, (short)testId);
            return Results.Ok(equipment);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error retrieving equipment by type {equipType} and test {testId}: {ex.Message}");
        }
    }

    private static async Task<IResult> GetCachedEquipmentCalibration(
        int equipmentId,
        ILookupService lookupService)
    {
        try
        {
            if (equipmentId <= 0)
            {
                return Results.BadRequest("Valid equipment ID is required");
            }

            var calibration = await lookupService.GetCachedEquipmentCalibrationAsync(equipmentId);
            
            if (calibration == null)
            {
                return Results.NotFound($"Equipment with ID {equipmentId} not found");
            }

            return Results.Ok(calibration);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error retrieving equipment calibration for ID {equipmentId}: {ex.Message}");
        }
    }

    private static async Task<IResult> RefreshEquipmentCache(ILookupService lookupService)
    {
        try
        {
            var success = await lookupService.RefreshEquipmentCacheAsync();
            
            if (success)
            {
                return Results.Ok(new { Message = "Equipment cache refreshed successfully" });
            }
            else
            {
                return Results.Problem("Failed to refresh equipment cache");
            }
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error refreshing equipment cache: {ex.Message}");
        }
    }

    private static async Task<IResult> RefreshEquipmentCacheByType(
        string equipType,
        ILookupService lookupService)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(equipType))
            {
                return Results.BadRequest("Equipment type is required");
            }

            var success = await lookupService.RefreshEquipmentCacheByTypeAsync(equipType);
            
            if (success)
            {
                return Results.Ok(new { Message = $"Equipment cache for type {equipType} refreshed successfully" });
            }
            else
            {
                return Results.Problem($"Failed to refresh equipment cache for type {equipType}");
            }
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error refreshing equipment cache for type {equipType}: {ex.Message}");
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

    private static async Task<IResult> RefreshParticleTypeCache(ILookupService lookupService)
    {
        try
        {
            var success = await lookupService.RefreshParticleTypeCacheAsync();
            
            if (success)
            {
                return Results.Ok(new { Message = "Particle type cache refreshed successfully" });
            }
            else
            {
                return Results.Problem("Failed to refresh particle type cache");
            }
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error refreshing particle type cache: {ex.Message}");
        }
    }

    // Comment Lookup endpoint implementations
    private static async Task<IResult> GetCommentsByArea(
        string area,
        ILookupService lookupService)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(area))
            {
                return Results.BadRequest("Area is required");
            }

            var comments = await lookupService.GetCommentsByAreaAsync(area);
            return Results.Ok(comments);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error retrieving comments for area {area}: {ex.Message}");
        }
    }

    private static async Task<IResult> GetCommentsByAreaAndType(
        string area,
        string type,
        ILookupService lookupService)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(area))
            {
                return Results.BadRequest("Area is required");
            }

            if (string.IsNullOrWhiteSpace(type))
            {
                return Results.BadRequest("Type is required");
            }

            var comments = await lookupService.GetCommentsByAreaAndTypeAsync(area, type);
            return Results.Ok(comments);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error retrieving comments for area {area} and type {type}: {ex.Message}");
        }
    }

    private static async Task<IResult> GetCommentAreas(ILookupService lookupService)
    {
        try
        {
            var areas = await lookupService.GetCommentAreasAsync();
            return Results.Ok(areas);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error retrieving comment areas: {ex.Message}");
        }
    }

    private static async Task<IResult> GetCommentTypes(
        string area,
        ILookupService lookupService)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(area))
            {
                return Results.BadRequest("Area is required");
            }

            var types = await lookupService.GetCommentTypesAsync(area);
            return Results.Ok(types);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error retrieving comment types for area {area}: {ex.Message}");
        }
    }

    private static async Task<IResult> RefreshCommentCache(ILookupService lookupService)
    {
        try
        {
            var success = await lookupService.RefreshCommentCacheAsync();
            
            if (success)
            {
                return Results.Ok(new { Message = "Comment cache refreshed successfully" });
            }
            else
            {
                return Results.Problem("Failed to refresh comment cache");
            }
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error refreshing comment cache: {ex.Message}");
        }
    }

    // Cache Management endpoint implementations
    private static async Task<IResult> RefreshAllCaches(ILookupService lookupService)
    {
        try
        {
            var success = await lookupService.RefreshAllCachesAsync();
            
            if (success)
            {
                return Results.Ok(new { Message = "All caches refreshed successfully" });
            }
            else
            {
                return Results.Problem("Failed to refresh some caches");
            }
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error refreshing all caches: {ex.Message}");
        }
    }

    private static async Task<IResult> GetCacheStatus(ILookupService lookupService)
    {
        try
        {
            var status = await lookupService.GetCacheStatusAsync();
            return Results.Ok(status);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error retrieving cache status: {ex.Message}");
        }
    }
}