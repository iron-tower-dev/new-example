using LabResultsApi.Models;
using LabResultsApi.DTOs;

namespace LabResultsApi.Services;

public interface IValidationService
{
    /// <summary>
    /// Validate test result data for a specific test type
    /// </summary>
    Task<ValidationResult> ValidateTestResultAsync(int testId, SaveTestResultRequest request);

    /// <summary>
    /// Validate individual trial data
    /// </summary>
    ValidationResult ValidateTrialData(int testId, object trialData, int trialNumber);

    /// <summary>
    /// Get validation rules for a specific test
    /// </summary>
    TestValidationRules GetTestValidationRules(int testId);

    /// <summary>
    /// Validate equipment selection for test
    /// </summary>
    Task<ValidationResult> ValidateEquipmentSelectionAsync(int testId, List<int> equipmentIds);

    /// <summary>
    /// Validate cross-field dependencies
    /// </summary>
    ValidationResult ValidateCrossFieldRules(int testId, Dictionary<string, object> fieldValues);

    /// <summary>
    /// Validate file upload for test
    /// </summary>
    ValidationResult ValidateFileUpload(int testId, IFormFile file, int trialNumber);

    /// <summary>
    /// Get field-specific validation rules
    /// </summary>
    List<FieldValidationRule> GetFieldValidationRules(int testId, string fieldName);

    /// <summary>
    /// Validate calculated results
    /// </summary>
    ValidationResult ValidateCalculatedResults(int testId, Dictionary<string, object> inputValues, Dictionary<string, object> calculatedValues);

    /// <summary>
    /// Validate TAN test data
    /// </summary>
    ValidationResult ValidateTanData(double sampleWeight, double finalBuret);
}