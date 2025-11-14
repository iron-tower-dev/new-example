using LabResultsApi.Models.Migration;

namespace LabResultsApi.Services.Migration;

public class SqlValidationService : ISqlValidationServiceImpl
{
    private readonly IQueryComparisonService _queryComparisonService;
    private readonly ILegacyQueryExecutorService _legacyQueryExecutorService;
    private readonly IValidationReportingService _reportingService;
    private readonly ILogger<SqlValidationService> _logger;

    public SqlValidationService(
        IQueryComparisonService queryComparisonService,
        ILegacyQueryExecutorService legacyQueryExecutorService,
        IValidationReportingService reportingService,
        ILogger<SqlValidationService> logger)
    {
        _queryComparisonService = queryComparisonService;
        _legacyQueryExecutorService = legacyQueryExecutorService;
        _reportingService = reportingService;
        _logger = logger;
    }

    public async Task<ValidationResult> ValidateAllQueriesAsync(ValidationOptions options)
    {
        try
        {
            _logger.LogInformation("Starting validation of all queries with options: CompareResults={CompareResults}, ComparePerformance={ComparePerformance}", 
                options.CompareQueryResults, options.ComparePerformance);

            // Set legacy connection string
            if (!string.IsNullOrEmpty(options.LegacyConnectionString))
            {
                _legacyQueryExecutorService.SetConnectionString(options.LegacyConnectionString);
            }

            // Test legacy connection
            var connectionTest = await TestLegacyConnectionAsync();
            if (!connectionTest)
            {
                throw new InvalidOperationException("Cannot connect to legacy database. Please check the connection string.");
            }

            // Get available queries
            var availableQueries = await GetAvailableQueriesAsync();
            
            // Filter queries based on include/exclude lists
            var queriesToValidate = FilterQueries(availableQueries, options);
            
            // Build query pairs
            var queryPairs = await BuildQueryPairsAsync(queriesToValidate);
            
            // Validate queries
            var result = await ValidateQueriesAsync(queryPairs, options);
            
            _logger.LogInformation("Validation completed. Validated: {Validated}, Matched: {Matched}, Failed: {Failed}", 
                result.QueriesValidated, result.QueriesMatched, result.QueriesFailed);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during query validation");
            throw;
        }
    }

    public async Task<QueryComparisonResult> CompareQueryAsync(string queryName, string currentQuery, string legacyQuery, Dictionary<string, object>? parameters = null)
    {
        try
        {
            _logger.LogInformation("Comparing query: {QueryName}", queryName);
            
            var result = await _queryComparisonService.CompareQueriesAsync(queryName, currentQuery, legacyQuery, parameters);
            
            _logger.LogInformation("Query comparison completed for {QueryName}. Matches: {DataMatches}", queryName, result.DataMatches);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error comparing query {QueryName}", queryName);
            throw;
        }
    }

    public async Task<PerformanceComparisonResult> ComparePerformanceAsync(string queryName, string currentQuery, string legacyQuery, Dictionary<string, object>? parameters = null)
    {
        try
        {
            _logger.LogInformation("Comparing performance for query: {QueryName}", queryName);
            
            var result = await _queryComparisonService.ComparePerformanceAsync(queryName, currentQuery, legacyQuery, parameters);
            
            _logger.LogInformation("Performance comparison completed for {QueryName}. Current: {CurrentTime}ms, Legacy: {LegacyTime}ms", 
                queryName, result.CurrentExecutionTime.TotalMilliseconds, result.LegacyExecutionTime.TotalMilliseconds);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error comparing performance for query {QueryName}", queryName);
            throw;
        }
    }

