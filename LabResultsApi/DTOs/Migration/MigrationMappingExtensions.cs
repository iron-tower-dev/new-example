using LabResultsApi.Models.Migration;

namespace LabResultsApi.DTOs.Migration;

public static class MigrationMappingExtensions
{
    public static MigrationStatusDto ToDto(this MigrationResult result)
    {
        return new MigrationStatusDto
        {
            MigrationId = result.MigrationId,
            Status = result.Status.ToString(),
            StartTime = result.StartTime,
            EndTime = result.EndTime,
            ProgressPercentage = result.Statistics.ProgressPercentage,
            Duration = result.EndTime.HasValue ? result.EndTime.Value - result.StartTime : null,
            Statistics = result.Statistics.ToDto(),
            RecentErrors = result.Errors.TakeLast(5).Select(e => e.ToDto()).ToList(),
            CurrentOperation = result.CurrentOperation,
            EstimatedTimeRemaining = result.Statistics.EstimatedTimeRemaining
        };
    }

    public static MigrationStatisticsDto ToDto(this MigrationStatistics stats)
    {
        return new MigrationStatisticsDto
        {
            TotalTables = stats.TotalTables,
            TablesProcessed = stats.TablesProcessed,
            TotalRecords = stats.TotalRecords,
            RecordsProcessed = stats.RecordsProcessed,
            ErrorCount = stats.ErrorCount,
            ProgressPercentage = stats.ProgressPercentage
        };
    }

    public static MigrationErrorDto ToDto(this MigrationError error)
    {
        return new MigrationErrorDto
        {
            Timestamp = error.Timestamp,
            Level = error.Level.ToString(),
            Component = error.Component,
            Message = error.Message,
            Details = error.Details,
            TableName = error.TableName,
            RecordNumber = error.RecordNumber
        };
    }

    public static MigrationReportDto ToReportDto(this MigrationResult result)
    {
        return new MigrationReportDto
        {
            MigrationId = result.MigrationId,
            GeneratedAt = DateTime.UtcNow,
            Status = result.Status.ToString(),
            Duration = result.EndTime.HasValue ? result.EndTime.Value - result.StartTime : TimeSpan.Zero,
            Summary = result.Statistics.ToSummaryDto(result.Status == MigrationStatus.Completed),
            Errors = result.Errors.Select(e => e.ToDto()).ToList(),
            Recommendations = new List<string>()
        };
    }

    public static MigrationSummaryDto ToSummaryDto(this MigrationStatistics stats, bool success)
    {
        return new MigrationSummaryDto
        {
            TotalTables = stats.TotalTables,
            TablesProcessed = stats.TablesProcessed,
            TotalRecords = stats.TotalRecords,
            RecordsProcessed = stats.RecordsProcessed,
            ErrorCount = stats.ErrorCount,
            Success = success,
            OverallProgressPercentage = stats.ProgressPercentage
        };
    }

    public static SeedingReportDto ToDto(this SeedingResult result)
    {
        return new SeedingReportDto
        {
            TablesProcessed = result.TablesProcessed,
            TablesCreated = result.TablesCreated,
            RecordsInserted = result.RecordsInserted,
            RecordsSkipped = result.RecordsSkipped,
            Duration = result.Duration,
            Success = result.Success,
            TableReports = result.TableResults.Select(t => t.ToDto()).ToList()
        };
    }

    public static TableSeedingReportDto ToDto(this TableSeedingResult result)
    {
        return new TableSeedingReportDto
        {
            TableName = result.TableName,
            Success = result.Success,
            RecordsProcessed = result.RecordsProcessed,
            RecordsInserted = result.RecordsInserted,
            RecordsSkipped = result.RecordsSkipped,
            Duration = result.Duration,
            Errors = result.Errors,
            TableCreated = result.TableCreated
        };
    }

    public static ValidationReportDto ToDto(this LabResultsApi.Models.Migration.ValidationResult result)
    {
        return new ValidationReportDto
        {
            QueriesValidated = result.QueriesValidated,
            QueriesMatched = result.QueriesMatched,
            QueriesFailed = result.QueriesFailed,
            MatchPercentage = result.Summary.MatchPercentage,
            Duration = result.Duration,
            Success = result.Success,
            QueryReports = result.Results.Select(r => r.ToDto()).ToList(),
            Summary = result.Summary.ToDto()
        };
    }

