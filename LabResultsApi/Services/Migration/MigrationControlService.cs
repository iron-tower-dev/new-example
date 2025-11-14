using LabResultsApi.Models.Migration;
using LabResultsApi.Services.Migration;
using System.Collections.Concurrent;

namespace LabResultsApi.Services.Migration;

public class MigrationControlService : IMigrationControlService, IDisposable
{
    private readonly ILogger<MigrationControlService> _logger;
    private readonly ISqlValidationService _validationService;
    private readonly ISsoMigrationService _ssoMigrationService;
    private readonly ConcurrentDictionary<Guid, MigrationResult> _migrationHistory;
    private readonly SemaphoreSlim _migrationSemaphore;
    private MigrationResult? _currentMigration;
    private CancellationTokenSource? _cancellationTokenSource;

    public event EventHandler<MigrationProgressEventArgs>? ProgressUpdated;
    public event EventHandler<MigrationStatusEventArgs>? StatusChanged;
    public event EventHandler<MigrationErrorEventArgs>? ErrorOccurred;

    public MigrationControlService(
        ILogger<MigrationControlService> logger,
        ISqlValidationService validationService,
        ISsoMigrationService ssoMigrationService)
    {
        _logger = logger;
        _validationService = validationService;
        _ssoMigrationService = ssoMigrationService;
        _migrationHistory = new ConcurrentDictionary<Guid, MigrationResult>();
        _migrationSemaphore = new SemaphoreSlim(1, 1);
    }

    public async Task<MigrationResult> ExecuteFullMigrationAsync(MigrationOptions options, CancellationToken cancellationToken = default)
    {
        await _migrationSemaphore.WaitAsync(cancellationToken);
        
        try
        {
            if (_currentMigration?.IsCompleted == false)
            {
                throw new InvalidOperationException("A migration is already in progress");
            }

            var migrationId = Guid.NewGuid();
            _currentMigration = new MigrationResult
            {
                MigrationId = migrationId,
                StartTime = DateTime.UtcNow,
                Status = MigrationStatus.InProgress
            };

            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var combinedToken = _cancellationTokenSource.Token;

            _logger.LogInformation("Starting migration {MigrationId} with options: {@Options}", migrationId, options);
            
            await UpdateStatusAsync(MigrationStatus.InProgress, "Migration started");

            try
            {
                // Phase 1: SQL Validation (if enabled)
                if (options.ValidateAgainstLegacy && !combinedToken.IsCancellationRequested)
                {
                    await UpdateProgressAsync(0, "Starting SQL validation");
                    _currentMigration.ValidationResult = await ExecuteValidationPhaseAsync(options.ValidationOptions, combinedToken);
                    await UpdateProgressAsync(50, "SQL validation completed");
                }

                // Phase 2: SSO Migration (if enabled)
                if (options.RemoveAuthentication && !combinedToken.IsCancellationRequested)
                {
                    await UpdateProgressAsync(50, "Starting authentication removal");
                    _currentMigration.AuthRemovalResult = await ExecuteAuthRemovalPhaseAsync(options.AuthRemovalOptions, combinedToken);
                    await UpdateProgressAsync(100, "Authentication removal completed");
                }

                if (combinedToken.IsCancellationRequested)
                {
                    await UpdateStatusAsync(MigrationStatus.Cancelled, "Migration was cancelled");
                }
                else
                {
                    await UpdateStatusAsync(MigrationStatus.Completed, "Migration completed successfully");
                    await UpdateProgressAsync(100, "Migration completed");
                }
            }
            catch (OperationCanceledException)
            {
                await UpdateStatusAsync(MigrationStatus.Cancelled, "Migration was cancelled");
                _logger.LogWarning("Migration {MigrationId} was cancelled", migrationId);
            }
            catch (Exception ex)
            {
                await HandleCriticalErrorAsync(ex, "Migration failed with critical error");
                await UpdateStatusAsync(MigrationStatus.Failed, $"Migration failed: {ex.Message}");
            }

            _currentMigration.EndTime = DateTime.UtcNow;
            _migrationHistory.TryAdd(migrationId, _currentMigration);

            _logger.LogInformation("Migration {MigrationId} completed with status {Status}", 
                migrationId, _currentMigration.Status);

            return _currentMigration;
        }
        finally
        {
            _migrationSemaphore.Release();
        }
    }

    public async Task<MigrationStatus> GetMigrationStatusAsync()
    {
        await Task.CompletedTask;
        return _currentMigration?.Status ?? MigrationStatus.NotStarted;
    }

    public async Task<MigrationResult?> GetCurrentMigrationAsync()
    {
        await Task.CompletedTask;
        return _currentMigration;
    }

