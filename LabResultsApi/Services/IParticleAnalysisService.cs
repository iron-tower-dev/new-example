using LabResultsApi.DTOs;
using LabResultsApi.Models;

namespace LabResultsApi.Services;

public interface IParticleAnalysisService
{
    // Definition methods
    Task<IEnumerable<ParticleTypeDto>> GetParticleTypesAsync();
    Task<IEnumerable<ParticleSubTypeCategoryDto>> GetSubTypeCategoriesAsync();
    
    // CRUD methods for particle analysis data
    Task<List<ParticleType>> GetParticleTypeDataAsync(int sampleId, int testId);
    Task<List<ParticleSubType>> GetParticleSubTypeDataAsync(int sampleId, int testId);
    Task<InspectFilter?> GetInspectFilterAsync(int sampleId, int testId);
    Task<int> SaveParticleTypeAsync(ParticleType particleType);
    Task<int> SaveParticleSubTypeAsync(ParticleSubType particleSubType);
    Task<int> SaveInspectFilterAsync(InspectFilter inspectFilter);
    Task<int> DeleteParticleTypesAsync(int sampleId, int testId);
    Task<int> DeleteParticleSubTypesAsync(int sampleId, int testId);
    Task<int> DeleteInspectFilterAsync(int sampleId, int testId);
}
