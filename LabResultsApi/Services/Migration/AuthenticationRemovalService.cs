using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using LabResultsApi.DTOs.Migration;
using LabResultsApi.Models.Migration;
using LabResultsApi.Services.Migration;

namespace LabResultsApi.Services.Migration;

public class AuthenticationRemovalService : IAuthenticationRemovalService
{
    private readonly ILogger<AuthenticationRemovalService> _logger;
    private readonly IMigrationLoggingService _migrationLogger;
    private readonly Guid _migrationId;
    private readonly string _backupDirectory;
    private readonly string _programFilePath;
    private readonly string _appSettingsPath;
    private readonly string _appSettingsDevPath;

    public AuthenticationRemovalService(
        ILogger<AuthenticationRemovalService> logger,
        IMigrationLoggingService migrationLogger,
        IWebHostEnvironment environment)
    {
        _logger = logger;
        _migrationLogger = migrationLogger;
        _migrationId = Guid.NewGuid();
        _backupDirectory = Path.Combine(environment.ContentRootPath, "Backups", "Authentication");
        _programFilePath = Path.Combine(environment.ContentRootPath, "Program.cs");
        _appSettingsPath = Path.Combine(environment.ContentRootPath, "appsettings.json");
        _appSettingsDevPath = Path.Combine(environment.ContentRootPath, "appsettings.Development.json");
        
        Directory.CreateDirectory(_backupDirectory);
    }

    public async Task<DTOs.Migration.AuthRemovalResult> RemoveJwtMiddlewareAsync()
    {
        var result = new DTOs.Migration.AuthRemovalResult();
        
        try
        {
            _logger.LogInformation("Starting JWT middleware removal");
            await _migrationLogger.LogAsync(_migrationId, LabResultsApi.Models.Migration.LogLevel.Information, "Starting JWT middleware removal");

            if (!File.Exists(_programFilePath))
            {
                result.Errors.Add($"Program.cs not found at {_programFilePath}");
                return result;
            }

            var programContent = await File.ReadAllTextAsync(_programFilePath);
            var originalContent = programContent;

            // Remove JWT authentication configuration
            var jwtConfigPattern = @"// Add JWT Authentication\s*\n.*?\.AddJwtBearer\(options\s*=>\s*\{.*?\}\);";
            programContent = Regex.Replace(programContent, jwtConfigPattern, "", RegexOptions.Singleline | RegexOptions.IgnoreCase);

            // Remove authentication and authorization middleware
            programContent = programContent.Replace("app.UseAuthentication();", "");
            programContent = programContent.Replace("app.UseAuthorization();", "");

            // Remove authentication service registration
            programContent = programContent.Replace("builder.Services.AddAuthorization();", "");

            // Remove JWT-related using statements
            var jwtUsings = new[]
            {
                "using Microsoft.AspNetCore.Authentication.JwtBearer;",
                "using Microsoft.IdentityModel.Tokens;",
                "using System.Text;"
            };

            foreach (var usingStatement in jwtUsings)
            {
                programContent = programContent.Replace(usingStatement + Environment.NewLine, "");
                programContent = programContent.Replace(usingStatement, "");
            }

            // Clean up extra blank lines
            programContent = Regex.Replace(programContent, @"\n\s*\n\s*\n", "\n\n");

            if (programContent != originalContent)
            {
                await File.WriteAllTextAsync(_programFilePath, programContent);
                result.ModifiedFiles.Add(_programFilePath);
                result.RemovedComponents.Add("JWT Authentication Configuration");
                result.RemovedComponents.Add("Authentication Middleware");
                result.RemovedComponents.Add("Authorization Middleware");
                
                _logger.LogInformation("Successfully removed JWT middleware from Program.cs");
                await _migrationLogger.LogAsync(_migrationId, LabResultsApi.Models.Migration.LogLevel.Information, "Successfully removed JWT middleware from Program.cs");
            }

            result.Success = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing JWT middleware");
            await _migrationLogger.LogAsync(_migrationId, LabResultsApi.Models.Migration.LogLevel.Error, $"Error removing JWT middleware: {ex.Message}", ex);
            result.Errors.Add($"Error removing JWT middleware: {ex.Message}");
        }

        return result;
    }

