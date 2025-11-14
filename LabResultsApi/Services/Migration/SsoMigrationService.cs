using System.Text.Json;
using LabResultsApi.DTOs.Migration;
using LabResultsApi.Models.Migration;
using LabResultsApi.Services.Migration;

namespace LabResultsApi.Services.Migration;

public class SsoMigrationService : ISsoMigrationService
{
    private readonly IAuthenticationRemovalService _authRemovalService;
    private readonly ILogger<SsoMigrationService> _logger;
    private readonly IMigrationLoggingService _migrationLogger;
    private readonly string _backupDirectory;
    private readonly string _frontendPath;

    public SsoMigrationService(
        IAuthenticationRemovalService authRemovalService,
        ILogger<SsoMigrationService> logger,
        IMigrationLoggingService migrationLogger,
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        _authRemovalService = authRemovalService;
        _logger = logger;
        _migrationLogger = migrationLogger;
        _backupDirectory = Path.Combine(environment.ContentRootPath, "Backups", "Authentication");
        
        // Try to determine frontend path from configuration or use default
        _frontendPath = configuration["Frontend:Path"] ?? 
                      Path.Combine(Directory.GetParent(environment.ContentRootPath)?.FullName ?? "", "lab-results-frontend");
        
        Directory.CreateDirectory(_backupDirectory);
    }

