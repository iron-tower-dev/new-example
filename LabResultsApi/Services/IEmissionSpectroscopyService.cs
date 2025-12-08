using LabResultsApi.Models;

namespace LabResultsApi.Services;

public interface IEmissionSpectroscopyService
{
    Task<List<EmissionSpectroscopy>> GetEmissionSpectroscopyAsync(int sampleId, int testId);
    Task<int> SaveEmissionSpectroscopyAsync(EmissionSpectroscopy data);
    Task<int> UpdateEmissionSpectroscopyAsync(EmissionSpectroscopy data);
    Task<int> DeleteEmissionSpectroscopyAsync(int sampleId, int testId, int trialNum);
    Task<int> ScheduleFerrographyAsync(int sampleId);
    Task<bool> SampleExistsAsync(int sampleId);
}
