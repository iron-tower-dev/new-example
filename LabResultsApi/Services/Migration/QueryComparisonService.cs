using System.Diagnostics;
using System.Globalization;
using LabResultsApi.Models.Migration;
using LabResultsApi.Services;

namespace LabResultsApi.Services.Migration;

public class QueryComparisonService : IQueryComparisonService
{
    private readonly IRawSqlService _currentDbService;
    private readonly ILegacyQueryExecutorService _legacyDbService;
    private readonly ILogger<QueryComparisonService> _logger;

    public QueryComparisonService(
        IRawSqlService currentDbService,
        ILegacyQueryExecutorService legacyDbService,
        ILogger<QueryComparisonService> logger)
    {
        _currentDbService = currentDbService;
        _legacyDbService = legacyDbService;
        _logger = logger;
    }

    public async Task<QueryComparisonResult> CompareQueriesAsync(string queryName, string currentQuery, string legacyQuery, Dictionary<string, object>? parameters = null)
    {
        var result = new QueryComparisonResult
        {
            QueryName = queryName,
            CurrentQuery = currentQuery,
            LegacyQuery = legacyQuery
        };

        try
        {
            _logger.LogInformation("Starting query comparison for {QueryName}", queryName);

            // Execute current query
            var currentStopwatch = Stopwatch.StartNew();
            List<Dictionary<string, object>> currentData;
            
            try
            {
                // For current database, we'll use a generic approach since we don't have direct access to ExecuteQuery
                // This is a simplified implementation - in practice, you'd need to adapt based on your current DB service
                currentData = await ExecuteCurrentQueryAsync(currentQuery, parameters);
                currentStopwatch.Stop();
                result.CurrentExecutionTime = currentStopwatch.Elapsed;
                result.CurrentRowCount = currentData.Count;
            }
            catch (Exception ex)
            {
                currentStopwatch.Stop();
                result.CurrentExecutionTime = currentStopwatch.Elapsed;
                result.Error = $"Current query failed: {ex.Message}";
                _logger.LogError(ex, "Error executing current query for {QueryName}", queryName);
                return result;
            }

            // Execute legacy query
            var legacyStopwatch = Stopwatch.StartNew();
            List<Dictionary<string, object>> legacyData;
            
            try
            {
                var legacyResult = parameters != null 
                    ? await _legacyDbService.ExecuteQueryWithParametersAsync(legacyQuery, parameters)
                    : await _legacyDbService.ExecuteQueryAsync(legacyQuery);
                
                legacyStopwatch.Stop();
                result.LegacyExecutionTime = legacyStopwatch.Elapsed;
                
                if (!legacyResult.Success)
                {
                    result.Error = $"Legacy query failed: {legacyResult.Error}";
                    return result;
                }
                
                legacyData = legacyResult.Data;
                result.LegacyRowCount = legacyData.Count;
            }
            catch (Exception ex)
            {
                legacyStopwatch.Stop();
                result.LegacyExecutionTime = legacyStopwatch.Elapsed;
                result.Error = $"Legacy query failed: {ex.Message}";
                _logger.LogError(ex, "Error executing legacy query for {QueryName}", queryName);
                return result;
            }

            // Compare data
            result.Discrepancies = await CompareDataAsync(currentData, legacyData, queryName);
            result.DataMatches = result.Discrepancies.Count == 0;

            _logger.LogInformation("Query comparison completed for {QueryName}. Matches: {DataMatches}, Discrepancies: {DiscrepancyCount}", 
                queryName, result.DataMatches, result.Discrepancies.Count);
        }
        catch (Exception ex)
        {
            result.Error = $"Comparison failed: {ex.Message}";
            _logger.LogError(ex, "Error during query comparison for {QueryName}", queryName);
        }

        return result;
    }

