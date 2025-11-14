using LabResultsApi.DTOs;

namespace LabResultsApi.Services;

public interface IParticleAnalysisService
{
    Task<IEnumerable<ParticleTypeDto>> GetParticleTypesAsync();
    Task<IEnumerable<ParticleSubTypeCategoryDto>> GetSubTypeCategoriesAsync();
}