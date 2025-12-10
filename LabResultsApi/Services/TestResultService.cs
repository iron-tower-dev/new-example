using Microsoft.EntityFrameworkCore;
using LabResultsApi.Data;
using LabResultsApi.DTOs;
using LabResultsApi.Models;

namespace LabResultsApi.Services;

public class TestResultService : ITestResultService
{
    private readonly LabDbContext _context;
    private readonly ICalculationService _calculationService;
    private readonly ISampleService _sampleService;
    private readonly ILogger<TestResultService> _logger;

    public TestResultService(
        LabDbContext context, 
        ICalculationService calculationService,
        ISampleService sampleService,
        ILogger<TestResultService> logger)
    {
        _context = context;
        _calculationService = calculationService;
        _sampleService = sampleService;
        _logger = logger;
    }

    public async Task<IEnumerable<TestDto>> GetTestsAsync()
    {
        _logger.LogInformation("Getting all available tests");
        
        try
        {
            var tests = await _context.Tests
                .Where(t => t.Exclude != "Y")
                .OrderBy(t => t.Name)
                .Select(t => new TestDto
                {
                    TestId = t.Id ?? 0,
                    TestName = t.Name ?? string.Empty,
                    TestDescription = t.Abbrev, // Use abbreviation as description
                    Active = t.Exclude != "Y"
                })
                .ToListAsync();

            return tests;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tests");
            throw;
        }
    }

    public async Task<IEnumerable<TestDto>> GetQualifiedTestsAsync(string employeeId)
    {
        _logger.LogInformation("Getting qualified tests for employee {EmployeeId}", employeeId);
        
        try
        {
            // Get user qualifications and map to available tests
            // Tests are directly linked to TestStands via Test.testStandID
            var qualifiedTests = await (from q in _context.LubeTechQualifications
                                      join t in _context.Tests on q.TestStandId equals t.TestStandId
                                      where q.EmployeeId == employeeId && 
                                            t.Exclude != "Y"
                                      select new TestDto
                                      {
                                          TestId = t.Id ?? 0,
                                          TestName = t.Name ?? string.Empty,
                                          TestDescription = t.Abbrev,
                                          Active = t.Exclude != "Y"
                                      })
                                      .Distinct()
                                      .OrderBy(t => t.TestName)
                                      .ToListAsync();

            _logger.LogInformation("Found {Count} qualified tests for employee {EmployeeId}", qualifiedTests.Count, employeeId);
            return qualifiedTests;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting qualified tests for employee {EmployeeId}", employeeId);
            throw;
        }
    }

    public async Task<TestDto?> GetTestAsync(int testId)
    {
        _logger.LogInformation("Getting test {TestId}", testId);
        
        try
        {
            var test = await _context.Tests
                .Where(t => t.Id == testId)
                .Select(t => new TestDto
                {
                    TestId = t.Id ?? 0,
                    TestName = t.Name ?? string.Empty,
                    TestDescription = t.Abbrev,
                    Active = t.Exclude != "Y"
                })
                .FirstOrDefaultAsync();

            return test;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting test {TestId}", testId);
            throw;
        }
    }

    public async Task<TestTemplateDto?> GetTestTemplateAsync(int testId)
    {
        _logger.LogInformation("Getting test template for test {TestId}", testId);
        
        try
        {
            var test = await GetTestAsync(testId);
            if (test == null) return null;

            // Generate test template based on test type
            var template = GenerateTestTemplate(test);
            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting test template for test {TestId}", testId);
            throw;
        }
    }