    public async Task<ValidationResult> ValidateQueriesAsync(Dictionary<string, QueryPair> queries, ValidationOptions options)
    {
        try
        {
            _logger.LogInformation("Validating {QueryCount} queries", queries.Count);
            
            var result = await _queryComparisonService.ValidateQueriesAsync(queries);
            
            // Apply options-based filtering
            if (options.MaxDiscrepanciesToReport > 0)
            {
                foreach (var queryResult in result.Results)
                {
                    if (queryResult.Discrepancies.Count > options.MaxDiscrepanciesToReport)
                    {
                        queryResult.Discrepancies = queryResult.Discrepancies
                            .Take(options.MaxDiscrepanciesToReport)
                            .ToList();
                    }
                }
            }
            
            // Filter minor differences if requested
            if (options.IgnoreMinorDifferences)
            {
                FilterMinorDifferences(result);
            }
            
            _logger.LogInformation("Query validation completed");
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during query validation");
            throw;
        }
    }

    public async Task<bool> TestLegacyConnectionAsync()
    {
        try
        {
            _logger.LogInformation("Testing legacy database connection");
            
            var result = await _legacyQueryExecutorService.TestConnectionAsync();
            
            _logger.LogInformation("Legacy database connection test result: {Result}", result);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing legacy database connection");
            return false;
        }
    }

    public void SetLegacyConnectionString(string connectionString)
    {
        try
        {
            _logger.LogInformation("Setting legacy database connection string");
            
            _legacyQueryExecutorService.SetConnectionString(connectionString);
            
            _logger.LogInformation("Legacy database connection string set successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting legacy database connection string");
            throw;
        }
    }

    public async Task<List<string>> GetAvailableQueriesAsync()
    {
        try
        {
            _logger.LogInformation("Getting available queries for validation");
            
            // This is a simplified implementation. In practice, you would:
            // 1. Read from a configuration file or database
            // 2. Scan stored procedures or views
            // 3. Load from a query repository
            
            var queries = new List<string>
            {
                "GetTestReadings",
                "GetEmissionSpectroscopy",
                "GetSampleHistory",
                "GetParticleTypes",
                "GetParticleSubTypes",
                "GetInspectFilter",
                "GetSamplesByDateRange",
                "GetTestsByEquipment",
                "GetLubeTechQualifications",
                "GetActiveTests"
            };
            
            _logger.LogInformation("Found {QueryCount} available queries", queries.Count);
            
            return queries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available queries");
            throw;
        }
    }

    private List<string> FilterQueries(List<string> availableQueries, ValidationOptions options)
    {
        var filtered = availableQueries.AsEnumerable();
        
        // Apply include filter
        if (options.IncludeQueries.Any())
        {
            filtered = filtered.Where(q => options.IncludeQueries.Contains(q, StringComparer.OrdinalIgnoreCase));
        }
        
        // Apply exclude filter
        if (options.ExcludeQueries.Any())
        {
            filtered = filtered.Where(q => !options.ExcludeQueries.Contains(q, StringComparer.OrdinalIgnoreCase));
        }
        
        var result = filtered.ToList();
        
        _logger.LogInformation("Filtered queries: {FilteredCount} out of {TotalCount}", result.Count, availableQueries.Count);
        
        return result;
    }