    public static QueryComparisonReportDto ToDto(this QueryComparisonResult result)
    {
        return new QueryComparisonReportDto
        {
            QueryName = result.QueryName,
            DataMatches = result.DataMatches,
            CurrentRowCount = result.CurrentRowCount,
            LegacyRowCount = result.LegacyRowCount,
            DiscrepancyCount = result.Discrepancies.Count,
            CurrentExecutionTime = result.CurrentExecutionTime,
            LegacyExecutionTime = result.LegacyExecutionTime,
            PerformanceRatio = result.LegacyExecutionTime.TotalMilliseconds > 0
                ? result.CurrentExecutionTime.TotalMilliseconds / result.LegacyExecutionTime.TotalMilliseconds
                : 0,
            Error = result.Error
        };
    }

    public static ValidationSummaryDto ToDto(this ValidationSummary summary)
    {
        return new ValidationSummaryDto
        {
            MatchPercentage = summary.MatchPercentage,
            TotalDiscrepancies = summary.TotalDiscrepancies,
            AverageCurrentExecutionTime = summary.AverageCurrentExecutionTime,
            AverageLegacyExecutionTime = summary.AverageLegacyExecutionTime,
            CriticalIssues = summary.CriticalIssues
        };
    }

    public static AuthRemovalReportDto ToDto(this LabResultsApi.Models.Migration.AuthRemovalResult result)
    {
        return new AuthRemovalReportDto
        {
            Success = result.Success,
            RemovedComponentsCount = result.RemovedComponents.Count,
            ModifiedFilesCount = result.ModifiedFiles.Count,
            BackupFilesCount = result.BackupFiles.Count,
            BackupLocation = result.BackupLocation,
            Duration = result.Duration,
            Errors = result.Errors
        };
    }

    public static MigrationOptions ToModel(this MigrationOptionsDto dto)
    {
        return new MigrationOptions
        {
            ClearExistingData = dto.ClearExistingData,
            CreateMissingTables = dto.CreateMissingTables,
            ValidateAgainstLegacy = dto.ValidateAgainstLegacy,
            RemoveAuthentication = dto.RemoveAuthentication,
            IncludeTables = dto.IncludeTables,
            ExcludeTables = dto.ExcludeTables,
            SeedingOptions = dto.SeedingOptions.ToModel(),
            ValidationOptions = dto.ValidationOptions.ToModel(),
            AuthRemovalOptions = dto.AuthRemovalOptions.ToModel(),
            MaxConcurrentOperations = dto.MaxConcurrentOperations,
            OperationTimeout = TimeSpan.FromMinutes(dto.OperationTimeoutMinutes)
        };
    }

    public static SeedingOptions ToModel(this SeedingOptionsDto dto)
    {
        return new SeedingOptions
        {
            ClearExistingData = dto.ClearExistingData,
            CreateMissingTables = dto.CreateMissingTables,
            BatchSize = dto.BatchSize,
            ContinueOnError = dto.ContinueOnError,
            ValidateBeforeInsert = dto.ValidateBeforeInsert,
            UseTransactions = dto.UseTransactions,
            CommandTimeout = TimeSpan.FromMinutes(dto.CommandTimeoutMinutes)
        };
    }

    public static ValidationOptions ToModel(this ValidationOptionsDto dto)
    {
        return new ValidationOptions
        {
            CompareQueryResults = dto.CompareQueryResults,
            ComparePerformance = dto.ComparePerformance,
            GenerateDetailedReports = dto.GenerateDetailedReports,
            MaxDiscrepanciesToReport = dto.MaxDiscrepanciesToReport,
            PerformanceThresholdPercent = dto.PerformanceThresholdPercent,
            QueryTimeout = TimeSpan.FromMinutes(dto.QueryTimeoutMinutes),
            LegacyConnectionString = dto.LegacyConnectionString,
            IgnoreMinorDifferences = dto.IgnoreMinorDifferences
        };
    }

    public static AuthRemovalOptions ToModel(this AuthRemovalOptionsDto dto)
    {
        return new AuthRemovalOptions
        {
            CreateBackup = dto.CreateBackup,
            BackupDirectory = dto.BackupDirectory,
            RemoveFromApi = dto.RemoveFromApi,
            RemoveFromFrontend = dto.RemoveFromFrontend,
            UpdateDocumentation = dto.UpdateDocumentation,
            FilesToExclude = dto.FilesToExclude
        };
    }

    public static LabResultsApi.DTOs.Migration.BackupResult ToDto(this LabResultsApi.Services.BackupResult result)
    {
        return new LabResultsApi.DTOs.Migration.BackupResult
        {
            Success = result.Success,
            BackupId = result.BackupId.ToString(),
            BackupLocation = result.BackupLocation,
            BackedUpFiles = result.BackedUpFiles,
            BackupSizeBytes = result.BackupSizeBytes,
            Errors = result.Errors,
            Timestamp = result.BackupTimestamp
        };
    }
}
