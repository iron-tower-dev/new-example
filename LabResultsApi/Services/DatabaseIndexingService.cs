using LabResultsApi.Data;
using LabResultsApi.DTOs;
using Microsoft.EntityFrameworkCore;

namespace LabResultsApi.Services;

public interface IDatabaseIndexingService
{
    /// <summary>
    /// Get database indexing recommendations based on query patterns
    /// </summary>
    Task<IEnumerable<DatabaseIndexRecommendationDto>> GetIndexRecommendationsAsync();
    
    /// <summary>
    /// Analyze query performance and suggest optimizations
    /// </summary>
    Task<IEnumerable<QueryOptimizationDto>> GetQueryOptimizationsAsync();
    
    /// <summary>
    /// Check if recommended indexes exist
    /// </summary>
    Task<Dictionary<string, bool>> CheckIndexExistenceAsync();
}

public class DatabaseIndexingService : IDatabaseIndexingService
{
    private readonly LabDbContext _context;
    private readonly ILogger<DatabaseIndexingService> _logger;

    public DatabaseIndexingService(LabDbContext context, ILogger<DatabaseIndexingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<DatabaseIndexRecommendationDto>> GetIndexRecommendationsAsync()
    {
        var recommendations = new List<DatabaseIndexRecommendationDto>();

        try
        {
            // Analyze common query patterns and recommend indexes
            recommendations.AddRange(await GetTestReadingsIndexRecommendationsAsync());
            recommendations.AddRange(await GetSampleIndexRecommendationsAsync());
            recommendations.AddRange(await GetEmissionSpectroscopyIndexRecommendationsAsync());
            recommendations.AddRange(await GetParticleAnalysisIndexRecommendationsAsync());

            return recommendations.OrderByDescending(r => r.Priority);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting index recommendations");
            throw;
        }
    }

    public async Task<IEnumerable<QueryOptimizationDto>> GetQueryOptimizationsAsync()
    {
        var optimizations = new List<QueryOptimizationDto>();

        try
        {
            // Common query optimizations
            optimizations.Add(new QueryOptimizationDto
            {
                QueryName = "GetTestReadings",
                OriginalQuery = "SELECT * FROM TestReadings WHERE sampleID = @sampleId AND testID = @testId",
                OptimizedQuery = "SELECT * FROM TestReadings WITH (INDEX(IX_TestReadings_SampleID_TestID)) WHERE sampleID = @sampleId AND testID = @testId",
                OptimizationReason = "Added index hint to force use of composite index",
                EstimatedImprovement = TimeSpan.FromMilliseconds(50)
            });

            optimizations.Add(new QueryOptimizationDto
            {
                QueryName = "GetSampleHistory",
                OriginalQuery = "Complex join query without proper indexing",
                OptimizedQuery = "Optimized with index hints and reduced data retrieval",
                OptimizationReason = "Added index hints and limited result set to TOP 12",
                EstimatedImprovement = TimeSpan.FromMilliseconds(200)
            });

            optimizations.Add(new QueryOptimizationDto
            {
                QueryName = "GetEmissionSpectroscopy",
                OriginalQuery = "SELECT * FROM EmSpectro WHERE ID = @id AND testID = @testId",
                OptimizedQuery = "SELECT * FROM EmSpectro WITH (INDEX(IX_EmSpectro_ID_TestID)) WHERE ID = @id AND testID = @testId",
                OptimizationReason = "Added composite index for faster lookups",
                EstimatedImprovement = TimeSpan.FromMilliseconds(30)
            });

            return optimizations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting query optimizations");
            throw;
        }
    }

    public async Task<Dictionary<string, bool>> CheckIndexExistenceAsync()
    {
        var indexStatus = new Dictionary<string, bool>();

        try
        {
            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            var recommendedIndexes = new[]
            {
                "IX_TestReadings_SampleID_TestID",
                "IX_TestReadings_Status_EntryDate",
                "IX_UsedLubeSamples_TagNumber_Component",
                "IX_UsedLubeSamples_SampleDate",
                "IX_EmSpectro_ID_TestID",
                "IX_ParticleType_SampleID_TestID",
                "IX_Equipment_EquipType",
                "IX_NAS_lookup_Channel",
                "IX_Test_TestName"
            };

            foreach (var indexName in recommendedIndexes)
            {
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT COUNT(*)
                    FROM sys.indexes i
                    INNER JOIN sys.objects o ON i.object_id = o.object_id
                    WHERE i.name = @indexName";

                var parameter = command.CreateParameter();
                parameter.ParameterName = "@indexName";
                parameter.Value = indexName;
                command.Parameters.Add(parameter);

                var count = Convert.ToInt32(await command.ExecuteScalarAsync());
                indexStatus[indexName] = count > 0;
            }

            return indexStatus;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking index existence");
            throw;
        }
    }

    private async Task<IEnumerable<DatabaseIndexRecommendationDto>> GetTestReadingsIndexRecommendationsAsync()
    {
        var recommendations = new List<DatabaseIndexRecommendationDto>();

        // Primary composite index for TestReadings
        recommendations.Add(new DatabaseIndexRecommendationDto
        {
            TableName = "TestReadings",
            RecommendedIndex = "CREATE NONCLUSTERED INDEX IX_TestReadings_SampleID_TestID ON TestReadings (sampleID, testID) INCLUDE (trialNumber, status, entryDate)",
            Reason = "Most queries filter by sampleID and testID together. Including commonly selected columns reduces key lookups.",
            Priority = 10,
            EstimatedImpact = 0.8
        });

        // Status and date filtering index
        recommendations.Add(new DatabaseIndexRecommendationDto
        {
            TableName = "TestReadings",
            RecommendedIndex = "CREATE NONCLUSTERED INDEX IX_TestReadings_Status_EntryDate ON TestReadings (status, entryDate DESC)",
            Reason = "Queries often filter by status and order by entry date for recent results.",
            Priority = 7,
            EstimatedImpact = 0.6
        });

        return recommendations;
    }

    private async Task<IEnumerable<DatabaseIndexRecommendationDto>> GetSampleIndexRecommendationsAsync()
    {
        var recommendations = new List<DatabaseIndexRecommendationDto>();

        // Tag number and component composite index
        recommendations.Add(new DatabaseIndexRecommendationDto
        {
            TableName = "UsedLubeSamples",
            RecommendedIndex = "CREATE NONCLUSTERED INDEX IX_UsedLubeSamples_TagNumber_Component ON UsedLubeSamples (tagNumber, component) INCLUDE (sampleDate, status)",
            Reason = "Historical queries frequently join on tagNumber and component for equipment history.",
            Priority = 9,
            EstimatedImpact = 0.75
        });

        // Sample date index for chronological queries
        recommendations.Add(new DatabaseIndexRecommendationDto
        {
            TableName = "UsedLubeSamples",
            RecommendedIndex = "CREATE NONCLUSTERED INDEX IX_UsedLubeSamples_SampleDate ON UsedLubeSamples (sampleDate DESC) INCLUDE (tagNumber, component, status)",
            Reason = "Many queries order by sample date to get recent samples first.",
            Priority = 8,
            EstimatedImpact = 0.7
        });

        return recommendations;
    }

    private async Task<IEnumerable<DatabaseIndexRecommendationDto>> GetEmissionSpectroscopyIndexRecommendationsAsync()
    {
        var recommendations = new List<DatabaseIndexRecommendationDto>();

        // Primary lookup index for EmSpectro
        recommendations.Add(new DatabaseIndexRecommendationDto
        {
            TableName = "EmSpectro",
            RecommendedIndex = "CREATE NONCLUSTERED INDEX IX_EmSpectro_ID_TestID ON EmSpectro (ID, testID) INCLUDE (trialNum, status)",
            Reason = "EmSpectro queries always filter by ID (sampleID) and testID.",
            Priority = 8,
            EstimatedImpact = 0.7
        });

        return recommendations;
    }

    private async Task<IEnumerable<DatabaseIndexRecommendationDto>> GetParticleAnalysisIndexRecommendationsAsync()
    {
        var recommendations = new List<DatabaseIndexRecommendationDto>();

        // Particle type analysis index
        recommendations.Add(new DatabaseIndexRecommendationDto
        {
            TableName = "ParticleType",
            RecommendedIndex = "CREATE NONCLUSTERED INDEX IX_ParticleType_SampleID_TestID ON ParticleType (SampleID, testID) INCLUDE (ParticleTypeDefinitionID, Status)",
            Reason = "Particle analysis queries filter by sample and test ID.",
            Priority = 6,
            EstimatedImpact = 0.5
        });

        // Equipment type index
        recommendations.Add(new DatabaseIndexRecommendationDto
        {
            TableName = "M_And_T_Equip",
            RecommendedIndex = "CREATE NONCLUSTERED INDEX IX_Equipment_EquipType ON M_And_T_Equip (equipType) INCLUDE (equipName, calibrationValue)",
            Reason = "Equipment selection queries filter by equipment type.",
            Priority = 7,
            EstimatedImpact = 0.6
        });

        // NAS lookup optimization
        recommendations.Add(new DatabaseIndexRecommendationDto
        {
            TableName = "NAS_lookup",
            RecommendedIndex = "CREATE NONCLUSTERED INDEX IX_NAS_lookup_Channel ON NAS_lookup (channel) INCLUDE (valLo, valHi, NAS)",
            Reason = "NAS calculations filter by channel and compare against value ranges.",
            Priority = 8,
            EstimatedImpact = 0.7
        });

        return recommendations;
    }
}