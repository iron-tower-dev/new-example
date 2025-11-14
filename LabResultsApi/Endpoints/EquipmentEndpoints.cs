using LabResultsApi.DTOs;
using LabResultsApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LabResultsApi.Endpoints;

public static class EquipmentEndpoints
{
    public static void MapEquipmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/equipment")
            .WithTags("Equipment")
            .WithOpenApi();

        // Get equipment by type for dropdowns
        group.MapGet("/by-type/{equipType}", GetEquipmentByType)
            .WithName("GetEquipmentByType")
            .WithSummary("Get equipment list by type for dropdown selection")
            .WithDescription("Returns equipment filtered by type, excluding inactive equipment and including due date warnings");

        // Get equipment calibration value
        group.MapGet("/{equipmentId:int}/calibration", GetEquipmentCalibration)
            .WithName("GetEquipmentCalibration")
            .WithSummary("Get equipment calibration information")
            .WithDescription("Returns calibration value and validation status for specific equipment");

        // Validate equipment
        group.MapGet("/{equipmentId:int}/validate", ValidateEquipment)
            .WithName("ValidateEquipment")
            .WithSummary("Validate equipment status and due dates")
            .WithDescription("Returns validation result including due date status and warnings");

        // Get all equipment with optional filtering
        group.MapGet("/", GetAllEquipment)
            .WithName("GetAllEquipment")
            .WithSummary("Get all equipment with optional filtering")
            .WithDescription("Returns all equipment with optional filters for type, test ID, and status");

        // Get equipment by ID
        group.MapGet("/{id:int}", GetEquipmentById)
            .WithName("GetEquipmentById")
            .WithSummary("Get equipment by ID")
            .WithDescription("Returns specific equipment details by ID");

        // Get equipment types
        group.MapGet("/types", GetEquipmentTypes)
            .WithName("GetEquipmentTypes")
            .WithSummary("Get all available equipment types")
            .WithDescription("Returns list of distinct equipment types");

        // Check if equipment is due soon
        group.MapGet("/{equipmentId:int}/due-soon", IsEquipmentDueSoon)
            .WithName("IsEquipmentDueSoon")
            .WithSummary("Check if equipment calibration is due soon")
            .WithDescription("Returns boolean indicating if equipment calibration is due within warning period");
    }

    private static async Task<IResult> GetEquipmentByType(
        string equipType,
        [FromQuery] short? testId,
        [FromQuery] string? lubeType,
        IEquipmentService equipmentService)
    {
        try
        {
            var equipment = await equipmentService.GetEquipmentByTypeAsync(equipType, testId, lubeType);
            return Results.Ok(equipment);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error retrieving equipment",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> GetEquipmentCalibration(
        int equipmentId,
        IEquipmentService equipmentService)
    {
        try
        {
            var calibration = await equipmentService.GetEquipmentCalibrationAsync(equipmentId);
            
            if (calibration == null)
                return Results.NotFound($"Equipment with ID {equipmentId} not found");

            return Results.Ok(calibration);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error retrieving equipment calibration",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> ValidateEquipment(
        int equipmentId,
        IEquipmentService equipmentService)
    {
        try
        {
            var validation = await equipmentService.ValidateEquipmentAsync(equipmentId);
            return Results.Ok(validation);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error validating equipment",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> GetAllEquipment(
        IEquipmentService equipmentService,
        [FromQuery] string? equipType,
        [FromQuery] short? testId,
        [FromQuery] string? lubeType,
        [FromQuery] bool excludeInactive = true,
        [FromQuery] bool includeOverdue = false)
    {
        try
        {
            var filter = new EquipmentFilterDto
            {
                EquipType = equipType,
                TestId = testId,
                LubeType = lubeType,
                ExcludeInactive = excludeInactive,
                IncludeOverdue = includeOverdue
            };

            var equipment = await equipmentService.GetAllEquipmentAsync(filter);
            return Results.Ok(equipment);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error retrieving equipment",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> GetEquipmentById(
        int id,
        IEquipmentService equipmentService)
    {
        try
        {
            var equipment = await equipmentService.GetEquipmentByIdAsync(id);
            
            if (equipment == null)
                return Results.NotFound($"Equipment with ID {id} not found");

            return Results.Ok(equipment);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error retrieving equipment",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> GetEquipmentTypes(IEquipmentService equipmentService)
    {
        try
        {
            var types = await equipmentService.GetEquipmentTypesAsync();
            return Results.Ok(types);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error retrieving equipment types",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> IsEquipmentDueSoon(
        int equipmentId,
        IEquipmentService equipmentService,
        [FromQuery] int warningDays = 30)
    {
        try
        {
            var isDueSoon = await equipmentService.IsEquipmentDueSoonAsync(equipmentId, warningDays);
            return Results.Ok(new { EquipmentId = equipmentId, IsDueSoon = isDueSoon, WarningDays = warningDays });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error checking equipment due date",
                detail: ex.Message,
                statusCode: 500);
        }
    }
}