using System.Data;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using LabResultsApi.Models.Migration;

namespace LabResultsApi.Services.Migration;

public class LegacyQueryExecutorService : ILegacyQueryExecutorService
{
    private readonly ILogger<LegacyQueryExecutorService> _logger;
    private string _connectionString = string.Empty;

    public LegacyQueryExecutorService(ILogger<LegacyQueryExecutorService> logger)
    {
        _logger = logger;
    }

    public void SetConnectionString(string connectionString)
    {
        _connectionString = connectionString;
        _logger.LogInformation("Legacy database connection string updated");
    }

    public async Task<bool> TestConnectionAsync()
    {
        if (string.IsNullOrEmpty(_connectionString))
        {
            _logger.LogWarning("Legacy connection string not configured");
            return false;
        }

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            command.CommandTimeout = 30;
            
            await command.ExecuteScalarAsync();
            
            _logger.LogInformation("Legacy database connection test successful");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Legacy database connection test failed: {ErrorMessage}", ex.Message);
            return false;
        }
    }

    public async Task<QueryExecutionResult> ExecuteQueryAsync(string query, int timeoutMinutes = 2)
    {
        return await ExecuteQueryWithParametersAsync(query, new Dictionary<string, object>(), timeoutMinutes);
    }

    public async Task<QueryExecutionResult> ExecuteQueryWithParametersAsync(string query, Dictionary<string, object> parameters, int timeoutMinutes = 2)
    {
        var result = new QueryExecutionResult
        {
            Query = query,
            ExecutedAt = DateTime.UtcNow
        };

        if (string.IsNullOrEmpty(_connectionString))
        {
            result.Error = "Legacy connection string not configured";
            _logger.LogError("Legacy connection string not configured for query execution");
            return result;
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = query;
            command.CommandTimeout = timeoutMinutes * 60;

            // Add parameters
            foreach (var param in parameters)
            {
                var sqlParam = command.CreateParameter();
                sqlParam.ParameterName = param.Key.StartsWith("@") ? param.Key : $"@{param.Key}";
                sqlParam.Value = param.Value ?? DBNull.Value;
                command.Parameters.Add(sqlParam);
            }

            using var reader = await command.ExecuteReaderAsync();
            
            var data = new List<Dictionary<string, object>>();
            var fieldNames = new List<string>();
            
            // Get field names
            for (int i = 0; i < reader.FieldCount; i++)
            {
                fieldNames.Add(reader.GetName(i));
            }

            // Read data
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    row[fieldNames[i]] = value;
                }
                data.Add(row);
            }

            stopwatch.Stop();

            result.Success = true;
            result.Data = data;
            result.RowCount = data.Count;
            result.ExecutionTime = stopwatch.Elapsed;

            _logger.LogInformation("Legacy query executed successfully. Rows: {RowCount}, Time: {ExecutionTime}ms", 
                result.RowCount, result.ExecutionTime.TotalMilliseconds);
        }
        catch (SqlException ex)
        {
            stopwatch.Stop();
            result.ExecutionTime = stopwatch.Elapsed;
            result.Error = $"SQL Error: {ex.Message}";
            result.StackTrace = ex.StackTrace;
            
            _logger.LogError(ex, "SQL error executing legacy query: {ErrorMessage}", ex.Message);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.ExecutionTime = stopwatch.Elapsed;
            result.Error = ex.Message;
            result.StackTrace = ex.StackTrace;
            
            _logger.LogError(ex, "Error executing legacy query: {ErrorMessage}", ex.Message);
        }

        return result;
    }

    public async Task<List<Dictionary<string, object>>> GetQueryResultsAsync(string query, int timeoutMinutes = 2)
    {
        var result = await ExecuteQueryAsync(query, timeoutMinutes);
        return result.Success ? result.Data : new List<Dictionary<string, object>>();
    }

    public async Task<List<Dictionary<string, object>>> GetQueryResultsWithParametersAsync(string query, Dictionary<string, object> parameters, int timeoutMinutes = 2)
    {
        var result = await ExecuteQueryWithParametersAsync(query, parameters, timeoutMinutes);
        return result.Success ? result.Data : new List<Dictionary<string, object>>();
    }

    public async Task<int> GetRowCountAsync(string query, int timeoutMinutes = 2)
    {
        var result = await ExecuteQueryAsync(query, timeoutMinutes);
        return result.Success ? result.RowCount : 0;
    }

    public async Task<DataTable> GetDataTableAsync(string query, int timeoutMinutes = 2)
    {
        var dataTable = new DataTable();

        if (string.IsNullOrEmpty(_connectionString))
        {
            _logger.LogError("Legacy connection string not configured for DataTable query");
            return dataTable;
        }

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = query;
            command.CommandTimeout = timeoutMinutes * 60;

            using var adapter = new SqlDataAdapter(command);
            adapter.Fill(dataTable);

            _logger.LogInformation("Legacy query DataTable filled successfully. Rows: {RowCount}", dataTable.Rows.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing legacy query for DataTable: {ErrorMessage}", ex.Message);
        }

        return dataTable;
    }
}