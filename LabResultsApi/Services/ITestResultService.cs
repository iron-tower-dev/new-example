using LabResultsApi.DTOs;

namespace LabResultsApi.Services;

public interface ITestResultService
{
    Task<IEnumerable<TestDto>> GetTestsAsync();
    Task<IEnumerable<TestDto>> GetQualifiedTestsAsync(string employeeId);
    Task<TestDto?> GetTestAsync(int testId);
    Task<TestTemplateDto?> GetTestTemplateAsync(int testId);
    Task<TestResultDto?> GetTestResultsAsync(int sampleId, int testId);
    Task<int> SaveTestResultsAsync(SaveTestResultRequest request);
    Task<int> UpdateTestResultsAsync(int sampleId, int testId, SaveTestResultRequest request);
    Task<int> DeleteTestResultsAsync(int sampleId, int testId);
    Task<TestCalculationResult> CalculateTestResultAsync(TestCalculationRequest request);
    Task<IEnumerable<TestResultDto>> GetTestResultsHistoryAsync(int sampleId, int testId, int count = 12);
}