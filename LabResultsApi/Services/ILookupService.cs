using LabResultsApi.DTOs;

namespace LabResultsApi.Services;

public interface ILookupService
{
    // NAS Lookup
    Task<NasLookupResult> CalculateNASAsync(NasLookupRequest request);
    Task<IEnumerable<NasLookupDto>> GetNASLookupTableAsync();
    Task<int?> GetNASForParticleCountAsync(int channel, int particleCount);
    Task<bool> RefreshNASCacheAsync();
    
    // NLGI Lookup
    Task<string?> GetNLGIForPenetrationAsync(int penetrationValue);
    Task<IEnumerable<NlgiLookupDto>> GetNLGILookupTableAsync();
    Task<bool> RefreshNLGICacheAsync();
    
    // MTE Equipment Lookup with Caching
    Task<IEnumerable<EquipmentSelectionDto>> GetCachedEquipmentByTypeAsync(string equipType, short? testId = null);
    Task<EquipmentCalibrationDto?> GetCachedEquipmentCalibrationAsync(int equipmentId);
    Task<bool> RefreshEquipmentCacheAsync();
    Task<bool> RefreshEquipmentCacheByTypeAsync(string equipType);
    
    // Particle Type Lookup
    Task<IEnumerable<ParticleTypeDefinitionDto>> GetParticleTypeDefinitionsAsync();
    Task<IEnumerable<ParticleSubTypeDefinitionDto>> GetParticleSubTypeDefinitionsAsync(int categoryId);
    Task<IEnumerable<ParticleSubTypeCategoryDefinitionDto>> GetParticleSubTypeCategoriesAsync();
    Task<bool> RefreshParticleTypeCacheAsync();
    
    // Comment Lookup
    Task<IEnumerable<CommentDto>> GetCommentsByAreaAsync(string area);
    Task<IEnumerable<CommentDto>> GetCommentsByAreaAndTypeAsync(string area, string type);
    Task<IEnumerable<string>> GetCommentAreasAsync();
    Task<IEnumerable<string>> GetCommentTypesAsync(string area);
    Task<bool> RefreshCommentCacheAsync();
    
    // Cache Management
    Task<bool> RefreshAllCachesAsync();
    Task<CacheStatusDto> GetCacheStatusAsync();
}