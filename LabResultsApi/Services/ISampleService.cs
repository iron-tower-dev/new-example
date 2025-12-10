using LabResultsApi.DTOs;

namespace LabResultsApi.Services;

public interface ISampleService
{
    Task<IEnumerable<SampleDto>> GetSamplesByTestAsync(int testId);
    Task<IEnumerable<SampleDto>> GetSamplesAsync(SampleFilterDto? filter = null);
    Task<SampleDto?> GetSampleAsync(int sampleId);
    Task<IEnumerable<SampleHistoryDto>> GetSampleHistoryAsync(int sampleId, int testId);
    Task<bool> TestDatabaseConnectionAsync();
}
