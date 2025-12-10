using LabResultsApi.DTOs;

namespace LabResultsApi.Services;

public interface ILookupService
{
    // NAS Lookup
    Task<NasLookupResult> CalculateNASAsync(NasLookupRequest request);
    Task<IEnumerable<NasLookupDto>> GetNASLookupTableAsync();
    Task<int?> GetNASForParticleCountAsync(int channel, int particleCount);
    
    // NLGI Lookup
    Task<string?> GetNLGIForPenetrationAsync(int penetrationValue);
    Task<IEnumerable<NlgiLookupDto>> GetNLGILookupTableAsync();
    
    // Particle Type Lookup
    Task<IEnumerable<ParticleTypeDefinitionDto>> GetParticleTypeDefinitionsAsync();
    Task<IEnumerable<ParticleSubTypeDefinitionDto>> GetParticleSubTypeDefinitionsAsync(int categoryId);
    Task<IEnumerable<ParticleSubTypeCategoryDefinitionDto>> GetParticleSubTypeCategoriesAsync();
}
