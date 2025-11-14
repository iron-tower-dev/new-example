using LabResultsApi.Models;
using LabResultsApi.DTOs;
using LabResultsApi.Data;
using Microsoft.EntityFrameworkCore;
using ValidationResult = LabResultsApi.Models.ValidationResult;

namespace LabResultsApi.Services;

public class ValidationService : IValidationService
{
    private readonly LabDbContext _context;
    private readonly ILogger<ValidationService> _logger;
    private readonly Dictionary<int, TestValidationRules> _testRules;

    public ValidationService(LabDbContext context, ILogger<ValidationService> logger)
    {
        _context = context;
        _logger = logger;
        _testRules = InitializeTestValidationRules();
    }

    public async Task<ValidationResult> ValidateTestResultAsync(int testId, SaveTestResultRequest request)
    {
        var result = new ValidationResult();

        try
        {
            if (request.SampleId <= 0)
            {
                result.AddError("SampleId", "Sample ID must be greater than zero");
                return result;
            }

            if (testId <= 0)
            {
                result.AddError("TestId", "Test ID must be greater than zero");
                return result;
            }

            var testExists = await _context.Tests.AnyAsync(t => t.TestId == testId && t.Active);
            if (!testExists)
            {
                result.AddError("TestId", "Test not found or inactive");
                return result;
            }

            var sampleExists = await _context.UsedLubeSamples.AnyAsync(s => s.Id == request.SampleId);
            if (!sampleExists)
            {
                result.AddError("SampleId", "Sample not found");
                return result;
            }

            result.IsValid = !result.HasErrors;
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating test result for test {TestId}", testId);
            result.AddError("General", "Validation failed due to system error");
            return result;
        }
    }

    public ValidationResult ValidateTrialData(int testId, object trialData, int trialNumber)
    {
        var result = new ValidationResult();

        if (trialNumber < 1 || trialNumber > 4)
        {
            result.AddError("TrialNumber", "Trial number must be between 1 and 4");
            return result;
        }

        if (trialData == null)
        {
            result.AddError("TrialData", "Trial data cannot be null");
            return result;
        }

        result.IsValid = !result.HasErrors;
        return result;
    }

    public TestValidationRules GetTestValidationRules(int testId)
    {
        return _testRules.GetValueOrDefault(testId, new TestValidationRules { TestId = testId });
    }

    public async Task<ValidationResult> ValidateEquipmentSelectionAsync(int testId, List<int> equipmentIds)
    {
        var result = new ValidationResult();

        if (equipmentIds == null || !equipmentIds.Any())
        {
            result.AddError("Equipment", "At least one piece of equipment must be selected");
            return result;
        }

        var duplicates = equipmentIds.GroupBy(id => id).Where(g => g.Count() > 1).Select(g => g.Key);
        foreach (var duplicate in duplicates)
        {
            result.AddError("Equipment", $"Equipment {duplicate} is selected multiple times");
        }

        result.IsValid = !result.HasErrors;
        return result;
    }

    public ValidationResult ValidateCrossFieldRules(int testId, Dictionary<string, object> fieldValues)
    {
        var result = new ValidationResult();
        result.IsValid = !result.HasErrors;
        return result;
    }

    public ValidationResult ValidateFileUpload(int testId, IFormFile file, int trialNumber)
    {
        var result = new ValidationResult();

        if (file == null || file.Length == 0)
        {
            result.AddError("File", "File is required");
            return result;
        }

        if (file.Length > 10 * 1024 * 1024)
        {
            result.AddError("File", "File size exceeds 10MB limit");
        }

        var allowedExtensions = new[] { ".dat", ".txt", ".csv", ".xlsx" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(fileExtension))
        {
            result.AddError("File", $"File type {fileExtension} is not allowed");
        }

        result.IsValid = !result.HasErrors;
        return result;
    }

    public List<FieldValidationRule> GetFieldValidationRules(int testId, string fieldName)
    {
        var testRules = GetTestValidationRules(testId);
        return testRules.FieldRules.GetValueOrDefault(fieldName, new List<FieldValidationRule>());
    }