    public async Task<DTOs.Migration.AuthRemovalResult> RemoveAuthenticationServicesAsync()
    {
        var result = new DTOs.Migration.AuthRemovalResult();
        
        try
        {
            _logger.LogInformation("Starting authentication services removal");
            await _migrationLogger.LogAsync(Guid.NewGuid(), Models.Migration.LogLevel.Information, "Starting authentication services removal");

            if (!File.Exists(_programFilePath))
            {
                result.Errors.Add($"Program.cs not found at {_programFilePath}");
                return result;
            }

            var programContent = await File.ReadAllTextAsync(_programFilePath);
            var originalContent = programContent;

            // Remove authentication service registrations
            var authServiceLines = new[]
            {
                "builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();",
                "builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();"
            };

            foreach (var line in authServiceLines)
            {
                programContent = programContent.Replace(line + Environment.NewLine, "");
                programContent = programContent.Replace(line, "");
            }

            // Remove authentication endpoint mapping
            programContent = programContent.Replace("app.MapAuthenticationEndpoints();", "");

            if (programContent != originalContent)
            {
                await File.WriteAllTextAsync(_programFilePath, programContent);
                result.ModifiedFiles.Add(_programFilePath);
                result.RemovedComponents.Add("Authentication Service Registration");
                result.RemovedComponents.Add("Authorization Service Registration");
                result.RemovedComponents.Add("Authentication Endpoints Mapping");
                
                _logger.LogInformation("Successfully removed authentication services from Program.cs");
                await _migrationLogger.LogAsync(Guid.NewGuid(), Models.Migration.LogLevel.Information, "Successfully removed authentication services from Program.cs");
            }

            result.Success = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing authentication services");
            await _migrationLogger.LogAsync(Guid.NewGuid(), Models.Migration.LogLevel.Error, $"Error removing authentication services: {ex.Message}", ex);
            result.Errors.Add($"Error removing authentication services: {ex.Message}");
        }

        return result;
    }

    public async Task<DTOs.Migration.AuthRemovalResult> CleanupAuthDependenciesAsync()
    {
        var result = new DTOs.Migration.AuthRemovalResult();
        
        try
        {
            _logger.LogInformation("Starting authentication dependencies cleanup");
            await _migrationLogger.LogAsync(Guid.NewGuid(), Models.Migration.LogLevel.Information, "Starting authentication dependencies cleanup");

            // Clean up JWT configuration from appsettings files
            await CleanupJwtConfigFromFile(_appSettingsPath, result);
            await CleanupJwtConfigFromFile(_appSettingsDevPath, result);

            result.Success = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up authentication dependencies");
            await _migrationLogger.LogAsync(Guid.NewGuid(), Models.Migration.LogLevel.Error, $"Error cleaning up authentication dependencies: {ex.Message}", ex);
            result.Errors.Add($"Error cleaning up authentication dependencies: {ex.Message}");
        }

        return result;
    }

    public async Task<DTOs.Migration.BackupResult> BackupProgramConfigurationAsync()
    {
        var result = new DTOs.Migration.BackupResult();
        
        try
        {
            var backupId = $"auth_backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}";
            var backupPath = Path.Combine(_backupDirectory, backupId);
            Directory.CreateDirectory(backupPath);

            _logger.LogInformation("Creating authentication configuration backup: {BackupId}", backupId);
            await _migrationLogger.LogAsync(Guid.NewGuid(), Models.Migration.LogLevel.Information, $"Creating authentication configuration backup: {backupId}");

            var filesToBackup = new[]
            {
                _programFilePath,
                _appSettingsPath,
                _appSettingsDevPath
            };

            long totalSize = 0;

            foreach (var filePath in filesToBackup)
            {
                if (File.Exists(filePath))
                {
                    var fileName = Path.GetFileName(filePath);
                    var backupFilePath = Path.Combine(backupPath, fileName);
                    File.Copy(filePath, backupFilePath);
                    
                    var fileInfo = new FileInfo(filePath);
                    totalSize += fileInfo.Length;
                    
                    result.BackedUpFiles.Add(filePath);
                    _logger.LogDebug("Backed up file: {FilePath} to {BackupPath}", filePath, backupFilePath);
                }
            }

            // Create backup metadata
            var metadata = new
            {
                BackupId = backupId,
                CreatedAt = DateTime.UtcNow,
                Description = "Authentication configuration backup before SSO migration",
                BackedUpFiles = result.BackedUpFiles,
                TotalSizeBytes = totalSize
            };

            var metadataPath = Path.Combine(backupPath, "backup_metadata.json");
            await File.WriteAllTextAsync(metadataPath, JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true }));

            result.Success = true;
            result.BackupId = backupId;
            result.BackupLocation = backupPath;
            result.BackupSizeBytes = totalSize;