    public async Task<List<DataDiscrepancy>> CompareDataAsync(List<Dictionary<string, object>> currentData, List<Dictionary<string, object>> legacyData, string queryName)
    {
        var discrepancies = new List<DataDiscrepancy>();

        try
        {
            _logger.LogInformation("Comparing data for {QueryName}. Current rows: {CurrentRows}, Legacy rows: {LegacyRows}", 
                queryName, currentData.Count, legacyData.Count);

            // Check row count differences
            if (currentData.Count != legacyData.Count)
            {
                discrepancies.Add(new DataDiscrepancy
                {
                    FieldName = "RowCount",
                    CurrentValue = currentData.Count,
                    LegacyValue = legacyData.Count,
                    RowIdentifier = "TOTAL",
                    Type = DiscrepancyType.ValueMismatch,
                    Description = $"Row count mismatch: Current={currentData.Count}, Legacy={legacyData.Count}"
                });
            }

            // If no data in either, return early
            if (currentData.Count == 0 && legacyData.Count == 0)
            {
                return discrepancies;
            }

            // Get all field names from both datasets
            var currentFields = currentData.FirstOrDefault()?.Keys.ToHashSet() ?? new HashSet<string>();
            var legacyFields = legacyData.FirstOrDefault()?.Keys.ToHashSet() ?? new HashSet<string>();
            var allFields = currentFields.Union(legacyFields).ToList();

            // Check for missing fields
            foreach (var field in legacyFields.Except(currentFields))
            {
                discrepancies.Add(new DataDiscrepancy
                {
                    FieldName = field,
                    CurrentValue = null,
                    LegacyValue = "EXISTS",
                    RowIdentifier = "SCHEMA",
                    Type = DiscrepancyType.MissingInCurrent,
                    Description = $"Field '{field}' exists in legacy but not in current"
                });
            }

            foreach (var field in currentFields.Except(legacyFields))
            {
                discrepancies.Add(new DataDiscrepancy
                {
                    FieldName = field,
                    CurrentValue = "EXISTS",
                    LegacyValue = null,
                    RowIdentifier = "SCHEMA",
                    Type = DiscrepancyType.MissingInLegacy,
                    Description = $"Field '{field}' exists in current but not in legacy"
                });
            }

            // Compare data row by row (simplified approach - assumes same order)
            var maxRows = Math.Min(currentData.Count, legacyData.Count);
            var commonFields = currentFields.Intersect(legacyFields).ToList();

            for (int i = 0; i < maxRows; i++)
            {
                var currentRow = currentData[i];
                var legacyRow = legacyData[i];
                var rowId = GenerateRowIdentifier(currentRow, commonFields.Take(3).ToList());

                foreach (var field in commonFields)
                {
                    var currentValue = currentRow.GetValueOrDefault(field);
                    var legacyValue = legacyRow.GetValueOrDefault(field);

                    if (!AreValuesEqual(currentValue, legacyValue, field))
                    {
                        var discrepancyType = DetermineDiscrepancyType(currentValue, legacyValue);
                        
                        discrepancies.Add(new DataDiscrepancy
                        {
                            FieldName = field,
                            CurrentValue = currentValue,
                            LegacyValue = legacyValue,
                            RowIdentifier = rowId,
                            Type = discrepancyType,
                            Description = $"Value mismatch in field '{field}' for row {rowId}"
                        });
                    }
                }
            }

            // Check for extra rows
            if (currentData.Count > legacyData.Count)
            {
                for (int i = legacyData.Count; i < currentData.Count; i++)
                {
                    var rowId = GenerateRowIdentifier(currentData[i], commonFields.Take(3).ToList());
                    discrepancies.Add(new DataDiscrepancy
                    {
                        FieldName = "Row",
                        CurrentValue = "EXISTS",
                        LegacyValue = null,
                        RowIdentifier = rowId,
                        Type = DiscrepancyType.MissingInLegacy,
                        Description = $"Extra row in current data: {rowId}"
                    });
                }
            }
            else if (legacyData.Count > currentData.Count)
            {
                for (int i = currentData.Count; i < legacyData.Count; i++)
                {
                    var rowId = GenerateRowIdentifier(legacyData[i], commonFields.Take(3).ToList());
                    discrepancies.Add(new DataDiscrepancy
                    {
                        FieldName = "Row",
                        CurrentValue = null,
                        LegacyValue = "EXISTS",
                        RowIdentifier = rowId,
                        Type = DiscrepancyType.MissingInCurrent,
                        Description = $"Missing row in current data: {rowId}"
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error comparing data for {QueryName}", queryName);
            discrepancies.Add(new DataDiscrepancy
            {
                FieldName = "COMPARISON_ERROR",
                CurrentValue = null,
                LegacyValue = null,
                RowIdentifier = "ERROR",
                Type = DiscrepancyType.ValueMismatch,
                Description = $"Error during data comparison: {ex.Message}"
            });
        }

        return discrepancies;
    }

    public async Task<PerformanceComparisonResult> ComparePerformanceAsync(string queryName, string currentQuery, string legacyQuery, Dictionary<string, object>? parameters = null)
    {
        var result = new PerformanceComparisonResult
        {
            QueryName = queryName
        };

        try
        {
            _logger.LogInformation("Starting performance comparison for {QueryName}", queryName);

            // Execute current query multiple times for average
            var currentTimes = new List<TimeSpan>();
            for (int i = 0; i < 3; i++)
            {
                var stopwatch = Stopwatch.StartNew();
                try
                {
                    await ExecuteCurrentQueryAsync(currentQuery, parameters);
                    stopwatch.Stop();
                    currentTimes.Add(stopwatch.Elapsed);
                }
                catch
                {
                    stopwatch.Stop();
                    currentTimes.Add(stopwatch.Elapsed);
                }
            }

            // Execute legacy query multiple times for average
            var legacyTimes = new List<TimeSpan>();
            for (int i = 0; i < 3; i++)
            {
                var stopwatch = Stopwatch.StartNew();
                try
                {
                    var legacyResult = parameters != null 
                        ? await _legacyDbService.ExecuteQueryWithParametersAsync(legacyQuery, parameters)
                        : await _legacyDbService.ExecuteQueryAsync(legacyQuery);
                    stopwatch.Stop();
                    legacyTimes.Add(stopwatch.Elapsed);
                }
                catch
                {
                    stopwatch.Stop();
                    legacyTimes.Add(stopwatch.Elapsed);
                }
            }

            // Calculate averages
            result.CurrentExecutionTime = TimeSpan.FromMilliseconds(currentTimes.Average(t => t.TotalMilliseconds));
            result.LegacyExecutionTime = TimeSpan.FromMilliseconds(legacyTimes.Average(t => t.TotalMilliseconds));

            _logger.LogInformation("Performance comparison completed for {QueryName}. Current: {CurrentTime}ms, Legacy: {LegacyTime}ms", 
                queryName, result.CurrentExecutionTime.TotalMilliseconds, result.LegacyExecutionTime.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during performance comparison for {QueryName}", queryName);
        }

        return result;
    }

    public async Task<ValidationResult> ValidateQueriesAsync(Dictionary<string, QueryPair> queries)
    {
        var result = new ValidationResult
        {
            StartTime = DateTime.UtcNow
        };

        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Starting validation of {QueryCount} queries", queries.Count);

            var tasks = queries.Select(async kvp =>
            {
                try
                {
                    var comparison = await CompareQueriesAsync(kvp.Key, kvp.Value.CurrentQuery, kvp.Value.LegacyQuery, kvp.Value.Parameters);
                    return comparison;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error validating query {QueryName}", kvp.Key);
                    return new QueryComparisonResult
                    {
                        QueryName = kvp.Key,
                        Error = ex.Message,
                        DataMatches = false
                    };
                }
            });

            var comparisons = await Task.WhenAll(tasks);
            result.Results = comparisons.ToList();

            // Calculate summary statistics
            result.QueriesValidated = comparisons.Length;
            result.QueriesMatched = comparisons.Count(c => c.DataMatches && string.IsNullOrEmpty(c.Error));
            result.QueriesFailed = comparisons.Count(c => !string.IsNullOrEmpty(c.Error));

            // Calculate performance averages
            var successfulComparisons = comparisons.Where(c => string.IsNullOrEmpty(c.Error)).ToList();
            if (successfulComparisons.Any())
            {
                result.Summary.AverageCurrentExecutionTime = TimeSpan.FromMilliseconds(
                    successfulComparisons.Average(c => c.CurrentExecutionTime.TotalMilliseconds));
                result.Summary.AverageLegacyExecutionTime = TimeSpan.FromMilliseconds(
                    successfulComparisons.Average(c => c.LegacyExecutionTime.TotalMilliseconds));
            }

            // Identify critical issues
            result.Summary.CriticalIssues = comparisons
                .Where(c => !c.DataMatches || !string.IsNullOrEmpty(c.Error))
                .Select(c => $"{c.QueryName}: {(string.IsNullOrEmpty(c.Error) ? "Data mismatch" : c.Error)}")
                .ToList();

            result.Summary.QueriesValidated = result.QueriesValidated;
            result.Summary.QueriesMatched = result.QueriesMatched;
            result.Summary.QueriesFailed = result.QueriesFailed;
            result.Summary.TotalDiscrepancies = comparisons.Sum(c => c.Discrepancies.Count);

            stopwatch.Stop();
            result.Duration = stopwatch.Elapsed;
            result.EndTime = DateTime.UtcNow;

            _logger.LogInformation("Query validation completed. Validated: {Validated}, Matched: {Matched}, Failed: {Failed}", 
                result.QueriesValidated, result.QueriesMatched, result.QueriesFailed);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.Duration = stopwatch.Elapsed;
            result.EndTime = DateTime.UtcNow;
            _logger.LogError(ex, "Error during query validation");
        }

        return result;
    }

    public bool AreValuesEqual(object? currentValue, object? legacyValue, string fieldName)
    {
        // Handle nulls
        if (currentValue == null && legacyValue == null) return true;
        if (currentValue == null || legacyValue == null) return false;

        // Handle DBNull
        if (currentValue == DBNull.Value && legacyValue == DBNull.Value) return true;
        if (currentValue == DBNull.Value || legacyValue == DBNull.Value) return false;

        // Convert to strings for comparison
        var currentStr = currentValue.ToString();
        var legacyStr = legacyValue.ToString();

        // Direct string comparison
        if (string.Equals(currentStr, legacyStr, StringComparison.OrdinalIgnoreCase))
            return true;

        // Try numeric comparison for numeric fields
        if (IsNumericField(fieldName) && 
            decimal.TryParse(currentStr, out var currentDecimal) && 
            decimal.TryParse(legacyStr, out var legacyDecimal))
        {
            return Math.Abs(currentDecimal - legacyDecimal) < 0.0001m;
        }

        // Try date comparison
        if (DateTime.TryParse(currentStr, out var currentDate) && 
            DateTime.TryParse(legacyStr, out var legacyDate))
        {
            return Math.Abs((currentDate - legacyDate).TotalSeconds) < 1;
        }

        return false;
    }

    public string GenerateRowIdentifier(Dictionary<string, object> row, List<string> keyFields)
    {
        if (!keyFields.Any())
        {
            keyFields = row.Keys.Take(3).ToList();
        }

        var identifierParts = keyFields
            .Where(field => row.ContainsKey(field))
            .Select(field => $"{field}={row[field]}")
            .ToList();

        return identifierParts.Any() ? string.Join(", ", identifierParts) : "Unknown";
    }

    private async Task<List<Dictionary<string, object>>> ExecuteCurrentQueryAsync(string query, Dictionary<string, object>? parameters)
    {
        // This is a simplified implementation. In practice, you'd need to adapt this
        // based on your current database service capabilities
        // For now, we'll use a basic approach that works with the existing RawSqlService
        
        try
        {
            // Since RawSqlService doesn't have a generic query execution method,
            // we'll need to use the database context directly or extend the service
            // This is a placeholder implementation
            
            var results = new List<Dictionary<string, object>>();
            
            // You would implement the actual query execution here
            // This might involve extending your RawSqlService or using Entity Framework directly
            
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing current query");
            throw;
        }
    }

    private DiscrepancyType DetermineDiscrepancyType(object? currentValue, object? legacyValue)
    {
        if (currentValue == null && legacyValue != null) return DiscrepancyType.MissingInCurrent;
        if (currentValue != null && legacyValue == null) return DiscrepancyType.MissingInLegacy;
        
        if (currentValue?.GetType() != legacyValue?.GetType()) return DiscrepancyType.TypeMismatch;
        
        return DiscrepancyType.ValueMismatch;
    }

    private bool IsNumericField(string fieldName)
    {
        var numericFieldPatterns = new[] { "id", "count", "value", "amount", "price", "quantity", "number", "num" };
        return numericFieldPatterns.Any(pattern => fieldName.ToLower().Contains(pattern));
    }
}