using Microsoft.EntityFrameworkCore;
using LabResultsApi.Data;
using LabResultsApi.DTOs;
using LabResultsApi.Models;

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

    public async Task<IEnumerable<ParticleTypeDefinitionDto>> GetParticleTypesAsync()
    {
        _logger.LogInformation("Getting particle type definitions");
        
        try
        {
            var particleTypes = await _context.ParticleTypeDefinitions
                .Where(pt => pt.Active == "1")
                .OrderBy(pt => pt.SortOrder)
                .Select(pt => new ParticleTypeDefinitionDto
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

    public async Task<IEnumerable<ParticleSubTypeCategoryDefinitionDto>> GetSubTypeCategoriesAsync()
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
            var result = categories.Select(category => new ParticleSubTypeCategoryDefinitionDto
            {
                Id = category.Id,
                Description = category.Description,
                Active = category.Active == "1",
                SortOrder = category.SortOrder ?? 0,
                SubTypes = subTypes
                    .Where(st => st.ParticleSubTypeCategoryId == category.Id)
                    .Select(st => new ParticleSubTypeDefinitionDto
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

    // CRUD methods for particle analysis data
    public async Task<List<ParticleType>> GetParticleTypeDataAsync(int sampleId, int testId)
    {
        try
        {
            _logger.LogInformation("Getting particle types for sample {SampleId}, test {TestId}", sampleId, testId);
            
            return await _context.ParticleTypes
                .FromSqlRaw(@"
                    SELECT SampleID, testID, ParticleTypeDefinitionID, Status, Comments
                    FROM ParticleType 
                    WHERE SampleID = {0} AND testID = {1}", sampleId, testId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting particle types for sample {SampleId}, test {TestId}", sampleId, testId);
            throw;
        }
    }

    public async Task<List<ParticleSubType>> GetParticleSubTypeDataAsync(int sampleId, int testId)
    {
        try
        {
            _logger.LogInformation("Getting particle sub-types for sample {SampleId}, test {TestId}", sampleId, testId);
            
            return await _context.ParticleSubTypes
                .FromSqlRaw(@"
                    SELECT SampleID, testID, ParticleTypeDefinitionID, ParticleSubTypeCategoryID, Value
                    FROM ParticleSubType 
                    WHERE SampleID = {0} AND testID = {1}", sampleId, testId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting particle sub-types for sample {SampleId}, test {TestId}", sampleId, testId);
            throw;
        }
    }

    public async Task<InspectFilter?> GetInspectFilterAsync(int sampleId, int testId)
    {
        try
        {
            _logger.LogInformation("Getting inspect filter data for sample {SampleId}, test {TestId}", sampleId, testId);
            
            var results = await _context.InspectFilters
                .FromSqlRaw(@"
                    SELECT ID, testID, narrative, major, minor, trace
                    FROM InspectFilter 
                    WHERE ID = {0} AND testID = {1}", sampleId, testId)
                .ToListAsync();
                
            return results.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inspect filter data for sample {SampleId}, test {TestId}", sampleId, testId);
            throw;
        }
    }

    public async Task<int> SaveParticleTypeAsync(ParticleType particleType)
    {
        try
        {
            _logger.LogInformation("Saving particle type for sample {SampleId}, test {TestId}, particle type {ParticleTypeDefinitionId}", 
                particleType.SampleId, particleType.TestId, particleType.ParticleTypeDefinitionId);
            
            return await _context.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ParticleType (SampleID, testID, ParticleTypeDefinitionID, Status, Comments)
                VALUES ({0}, {1}, {2}, {3}, {4})",
                particleType.SampleId, particleType.TestId, particleType.ParticleTypeDefinitionId, 
                particleType.Status, particleType.Comments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving particle type for sample {SampleId}, test {TestId}", 
                particleType.SampleId, particleType.TestId);
            throw;
        }
    }

    public async Task<int> SaveParticleSubTypeAsync(ParticleSubType particleSubType)
    {
        try
        {
            _logger.LogInformation("Saving particle sub-type for sample {SampleId}, test {TestId}, particle type {ParticleTypeDefinitionId}, category {ParticleSubTypeCategoryId}", 
                particleSubType.SampleId, particleSubType.TestId, particleSubType.ParticleTypeDefinitionId, particleSubType.ParticleSubTypeCategoryId);
            
            return await _context.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ParticleSubType (SampleID, testID, ParticleTypeDefinitionID, ParticleSubTypeCategoryID, Value)
                VALUES ({0}, {1}, {2}, {3}, {4})",
                particleSubType.SampleId, particleSubType.TestId, particleSubType.ParticleTypeDefinitionId, 
                particleSubType.ParticleSubTypeCategoryId, particleSubType.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving particle sub-type for sample {SampleId}, test {TestId}", 
                particleSubType.SampleId, particleSubType.TestId);
            throw;
        }
    }

    public async Task<int> SaveInspectFilterAsync(InspectFilter inspectFilter)
    {
        try
        {
            _logger.LogInformation("Saving inspect filter data for sample {SampleId}, test {TestId}", 
                inspectFilter.Id, inspectFilter.TestId);
            
            return await _context.Database.ExecuteSqlRawAsync(@"
                INSERT INTO InspectFilter (ID, testID, narrative, major, minor, trace)
                VALUES ({0}, {1}, {2}, {3}, {4}, {5})",
                inspectFilter.Id, inspectFilter.TestId, inspectFilter.Narrative, 
                inspectFilter.Major, inspectFilter.Minor, inspectFilter.Trace);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving inspect filter data for sample {SampleId}, test {TestId}", 
                inspectFilter.Id, inspectFilter.TestId);
            throw;
        }
    }

    public async Task<int> DeleteParticleTypesAsync(int sampleId, int testId)
    {
        try
        {
            _logger.LogInformation("Deleting particle types for sample {SampleId}, test {TestId}", sampleId, testId);
            
            return await _context.Database.ExecuteSqlRawAsync(@"
                DELETE FROM ParticleType 
                WHERE SampleID = {0} AND testID = {1}", sampleId, testId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting particle types for sample {SampleId}, test {TestId}", sampleId, testId);
            throw;
        }
    }

    public async Task<int> DeleteParticleSubTypesAsync(int sampleId, int testId)
    {
        try
        {
            _logger.LogInformation("Deleting particle sub-types for sample {SampleId}, test {TestId}", sampleId, testId);
            
            return await _context.Database.ExecuteSqlRawAsync(@"
                DELETE FROM ParticleSubType 
                WHERE SampleID = {0} AND testID = {1}", sampleId, testId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting particle sub-types for sample {SampleId}, test {TestId}", sampleId, testId);
            throw;
        }
    }

    public async Task<int> DeleteInspectFilterAsync(int sampleId, int testId)
    {
        try
        {
            _logger.LogInformation("Deleting inspect filter data for sample {SampleId}, test {TestId}", sampleId, testId);
            
            return await _context.Database.ExecuteSqlRawAsync(@"
                DELETE FROM InspectFilter 
                WHERE ID = {0} AND testID = {1}", sampleId, testId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting inspect filter data for sample {SampleId}, test {TestId}", sampleId, testId);
            throw;
        }
    }
}