    private async Task<Dictionary<string, QueryPair>> BuildQueryPairsAsync(List<string> queryNames)
    {
        var queryPairs = new Dictionary<string, QueryPair>();
        
        foreach (var queryName in queryNames)
        {
            try
            {
                var queryPair = await BuildQueryPairAsync(queryName);
                if (queryPair != null)
                {
                    queryPairs[queryName] = queryPair;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not build query pair for {QueryName}", queryName);
            }
        }
        
        _logger.LogInformation("Built {QueryPairCount} query pairs", queryPairs.Count);
        
        return queryPairs;
    }

    private async Task<QueryPair?> BuildQueryPairAsync(string queryName)
    {
        // This is a simplified implementation. In practice, you would:
        // 1. Load current query from your API/service layer
        // 2. Load legacy query from VB.NET application or stored procedures
        // 3. Map parameters between the two systems
        
        var currentQuery = GetCurrentQueryByName(queryName);
        var legacyQuery = GetLegacyQueryByName(queryName);
        
        if (string.IsNullOrEmpty(currentQuery) || string.IsNullOrEmpty(legacyQuery))
        {
            _logger.LogWarning("Could not find current or legacy query for {QueryName}", queryName);
            return null;
        }
        
        return new QueryPair
        {
            CurrentQuery = currentQuery,
            LegacyQuery = legacyQuery,
            Parameters = GetQueryParameters(queryName),
            KeyFields = GetKeyFields(queryName),
            IgnoreMinorDifferences = true
        };
    }

    private string GetCurrentQueryByName(string queryName)
    {
        // This would typically load from your current API service layer
        // For now, return sample queries based on the existing RawSqlService
        return queryName switch
        {
            "GetTestReadings" => @"
                SELECT sampleID, testID, trialNumber, value1, value2, value3, 
                       trialCalc, ID1, ID2, ID3, trialComplete, status, 
                       schedType, entryID, validateID, entryDate, valiDate, MainComments
                FROM TestReadings 
                WHERE sampleID = @sampleId AND testID = @testId 
                ORDER BY trialNumber",
            
            "GetEmissionSpectroscopy" => @"
                SELECT ID, testID, trialNum, Na, Cr, Sn, Si, Mo, Ca, Al, Ba, Mg, 
                       Ni, Mn, Zn, P, Ag, Pb, H, B, Cu, Fe, trialDate, status
                FROM EmSpectro 
                WHERE ID = @sampleId AND testID = @testId 
                ORDER BY trialNum",
            
            "GetParticleTypes" => @"
                SELECT SampleID, testID, ParticleTypeDefinitionID, Status, Comments
                FROM ParticleType 
                WHERE SampleID = @sampleId AND testID = @testId",
            
            _ => string.Empty
        };
    }

    private string GetLegacyQueryByName(string queryName)
    {
        // This would typically load from VB.NET application or legacy stored procedures
        // For now, return the same queries (assuming they're identical for testing)
        return GetCurrentQueryByName(queryName);
    }

    private Dictionary<string, object>? GetQueryParameters(string queryName)
    {
        // Return sample parameters for testing
        return queryName switch
        {
            "GetTestReadings" or "GetEmissionSpectroscopy" or "GetParticleTypes" => 
                new Dictionary<string, object> { { "sampleId", 1 }, { "testId", 100 } },
            _ => null
        };
    }

    private List<string> GetKeyFields(string queryName)
    {
        // Return key fields for row identification
        return queryName switch
        {
            "GetTestReadings" => new List<string> { "sampleID", "testID", "trialNumber" },
            "GetEmissionSpectroscopy" => new List<string> { "ID", "testID", "trialNum" },
            "GetParticleTypes" => new List<string> { "SampleID", "testID", "ParticleTypeDefinitionID" },
            _ => new List<string> { "ID" }
        };
    }

    private void FilterMinorDifferences(ValidationResult result)
    {
        foreach (var queryResult in result.Results)
        {
            queryResult.Discrepancies = queryResult.Discrepancies
                .Where(d => !IsMinorDifference(d))
                .ToList();
        }
        
        // Recalculate summary statistics
        result.Summary.TotalDiscrepancies = result.Results.Sum(r => r.Discrepancies.Count);
    }

    private bool IsMinorDifference(DataDiscrepancy discrepancy)
    {
        // Define what constitutes a "minor" difference
        return discrepancy.Type switch
        {
            DiscrepancyType.FormatDifference => true,
            DiscrepancyType.ValueMismatch when IsMinorValueDifference(discrepancy) => true,
            _ => false
        };
    }

    private bool IsMinorValueDifference(DataDiscrepancy discrepancy)
    {
        // Check for minor numeric differences, whitespace differences, etc.
        if (discrepancy.CurrentValue is decimal currentDecimal && 
            discrepancy.LegacyValue is decimal legacyDecimal)
        {
            return Math.Abs(currentDecimal - legacyDecimal) < 0.01m;
        }
        
        if (discrepancy.CurrentValue is string currentString && 
            discrepancy.LegacyValue is string legacyString)
        {
            return string.Equals(currentString?.Trim(), legacyString?.Trim(), StringComparison.OrdinalIgnoreCase);
        }
        
        return false;
    }
}