using Microsoft.AspNetCore.Mvc;
using LabResultsApi.Services.Migration;
using LabResultsApi.DTOs.Migration;
using LabResultsApi.Models.Migration;
using AutoMapper;
using System.Text.Json;

namespace LabResultsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MigrationController : ControllerBase
{
    private readonly IMigrationControlService _migrationService;
    private readonly IMapper _mapper;
    private readonly ILogger<MigrationController> _logger;

    public MigrationController(
        IMigrationControlService migrationService,
        IMapper mapper,
        ILogger<MigrationController> logger)
    {
        _migrationService = migrationService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Start a new migration with the specified options
    /// </summary>
    [HttpPost("start")]
    public async Task<ActionResult<MigrationStatusDto>> StartMigration([FromBody] StartMigrationRequest request)
    {
        try
        {
            if (await _migrationService.IsMigrationRunningAsync())
            {
                return Conflict("A migration is already in progress");
            }

            var options = _mapper.Map<MigrationOptions>(request.Options);
            var result = await _migrationService.ExecuteFullMigrationAsync(options);
            var statusDto = _mapper.Map<MigrationStatusDto>(result);

            _logger.LogInformation("Migration started with ID {MigrationId}", result.MigrationId);
            
            return Ok(statusDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start migration");
            return StatusCode(500, $"Failed to start migration: {ex.Message}");
        }
    }

    /// <summary>
    /// Get the current migration status
    /// </summary>
    [HttpGet("status")]
    public async Task<ActionResult<MigrationStatusDto>> GetMigrationStatus()
    {
        try
        {
            var currentMigration = await _migrationService.GetCurrentMigrationAsync();
            
            if (currentMigration == null)
            {
                return Ok(new MigrationStatusDto 
                { 
                    Status = MigrationStatus.NotStarted.ToString(),
                    MigrationId = Guid.Empty
                });
            }

            var statusDto = _mapper.Map<MigrationStatusDto>(currentMigration);
            return Ok(statusDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get migration status");
            return StatusCode(500, $"Failed to get migration status: {ex.Message}");
        }
    }

    /// <summary>
    /// Cancel the current migration
    /// </summary>
    [HttpPost("cancel")]
    public async Task<ActionResult> CancelMigration()
    {
        try
        {
            var cancelled = await _migrationService.CancelMigrationAsync();
            
            if (!cancelled)
            {
                return BadRequest("No migration is currently running");
            }

            _logger.LogInformation("Migration cancellation requested");
            return Ok("Migration cancellation requested");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel migration");
            return StatusCode(500, $"Failed to cancel migration: {ex.Message}");
        }
    }

    /// <summary>
    /// Get migration result by ID
    /// </summary>
    [HttpGet("{migrationId}")]
    public async Task<ActionResult<MigrationReportDto>> GetMigrationResult(Guid migrationId)
    {
        try
        {
            var result = await _migrationService.GetMigrationResultAsync(migrationId);
            
            if (result == null)
            {
                return NotFound($"Migration with ID {migrationId} not found");
            }

            var reportDto = _mapper.Map<MigrationReportDto>(result);
            return Ok(reportDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get migration result for {MigrationId}", migrationId);
            return StatusCode(500, $"Failed to get migration result: {ex.Message}");
        }
    }

    /// <summary>
    /// Get migration history
    /// </summary>
    [HttpGet("history")]
    public async Task<ActionResult<List<MigrationStatusDto>>> GetMigrationHistory([FromQuery] int limit = 10)
    {
        try
        {
            var history = await _migrationService.GetMigrationHistoryAsync(limit);
            var historyDto = _mapper.Map<List<MigrationStatusDto>>(history);
            
            return Ok(historyDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get migration history");
            return StatusCode(500, $"Failed to get migration history: {ex.Message}");
        }
    }

    /// <summary>
    /// Check if a migration is currently running
    /// </summary>
    [HttpGet("running")]
    public async Task<ActionResult<bool>> IsMigrationRunning()
    {
        try
        {
            var isRunning = await _migrationService.IsMigrationRunningAsync();
            return Ok(isRunning);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if migration is running");
            return StatusCode(500, $"Failed to check migration status: {ex.Message}");
        }
    }

    /// <summary>
    /// Get detailed migration progress with real-time statistics
    /// </summary>
    [HttpGet("progress")]
    public async Task<ActionResult<MigrationProgressDto>> GetMigrationProgress()
    {
        try
        {
            var currentMigration = await _migrationService.GetCurrentMigrationAsync();
            
            if (currentMigration == null)
            {
                return NotFound("No migration is currently running");
            }

            var progressDto = new MigrationProgressDto
            {
                MigrationId = currentMigration.MigrationId,
                ProgressPercentage = currentMigration.Statistics.ProgressPercentage,
                CurrentOperation = currentMigration.CurrentOperation ?? "Unknown",
                Statistics = _mapper.Map<MigrationStatisticsDto>(currentMigration.Statistics),
                EstimatedTimeRemaining = currentMigration.EstimatedTimeRemaining,
                StartTime = currentMigration.StartTime,
                ElapsedTime = DateTime.UtcNow - currentMigration.StartTime,
                Status = currentMigration.Status.ToString()
            };

            return Ok(progressDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get migration progress");
            return StatusCode(500, $"Failed to get migration progress: {ex.Message}");
        }
    }

    /// <summary>
    /// Generate and download migration report
    /// </summary>
    [HttpGet("{migrationId}/report")]
    public async Task<ActionResult> DownloadMigrationReport(Guid migrationId, [FromQuery] string format = "json")
    {
        try
        {
            var result = await _migrationService.GetMigrationResultAsync(migrationId);
            
            if (result == null)
            {
                return NotFound($"Migration with ID {migrationId} not found");
            }

            var reportDto = _mapper.Map<MigrationReportDto>(result);

            switch (format.ToLowerInvariant())
            {
                case "json":
                    return File(
                        System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(reportDto, new JsonSerializerOptions { WriteIndented = true }),
                        "application/json",
                        $"migration-report-{migrationId}.json"
                    );
                
                case "csv":
                    var csvContent = GenerateCsvReport(reportDto);
                    return File(
                        System.Text.Encoding.UTF8.GetBytes(csvContent),
                        "text/csv",
                        $"migration-report-{migrationId}.csv"
                    );
                
                default:
                    return BadRequest("Supported formats: json, csv");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate migration report for {MigrationId}", migrationId);
            return StatusCode(500, $"Failed to generate migration report: {ex.Message}");
        }
    }

    /// <summary>
    /// Download migration logs
    /// </summary>
    [HttpGet("{migrationId}/logs")]
    public async Task<ActionResult> DownloadMigrationLogs(Guid migrationId, [FromQuery] string level = "all")
    {
        try
        {
            var result = await _migrationService.GetMigrationResultAsync(migrationId);
            
            if (result == null)
            {
                return NotFound($"Migration with ID {migrationId} not found");
            }

            // Filter logs based on level and map to DTOs
            var allErrorDtos = result.Errors.Select(e => new MigrationErrorDto
            {
                Timestamp = e.Timestamp,
                Level = e.Level.ToString(),
                Component = e.Component,
                Message = e.Message,
                Details = e.Details,
                TableName = e.TableName,
                RecordNumber = e.RecordNumber
            }).ToList();

            var filteredErrors = level.ToLowerInvariant() switch
            {
                "error" => allErrorDtos.Where(e => e.Level == "Error").ToList(),
                "warning" => allErrorDtos.Where(e => e.Level == "Warning" || e.Level == "Error").ToList(),
                "info" => allErrorDtos.ToList(),
                _ => allErrorDtos
            };

            var logContent = GenerateLogContent(filteredErrors);
            
            return File(
                System.Text.Encoding.UTF8.GetBytes(logContent),
                "text/plain",
                $"migration-logs-{migrationId}.txt"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate migration logs for {MigrationId}", migrationId);
            return StatusCode(500, $"Failed to generate migration logs: {ex.Message}");
        }
    }

    /// <summary>
    /// Get migration statistics summary
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<MigrationStatisticsSummaryDto>> GetMigrationStatistics([FromQuery] int days = 30)
    {
        try
        {
            var history = await _migrationService.GetMigrationHistoryAsync(100); // Get more for statistics
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            var recentMigrations = history.Where(m => m.StartTime >= cutoffDate).ToList();

            var statistics = new MigrationStatisticsSummaryDto
            {
                TotalMigrations = recentMigrations.Count,
                SuccessfulMigrations = recentMigrations.Count(m => m.Status == MigrationStatus.Completed),
                FailedMigrations = recentMigrations.Count(m => m.Status == MigrationStatus.Failed),
                CancelledMigrations = recentMigrations.Count(m => m.Status == MigrationStatus.Cancelled),
                AverageDuration = recentMigrations.Any() 
                    ? TimeSpan.FromTicks((long)recentMigrations.Where(m => m.EndTime.HasValue)
                        .Select(m => (m.EndTime!.Value - m.StartTime).Ticks).DefaultIfEmpty(0).Average())
                    : TimeSpan.Zero,
                TotalRecordsProcessed = recentMigrations.Sum(m => m.Statistics.RecordsProcessed),
                TotalTablesProcessed = recentMigrations.Sum(m => m.Statistics.TablesProcessed),
                PeriodDays = days
            };

            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get migration statistics");
            return StatusCode(500, $"Failed to get migration statistics: {ex.Message}");
        }
    }

    /// <summary>
    /// Pause the current migration
    /// </summary>
    [HttpPost("pause")]
    public async Task<ActionResult> PauseMigration()
    {
        try
        {
            var currentMigration = await _migrationService.GetCurrentMigrationAsync();
            
            if (currentMigration == null)
            {
                return BadRequest("No migration is currently running");
            }

            if (currentMigration.Status != MigrationStatus.InProgress)
            {
                return BadRequest($"Migration is not in a pausable state. Current status: {currentMigration.Status}");
            }

            // Note: This would require implementing pause functionality in the service
            _logger.LogInformation("Migration pause requested for {MigrationId}", currentMigration.MigrationId);
            return Ok("Migration pause functionality not yet implemented");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to pause migration");
            return StatusCode(500, $"Failed to pause migration: {ex.Message}");
        }
    }

    /// <summary>
    /// Resume a paused migration
    /// </summary>
    [HttpPost("resume")]
    public async Task<ActionResult> ResumeMigration()
    {
        try
        {
            var currentMigration = await _migrationService.GetCurrentMigrationAsync();
            
            if (currentMigration == null)
            {
                return BadRequest("No migration found to resume");
            }

            if (currentMigration.Status != MigrationStatus.Paused)
            {
                return BadRequest($"Migration is not paused. Current status: {currentMigration.Status}");
            }

            // Note: This would require implementing resume functionality in the service
            _logger.LogInformation("Migration resume requested for {MigrationId}", currentMigration.MigrationId);
            return Ok("Migration resume functionality not yet implemented");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resume migration");
            return StatusCode(500, $"Failed to resume migration: {ex.Message}");
        }
    }

    private string GenerateCsvReport(MigrationReportDto report)
    {
        var csv = new System.Text.StringBuilder();
        
        // Header
        csv.AppendLine("Migration Report");
        csv.AppendLine($"Migration ID,{report.MigrationId}");
        csv.AppendLine($"Status,{report.Status}");
        csv.AppendLine($"Duration,{report.Duration}");
        csv.AppendLine($"Generated At,{report.GeneratedAt}");
        csv.AppendLine();
        
        // Summary
        csv.AppendLine("Summary");
        csv.AppendLine("Metric,Value");
        csv.AppendLine($"Total Tables,{report.Summary.TotalTables}");
        csv.AppendLine($"Tables Processed,{report.Summary.TablesProcessed}");
        csv.AppendLine($"Total Records,{report.Summary.TotalRecords}");
        csv.AppendLine($"Records Processed,{report.Summary.RecordsProcessed}");
        csv.AppendLine($"Error Count,{report.Summary.ErrorCount}");
        csv.AppendLine($"Success,{report.Summary.Success}");
        csv.AppendLine();
        
        // Errors
        if (report.Errors.Any())
        {
            csv.AppendLine("Errors");
            csv.AppendLine("Timestamp,Level,Component,Message,Table,Record");
            foreach (var error in report.Errors)
            {
                csv.AppendLine($"{error.Timestamp},{error.Level},{error.Component},\"{error.Message}\",{error.TableName},{error.RecordNumber}");
            }
        }
        
        return csv.ToString();
    }

    private string GenerateLogContent(List<MigrationErrorDto> errors)
    {
        var log = new System.Text.StringBuilder();
        
        log.AppendLine("Migration Log");
        log.AppendLine("=============");
        log.AppendLine();
        
        foreach (var error in errors.OrderBy(e => e.Timestamp))
        {
            log.AppendLine($"[{error.Timestamp:yyyy-MM-dd HH:mm:ss}] [{error.Level}] {error.Component}");
            log.AppendLine($"Message: {error.Message}");
            
            if (!string.IsNullOrEmpty(error.TableName))
                log.AppendLine($"Table: {error.TableName}");
                
            if (error.RecordNumber.HasValue)
                log.AppendLine($"Record: {error.RecordNumber}");
                
            if (!string.IsNullOrEmpty(error.Details))
                log.AppendLine($"Details: {error.Details}");
                
            log.AppendLine();
        }
        
        return log.ToString();
    }
}