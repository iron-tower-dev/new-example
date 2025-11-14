using LabResultsApi.Models.Migration;

namespace LabResultsApi.Services.Migration;

public interface IQueryComparisonService
{
    Task<QueryComparisonResult> CompareQueriesAsync(string queryName, string currentQuery, string legacyQuery, Dictionary<string, object>? parameters = null);
    Task<List<DataDiscrepancy>> CompareDataAsync(List<Dictionary<string, object>> currentData, List<Dictionary<string, object>> legacyData, string queryName);
    Task<PerformanceComparisonResult> ComparePerformanceAsync(string queryName, string currentQuery, string legacyQuery, Dictionary<string, object>? parameters = null);
    Task<ValidationResult> ValidateQueriesAsync(Dictionary<string, QueryPair> queries);
    bool AreValuesEqual(object? currentValue, object? legacyValue, string fieldName);
    string GenerateRowIdentifier(Dictionary<string, object> row, List<string> keyFields);
}

public class QueryPair
{
    public string CurrentQuery { get; set; } = string.Empty;
    public string LegacyQuery { get; set; } = string.Empty;
    public Dictionary<string, object>? Parameters { get; set; }
    public List<string> KeyFields { get; set; } = new();
    public bool IgnoreMinorDifferences { get; set; } = true;
}