            _logger.LogInformation("Successfully created authentication backup: {BackupId} ({Size} bytes)", backupId, totalSize);
            await _migrationLogger.LogAsync(Guid.NewGuid(), Models.Migration.LogLevel.Information, $"Successfully created authentication backup: {backupId} ({totalSize} bytes)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating authentication backup");
            await _migrationLogger.LogAsync(Guid.NewGuid(), Models.Migration.LogLevel.Error, $"Error creating authentication backup: {ex.Message}", ex);
            result.Errors.Add($"Error creating authentication backup: {ex.Message}");
        }

        return result;
    }

    public async Task<DTOs.Migration.RollbackResult> RestoreProgramConfigurationAsync(string backupId)
    {
        var result = new DTOs.Migration.RollbackResult { BackupId = backupId };
        
        try
        {
            var backupPath = Path.Combine(_backupDirectory, backupId);
            
            if (!Directory.Exists(backupPath))
            {
                result.Errors.Add($"Backup directory not found: {backupPath}");
                return result;
            }

            _logger.LogInformation("Restoring authentication configuration from backup: {BackupId}", backupId);
            await _migrationLogger.LogAsync(Guid.NewGuid(), Models.Migration.LogLevel.Information, $"Restoring authentication configuration from backup: {backupId}");

            var metadataPath = Path.Combine(backupPath, "backup_metadata.json");
            if (!File.Exists(metadataPath))
            {
                result.Errors.Add($"Backup metadata not found: {metadataPath}");
                return result;
            }

            var metadataJson = await File.ReadAllTextAsync(metadataPath);
            var metadata = JsonSerializer.Deserialize<JsonElement>(metadataJson);
            
            if (metadata.TryGetProperty("BackedUpFiles", out var backedUpFilesElement))
            {
                var backedUpFiles = backedUpFilesElement.EnumerateArray()
                    .Select(x => x.GetString())
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToList();

                foreach (var originalFilePath in backedUpFiles)
                {
                    if (string.IsNullOrEmpty(originalFilePath)) continue;
                    
                    var fileName = Path.GetFileName(originalFilePath);
                    var backupFilePath = Path.Combine(backupPath, fileName);
                    
                    if (File.Exists(backupFilePath))
                    {
                        File.Copy(backupFilePath, originalFilePath, overwrite: true);
                        result.RestoredFiles.Add(originalFilePath);
                        _logger.LogDebug("Restored file: {FilePath} from {BackupPath}", originalFilePath, backupFilePath);
                    }
                }
            }

            result.Success = true;
            _logger.LogInformation("Successfully restored authentication configuration from backup: {BackupId}", backupId);
            await _migrationLogger.LogAsync(Guid.NewGuid(), Models.Migration.LogLevel.Information, $"Successfully restored authentication configuration from backup: {backupId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring authentication configuration from backup: {BackupId}", backupId);
            await _migrationLogger.LogAsync(Guid.NewGuid(), Models.Migration.LogLevel.Error, $"Error restoring authentication configuration from backup {backupId}: {ex.Message}", ex);
            result.Errors.Add($"Error restoring authentication configuration: {ex.Message}");
        }

        return result;
    }

    private async Task CleanupJwtConfigFromFile(string filePath, DTOs.Migration.AuthRemovalResult result)
    {
        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Configuration file not found: {FilePath}", filePath);
            return;
        }

        try
        {
            var content = await File.ReadAllTextAsync(filePath);
            var jsonDocument = JsonDocument.Parse(content);
            var rootElement = jsonDocument.RootElement;

            var newConfig = new Dictionary<string, object>();

            foreach (var property in rootElement.EnumerateObject())
            {
                if (property.Name.Equals("Jwt", StringComparison.OrdinalIgnoreCase))
                {
                    result.RemovedComponents.Add($"JWT Configuration from {Path.GetFileName(filePath)}");
                    continue; // Skip JWT configuration
                }

                newConfig[property.Name] = JsonSerializer.Deserialize<object>(property.Value.GetRawText());
            }

            var newContent = JsonSerializer.Serialize(newConfig, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, newContent);
            
            result.ModifiedFiles.Add(filePath);
            _logger.LogInformation("Removed JWT configuration from {FilePath}", filePath);
            await _migrationLogger.LogAsync(Guid.NewGuid(), Models.Migration.LogLevel.Information, $"Removed JWT configuration from {Path.GetFileName(filePath)}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up JWT configuration from {FilePath}", filePath);
            result.Errors.Add($"Error cleaning up JWT configuration from {Path.GetFileName(filePath)}: {ex.Message}");
        }
    }
}