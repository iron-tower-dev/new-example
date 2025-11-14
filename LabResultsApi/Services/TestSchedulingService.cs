using Microsoft.EntityFrameworkCore;
using LabResultsApi.Data;
using LabResultsApi.DTOs;

namespace LabResultsApi.Services;

public class TestSchedulingService : ITestSchedulingService
{
    private readonly LabDbContext _context;
    private readonly IRawSqlService _rawSqlService;
    private readonly ILogger<TestSchedulingService> _logger;

    public TestSchedulingService(
        LabDbContext context,
        IRawSqlService rawSqlService,
        ILogger<TestSchedulingService> logger)
    {
        _context = context;
        _rawSqlService = rawSqlService;
        _logger = logger;
    }

    public async Task<TestSchedulingResult> AutoScheduleTestsAsync(int sampleId, int completedTestId, string tagNumber, string component, string location)
    {
        _logger.LogInformation("Auto-scheduling tests for sample {SampleId}, completed test {CompletedTestId}", sampleId, completedTestId);
        
        var result = new TestSchedulingResult { Success = true };
        
        try
        {
            // Get test rules for this equipment
            var rules = await GetTestRulesAsync(tagNumber, component, location);
            
            foreach (var rule in rules.Where(r => r.IsActive))
            {
                if (rule.RuleType == "ADD" && rule.TriggerTestId == completedTestId)
                {
                    // Check if we should add this test
                    if (await ShouldScheduleTestAsync(sampleId, rule.TestId, tagNumber, component, location))
                    {
                        var scheduled = await ScheduleTestAsync(sampleId, rule.TestId, "AUTO", $"Triggered by test {completedTestId}");
                        if (scheduled)
                        {
                            result.TestsAdded.Add(rule.TestId);
                            result.Messages.Add($"Scheduled test {rule.TestId} ({rule.TestName})");
                        }
                    }
                }
                else if (rule.RuleType == "REMOVE" && rule.TriggerTestId == completedTestId)
                {
                    // Check if we should remove this test
                    var removed = await RemoveScheduledTestAsync(sampleId, rule.TestId);
                    if (removed)
                    {
                        result.TestsRemoved.Add(rule.TestId);
                        result.Messages.Add($"Removed test {rule.TestId} ({rule.TestName})");
                    }
                }
            }

            // Handle specific test scheduling logic from legacy system
            await HandleSpecificTestSchedulingAsync(sampleId, completedTestId, result);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error auto-scheduling tests for sample {SampleId}", sampleId);
            result.Success = false;
            result.ErrorMessage = ex.Message;
            return result;
        }
    }