    public ValidationResult ValidateCalculatedResults(int testId, Dictionary<string, object> inputValues, Dictionary<string, object> calculatedValues)
    {
        var result = new ValidationResult();
        result.IsValid = !result.HasErrors;
        return result;
    }

    private Dictionary<int, TestValidationRules> InitializeTestValidationRules()
    {
        var rules = new Dictionary<int, TestValidationRules>();

        rules[1] = new TestValidationRules
        {
            TestId = 1,
            TestName = "TAN by Color Indication",
            FieldRules = new Dictionary<string, List<FieldValidationRule>>
            {
                ["sampleWeight"] = new List<FieldValidationRule>
                {
                    new() { Type = "required", Message = "Sample weight is required" },
                    new() { Type = "positive", Message = "Sample weight must be positive" }
                }
            }
        };

        return rules;
    }

    // Additional validation methods for tests
    public ValidationResult ValidateTanData(double sampleWeight, double finalBuret)
    {
        var result = new ValidationResult();

        if (sampleWeight <= 0)
        {
            result.AddError("SampleWeight", "Sample weight must be greater than zero");
        }
        else if (sampleWeight < 0.1)
        {
            result.AddWarning("SampleWeight", "Sample weight is very low (<0.1g). Results may be inaccurate.");
        }

        if (finalBuret < 0)
        {
            result.AddError("FinalBuret", "Final buret reading cannot be negative");
        }

        var tanResult = (finalBuret * 5.61) / sampleWeight;
        if (tanResult > 50)
        {
            result.AddWarning("TanResult", "TAN value seems unusually high (>50 mg KOH/g). Please verify.");
        }

        result.IsValid = !result.HasErrors;
        return result;
    }

    public ValidationResult ValidateViscosityData(double stopwatchTime, double tubeCalibration)
    {
        var result = new ValidationResult();

        if (stopwatchTime <= 0)
        {
            result.AddError("StopwatchTime", "Stopwatch time must be greater than zero");
        }

        if (tubeCalibration <= 0)
        {
            result.AddError("TubeCalibration", "Tube calibration value must be greater than zero");
        }

        var viscosityResult = stopwatchTime * tubeCalibration;
        if (viscosityResult > 10000)
        {
            result.AddWarning("ViscosityResult", "Viscosity value seems unusually high (>10,000 cSt). Please verify.");
        }

        result.IsValid = !result.HasErrors;
        return result;
    }

    public ValidationResult ValidateParticleCountData(Dictionary<string, double> particleData)
    {
        var result = new ValidationResult();

        if (particleData == null || !particleData.Any())
        {
            result.AddError("ParticleData", "At least one particle count channel is required");
            return result;
        }

        foreach (var kvp in particleData)
        {
            if (kvp.Value < 0)
            {
                result.AddError($"Channel{kvp.Key}", "Particle count cannot be negative");
            }
            else if (kvp.Value > 1000000)
            {
                result.AddWarning($"Channel{kvp.Key}", "Particle count seems unusually high (>1,000,000). Please verify.");
            }
        }

        var sortedChannels = particleData.OrderBy(kvp => double.Parse(kvp.Key)).ToList();
        for (int i = 1; i < sortedChannels.Count; i++)
        {
            if (sortedChannels[i].Value > sortedChannels[i - 1].Value * 2)
            {
                result.AddWarning("Distribution", "Unusual particle count distribution detected. Please verify data.");
                break;
            }
        }

        result.IsValid = !result.HasErrors;
        return result;
    }

    public ValidationResult ValidateRequiredField(object value, string fieldName)
    {
        var result = new ValidationResult();

        if (value == null || (value is string str && string.IsNullOrWhiteSpace(str)))
        {
            result.AddError(fieldName, $"{fieldName} is required");
        }

        result.IsValid = !result.HasErrors;
        return result;
    }

    public ValidationResult ValidateNumericRange(double value, double min, double max, string fieldName)
    {
        var result = new ValidationResult();

        if (value < min || value > max)
        {
            result.AddError(fieldName, $"{fieldName} must be between {min} and {max}");
        }

        result.IsValid = !result.HasErrors;
        return result;
    }
}