    public async Task<TestResultDto?> GetTestResultsAsync(int sampleId, int testId)
    {
        _logger.LogInformation("Getting test results for sample {SampleId}, test {TestId}", sampleId, testId);
        
        try
        {
            var testReadings = await GetTestReadingsAsync(sampleId, testId);
            
            if (!testReadings.Any()) return null;

            var result = new TestResultDto
            {
                SampleId = sampleId,
                TestId = testId,
                Status = testReadings.FirstOrDefault()?.Status ?? "X",
                EntryId = testReadings.FirstOrDefault()?.EntryId,
                EntryDate = testReadings.FirstOrDefault()?.EntryDate,
                Comments = testReadings.FirstOrDefault()?.MainComments,
                Trials = new List<TrialResultDto>()
            };

            foreach (var reading in testReadings.OrderBy(r => r.TrialNumber))
            {
                var trial = new TrialResultDto
                {
                    TrialNumber = reading.TrialNumber,
                    CalculatedResult = reading.TrialCalc,
                    IsComplete = reading.TrialComplete ?? false,
                    Values = new Dictionary<string, object?>()
                };

                // Map values based on test type
                MapTestReadingToTrialValues(testId, reading, trial.Values);
                
                result.Trials.Add(trial);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting test results for sample {SampleId}, test {TestId}", sampleId, testId);
            throw;
        }
    }

    public async Task<int> SaveTestResultsAsync(SaveTestResultRequest request)
    {
        _logger.LogInformation("Saving test results for sample {SampleId}, test {TestId}", request.SampleId, request.TestId);
        
        try
        {
            var totalSaved = 0;
            
            foreach (var trial in request.Trials)
            {
                var testReading = new TestReading
                {
                    SampleId = request.SampleId,
                    TestId = request.TestId,
                    TrialNumber = trial.TrialNumber,
                    Status = "E", // In Progress
                    EntryId = request.EntryId,
                    EntryDate = DateTime.UtcNow,
                    MainComments = request.Comments,
                    TrialComplete = trial.IsComplete
                };

                // Map trial values to TestReading fields based on test type
                MapTrialValuesToTestReading(request.TestId, trial.Values, testReading);

                // Calculate result if needed
                if (trial.Values.Any(v => v.Value != null))
                {
                    var calculationRequest = new TestCalculationRequest
                    {
                        TestId = request.TestId,
                        InputValues = trial.Values
                    };
                    
                    var calculationResult = await CalculateTestResultAsync(calculationRequest);
                    if (calculationResult.IsValid)
                    {
                        testReading.TrialCalc = calculationResult.Result;
                    }
                }

                var saved = await SaveTestReadingAsync(testReading);
                totalSaved += saved;
            }

            return totalSaved;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving test results for sample {SampleId}, test {TestId}", request.SampleId, request.TestId);
            throw;
        }
    }

    public async Task<int> UpdateTestResultsAsync(int sampleId, int testId, SaveTestResultRequest request)
    {
        _logger.LogInformation("Updating test results for sample {SampleId}, test {TestId}", sampleId, testId);
        
        try
        {
            // Delete existing results first
            await DeleteTestReadingsAsync(sampleId, testId);
            
            // Save new results
            return await SaveTestResultsAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating test results for sample {SampleId}, test {TestId}", sampleId, testId);
            throw;
        }
    }

    public async Task<int> DeleteTestResultsAsync(int sampleId, int testId)
    {
        _logger.LogInformation("Deleting test results for sample {SampleId}, test {TestId}", sampleId, testId);
        
        try
        {
            return await DeleteTestReadingsAsync(sampleId, testId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting test results for sample {SampleId}, test {TestId}", sampleId, testId);
            throw;
        }
    }

    public async Task<TestCalculationResult> CalculateTestResultAsync(TestCalculationRequest request)
    {
        _logger.LogInformation("Calculating test result for test {TestId}", request.TestId);
        
        try
        {
            return await _calculationService.CalculateAsync(request.TestId, request.InputValues);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating test result for test {TestId}", request.TestId);
            return new TestCalculationResult
            {
                IsValid = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<IEnumerable<TestResultDto>> GetTestResultsHistoryAsync(int sampleId, int testId, int count = 12)
    {
        _logger.LogInformation("Getting test results history for sample {SampleId}, test {TestId}", sampleId, testId);
        
        try
        {
            var historyData = await _sampleService.GetSampleHistoryAsync(sampleId, testId);
            
            var results = new List<TestResultDto>();
            
            foreach (var historyItem in historyData.Take(count))
            {
                var testResult = await GetTestResultsAsync(historyItem.SampleId, testId);
                if (testResult != null)
                {
                    results.Add(testResult);
                }
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting test results history for sample {SampleId}, test {TestId}", sampleId, testId);
            throw;
        }
    }

    private TestTemplateDto GenerateTestTemplate(TestDto test)
    {
        var template = new TestTemplateDto
        {
            TestId = test.TestId,
            TestName = test.TestName,
            TestDescription = test.TestDescription,
            MaxTrials = 4,
            RequiresCalculation = true,
            SupportsFileUpload = false,
            Fields = new List<TestFieldDto>()
        };

        // Generate fields based on test name/type
        switch (test.TestName?.ToUpper())
        {
            case "TAN":
            case "TAN BY COLOR INDICATION":
                template.Fields = GetTanTestFields();
                template.CalculationFormula = "(Final Buret * 5.61) / Sample Weight";
                break;
                
            case "WATER-KF":
            case "WATER BY KARL FISCHER":
                template.Fields = GetWaterKfTestFields();
                template.SupportsFileUpload = true;
                template.RequiresCalculation = false;
                break;
                
            case "TBN":
            case "TBN BY AUTO TITRATION":
                template.Fields = GetTbnTestFields();
                template.RequiresCalculation = false;
                break;
                
            default:
                template.Fields = GetGenericTestFields();
                template.RequiresCalculation = false;
                break;
        }

        return template;
    }

    private List<TestFieldDto> GetTanTestFields()
    {
        return new List<TestFieldDto>
        {
            new TestFieldDto
            {
                FieldName = "sampleWeight",
                DisplayName = "Sample Weight",
                FieldType = "number",
                IsRequired = true,
                MinValue = 0.01,
                DecimalPlaces = 2,
                Unit = "g",
                HelpText = "Weight of the sample in grams"
            },
            new TestFieldDto
            {
                FieldName = "finalBuret",
                DisplayName = "Final Buret",
                FieldType = "number",
                IsRequired = true,
                MinValue = 0,
                DecimalPlaces = 2,
                Unit = "mL",
                HelpText = "Final buret reading in milliliters"
            },
            new TestFieldDto
            {
                FieldName = "tanResult",
                DisplayName = "TAN Result",
                FieldType = "number",
                IsCalculated = true,
                DecimalPlaces = 2,
                Unit = "mg KOH/g",
                HelpText = "Calculated TAN value"
            }
        };
    }

    private List<TestFieldDto> GetWaterKfTestFields()
    {
        return new List<TestFieldDto>
        {
            new TestFieldDto
            {
                FieldName = "waterContent",
                DisplayName = "Water Content",
                FieldType = "number",
                IsRequired = true,
                MinValue = 0,
                DecimalPlaces = 3,
                Unit = "%",
                HelpText = "Water content percentage"
            },
            new TestFieldDto
            {
                FieldName = "dataFile",
                DisplayName = "Data File",
                FieldType = "file",
                IsRequired = false,
                HelpText = "Upload instrument data file"
            }
        };
    }

    private List<TestFieldDto> GetTbnTestFields()
    {
        return new List<TestFieldDto>
        {
            new TestFieldDto
            {
                FieldName = "tbnResult",
                DisplayName = "TBN Result",
                FieldType = "number",
                IsRequired = true,
                MinValue = 0,
                DecimalPlaces = 2,
                Unit = "mg KOH/g",
                HelpText = "Total Base Number result"
            }
        };
    }

    private List<TestFieldDto> GetGenericTestFields()
    {
        return new List<TestFieldDto>
        {
            new TestFieldDto
            {
                FieldName = "result",
                DisplayName = "Result",
                FieldType = "number",
                IsRequired = true,
                DecimalPlaces = 2,
                HelpText = "Test result value"
            }
        };
    }

    private void MapTestReadingToTrialValues(int testId, TestReading reading, Dictionary<string, object?> values)
    {
        // Map based on test type - this is a simplified mapping
        // In a real implementation, you might want to use a more sophisticated mapping system
        values["value1"] = reading.Value1;
        values["value2"] = reading.Value2;
        values["value3"] = reading.Value3;
        values["id1"] = reading.Id1;
        values["id2"] = reading.Id2;
        values["id3"] = reading.Id3;
    }

    private void MapTrialValuesToTestReading(int testId, Dictionary<string, object?> values, TestReading reading)
    {
        // Map trial values to TestReading fields based on test type
        if (values.TryGetValue("sampleWeight", out var sampleWeight))
            reading.Value1 = Convert.ToDouble(sampleWeight);
        
        if (values.TryGetValue("finalBuret", out var finalBuret))
            reading.Value2 = Convert.ToDouble(finalBuret);
            
        if (values.TryGetValue("waterContent", out var waterContent))
            reading.Value1 = Convert.ToDouble(waterContent);
            
        if (values.TryGetValue("tbnResult", out var tbnResult))
            reading.Value1 = Convert.ToDouble(tbnResult);
            
        if (values.TryGetValue("result", out var result))
            reading.Value1 = Convert.ToDouble(result);
    }

    #region TestReadings Data Access Methods

    private async Task<List<TestReading>> GetTestReadingsAsync(int sampleId, int testId)
    {
        try
        {
            _logger.LogInformation("Getting test readings for sample {SampleId}, test {TestId}", sampleId, testId);
            
            return await _context.TestReadings
                .FromSqlRaw(@"
                    SELECT sampleID, testID, trialNumber, value1, value2, value3, 
                           trialCalc, ID1, ID2, ID3, trialComplete, status, 
                           schedType, entryID, validateID, entryDate, valiDate, MainComments
                    FROM TestReadings 
                    WHERE sampleID = {0} AND testID = {1} 
                    ORDER BY trialNumber", sampleId, testId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting test readings for sample {SampleId}, test {TestId}", sampleId, testId);
            throw;
        }
    }

    private async Task<int> SaveTestReadingAsync(TestReading reading)
    {
        try
        {
            _logger.LogInformation("Saving test reading for sample {SampleId}, test {TestId}, trial {TrialNumber}", 
                reading.SampleId, reading.TestId, reading.TrialNumber);
            
            return await _context.Database.ExecuteSqlRawAsync(@"
                INSERT INTO TestReadings 
                (sampleID, testID, trialNumber, value1, value2, value3, trialCalc, 
                 ID1, ID2, ID3, status, entryDate, MainComments, entryID)
                VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13})",
                reading.SampleId, reading.TestId, reading.TrialNumber, 
                reading.Value1, reading.Value2, reading.Value3, reading.TrialCalc,
                reading.Id1, reading.Id2, reading.Id3, reading.Status, 
                reading.EntryDate, reading.MainComments, reading.EntryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving test reading for sample {SampleId}, test {TestId}", 
                reading.SampleId, reading.TestId);
            throw;
        }
    }

    private async Task<int> DeleteTestReadingsAsync(int sampleId, int testId)
    {
        try
        {
            _logger.LogInformation("Deleting test readings for sample {SampleId}, test {TestId}", sampleId, testId);
            
            return await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM TestReadings WHERE sampleID = {0} AND testID = {1}",
                sampleId, testId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting test readings for sample {SampleId}, test {TestId}", sampleId, testId);
            throw;
        }
    }

    #endregion
}
