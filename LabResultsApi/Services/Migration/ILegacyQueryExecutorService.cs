using System.Data;
using LabResultsApi.Models.Migration;

namespace LabResultsApi.Services.Migration;

public interface ILegacyQueryExecutorService
{
    Task<bool> TestConnectionAsync();
    Task<QueryExecutionResult> ExecuteQueryAsync(string query, int timeoutMinutes = 2);
    Task<QueryExecutionResult> ExecuteQueryWithParametersAsync(string query, Dictionary<string, object> parameters, int timeoutMinutes = 2);
    Task<List<Dictionary<string, object>>> GetQueryResultsAsync(string query, int timeoutMinutes = 2);
    Task<List<Dictionary<string, object>>> GetQueryResultsWithParametersAsync(string query, Dictionary<string, object> parameters, int timeoutMinutes = 2);
    Task<int> GetRowCountAsync(string query, int timeoutMinutes = 2);
    Task<DataTable> GetDataTableAsync(string query, int timeoutMinutes = 2);
    void SetConnectionString(string connectionString);
}

public class QueryExecutionResult
{
    public bool Success { get; set; }
    public List<Dictionary<string, object>> Data { get; set; } = new();
    public int RowCount { get; set; }
    public TimeSpan ExecutionTime { get; set; }
    public string? Error { get; set; }
    public string? StackTrace { get; set; }
    public DateTime ExecutedAt { get; set; }
    public string Query { get; set; } = string.Empty;
}