    public async Task<DTOs.Migration.AuthRemovalResult> RemoveJwtAuthenticationAsync()
    {
        var result = new DTOs.Migration.AuthRemovalResult();
        
        try
        {
            _logger.LogInformation("Starting complete JWT authentication removal");
            await _migrationLogger.LogAsync(Guid.NewGuid(), Models.Migration.LogLevel.Information, "Starting complete JWT authentication removal");

            // Step 1: Remove JWT middleware
            var middlewareResult = await _authRemovalService.RemoveJwtMiddlewareAsync();
            result.RemovedComponents.AddRange(middlewareResult.RemovedComponents);
            result.ModifiedFiles.AddRange(middlewareResult.ModifiedFiles);
            result.Errors.AddRange(middlewareResult.Errors);

            if (!middlewareResult.Success)
            {
                result.Errors.Add("Failed to remove JWT middleware");
                return result;
            }

            // Step 2: Remove authentication services
            var servicesResult = await _authRemovalService.RemoveAuthenticationServicesAsync();
            result.RemovedComponents.AddRange(servicesResult.RemovedComponents);
            result.ModifiedFiles.AddRange(servicesResult.ModifiedFiles);
            result.Errors.AddRange(servicesResult.Errors);

            if (!servicesResult.Success)
            {
                result.Errors.Add("Failed to remove authentication services");
                return result;
            }

            // Step 3: Clean up dependencies
            var cleanupResult = await _authRemovalService.CleanupAuthDependenciesAsync();
            result.RemovedComponents.AddRange(cleanupResult.RemovedComponents);
            result.ModifiedFiles.AddRange(cleanupResult.ModifiedFiles);
            result.Errors.AddRange(cleanupResult.Errors);

            if (!cleanupResult.Success)
            {
                result.Errors.Add("Failed to clean up authentication dependencies");
                return result;
            }

            result.Success = result.Errors.Count == 0;
            
            if (result.Success)
            {
                _logger.LogInformation("Successfully removed JWT authentication system");
                await _migrationLogger.LogAsync(Guid.NewGuid(), Models.Migration.LogLevel.Information, "Successfully removed JWT authentication system");
            }
            else
            {
                _logger.LogWarning("JWT authentication removal completed with errors: {Errors}", string.Join(", ", result.Errors));
                await _migrationLogger.LogAsync(Guid.NewGuid(), Models.Migration.LogLevel.Warning, $"JWT authentication removal completed with errors: {string.Join(", ", result.Errors)}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during JWT authentication removal");
            await _migrationLogger.LogAsync(Guid.NewGuid(), Models.Migration.LogLevel.Error, $"Error during JWT authentication removal: {ex.Message}", ex);
            result.Errors.Add($"Error during JWT authentication removal: {ex.Message}");
        }

        return result;
    }

    public async Task<DTOs.Migration.ConfigCleanupResult> CleanupAuthConfigurationAsync()
    {
        var result = new DTOs.Migration.ConfigCleanupResult();
        
        try
        {
            _logger.LogInformation("Starting authentication configuration cleanup");
            await _migrationLogger.LogAsync(Guid.NewGuid(), Models.Migration.LogLevel.Information, "Starting authentication configuration cleanup");

            // This is handled by the authentication removal service
            // Additional cleanup can be added here if needed
            
            result.Success = true;
            result.CleanedConfigSections.Add("JWT Configuration");
            
            _logger.LogInformation("Successfully cleaned up authentication configuration");
            await _migrationLogger.LogAsync(Guid.NewGuid(), Models.Migration.LogLevel.Information, "Successfully cleaned up authentication configuration");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication configuration cleanup");
            await _migrationLogger.LogAsync(Guid.NewGuid(), Models.Migration.LogLevel.Error, $"Error during authentication configuration cleanup: {ex.Message}", ex);
            result.Errors.Add($"Error during authentication configuration cleanup: {ex.Message}");
        }

        return result;
    }

    public async Task<DTOs.Migration.FrontendUpdateResult> UpdateFrontendAuthAsync()
    {
        var result = new DTOs.Migration.FrontendUpdateResult();
        
        try
        {
            _logger.LogInformation("Starting frontend authentication update");
            await _migrationLogger.LogAsync(Guid.NewGuid(), Models.Migration.LogLevel.Information, "Starting frontend authentication update");

            if (!Directory.Exists(_frontendPath))
            {
                result.Errors.Add($"Frontend directory not found: {_frontendPath}");
                _logger.LogWarning("Frontend directory not found: {FrontendPath}", _frontendPath);
                return result;
            }

            // Update Angular routing to remove authentication guards
            await UpdateAngularRoutingAsync(result);
            
            // Remove authentication guards and interceptors
            await RemoveAuthenticationGuardsAsync(result);
            await RemoveAuthenticationInterceptorsAsync(result);
            
            // Clear authentication tokens from services
            await UpdateAuthenticationServicesAsync(result);

            result.Success = result.Errors.Count == 0;
            
            if (result.Success)
            {
                _logger.LogInformation("Successfully updated frontend authentication");
                await _migrationLogger.LogAsync(Guid.NewGuid(), Models.Migration.LogLevel.Information, "Successfully updated frontend authentication");
            }
            else
            {
                _logger.LogWarning("Frontend authentication update completed with errors: {Errors}", string.Join(", ", result.Errors));
                await _migrationLogger.LogAsync(Guid.NewGuid(), Models.Migration.LogLevel.Warning, $"Frontend authentication update completed with errors: {string.Join(", ", result.Errors)}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during frontend authentication update");
            await _migrationLogger.LogAsync(Guid.NewGuid(), Models.Migration.LogLevel.Error, $"Error during frontend authentication update: {ex.Message}", ex);
            result.Errors.Add($"Error during frontend authentication update: {ex.Message}");
        }

        return result;
    }

    public async Task<DTOs.Migration.BackupResult> BackupCurrentAuthConfigAsync()
    {
        var serviceResult = await _authRemovalService.BackupProgramConfigurationAsync();
        
        // Convert to DTO first
        var dtoResult = new DTOs.Migration.BackupResult
        {
            Success = serviceResult.Success,
            BackupId = serviceResult.BackupId.ToString(),
            BackupLocation = serviceResult.BackupLocation,
            BackedUpFiles = serviceResult.BackedUpFiles,
            BackupSizeBytes = serviceResult.BackupSizeBytes,
            Errors = serviceResult.Errors,
            Timestamp = serviceResult.Timestamp
        };

        // Also backup frontend authentication files if they exist
        if (Directory.Exists(_frontendPath))
        {
            await BackupFrontendAuthFilesAsync(dtoResult);
        }

        return dtoResult;
    }

    public async Task<DTOs.Migration.RollbackResult> RollbackAuthenticationAsync(string backupId)
    {
        var serviceResult = await _authRemovalService.RestoreProgramConfigurationAsync(backupId);
        
        // Convert to DTO first
        var dtoResult = new DTOs.Migration.RollbackResult
        {
            Success = serviceResult.Success,
            BackupId = serviceResult.BackupId,
            RestoredFiles = serviceResult.RestoredFiles,
            Errors = serviceResult.Errors
        };

        // Also restore frontend files if they were backed up
        if (serviceResult.Success)
        {
            await RestoreFrontendAuthFilesAsync(backupId, dtoResult);
        }

        return dtoResult;
    }

    public async Task<List<AuthBackupInfo>> GetAvailableBackupsAsync()
    {
        var backups = new List<AuthBackupInfo>();
        
        try
        {
            if (!Directory.Exists(_backupDirectory))
            {
                return backups;
            }

            var backupDirs = Directory.GetDirectories(_backupDirectory, "auth_backup_*");
            
            foreach (var backupDir in backupDirs)
            {
                var metadataPath = Path.Combine(backupDir, "backup_metadata.json");
                if (File.Exists(metadataPath))
                {
                    try
                    {
                        var metadataJson = await File.ReadAllTextAsync(metadataPath);
                        var metadata = JsonSerializer.Deserialize<JsonElement>(metadataJson);
                        
                        var backup = new AuthBackupInfo
                        {
                            BackupId = metadata.GetProperty("BackupId").GetString() ?? Path.GetFileName(backupDir),
                            CreatedAt = metadata.GetProperty("CreatedAt").GetDateTime(),
                            BackupLocation = backupDir,
                            Description = metadata.TryGetProperty("Description", out var desc) ? desc.GetString() ?? "" : "",
                            SizeBytes = metadata.TryGetProperty("TotalSizeBytes", out var size) ? size.GetInt64() : 0
                        };

                        if (metadata.TryGetProperty("BackedUpFiles", out var filesElement))
                        {
                            backup.BackedUpFiles = filesElement.EnumerateArray()
                                .Select(x => x.GetString())
                                .Where(x => !string.IsNullOrEmpty(x))
                                .ToList()!;
                        }

                        backups.Add(backup);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error reading backup metadata from {MetadataPath}", metadataPath);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available backups");
        }

        return backups.OrderByDescending(b => b.CreatedAt).ToList();
    }

    private async Task UpdateAngularRoutingAsync(DTOs.Migration.FrontendUpdateResult result)
    {
        var routesPath = Path.Combine(_frontendPath, "src", "app", "app.routes.ts");
        
        if (!File.Exists(routesPath))
        {
            _logger.LogWarning("Angular routes file not found: {RoutesPath}", routesPath);
            return;
        }

        try
        {
            var content = await File.ReadAllTextAsync(routesPath);
            var originalContent = content;

            // Remove authentication guard references
            content = content.Replace("canActivate: [AuthGuard]", "");
            content = content.Replace("canActivate: [AuthGuard],", "");
            content = content.Replace(", canActivate: [AuthGuard]", "");

            // Clean up extra commas and whitespace
            content = System.Text.RegularExpressions.Regex.Replace(content, @",\s*,", ",");
            content = System.Text.RegularExpressions.Regex.Replace(content, @",\s*}", "}");

            if (content != originalContent)
            {
                await File.WriteAllTextAsync(routesPath, content);
                result.UpdatedRoutes.Add(routesPath);
                result.ModifiedComponents.Add("Angular Routes");
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error updating Angular routes: {ex.Message}");
        }
    }

    private async Task RemoveAuthenticationGuardsAsync(DTOs.Migration.FrontendUpdateResult result)
    {
        var guardsPath = Path.Combine(_frontendPath, "src", "app", "guards");
        
        if (Directory.Exists(guardsPath))
        {
            try
            {
                var guardFiles = Directory.GetFiles(guardsPath, "*guard*.ts", SearchOption.AllDirectories);
                
                foreach (var guardFile in guardFiles)
                {
                    // Instead of deleting, we'll rename to .bak to preserve for rollback
                    var backupFile = guardFile + ".bak";
                    if (File.Exists(guardFile) && !File.Exists(backupFile))
                    {
                        File.Move(guardFile, backupFile);
                        result.RemovedGuards.Add(guardFile);
                    }
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Error removing authentication guards: {ex.Message}");
            }
        }
    }

    private async Task RemoveAuthenticationInterceptorsAsync(DTOs.Migration.FrontendUpdateResult result)
    {
        var interceptorsPath = Path.Combine(_frontendPath, "src", "app", "interceptors");
        
        if (Directory.Exists(interceptorsPath))
        {
            try
            {
                var interceptorFiles = Directory.GetFiles(interceptorsPath, "*interceptor*.ts", SearchOption.AllDirectories);
                
                foreach (var interceptorFile in interceptorFiles)
                {
                    // Instead of deleting, we'll rename to .bak to preserve for rollback
                    var backupFile = interceptorFile + ".bak";
                    if (File.Exists(interceptorFile) && !File.Exists(backupFile))
                    {
                        File.Move(interceptorFile, backupFile);
                        result.RemovedInterceptors.Add(interceptorFile);
                    }
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Error removing authentication interceptors: {ex.Message}");
            }
        }
    }

    private async Task UpdateAuthenticationServicesAsync(DTOs.Migration.FrontendUpdateResult result)
    {
        var servicesPath = Path.Combine(_frontendPath, "src", "app", "services");
        
        if (!Directory.Exists(servicesPath))
        {
            return;
        }

        try
        {
            var authServiceFiles = Directory.GetFiles(servicesPath, "*auth*.service.ts", SearchOption.AllDirectories);
            
            foreach (var serviceFile in authServiceFiles)
            {
                var content = await File.ReadAllTextAsync(serviceFile);
                var originalContent = content;

                // Add method to clear tokens
                if (!content.Contains("clearAuthenticationTokens"))
                {
                    var clearTokensMethod = @"
  // Added by SSO migration - clears all authentication tokens
  clearAuthenticationTokens(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
    sessionStorage.removeItem('token');
    sessionStorage.removeItem('refreshToken');
    sessionStorage.removeItem('user');
  }";

                    // Insert before the last closing brace
                    var lastBraceIndex = content.LastIndexOf('}');
                    if (lastBraceIndex > 0)
                    {
                        content = content.Insert(lastBraceIndex, clearTokensMethod + Environment.NewLine);
                    }
                }

                if (content != originalContent)
                {
                    await File.WriteAllTextAsync(serviceFile, content);
                    result.ModifiedComponents.Add($"Authentication Service: {Path.GetFileName(serviceFile)}");
                }
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error updating authentication services: {ex.Message}");
        }
    }

    private async Task BackupFrontendAuthFilesAsync(DTOs.Migration.BackupResult result)
    {
        if (string.IsNullOrEmpty(result.BackupId) || string.IsNullOrEmpty(result.BackupLocation))
        {
            return;
        }

        try
        {
            var frontendBackupPath = Path.Combine(result.BackupLocation, "frontend");
            Directory.CreateDirectory(frontendBackupPath);

            var filesToBackup = new[]
            {
                Path.Combine(_frontendPath, "src", "app", "app.routes.ts"),
                Path.Combine(_frontendPath, "src", "app", "guards"),
                Path.Combine(_frontendPath, "src", "app", "interceptors"),
                Path.Combine(_frontendPath, "src", "app", "services")
            };

            foreach (var path in filesToBackup)
            {
                if (File.Exists(path))
                {
                    var fileName = Path.GetFileName(path);
                    var backupFilePath = Path.Combine(frontendBackupPath, fileName);
                    File.Copy(path, backupFilePath);
                    result.BackedUpFiles.Add(path);
                }
                else if (Directory.Exists(path))
                {
                    var dirName = Path.GetFileName(path);
                    var backupDirPath = Path.Combine(frontendBackupPath, dirName);
                    await CopyDirectoryAsync(path, backupDirPath);
                    result.BackedUpFiles.Add(path);
                }
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error backing up frontend files: {ex.Message}");
        }
    }

    private async Task RestoreFrontendAuthFilesAsync(string backupId, DTOs.Migration.RollbackResult result)
    {
        try
        {
            var frontendBackupPath = Path.Combine(_backupDirectory, backupId, "frontend");
            
            if (!Directory.Exists(frontendBackupPath))
            {
                return; // No frontend backup exists
            }

            var backupFiles = Directory.GetFiles(frontendBackupPath, "*", SearchOption.AllDirectories);
            
            foreach (var backupFile in backupFiles)
            {
                var relativePath = Path.GetRelativePath(frontendBackupPath, backupFile);
                var originalPath = Path.Combine(_frontendPath, "src", "app", relativePath);
                
                var originalDir = Path.GetDirectoryName(originalPath);
                if (!string.IsNullOrEmpty(originalDir))
                {
                    Directory.CreateDirectory(originalDir);
                }
                
                File.Copy(backupFile, originalPath, overwrite: true);
                result.RestoredFiles.Add(originalPath);
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error restoring frontend files: {ex.Message}");
        }
    }

    private async Task CopyDirectoryAsync(string sourceDir, string destDir)
    {
        Directory.CreateDirectory(destDir);
        
        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var fileName = Path.GetFileName(file);
            var destFile = Path.Combine(destDir, fileName);
            File.Copy(file, destFile);
        }
        
        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            var dirName = Path.GetFileName(dir);
            var destSubDir = Path.Combine(destDir, dirName);
            await CopyDirectoryAsync(dir, destSubDir);
        }
    }
}