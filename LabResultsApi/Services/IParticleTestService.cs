using LabResultsApi.Models;

namespace LabResultsApi.Services;

public interface IParticleTestService
{
    Task<FilterResidueResult?> GetFilterResidueAsync(int sampleId, int testId);
    Task<int> SaveFilterResidueAsync(FilterResidueResult filterResidue);
    Task<int> DeleteFilterResidueAsync(int sampleId, int testId);
    Task<DebrisIdentificationResult?> GetDebrisIdentificationAsync(int sampleId, int testId);
    Task<int> SaveDebrisIdentificationAsync(DebrisIdentificationResult debrisId);
    Task<int> DeleteDebrisIdentificationAsync(int sampleId, int testId);
}
