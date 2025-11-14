using Microsoft.AspNetCore.Mvc;
using LabResultsApi.Services.Migration;
using LabResultsApi.DTOs.Migration;

namespace LabResultsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("SSO Migration")]
public class SsoMigrationController : ControllerBase
{
    private readonly ISsoMigrationService _ssoMigrationService;
    private readonly ILogger<SsoMigrationController> _logger;

    public SsoMigrationController(
        ISsoMigrationService ssoMigrationService,
        ILogger<SsoMigrationController> logger)
    {
        _ssoMigrationService = ssoMigrationService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a backup of the current authentication configuration
    /// </summary>
    [HttpPost("backup")]
    public async Task<ActionResult<BackupResult>> CreateBackupAsync()
    {
        try
        {
            _logger.LogInformation("Creating authentication configuration backup");
            var result = await _ssoMigrationService.BackupCurrentAuthConfigAsync();
            
            if (result.Success)
            {
                _logger.LogInformation("Successfully created backup: {BackupId}", result.BackupId);
                return Ok(result);
            }
            
            _logger.LogWarning("Backup creation failed: {Errors}", string.Join(", ", result.Errors));
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating authentication backup");
            return StatusCode(500, new { message = "An error occurred while creating the backup", error = ex.Message });
        }
    }

    /// <summary>
    /// Removes JWT authentication system from API and frontend
    /// </summary>
    [HttpPost("remove-jwt-auth")]
    public async Task<ActionResult<AuthRemovalResult>> RemoveJwtAuthenticationAsync()
    {
        try
        {
            _logger.LogInformation("Starting JWT authentication removal");
            var result = await _ssoMigrationService.RemoveJwtAuthenticationAsync();
            
            if (result.Success)
            {
                _logger.LogInformation("Successfully removed JWT authentication");
                return Ok(result);
            }
            
            _logger.LogWarning("JWT authentication removal completed with errors: {Errors}", string.Join(", ", result.Errors));
            return Ok(result); // Return 200 even with errors as partial success is still useful
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing JWT authentication");
            return StatusCode(500, new { message = "An error occurred while removing JWT authentication", error = ex.Message });
        }
    }

    /// <summary>
    /// Cleans up authentication configuration files
    /// </summary>
    [HttpPost("cleanup-config")]
    public async Task<ActionResult<ConfigCleanupResult>> CleanupAuthConfigurationAsync()
    {
        try
        {
            _logger.LogInformation("Starting authentication configuration cleanup");
            var result = await _ssoMigrationService.CleanupAuthConfigurationAsync();
            
            if (result.Success)
            {
                _logger.LogInformation("Successfully cleaned up authentication configuration");
                return Ok(result);
            }
            
            _logger.LogWarning("Configuration cleanup completed with errors: {Errors}", string.Join(", ", result.Errors));
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up authentication configuration");
            return StatusCode(500, new { message = "An error occurred while cleaning up configuration", error = ex.Message });
        }
    }

    /// <summary>
    /// Updates frontend authentication system to remove guards and interceptors
    /// </summary>
    [HttpPost("update-frontend")]
    public async Task<ActionResult<FrontendUpdateResult>> UpdateFrontendAuthAsync()
    {
        try
        {
            _logger.LogInformation("Starting frontend authentication update");
            var result = await _ssoMigrationService.UpdateFrontendAuthAsync();
            
            if (result.Success)
            {
                _logger.LogInformation("Successfully updated frontend authentication");
                return Ok(result);
            }
            
            _logger.LogWarning("Frontend update completed with errors: {Errors}", string.Join(", ", result.Errors));
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating frontend authentication");
            return StatusCode(500, new { message = "An error occurred while updating frontend authentication", error = ex.Message });
        }
    }

    /// <summary>
    /// Rolls back authentication changes using a backup
    /// </summary>
    [HttpPost("rollback/{backupId}")]
    public async Task<ActionResult<RollbackResult>> RollbackAuthenticationAsync(string backupId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(backupId))
            {
                return BadRequest(new { message = "Backup ID is required" });
            }

            _logger.LogInformation("Starting authentication rollback using backup: {BackupId}", backupId);
            var result = await _ssoMigrationService.RollbackAuthenticationAsync(backupId);
            
            if (result.Success)
            {
                _logger.LogInformation("Successfully rolled back authentication using backup: {BackupId}", backupId);
                return Ok(result);
            }
            
            _logger.LogWarning("Rollback completed with errors: {Errors}", string.Join(", ", result.Errors));
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rolling back authentication using backup: {BackupId}", backupId);
            return StatusCode(500, new { message = "An error occurred while rolling back authentication", error = ex.Message });
        }
    }

    /// <summary>
    /// Gets list of available authentication backups
    /// </summary>
    [HttpGet("backups")]
    public async Task<ActionResult<List<AuthBackupInfo>>> GetAvailableBackupsAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving available authentication backups");
            var backups = await _ssoMigrationService.GetAvailableBackupsAsync();
            
            _logger.LogInformation("Found {Count} available backups", backups.Count);
            return Ok(backups);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available backups");
            return StatusCode(500, new { message = "An error occurred while retrieving backups", error = ex.Message });
        }
    }

    /// <summary>
    /// Performs complete SSO migration (backup + remove auth + cleanup)
    /// </summary>
    [HttpPost("migrate-to-sso")]
    public async Task<ActionResult<SsoMigrationResult>> MigrateToSsoAsync()
    {
        try
        {
            _logger.LogInformation("Starting complete SSO migration process");
            
            var migrationResult = new SsoMigrationResult
            {
                StartTime = DateTime.UtcNow
            };

            // Step 1: Create backup
            _logger.LogInformation("Step 1: Creating backup");
            migrationResult.BackupResult = await _ssoMigrationService.BackupCurrentAuthConfigAsync();
            
            if (!migrationResult.BackupResult.Success)
            {
                migrationResult.Success = false;
                migrationResult.Errors.AddRange(migrationResult.BackupResult.Errors);
                migrationResult.EndTime = DateTime.UtcNow;
                return BadRequest(migrationResult);
            }

            // Step 2: Remove JWT authentication
            _logger.LogInformation("Step 2: Removing JWT authentication");
            migrationResult.AuthRemovalResult = await _ssoMigrationService.RemoveJwtAuthenticationAsync();
            
            // Step 3: Update frontend
            _logger.LogInformation("Step 3: Updating frontend");
            migrationResult.FrontendUpdateResult = await _ssoMigrationService.UpdateFrontendAuthAsync();
            
            // Step 4: Cleanup configuration
            _logger.LogInformation("Step 4: Cleaning up configuration");
            migrationResult.ConfigCleanupResult = await _ssoMigrationService.CleanupAuthConfigurationAsync();

            // Determine overall success
            migrationResult.Success = migrationResult.AuthRemovalResult.Success && 
                                    migrationResult.FrontendUpdateResult.Success && 
                                    migrationResult.ConfigCleanupResult.Success;

            // Collect all errors
            migrationResult.Errors.AddRange(migrationResult.AuthRemovalResult.Errors);
            migrationResult.Errors.AddRange(migrationResult.FrontendUpdateResult.Errors);
            migrationResult.Errors.AddRange(migrationResult.ConfigCleanupResult.Errors);

            migrationResult.EndTime = DateTime.UtcNow;

            if (migrationResult.Success)
            {
                _logger.LogInformation("SSO migration completed successfully in {Duration}ms", 
                    (migrationResult.EndTime - migrationResult.StartTime).TotalMilliseconds);
                return Ok(migrationResult);
            }
            else
            {
                _logger.LogWarning("SSO migration completed with errors: {Errors}", 
                    string.Join(", ", migrationResult.Errors));
                return Ok(migrationResult); // Return 200 as partial success is still useful
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during complete SSO migration");
            return StatusCode(500, new { message = "An error occurred during SSO migration", error = ex.Message });
        }
    }
}

public class SsoMigrationResult
{
    public bool Success { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public BackupResult BackupResult { get; set; } = new();
    public AuthRemovalResult AuthRemovalResult { get; set; } = new();
    public FrontendUpdateResult FrontendUpdateResult { get; set; } = new();
    public ConfigCleanupResult ConfigCleanupResult { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}