    public async Task<bool> ShouldScheduleTestAsync(int sampleId, int testId, string tagNumber, string component, string location)
    {
        try
        {
            // Check if test is already scheduled
            var existingCount = await _context.Database.SqlQueryRaw<int>(
                "SELECT COUNT(*) FROM TestReadings WHERE sampleID = {0} AND testID = {1}",
                sampleId, testId).FirstAsync();
            
            if (existingCount > 0)
            {
                _logger.LogDebug("Test {TestId} already scheduled for sample {SampleId}", testId, sampleId);
                return false;
            }

            // Check minimum interval requirements
            var sampleDate = await GetSampleDateAsync(sampleId);
            if (sampleDate.HasValue)
            {
                var meetsInterval = await CheckMinimumIntervalAsync(tagNumber, component, location, testId, sampleDate.Value);
                if (!meetsInterval)
                {
                    _logger.LogDebug("Test {TestId} does not meet minimum interval for sample {SampleId}", testId, sampleId);
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if test {TestId} should be scheduled for sample {SampleId}", testId, sampleId);
            return false;
        }
    }

    public async Task<TestSchedulingResult> RemoveUnneededTestsAsync(int sampleId, string tagNumber, string component, string location)
    {
        _logger.LogInformation("Removing unneeded tests for sample {SampleId}", sampleId);
        
        var result = new TestSchedulingResult { Success = true };
        
        try
        {
            // Get tests that can be removed based on delete criteria
            var sql = @"
                SELECT DISTINCT tr.testID 
                FROM TestReadings tr
                INNER JOIN vwTestDeleteCriteria tdc ON tr.sampleID = tdc.sampleid AND tr.testID = tdc.testid
                WHERE tr.sampleID = {0} AND tr.status = 'X'";

            var testsToRemove = await _context.Database.SqlQueryRaw<int>(sql, sampleId).ToListAsync();

            foreach (var testId in testsToRemove)
            {
                // Check minimum interval before removing
                var sampleDate = await GetSampleDateAsync(sampleId);
                if (sampleDate.HasValue)
                {
                    var canRemove = await CanRemoveTestAsync(sampleId, testId, tagNumber, component, location, sampleDate.Value);
                    if (canRemove)
                    {
                        var removed = await RemoveScheduledTestAsync(sampleId, testId);
                        if (removed)
                        {
                            result.TestsRemoved.Add(testId);
                            result.Messages.Add($"Removed unneeded test {testId}");
                        }
                    }
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing unneeded tests for sample {SampleId}", sampleId);
            result.Success = false;
            result.ErrorMessage = ex.Message;
            return result;
        }
    }

    public async Task<IEnumerable<ScheduledTestDto>> GetScheduledTestsAsync(int sampleId)
    {
        try
        {
            var sql = @"
                SELECT 
                    tr.testID as TestId,
                    t.testName as TestName,
                    tr.status as Status,
                    tr.entryDate as ScheduledDate,
                    ISNULL(tr.schedType, 'MANUAL') as ScheduleType,
                    '' as Reason
                FROM TestReadings tr
                INNER JOIN Test t ON tr.testID = t.testID
                WHERE tr.sampleID = @sampleId
                ORDER BY tr.testID";

            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            
            var sampleIdParam = command.CreateParameter();
            sampleIdParam.ParameterName = "@sampleId";
            sampleIdParam.Value = sampleId;
            command.Parameters.Add(sampleIdParam);
            
            var results = new List<ScheduledTestDto>();
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                results.Add(new ScheduledTestDto
                {
                    TestId = Convert.ToInt32(reader["TestId"]),
                    TestName = reader["TestName"]?.ToString() ?? string.Empty,
                    Status = reader["Status"]?.ToString() ?? string.Empty,
                    ScheduledDate = reader["ScheduledDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["ScheduledDate"]),
                    ScheduleType = reader["ScheduleType"]?.ToString() ?? "MANUAL",
                    Reason = reader["Reason"]?.ToString() ?? string.Empty
                });
            }
            
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting scheduled tests for sample {SampleId}", sampleId);
            throw;
        }
    }

    public async Task<bool> CheckMinimumIntervalAsync(string tagNumber, string component, string location, int testId, DateTime sampleDate)
    {
        try
        {
            // Get minimum interval from test schedule definition
            var sql = @"
                SELECT MinimumInterval 
                FROM vwTestScheduleDefinitionByEQID 
                WHERE Tag = {0} AND ComponentCode = {1} AND LocationCode = {2} AND TestID = {3}";

            var minInterval = await _context.Database.SqlQueryRaw<int?>(sql, tagNumber, component, location, testId)
                .FirstOrDefaultAsync();

            if (!minInterval.HasValue || minInterval.Value <= 0)
            {
                return true; // No minimum interval requirement
            }

            // Check last test date for this equipment/test combination
            var lastTestSql = @"
                SELECT MAX(s.sampleDate)
                FROM UsedLubeSamples s
                INNER JOIN TestReadings tr ON s.ID = tr.sampleID
                WHERE s.tagNumber = {0} AND s.component = {1} AND s.location = {2} 
                    AND tr.testID = {3} AND tr.status IN ('C', 'D')
                    AND s.sampleDate < {4}";

            var lastTestDate = await _context.Database.SqlQueryRaw<DateTime?>(lastTestSql, 
                tagNumber, component, location, testId, sampleDate).FirstOrDefaultAsync();

            if (!lastTestDate.HasValue)
            {
                return true; // No previous test, so interval is met
            }

            var daysSinceLastTest = (sampleDate - lastTestDate.Value).Days;
            return daysSinceLastTest >= minInterval.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking minimum interval for test {TestId}", testId);
            return true; // Default to allowing the test
        }
    }

    public async Task<IEnumerable<TestScheduleRuleDto>> GetTestRulesAsync(string tagNumber, string component, string location)
    {
        try
        {
            var sql = @"
                SELECT 
                    tsr.TestID,
                    t.testName as TestName,
                    tsr.RuleType,
                    tsr.Condition,
                    tsr.ThresholdValue,
                    tsr.ThresholdOperator,
                    tsr.TriggerTestID,
                    tsr.MinimumInterval,
                    tsr.IsActive
                FROM vwTestRulesByEQID tsr
                INNER JOIN Test t ON tsr.TestID = t.testID
                WHERE tsr.Tag = @tag AND tsr.ComponentCode = @component AND tsr.LocationCode = @location
                    AND tsr.IsActive = 1";

            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            
            var tagParam = command.CreateParameter();
            tagParam.ParameterName = "@tag";
            tagParam.Value = tagNumber;
            command.Parameters.Add(tagParam);
            
            var compParam = command.CreateParameter();
            compParam.ParameterName = "@component";
            compParam.Value = component;
            command.Parameters.Add(compParam);
            
            var locParam = command.CreateParameter();
            locParam.ParameterName = "@location";
            locParam.Value = location;
            command.Parameters.Add(locParam);
            
            var results = new List<TestScheduleRuleDto>();
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                results.Add(new TestScheduleRuleDto
                {
                    TestId = Convert.ToInt32(reader["TestID"]),
                    TestName = reader["TestName"]?.ToString() ?? string.Empty,
                    RuleType = reader["RuleType"]?.ToString() ?? string.Empty,
                    Condition = reader["Condition"]?.ToString() ?? string.Empty,
                    ThresholdValue = reader["ThresholdValue"] == DBNull.Value ? null : Convert.ToDouble(reader["ThresholdValue"]),
                    ThresholdOperator = reader["ThresholdOperator"] == DBNull.Value ? null : reader["ThresholdOperator"]?.ToString(),
                    TriggerTestId = reader["TriggerTestID"] == DBNull.Value ? null : Convert.ToInt32(reader["TriggerTestID"]),
                    MinimumInterval = reader["MinimumInterval"] == DBNull.Value ? null : Convert.ToInt32(reader["MinimumInterval"]),
                    IsActive = Convert.ToBoolean(reader["IsActive"])
                });
            }
            
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting test rules for equipment {TagNumber}-{Component}-{Location}", tagNumber, component, location);
            return new List<TestScheduleRuleDto>();
        }
    }

    public async Task<bool> ScheduleFerrographyAsync(int sampleId)
    {
        try
        {
            _logger.LogInformation("Scheduling Ferrography for sample {SampleId}", sampleId);
            
            const int ferrographyTestId = 210;
            return await ScheduleTestAsync(sampleId, ferrographyTestId, "AUTO", "Scheduled after Large Spectroscopy");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling Ferrography for sample {SampleId}", sampleId);
            return false;
        }
    }

    public async Task<bool> SetSampleScheduleTypeAsync(int sampleId, string scheduleType)
    {
        try
        {
            var sql = "UPDATE UsedLubeSamples SET schedule = {1} WHERE ID = {0}";
            var rowsAffected = await _context.Database.ExecuteSqlRawAsync(sql, sampleId, scheduleType);
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting schedule type for sample {SampleId}", sampleId);
            return false;
        }
    }

    private async Task<bool> ScheduleTestAsync(int sampleId, int testId, string scheduleType, string reason)
    {
        try
        {
            // Check if already scheduled
            var existingCount = await _context.Database.SqlQueryRaw<int>(
                "SELECT COUNT(*) FROM TestReadings WHERE sampleID = {0} AND testID = {1}",
                sampleId, testId).FirstAsync();
            
            if (existingCount > 0)
            {
                return false; // Already scheduled
            }

            var sql = @"
                INSERT INTO TestReadings (sampleID, testID, trialNumber, status, schedType, entryDate, entryID)
                VALUES ({0}, {1}, 1, 'X', {2}, {3}, 'SYSTEM')";

            var rowsAffected = await _context.Database.ExecuteSqlRawAsync(sql, 
                sampleId, testId, scheduleType, DateTime.Now);
            
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling test {TestId} for sample {SampleId}", testId, sampleId);
            return false;
        }
    }

    private async Task<bool> RemoveScheduledTestAsync(int sampleId, int testId)
    {
        try
        {
            var sql = "DELETE FROM TestReadings WHERE sampleID = {0} AND testID = {1} AND status = 'X'";
            var rowsAffected = await _context.Database.ExecuteSqlRawAsync(sql, sampleId, testId);
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing scheduled test {TestId} for sample {SampleId}", testId, sampleId);
            return false;
        }
    }

    private async Task<DateTime?> GetSampleDateAsync(int sampleId)
    {
        try
        {
            return await _context.Database.SqlQueryRaw<DateTime?>(
                "SELECT sampleDate FROM UsedLubeSamples WHERE ID = {0}", sampleId)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sample date for sample {SampleId}", sampleId);
            return null;
        }
    }

    private async Task<bool> CanRemoveTestAsync(int sampleId, int testId, string tagNumber, string component, string location, DateTime sampleDate)
    {
        try
        {
            // Check if removing this test would violate minimum interval requirements
            var minInterval = await CheckMinimumIntervalAsync(tagNumber, component, location, testId, sampleDate);
            return minInterval; // If minimum interval is met, we can remove the test
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if test {TestId} can be removed for sample {SampleId}", testId, sampleId);
            return false;
        }
    }

    private async Task HandleSpecificTestSchedulingAsync(int sampleId, int completedTestId, TestSchedulingResult result)
    {
        try
        {
            // Handle specific test scheduling logic from legacy system
            switch (completedTestId)
            {
                case 30: // Small Spectroscopy -> Large Spectroscopy
                    if (await ScheduleTestAsync(sampleId, 40, "AUTO", "Scheduled after Small Spectroscopy"))
                    {
                        result.TestsAdded.Add(40);
                        result.Messages.Add("Scheduled Large Spectroscopy after Small Spectroscopy");
                    }
                    break;
                    
                case 40: // Large Spectroscopy -> Ferrography
                    if (await ScheduleFerrographyAsync(sampleId))
                    {
                        result.TestsAdded.Add(210);
                        result.Messages.Add("Scheduled Ferrography after Large Spectroscopy");
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling specific test scheduling for completed test {CompletedTestId}", completedTestId);
        }
    }
}