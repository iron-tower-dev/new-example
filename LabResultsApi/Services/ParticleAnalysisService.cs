using Microsoft.EntityFrameworkCore;
using LabResultsApi.Data;
using LabResultsApi.DTOs;

namespace LabResultsApi.Services;

public class ParticleAnalysisService : IParticleAnalysisService
{
    private readonly LabDbContext _context;
    private readonly ILogger<ParticleAnalysisService> _logger;

    public ParticleAnalysisService(LabDbContext context, ILogger<ParticleAnalysisService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<ParticleTypeDto>> GetParticleTypesAsync()
    {
        _logger.LogInformation("Getting particle type definitions");
        
        try
        {
            var particleTypes = await _context.ParticleTypeDefinitions
                .Where(pt => pt.Active == "1")
                .OrderBy(pt => pt.SortOrder)
                .Select(pt => new ParticleTypeDto
                {
                    Id = pt.Id,
                    Type = pt.Type,
                    Description = pt.Description,
                    Image1 = pt.Image1,
                    Image2 = pt.Image2,
                    Active = pt.Active == "1",
                    SortOrder = pt.SortOrder ?? 0
                })
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} particle types", particleTypes.Count);
            return particleTypes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving particle types");
            throw;
        }
    }

    public async Task<IEnumerable<ParticleSubTypeCategoryDto>> GetSubTypeCategoriesAsync()
    {
        _logger.LogInformation("Getting particle sub-type categories");
        
        try
        {
            // Get categories
            var categories = await _context.ParticleSubTypeCategoryDefinitions
                .Where(c => c.Active == "1")
                .OrderBy(c => c.SortOrder)
                .ToListAsync();

            // Get sub-types for all categories
            var subTypes = await _context.ParticleSubTypeDefinitions
                .Where(st => st.Active == "1")
                .OrderBy(st => st.SortOrder)
                .ToListAsync();

            // Build the result with sub-types grouped by category
            var result = categories.Select(category => new ParticleSubTypeCategoryDto
            {
                Id = category.Id,
                Description = category.Description,
                Active = category.Active == "1",
                SortOrder = category.SortOrder ?? 0,
                SubTypes = subTypes
                    .Where(st => st.ParticleSubTypeCategoryId == category.Id)
                    .Select(st => new ParticleSubTypeDto
                    {
                        ParticleSubTypeCategoryId = st.ParticleSubTypeCategoryId,
                        Value = st.Value,
                        Description = st.Description,
                        Active = st.Active == "1",
                        SortOrder = st.SortOrder ?? 0
                    })
                    .ToList()
            }).ToList();

            _logger.LogInformation("Retrieved {Count} sub-type categories with {SubTypeCount} total sub-types", 
                result.Count, result.Sum(r => r.SubTypes.Count));
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving particle sub-type categories");
            throw;
        }
    }
}