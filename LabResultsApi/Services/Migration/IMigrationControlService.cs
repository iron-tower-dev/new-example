using LabResultsApi.Models.Migration;

namespace LabResultsApi.Services.Migration;

public interface IMigrationControlService
{
    Task<MigrationResult> ExecuteFullMigrationAsync(MigrationOptions options, CancellationToken cancellationToken = default);
    Task<MigrationStatus> GetMigrationStatusAsync();
    Task<MigrationResult?> GetCurrentMigrationAsync();
    Task<bool> CancelMigrationAsync();
    Task<MigrationResult?> GetMigrationResultAsync(Guid migrationId);
    Task<List<MigrationResult>> GetMigrationHistoryAsync(int limit = 10);
    Task<bool> IsMigrationRunningAsync();
    
    // Progress and status events
    event EventHandler<MigrationProgressEventArgs>? ProgressUpdated;
    event EventHandler<MigrationStatusEventArgs>? StatusChanged;
    event EventHandler<MigrationErrorEventArgs>? ErrorOccurred;
}

public class MigrationProgressEventArgs : EventArgs
{
    public Guid MigrationId { get; set; }
    public double ProgressPercentage { get; set; }
    public string CurrentOperation { get; set; } = string.Empty;
    public MigrationStatistics Statistics { get; set; } = new();
    public TimeSpan EstimatedTimeRemaining { get; set; }
}

public class MigrationStatusEventArgs : EventArgs
{
    public Guid MigrationId { get; set; }
    public MigrationStatus OldStatus { get; set; }
    public MigrationStatus NewStatus { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Message { get; set; }
}

public class MigrationErrorEventArgs : EventArgs
{
    public Guid MigrationId { get; set; }
    public MigrationError Error { get; set; } = new();
    public bool IsCritical { get; set; }
    public bool ShouldAbort { get; set; }
}