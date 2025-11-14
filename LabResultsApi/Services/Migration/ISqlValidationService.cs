using LabResultsApi.Models.Migration;

namespace LabResultsApi.Services.Migration;

public interface ISqlValidationServiceImpl
{
    Task<ValidationResult> ValidateAllQueriesAsync(ValidationOptions options);
    Task<QueryComparisonResult> CompareQueryAsync(string queryName, string currentQuery, string legacyQuery, Dictionary<string, object>? parameters = null);
    Task<PerformanceComparisonResult> ComparePerformanceAsync(string queryName, string currentQuery, string legacyQuery, Dictionary<string, object>? parameters = null);
    Task<ValidationResult> ValidateQueriesAsync(Dictionary<string, QueryPair> queries, ValidationOptions options);
    Task<bool> TestLegacyConnectionAsync();
    void SetLegacyConnectionString(string connectionString);
    Task<List<string>> GetAvailableQueriesAsync();
}