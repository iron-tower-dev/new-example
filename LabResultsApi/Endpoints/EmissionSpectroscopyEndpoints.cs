using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using LabResultsApi.Services;
using LabResultsApi.Models;
using LabResultsApi.DTOs;

namespace LabResultsApi.Endpoints;

public static class EmissionSpectroscopyEndpoints
{
    public static void MapEmissionSpectroscopyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/emission-spectroscopy")
            .WithTags("Emission Spectroscopy")
            .WithOpenApi();

        group.MapGet("/{sampleId}/{testId}", GetEmissionSpectroscopyData)
            .WithName("GetEmissionSpectroscopyData")
            .WithSummary("Get emission spectroscopy data for a sample and test")
            .Produces<List<EmissionSpectroscopyDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

        group.MapPost("/", CreateEmissionSpectroscopyData)
            .WithName("CreateEmissionSpectroscopyData")
            .WithSummary("Create new emission spectroscopy data")
            .Accepts<EmissionSpectroscopyCreateDto>("application/json")
            .Produces<EmissionSpectroscopyDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        group.MapPut("/{sampleId}/{testId}/{trialNum}", UpdateEmissionSpectroscopyData)
            .WithName("UpdateEmissionSpectroscopyData")
            .WithSummary("Update emission spectroscopy data")
            .Accepts<EmissionSpectroscopyUpdateDto>("application/json")
            .Produces<EmissionSpectroscopyDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

        group.MapDelete("/{sampleId}/{testId}/{trialNum}", DeleteEmissionSpectroscopyData)
            .WithName("DeleteEmissionSpectroscopyData")
            .WithSummary("Delete emission spectroscopy data")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

        group.MapPost("/{sampleId}/schedule-ferrography", ScheduleFerrography)
            .WithName("EmissionSpectroscopyScheduleFerrography")
            .WithSummary("Schedule Ferrography test for a sample")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> GetEmissionSpectroscopyData(
        int sampleId, 
        int testId, 
        IRawSqlService rawSqlService, 
        IMapper mapper,
        ILogger<Program> logger)
    {
        try
        {
            logger.LogInformation("Getting emission spectroscopy data for sample {SampleId}, test {TestId}", sampleId, testId);

            // Validate that the sample exists
            var sampleExists = await rawSqlService.SampleExistsAsync(sampleId);
            if (!sampleExists)
            {
                logger.LogWarning("Sample {SampleId} not found", sampleId);
                return Results.NotFound($"Sample {sampleId} not found");
            }

            var data = await rawSqlService.GetEmissionSpectroscopyAsync(sampleId, testId);
            var dtos = mapper.Map<List<EmissionSpectroscopyDto>>(data);

            logger.LogInformation("Retrieved {Count} emission spectroscopy records for sample {SampleId}, test {TestId}", 
                dtos.Count, sampleId, testId);

            return Results.Ok(dtos);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting emission spectroscopy data for sample {SampleId}, test {TestId}", sampleId, testId);
            return Results.Problem("An error occurred while retrieving emission spectroscopy data");
        }
    }

    private static async Task<IResult> CreateEmissionSpectroscopyData(
        [FromBody] EmissionSpectroscopyCreateDto createDto,
        IRawSqlService rawSqlService,
        IMapper mapper,
        ILogger<Program> logger)
    {
        try
        {
            logger.LogInformation("Creating emission spectroscopy data for sample {SampleId}, test {TestId}, trial {TrialNum}", 
                createDto.Id, createDto.TestId, createDto.TrialNum);

            // Validate input
            if (createDto.Id <= 0 || createDto.TestId <= 0 || createDto.TrialNum <= 0)
            {
                return Results.BadRequest("Sample ID, Test ID, and Trial Number must be positive integers");
            }

            // Validate that the sample exists
            var sampleExists = await rawSqlService.SampleExistsAsync(createDto.Id);
            if (!sampleExists)
            {
                logger.LogWarning("Sample {SampleId} not found", createDto.Id);
                return Results.BadRequest($"Sample {createDto.Id} not found");
            }

            // Map DTO to entity
            var entity = mapper.Map<EmissionSpectroscopy>(createDto);
            entity.TrialDate = DateTime.Now;
            // Note: Status property removed from model as it doesn't exist in SQL table

            // Save the data
            var result = await rawSqlService.SaveEmissionSpectroscopyAsync(entity);

            // Handle Ferrography scheduling if requested
            if (createDto.ScheduleFerrography && createDto.TrialNum == 1)
            {
                await rawSqlService.ScheduleFerrographyAsync(createDto.Id);
                logger.LogInformation("Ferrography scheduled for sample {SampleId}", createDto.Id);
            }

            // Return the created data
            var responseDto = mapper.Map<EmissionSpectroscopyDto>(entity);
            responseDto.ScheduleFerrography = createDto.ScheduleFerrography;

            logger.LogInformation("Created emission spectroscopy data for sample {SampleId}, test {TestId}, trial {TrialNum}", 
                createDto.Id, createDto.TestId, createDto.TrialNum);

            return Results.Created($"/api/emission-spectroscopy/{createDto.Id}/{createDto.TestId}", responseDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating emission spectroscopy data for sample {SampleId}, test {TestId}", 
                createDto.Id, createDto.TestId);
            return Results.Problem("An error occurred while creating emission spectroscopy data");
        }
    }