    public async Task<bool> CancelMigrationAsync()
    {
        await Task.CompletedTask;
        
        if (_currentMigration?.IsCompleted != false)
        {
            return false;
        }

        _cancellationTokenSource?.Cancel();
        _logger.LogInformation("Migration cancellation requested for {MigrationId}", _currentMigration.MigrationId);
        
        return true;
    }

    public async Task<MigrationResult?> GetMigrationResultAsync(Guid migrationId)
    {
        await Task.CompletedTask;
        _migrationHistory.TryGetValue(migrationId, out var result);
        return result;
    }

    public async Task<List<MigrationResult>> GetMigrationHistoryAsync(int limit = 10)
    {
        await Task.CompletedTask;
        return _migrationHistory.Values
            .OrderByDescending(m => m.StartTime)
            .Take(limit)
            .ToList();
    }

    public async Task<bool> IsMigrationRunningAsync()
    {
        await Task.CompletedTask;
        return _currentMigration?.IsCompleted == false;
    }



    private async Task<Models.Migration.ValidationResult> ExecuteValidationPhaseAsync(Models.Migration.ValidationOptions options, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting SQL validation phase");
            
            // This will be implemented when the validation service is available
            // For now, return a placeholder result
            await Task.Delay(100, cancellationToken); // Simulate work
            
            return new Models.Migration.ValidationResult
            {
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow,
                Duration = TimeSpan.FromMilliseconds(100),
                QueriesValidated = 0,
                QueriesMatched = 0,
                QueriesFailed = 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SQL validation phase failed");
            throw;
        }
    }

    private async Task<AuthRemovalResult> ExecuteAuthRemovalPhaseAsync(Models.Migration.AuthRemovalOptions options, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting authentication removal phase");
            
            // This will be implemented when the SSO migration service is available
            // For now, return a placeholder result
            await Task.Delay(100, cancellationToken); // Simulate work
            
            return new AuthRemovalResult
            {
                Success = true,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow,
                Duration = TimeSpan.FromMilliseconds(100),
                BackupLocation = options.BackupDirectory
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication removal phase failed");
            throw;
        }
    }

    private async Task UpdateStatusAsync(MigrationStatus newStatus, string? message = null)
    {
        if (_currentMigration == null) return;

        var oldStatus = _currentMigration.Status;
        _currentMigration.Status = newStatus;

        var eventArgs = new MigrationStatusEventArgs
        {
            MigrationId = _currentMigration.MigrationId,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            Timestamp = DateTime.UtcNow,
            Message = message
        };

        StatusChanged?.Invoke(this, eventArgs);
        
        _logger.LogInformation("Migration {MigrationId} status changed from {OldStatus} to {NewStatus}: {Message}",
            _currentMigration.MigrationId, oldStatus, newStatus, message);

        await Task.CompletedTask;
    }

    private async Task UpdateProgressAsync(double progressPercentage, string currentOperation)
    {
        if (_currentMigration == null) return;

        // Update progress percentage through the statistics object
        _currentMigration.Statistics.TablesProcessed = (int)(progressPercentage / 100.0 * _currentMigration.Statistics.TotalTables);

        var eventArgs = new MigrationProgressEventArgs
        {
            MigrationId = _currentMigration.MigrationId,
            ProgressPercentage = progressPercentage,
            CurrentOperation = currentOperation,
            Statistics = _currentMigration.Statistics,
            EstimatedTimeRemaining = _currentMigration.Statistics.EstimatedTimeRemaining
        };

        ProgressUpdated?.Invoke(this, eventArgs);
        
        _logger.LogDebug("Migration {MigrationId} progress: {Progress}% - {Operation}",
            _currentMigration.MigrationId, progressPercentage, currentOperation);

        await Task.CompletedTask;
    }

    private async Task HandleCriticalErrorAsync(Exception exception, string message)
    {
        if (_currentMigration == null) return;

        var error = new MigrationError
        {
            Timestamp = DateTime.UtcNow,
            Level = ErrorLevel.Critical,
            Component = "MigrationControl",
            Message = message,
            Details = exception.Message,
            StackTrace = exception.StackTrace
        };

        _currentMigration.Errors.Add(error);
        _currentMigration.Statistics.ErrorCount++;

        var eventArgs = new MigrationErrorEventArgs
        {
            MigrationId = _currentMigration.MigrationId,
            Error = error,
            IsCritical = true,
            ShouldAbort = true
        };

        ErrorOccurred?.Invoke(this, eventArgs);
        
        _logger.LogCritical(exception, "Critical error in migration {MigrationId}: {Message}",
            _currentMigration.MigrationId, message);

        await Task.CompletedTask;
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Dispose();
        _migrationSemaphore?.Dispose();
    }
}

// Placeholder interfaces that will be implemented in later tasks
public interface ISqlValidationService
{
    // Will be implemented in task 3
}