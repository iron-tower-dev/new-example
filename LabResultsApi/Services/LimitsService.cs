using Microsoft.EntityFrameworkCore;
using LabResultsApi.Data;
using LabResultsApi.DTOs;

namespace LabResultsApi.Services;

public class LimitsService : ILimitsService
{
    private readonly LabDbContext _context;
    private readonly ILogger<LimitsService> _logger;

    public LimitsService(LabDbContext context, ILogger<LimitsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<LimitsCheckResult> CheckLimitsAsync(int sampleId, int testId, Dictionary<string, double> results)
    {
        _logger.LogInformation("Checking limits for sample {SampleId}, test {TestId}", sampleId, testId);
        
        var result = new LimitsCheckResult { IsWithinLimits = true };
        
        try
        {
            // Get equipment info for the sample
            var equipmentInfo = await GetSampleEquipmentInfoAsync(sampleId);
            if (equipmentInfo == null)
            {
                result.Message = "Could not determine equipment information for sample";
                return result;
            }

            // Get limits for this equipment/test combination
            var limits = await GetLimitsAsync(equipmentInfo.TagNumber, equipmentInfo.Component, equipmentInfo.Location, testId);
            
            foreach (var kvp in results)
            {
                var parameter = kvp.Key;
                var value = kvp.Value;
                
                var limit = limits.FirstOrDefault(l => l.Parameter.Equals(parameter, StringComparison.OrdinalIgnoreCase));
                if (limit != null)
                {
                    // Check high limits
                    if (limit.HighLimit.HasValue && value > limit.HighLimit.Value)
                    {
                        result.IsWithinLimits = false;
                        result.Violations.Add(new LimitViolation
                        {
                            Parameter = parameter,
                            ActualValue = value,
                            LimitValue = limit.HighLimit.Value,
                            LimitType = "HIGH",
                            Severity = "CRITICAL",
                            Message = $"{parameter} value {value} exceeds high limit of {limit.HighLimit.Value}"
                        });
                    }
                    else if (limit.HighWarning.HasValue && value > limit.HighWarning.Value)
                    {
                        result.Warnings.Add(new LimitWarning
                        {
                            Parameter = parameter,
                            ActualValue = value,
                            WarningValue = limit.HighWarning.Value,
                            Message = $"{parameter} value {value} exceeds high warning of {limit.HighWarning.Value}"
                        });
                    }

                    // Check low limits
                    if (limit.LowLimit.HasValue && value < limit.LowLimit.Value)
                    {
                        result.IsWithinLimits = false;
                        result.Violations.Add(new LimitViolation
                        {
                            Parameter = parameter,
                            ActualValue = value,
                            LimitValue = limit.LowLimit.Value,
                            LimitType = "LOW",
                            Severity = "CRITICAL",
                            Message = $"{parameter} value {value} is below low limit of {limit.LowLimit.Value}"
                        });
                    }
                    else if (limit.LowWarning.HasValue && value < limit.LowWarning.Value)
                    {
                        result.Warnings.Add(new LimitWarning
                        {
                            Parameter = parameter,
                            ActualValue = value,
                            WarningValue = limit.LowWarning.Value,
                            Message = $"{parameter} value {value} is below low warning of {limit.LowWarning.Value}"
                        });
                    }
                }
            }

            // Determine overall status
            if (result.Violations.Any())
            {
                result.OverallStatus = "CRITICAL";
            }
            else if (result.Warnings.Any())
            {
                result.OverallStatus = "WARNING";
            }
            else
            {
                result.OverallStatus = "NORMAL";
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking limits for sample {SampleId}, test {TestId}", sampleId, testId);
            result.IsWithinLimits = false;
            result.Message = $"Error checking limits: {ex.Message}";
            return result;
        }
    }

    public async Task<bool> IsResultWithinLimitsAsync(int sampleId, int testId, string parameter, double result)
    {
        try
        {
            var results = new Dictionary<string, double> { { parameter, result } };
            var limitsCheck = await CheckLimitsAsync(sampleId, testId, results);
            return limitsCheck.IsWithinLimits;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if result is within limits for sample {SampleId}, test {TestId}, parameter {Parameter}", 
                sampleId, testId, parameter);
            return false;
        }
    }

    public async Task<IEnumerable<TestLimitDto>> GetLimitsAsync(string tagNumber, string component, string location, int testId)
    {
        try
        {
            var sql = @"
                SELECT 
                    l.ID as LimitId,
                    l.testID as TestId,
                    l.parameter as Parameter,
                    l.highLimit as HighLimit,
                    l.lowLimit as LowLimit,
                    l.highWarning as HighWarning,
                    l.lowWarning as LowWarning,
                    l.limitType as LimitType,
                    l.qualityClass as QualityClass,
                    l.isActive as IsActive
                FROM limits l
                INNER JOIN limits_xref lx ON l.ID = lx.limitID
                WHERE l.testID = @testId AND l.isActive = 1
                    AND (l.tagNumber = @tagNumber OR l.tagNumber IS NULL OR l.tagNumber = '')
                    AND (l.component = @component OR l.component IS NULL OR l.component = '')
                    AND (l.location = @location OR l.location IS NULL OR l.location = '')
                ORDER BY l.parameter";

            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            
            var testIdParam = command.CreateParameter();
            testIdParam.ParameterName = "@testId";
            testIdParam.Value = testId;
            command.Parameters.Add(testIdParam);
            
            var tagParam = command.CreateParameter();
            tagParam.ParameterName = "@tagNumber";
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
            
            var results = new List<TestLimitDto>();
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                results.Add(new TestLimitDto
                {
                    LimitId = Convert.ToInt32(reader["LimitId"]),
                    TestId = Convert.ToInt32(reader["TestId"]),
                    Parameter = reader["Parameter"]?.ToString() ?? string.Empty,
                    HighLimit = reader["HighLimit"] == DBNull.Value ? null : Convert.ToDouble(reader["HighLimit"]),
                    LowLimit = reader["LowLimit"] == DBNull.Value ? null : Convert.ToDouble(reader["LowLimit"]),
                    HighWarning = reader["HighWarning"] == DBNull.Value ? null : Convert.ToDouble(reader["HighWarning"]),
                    LowWarning = reader["LowWarning"] == DBNull.Value ? null : Convert.ToDouble(reader["LowWarning"]),
                    LimitType = reader["LimitType"] == DBNull.Value ? string.Empty : reader["LimitType"]?.ToString() ?? string.Empty,
                    QualityClass = reader["QualityClass"] == DBNull.Value ? string.Empty : reader["QualityClass"]?.ToString() ?? string.Empty,
                    IsActive = Convert.ToBoolean(reader["IsActive"])
                });
            }
            
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting limits for equipment {TagNumber}-{Component}-{Location}, test {TestId}", 
                tagNumber, component, location, testId);
            return new List<TestLimitDto>();
        }
    }

    public async Task<IEnumerable<LimitsCrossReferenceDto>> GetLimitsCrossReferenceAsync(int testId)
    {
        try
        {
            var sql = @"
                SELECT 
                    lx.ID as CrossRefId,
                    lx.testID as TestId,
                    lx.limitID as LimitId,
                    l.parameter as Parameter,
                    lx.condition as Condition,
                    lx.isActive as IsActive
                FROM limits_xref lx
                INNER JOIN limits l ON lx.limitID = l.ID
                WHERE lx.testID = @testId AND lx.isActive = 1";

            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            
            var testIdParam = command.CreateParameter();
            testIdParam.ParameterName = "@testId";
            testIdParam.Value = testId;
            command.Parameters.Add(testIdParam);
            
            var results = new List<LimitsCrossReferenceDto>();
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                results.Add(new LimitsCrossReferenceDto
                {
                    CrossRefId = Convert.ToInt32(reader["CrossRefId"]),
                    TestId = Convert.ToInt32(reader["TestId"]),
                    LimitId = Convert.ToInt32(reader["LimitId"]),
                    Parameter = reader["Parameter"]?.ToString() ?? string.Empty,
                    Condition = reader["Condition"] == DBNull.Value ? string.Empty : reader["Condition"]?.ToString() ?? string.Empty,
                    IsActive = Convert.ToBoolean(reader["IsActive"])
                });
            }
            
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting limits cross-reference for test {TestId}", testId);
            return new List<LimitsCrossReferenceDto>();
        }
    }

    public async Task<TestEvaluationResult> EvaluateTestResultsAsync(int sampleId, int testId, Dictionary<string, double> results)
    {
        try
        {
            var evaluation = new TestEvaluationResult();
            
            // Check limits first
            var limitsCheck = await CheckLimitsAsync(sampleId, testId, results);
            
            if (limitsCheck.Violations.Any())
            {
                evaluation.Status = "CRITICAL";
                evaluation.RequiresReview = true;
                evaluation.ReviewerLevel = "Q/QAG";
                evaluation.Recommendation = "Results exceed critical limits. Immediate review required.";
                evaluation.Alerts.AddRange(limitsCheck.Violations.Select(v => v.Message));
            }
            else if (limitsCheck.Warnings.Any())
            {
                evaluation.Status = "CAUTION";
                evaluation.RequiresReview = true;
                evaluation.ReviewerLevel = "TRAIN";
                evaluation.Recommendation = "Results show warning conditions. Review recommended.";
                evaluation.Alerts.AddRange(limitsCheck.Warnings.Select(w => w.Message));
            }
            else
            {
                evaluation.Status = "NORMAL";
                evaluation.RequiresReview = false;
                evaluation.Recommendation = "Results are within normal limits.";
            }

            // Check for trending issues
            await CheckTrendingAsync(sampleId, testId, results, evaluation);

            return evaluation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating test results for sample {SampleId}, test {TestId}", sampleId, testId);
            return new TestEvaluationResult
            {
                Status = "ERROR",
                RequiresReview = true,
                ReviewerLevel = "Q/QAG",
                Recommendation = "Error evaluating results. Manual review required."
            };
        }
    }

    public async Task<IEnumerable<LcdeLimitDto>> GetLcdeLimitsAsync(string tagNumber, string component, string location)
    {
        try
        {
            var sql = @"
                SELECT 
                    ll.ID as LcdeId,
                    ll.tagNumber as TagNumber,
                    ll.component as Component,
                    ll.location as Location,
                    ll.parameter as Parameter,
                    ll.normalHigh as NormalHigh,
                    ll.normalLow as NormalLow,
                    ll.cautionHigh as CautionHigh,
                    ll.cautionLow as CautionLow,
                    ll.criticalHigh as CriticalHigh,
                    ll.criticalLow as CriticalLow,
                    ll.effectiveDate as EffectiveDate
                FROM lcde_limits ll
                WHERE ll.tagNumber = @tagNumber AND ll.component = @component AND ll.location = @location
                    AND (ll.effectiveDate IS NULL OR ll.effectiveDate <= GETDATE())
                ORDER BY ll.parameter, ll.effectiveDate DESC";

            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            
            var tagParam = command.CreateParameter();
            tagParam.ParameterName = "@tagNumber";
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
            
            var results = new List<LcdeLimitDto>();
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                results.Add(new LcdeLimitDto
                {
                    LcdeId = Convert.ToInt32(reader["LcdeId"]),
                    TagNumber = reader["TagNumber"]?.ToString() ?? string.Empty,
                    Component = reader["Component"]?.ToString() ?? string.Empty,
                    Location = reader["Location"]?.ToString() ?? string.Empty,
                    Parameter = reader["Parameter"]?.ToString() ?? string.Empty,
                    NormalHigh = reader["NormalHigh"] == DBNull.Value ? null : Convert.ToDouble(reader["NormalHigh"]),
                    NormalLow = reader["NormalLow"] == DBNull.Value ? null : Convert.ToDouble(reader["NormalLow"]),
                    CautionHigh = reader["CautionHigh"] == DBNull.Value ? null : Convert.ToDouble(reader["CautionHigh"]),
                    CautionLow = reader["CautionLow"] == DBNull.Value ? null : Convert.ToDouble(reader["CautionLow"]),
                    CriticalHigh = reader["CriticalHigh"] == DBNull.Value ? null : Convert.ToDouble(reader["CriticalHigh"]),
                    CriticalLow = reader["CriticalLow"] == DBNull.Value ? null : Convert.ToDouble(reader["CriticalLow"]),
                    EffectiveDate = reader["EffectiveDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["EffectiveDate"])
                });
            }
            
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting LCDE limits for equipment {TagNumber}-{Component}-{Location}", 
                tagNumber, component, location);
            return new List<LcdeLimitDto>();
        }
    }

    public async Task<AlertResult> CheckAlertThresholdsAsync(int sampleId, int testId, Dictionary<string, double> results)
    {
        try
        {
            var alertResult = new AlertResult();
            
            // Get equipment info
            var equipmentInfo = await GetSampleEquipmentInfoAsync(sampleId);
            if (equipmentInfo == null)
            {
                return alertResult;
            }

            // Get LCDE limits for more sophisticated alerting
            var lcdeLimits = await GetLcdeLimitsAsync(equipmentInfo.TagNumber, equipmentInfo.Component, equipmentInfo.Location);
            
            foreach (var kvp in results)
            {
                var parameter = kvp.Key;
                var value = kvp.Value;
                
                var lcdeLimit = lcdeLimits.FirstOrDefault(l => l.Parameter.Equals(parameter, StringComparison.OrdinalIgnoreCase));
                if (lcdeLimit != null)
                {
                    // Check critical thresholds
                    if ((lcdeLimit.CriticalHigh.HasValue && value > lcdeLimit.CriticalHigh.Value) ||
                        (lcdeLimit.CriticalLow.HasValue && value < lcdeLimit.CriticalLow.Value))
                    {
                        alertResult.HasAlerts = true;
                        alertResult.Alerts.Add(new Alert
                        {
                            Type = "CRITICAL",
                            Parameter = parameter,
                            Message = $"Critical threshold exceeded for {parameter}: {value}",
                            Severity = "HIGH",
                            AlertDate = DateTime.Now
                        });
                    }
                    // Check caution thresholds
                    else if ((lcdeLimit.CautionHigh.HasValue && value > lcdeLimit.CautionHigh.Value) ||
                             (lcdeLimit.CautionLow.HasValue && value < lcdeLimit.CautionLow.Value))
                    {
                        alertResult.HasAlerts = true;
                        alertResult.Alerts.Add(new Alert
                        {
                            Type = "LIMIT",
                            Parameter = parameter,
                            Message = $"Caution threshold exceeded for {parameter}: {value}",
                            Severity = "MEDIUM",
                            AlertDate = DateTime.Now
                        });
                    }
                }
            }

            return alertResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking alert thresholds for sample {SampleId}, test {TestId}", sampleId, testId);
            return new AlertResult();
        }
    }

    private async Task<SampleEquipmentInfo?> GetSampleEquipmentInfoAsync(int sampleId)
    {
        try
        {
            var sql = @"
                SELECT tagNumber, component, location, qualityClass
                FROM UsedLubeSamples 
                WHERE ID = @sampleId";

            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            
            var sampleIdParam = command.CreateParameter();
            sampleIdParam.ParameterName = "@sampleId";
            sampleIdParam.Value = sampleId;
            command.Parameters.Add(sampleIdParam);
            
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                return new SampleEquipmentInfo
                {
                    TagNumber = reader["tagNumber"]?.ToString() ?? string.Empty,
                    Component = reader["component"]?.ToString() ?? string.Empty,
                    Location = reader["location"]?.ToString() ?? string.Empty,
                    QualityClass = reader["qualityClass"] == DBNull.Value ? string.Empty : reader["qualityClass"]?.ToString() ?? string.Empty
                };
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting equipment info for sample {SampleId}", sampleId);
            return null;
        }
    }

    private async Task CheckTrendingAsync(int sampleId, int testId, Dictionary<string, double> results, TestEvaluationResult evaluation)
    {
        try
        {
            // Get equipment info
            var equipmentInfo = await GetSampleEquipmentInfoAsync(sampleId);
            if (equipmentInfo == null) return;

            // Get historical results for trending analysis
            var sql = @"
                SELECT TOP 5 tr.value1, tr.value2, tr.value3, s.sampleDate
                FROM TestReadings tr
                INNER JOIN UsedLubeSamples s ON tr.sampleID = s.ID
                WHERE s.tagNumber = @tagNumber AND s.component = @component AND s.location = @location
                    AND tr.testID = @testId AND tr.status IN ('C', 'D')
                    AND s.ID < @sampleId
                ORDER BY s.sampleDate DESC";

            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            
            var tagParam = command.CreateParameter();
            tagParam.ParameterName = "@tagNumber";
            tagParam.Value = equipmentInfo.TagNumber;
            command.Parameters.Add(tagParam);
            
            var compParam = command.CreateParameter();
            compParam.ParameterName = "@component";
            compParam.Value = equipmentInfo.Component;
            command.Parameters.Add(compParam);
            
            var locParam = command.CreateParameter();
            locParam.ParameterName = "@location";
            locParam.Value = equipmentInfo.Location;
            command.Parameters.Add(locParam);
            
            var testIdParam = command.CreateParameter();
            testIdParam.ParameterName = "@testId";
            testIdParam.Value = testId;
            command.Parameters.Add(testIdParam);
            
            var sampleIdParam = command.CreateParameter();
            sampleIdParam.ParameterName = "@sampleId";
            sampleIdParam.Value = sampleId;
            command.Parameters.Add(sampleIdParam);
            
            var historicalValues = new List<double[]>();
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                historicalValues.Add(new double[]
                {
                    reader["value1"] == DBNull.Value ? 0 : Convert.ToDouble(reader["value1"]),
                    reader["value2"] == DBNull.Value ? 0 : Convert.ToDouble(reader["value2"]),
                    reader["value3"] == DBNull.Value ? 0 : Convert.ToDouble(reader["value3"])
                });
            }

            // Simple trending analysis - check if values are consistently increasing
            if (historicalValues.Count >= 3)
            {
                // Check for upward trend in first value (simplified)
                var recentValues = historicalValues.Take(3).Select(v => v[0]).ToList();
                if (recentValues.All(v => v > 0) && IsIncreasingTrend(recentValues))
                {
                    evaluation.Alerts.Add("Upward trend detected in recent results");
                    if (evaluation.Status == "NORMAL")
                    {
                        evaluation.Status = "CAUTION";
                        evaluation.RequiresReview = true;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking trending for sample {SampleId}, test {TestId}", sampleId, testId);
        }
    }

    private static bool IsIncreasingTrend(List<double> values)
    {
        for (int i = 1; i < values.Count; i++)
        {
            if (values[i] <= values[i - 1])
                return false;
        }
        return true;
    }

    private class SampleEquipmentInfo
    {
        public string TagNumber { get; set; } = string.Empty;
        public string Component { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string QualityClass { get; set; } = string.Empty;
    }
}