    private static async Task<IResult> UpdateEmissionSpectroscopyData(
        int sampleId,
        int testId,
        int trialNum,
        [FromBody] EmissionSpectroscopyUpdateDto updateDto,
        IRawSqlService rawSqlService,
        IMapper mapper,
        ILogger<Program> logger)
    {
        try
        {
            logger.LogInformation("Updating emission spectroscopy data for sample {SampleId}, test {TestId}, trial {TrialNum}", 
                sampleId, testId, trialNum);

            // Validate input
            if (sampleId <= 0 || testId <= 0 || trialNum <= 0)
            {
                return Results.BadRequest("Sample ID, Test ID, and Trial Number must be positive integers");
            }

            // Check if the record exists
            var existingData = await rawSqlService.GetEmissionSpectroscopyAsync(sampleId, testId);
            var existingRecord = existingData.FirstOrDefault(x => x.TrialNum == trialNum);
            
            if (existingRecord == null)
            {
                logger.LogWarning("Emission spectroscopy record not found for sample {SampleId}, test {TestId}, trial {TrialNum}", 
                    sampleId, testId, trialNum);
                return Results.NotFound("Emission spectroscopy record not found");
            }

            // Map update DTO to existing entity
            mapper.Map(updateDto, existingRecord);
            existingRecord.Id = sampleId;
            existingRecord.TestId = testId;
            existingRecord.TrialNum = trialNum;

            // Update the data
            var result = await rawSqlService.UpdateEmissionSpectroscopyAsync(existingRecord);

            // Handle Ferrography scheduling if requested and this is trial 1
            if (updateDto.ScheduleFerrography && trialNum == 1)
            {
                await rawSqlService.ScheduleFerrographyAsync(sampleId);
                logger.LogInformation("Ferrography scheduled for sample {SampleId}", sampleId);
            }

            // Return the updated data
            var responseDto = mapper.Map<EmissionSpectroscopyDto>(existingRecord);
            responseDto.ScheduleFerrography = updateDto.ScheduleFerrography;

            logger.LogInformation("Updated emission spectroscopy data for sample {SampleId}, test {TestId}, trial {TrialNum}", 
                sampleId, testId, trialNum);

            return Results.Ok(responseDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating emission spectroscopy data for sample {SampleId}, test {TestId}, trial {TrialNum}", 
                sampleId, testId, trialNum);
            return Results.Problem("An error occurred while updating emission spectroscopy data");
        }
    }

    private static async Task<IResult> DeleteEmissionSpectroscopyData(
        int sampleId,
        int testId,
        int trialNum,
        IRawSqlService rawSqlService,
        ILogger<Program> logger)
    {
        try
        {
            logger.LogInformation("Deleting emission spectroscopy data for sample {SampleId}, test {TestId}, trial {TrialNum}", 
                sampleId, testId, trialNum);

            // Validate input
            if (sampleId <= 0 || testId <= 0 || trialNum <= 0)
            {
                return Results.BadRequest("Sample ID, Test ID, and Trial Number must be positive integers");
            }

            // Check if the record exists
            var existingData = await rawSqlService.GetEmissionSpectroscopyAsync(sampleId, testId);
            var existingRecord = existingData.FirstOrDefault(x => x.TrialNum == trialNum);
            
            if (existingRecord == null)
            {
                logger.LogWarning("Emission spectroscopy record not found for sample {SampleId}, test {TestId}, trial {TrialNum}", 
                    sampleId, testId, trialNum);
                return Results.NotFound("Emission spectroscopy record not found");
            }

            // Delete the data
            var result = await rawSqlService.DeleteEmissionSpectroscopyAsync(sampleId, testId, trialNum);

            logger.LogInformation("Deleted emission spectroscopy data for sample {SampleId}, test {TestId}, trial {TrialNum}", 
                sampleId, testId, trialNum);

            return Results.NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting emission spectroscopy data for sample {SampleId}, test {TestId}, trial {TrialNum}", 
                sampleId, testId, trialNum);
            return Results.Problem("An error occurred while deleting emission spectroscopy data");
        }
    }

    private static async Task<IResult> ScheduleFerrography(
        int sampleId,
        IRawSqlService rawSqlService,
        ILogger<Program> logger)
    {
        try
        {
            logger.LogInformation("Scheduling Ferrography for sample {SampleId}", sampleId);

            // Validate that the sample exists
            var sampleExists = await rawSqlService.SampleExistsAsync(sampleId);
            if (!sampleExists)
            {
                logger.LogWarning("Sample {SampleId} not found", sampleId);
                return Results.BadRequest($"Sample {sampleId} not found");
            }

            var result = await rawSqlService.ScheduleFerrographyAsync(sampleId);

            if (result > 0)
            {
                logger.LogInformation("Ferrography scheduled successfully for sample {SampleId}", sampleId);
                return Results.Ok(new { Message = "Ferrography scheduled successfully", SampleId = sampleId });
            }
            else
            {
                logger.LogInformation("Ferrography already scheduled for sample {SampleId}", sampleId);
                return Results.Ok(new { Message = "Ferrography already scheduled", SampleId = sampleId });
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error scheduling Ferrography for sample {SampleId}", sampleId);
            return Results.Problem("An error occurred while scheduling Ferrography");
        }
    }
}