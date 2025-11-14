using Microsoft.EntityFrameworkCore;
using LabResultsApi.Data;
using LabResultsApi.DTOs;

namespace LabResultsApi.Services;

public class QualificationService : IQualificationService
{
    private readonly LabDbContext _context;
    private readonly ILogger<QualificationService> _logger;

    public QualificationService(LabDbContext context, ILogger<QualificationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<string?> GetUserQualificationAsync(string employeeId, int testId)
    {
        try
        {
            _logger.LogInformation("Getting qualification for employee {EmployeeId}, test {TestId}", employeeId, testId);
            
            var sql = @"
                SELECT ltq.qualificationLevel 
                FROM LubeTechQualification ltq
                INNER JOIN Test t ON ltq.testStandID = t.testStandID
                WHERE ltq.employeeID = {0} AND t.testID = {1}";

            var qualification = await _context.Database.SqlQueryRaw<string>(sql, employeeId, testId)
                .FirstOrDefaultAsync();

            return qualification;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting qualification for employee {EmployeeId}, test {TestId}", employeeId, testId);
            return null;
        }
    }

    public async Task<bool> IsUserQualifiedAsync(string employeeId, int testId)
    {
        try
        {
            var qualification = await GetUserQualificationAsync(employeeId, testId);
            return !string.IsNullOrEmpty(qualification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user {EmployeeId} is qualified for test {TestId}", employeeId, testId);
            return false;
        }
    }

    public async Task<bool> IsUserQualifiedToReviewAsync(string employeeId, int sampleId, int testId)
    {
        try
        {
            _logger.LogInformation("Checking review qualification for employee {EmployeeId}, sample {SampleId}, test {TestId}", 
                employeeId, sampleId, testId);
            
            var qualification = await GetUserQualificationAsync(employeeId, testId);
            
            // Only Q/QAG and MicrE can review results
            if (qualification != "Q/QAG" && qualification != "MicrE")
            {
                return false;
            }

            // Check if this user entered the original results (they cannot review their own work)
            var sql = @"
                SELECT entryID 
                FROM TestReadings 
                WHERE sampleID = {0} AND testID = {1}";

            var entryId = await _context.Database.SqlQueryRaw<string>(sql, sampleId, testId)
                .FirstOrDefaultAsync();

            if (entryId == employeeId)
            {
                _logger.LogInformation("User {EmployeeId} cannot review their own results for sample {SampleId}, test {TestId}", 
                    employeeId, sampleId, testId);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking review qualification for employee {EmployeeId}, sample {SampleId}, test {TestId}", 
                employeeId, sampleId, testId);
            return false;
        }
    }

    public async Task<IEnumerable<TestDto>> GetQualifiedTestsAsync(string employeeId)
    {
        try
        {
            _logger.LogInformation("Getting qualified tests for employee {EmployeeId}", employeeId);
            
            var sql = @"
                SELECT DISTINCT 
                    t.testID as TestId,
                    t.testName as TestName,
                    t.testDescription as TestDescription,
                    t.active as Active
                FROM LubeTechQualification ltq
                INNER JOIN Test t ON ltq.testStandID = t.testStandID
                WHERE ltq.employeeID = @employeeId AND t.active = 1
                ORDER BY t.testName";

            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            
            var empIdParam = command.CreateParameter();
            empIdParam.ParameterName = "@employeeId";
            empIdParam.Value = employeeId;
            command.Parameters.Add(empIdParam);
            
            var results = new List<TestDto>();
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                results.Add(new TestDto
                {
                    TestId = Convert.ToInt32(reader["TestId"]),
                    TestName = reader["TestName"]?.ToString() ?? string.Empty,
                    TestDescription = reader["TestDescription"] == DBNull.Value ? string.Empty : reader["TestDescription"]?.ToString() ?? string.Empty,
                    Active = Convert.ToBoolean(reader["Active"])
                });
            }
            
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting qualified tests for employee {EmployeeId}", employeeId);
            return new List<TestDto>();
        }
    }

    public async Task<IEnumerable<QualificationDetailDto>> GetUserQualificationsAsync(string employeeId)
    {
        try
        {
            _logger.LogInformation("Getting qualifications for employee {EmployeeId}", employeeId);
            
            var sql = @"
                SELECT 
                    ltq.employeeID as EmployeeId,
                    ltl.employeeName as EmployeeName,
                    ltq.testStandID as TestStandId,
                    ts.testStandName as TestStandName,
                    ltq.qualificationLevel as QualificationLevel,
                    ltq.qualificationDate as QualificationDate,
                    ltq.expirationDate as ExpirationDate,
                    ltq.isActive as IsActive,
                    ltq.notes as Notes
                FROM LubeTechQualification ltq
                INNER JOIN LubeTechList ltl ON ltq.employeeID = ltl.employeeID
                LEFT JOIN TestStand ts ON ltq.testStandID = ts.testStandID
                WHERE ltq.employeeID = @employeeId
                ORDER BY ltq.qualificationLevel, ts.testStandName";

            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            
            var empIdParam = command.CreateParameter();
            empIdParam.ParameterName = "@employeeId";
            empIdParam.Value = employeeId;
            command.Parameters.Add(empIdParam);
            
            var results = new List<QualificationDetailDto>();
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                results.Add(new QualificationDetailDto
                {
                    EmployeeId = reader["EmployeeId"]?.ToString() ?? string.Empty,
                    EmployeeName = reader["EmployeeName"] == DBNull.Value ? string.Empty : reader["EmployeeName"]?.ToString() ?? string.Empty,
                    TestStandId = Convert.ToInt32(reader["TestStandId"]),
                    TestStandName = reader["TestStandName"] == DBNull.Value ? string.Empty : reader["TestStandName"]?.ToString() ?? string.Empty,
                    QualificationLevel = reader["QualificationLevel"]?.ToString() ?? string.Empty,
                    QualificationDate = reader["QualificationDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["QualificationDate"]),
                    ExpirationDate = reader["ExpirationDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["ExpirationDate"]),
                    IsActive = Convert.ToBoolean(reader["IsActive"]),
                    Notes = reader["Notes"] == DBNull.Value ? null : reader["Notes"]?.ToString()
                });
            }
            
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting qualifications for employee {EmployeeId}", employeeId);
            return new List<QualificationDetailDto>();
        }
    }

    public async Task<bool> CanUserValidateResultsAsync(string validatorId, int sampleId, int testId)
    {
        try
        {
            // Check if user is qualified to review
            var canReview = await IsUserQualifiedToReviewAsync(validatorId, sampleId, testId);
            if (!canReview)
            {
                return false;
            }

            // Additional check: ensure validator is not the same as the person who entered the results
            var sql = @"
                SELECT entryID 
                FROM TestReadings 
                WHERE sampleID = {0} AND testID = {1}";

            var entryId = await _context.Database.SqlQueryRaw<string>(sql, sampleId, testId)
                .FirstOrDefaultAsync();

            return entryId != validatorId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user {ValidatorId} can validate results for sample {SampleId}, test {TestId}", 
                validatorId, sampleId, testId);
            return false;
        }
    }

    public async Task<string?> GetRequiredQualificationLevelAsync(int testId)
    {
        try
        {
            // Get the minimum required qualification level for a test
            // This is a simplified implementation - in reality, this might be more complex
            var sql = @"
                SELECT MIN(ltq.qualificationLevel)
                FROM LubeTechQualification ltq
                INNER JOIN Test t ON ltq.testStandID = t.testStandID
                WHERE t.testID = {0}";

            var requiredLevel = await _context.Database.SqlQueryRaw<string>(sql, testId)
                .FirstOrDefaultAsync();

            return requiredLevel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting required qualification level for test {TestId}", testId);
            return null;
        }
    }

    public async Task<bool> HasMinimumQualificationAsync(string employeeId, int testId, string requiredLevel)
    {
        try
        {
            var userQualification = await GetUserQualificationAsync(employeeId, testId);
            if (string.IsNullOrEmpty(userQualification))
            {
                return false;
            }

            // Define qualification hierarchy (from lowest to highest)
            var qualificationHierarchy = new Dictionary<string, int>
            {
                { "TRAIN", 1 },
                { "Q", 2 },
                { "Q/QAG", 3 },
                { "MicrE", 4 }
            };

            var userLevel = qualificationHierarchy.GetValueOrDefault(userQualification, 0);
            var requiredLevelValue = qualificationHierarchy.GetValueOrDefault(requiredLevel, 0);

            return userLevel >= requiredLevelValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking minimum qualification for employee {EmployeeId}, test {TestId}, required level {RequiredLevel}", 
                employeeId, testId, requiredLevel);
            return false;
        }
    }
}