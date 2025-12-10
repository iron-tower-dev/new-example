using LabResultsApi.Data;
using LabResultsApi.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace LabResultsApi.Services;

public class LookupService : ILookupService
{
    private readonly LabDbContext _context;
    private readonly ILogger<LookupService> _logger;
    private readonly IMemoryCache _cache;
    
    // Cache keys
    private const string NAS_CACHE_KEY = "nas_lookup_data";
    private const string NLGI_CACHE_KEY = "nlgi_lookup_data";
    private const string PARTICLE_TYPE_CACHE_KEY = "particle_types";
    private const string PARTICLE_SUBTYPE_CACHE_PREFIX = "particle_subtypes_";
    private const string PARTICLE_CATEGORY_CACHE_KEY = "particle_categories";
    
    // Cache expiration (1 hour)
    private static readonly TimeSpan _cacheExpiry = TimeSpan.FromHours(1);

    public LookupService(
        LabDbContext context, 
        ILogger<LookupService> logger,
        IMemoryCache cache)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
    }

    #region NAS Lookup Methods

    public async Task<NasLookupResult> CalculateNASAsync(NasLookupRequest request)
    {
        if (request.ParticleCounts == null || !request.ParticleCounts.Any())
        {
            return new NasLookupResult
            {
                IsValid = false,
                ErrorMessage = "No particle count data provided"
            };
        }

        var result = new NasLookupResult
        {
            ChannelNASValues = new Dictionary<int, int>(),
            IsValid = true
        };

        foreach (var (channel, count) in request.ParticleCounts)
        {
            if (count < 0) continue; // Skip negative values

            var nasValue = await GetNASForParticleCountAsync(channel, count);
            if (nasValue.HasValue && nasValue.Value > 0)
            {
                result.ChannelNASValues[channel] = nasValue.Value;
            }
        }

        result.HighestNAS = result.ChannelNASValues.Values.DefaultIfEmpty(0).Max();
        return result;
    }

    public async Task<IEnumerable<NasLookupDto>> GetNASLookupTableAsync()
    {
        return await _cache.GetOrCreateAsync(NAS_CACHE_KEY, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _cacheExpiry;
            
            var nasData = await _context.NasLookup
                .AsNoTracking()
                .OrderBy(n => n.Channel)
                .ThenBy(n => n.ValLo)
                .ToListAsync();

            return nasData.Select(n => new NasLookupDto
            {
                Channel = n.Channel ?? 0,
                ValLo = n.ValLo ?? 0,
                ValHi = n.ValHi ?? 0,
                NAS = n.NAS ?? 0
            }).ToList();
        }) ?? Enumerable.Empty<NasLookupDto>();
    }

    public async Task<int?> GetNASForParticleCountAsync(int channel, int particleCount)
    {
        if (channel < 1 || channel > 6 || particleCount < 0)
        {
            return null;
        }

        var nasTable = await GetNASLookupTableAsync();
        var channelData = nasTable.Where(n => n.Channel == channel).ToList();

        foreach (var entry in channelData)
        {
            if (particleCount >= entry.ValLo && particleCount <= entry.ValHi)
            {
                return entry.NAS;
            }
        }

        // If particle count is higher than all thresholds, return the highest NAS value
        return channelData.OrderByDescending(n => n.NAS).FirstOrDefault()?.NAS;
    }

    #endregion

    #region NLGI Lookup Methods

    public async Task<string?> GetNLGIForPenetrationAsync(int penetrationValue)
    {
        if (penetrationValue < 0)
        {
            return null;
        }

        var nlgiTable = await GetNLGILookupTableAsync();
        
        // Find matching range
        var match = nlgiTable.FirstOrDefault(n => 
            penetrationValue >= n.LowerValue && penetrationValue <= n.UpperValue);

        if (match != null)
        {
            return match.NLGIValue;
        }

        // Handle edge cases
        var minValue = nlgiTable.Min(n => n.LowerValue);
        var maxValue = nlgiTable.Max(n => n.UpperValue);

        if (penetrationValue < minValue)
        {
            return "6"; // Highest grade for very low penetration
        }

        if (penetrationValue > maxValue)
        {
            return "000"; // Lowest grade for very high penetration
        }

        return "2"; // Default grade
    }

    public async Task<IEnumerable<NlgiLookupDto>> GetNLGILookupTableAsync()
    {
        return await _cache.GetOrCreateAsync(NLGI_CACHE_KEY, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _cacheExpiry;
            
            var nlgiData = await _context.NlgiLookup
                .AsNoTracking()
                .OrderBy(n => n.LowerValue)
                .ToListAsync();

            return nlgiData.Select(n => new NlgiLookupDto
            {
                LowerValue = n.LowerValue ?? 0,
                UpperValue = n.UpperValue ?? 0,
                NLGIValue = n.NLGIValue ?? ""
            }).ToList();
        }) ?? Enumerable.Empty<NlgiLookupDto>();
    }

    #endregion

    #region Particle Type Lookup Methods

    public async Task<IEnumerable<ParticleTypeDefinitionDto>> GetParticleTypeDefinitionsAsync()
    {
        return await _cache.GetOrCreateAsync(PARTICLE_TYPE_CACHE_KEY, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _cacheExpiry;
            
            var particleTypes = await _context.ParticleTypeDefinitions
                .AsNoTracking()
                .Where(pt => pt.Active == "1")
                .OrderBy(pt => pt.SortOrder)
                .ToListAsync();

            return particleTypes.Select(pt => new ParticleTypeDefinitionDto
            {
                Id = pt.Id,
                Type = pt.Type,
                Description = pt.Description,
                Image1 = pt.Image1,
                Image2 = pt.Image2,
                Active = pt.Active == "1",
                SortOrder = pt.SortOrder ?? 0
            }).ToList();
        }) ?? Enumerable.Empty<ParticleTypeDefinitionDto>();
    }

    public async Task<IEnumerable<ParticleSubTypeDefinitionDto>> GetParticleSubTypeDefinitionsAsync(int categoryId)
    {
        var cacheKey = $"{PARTICLE_SUBTYPE_CACHE_PREFIX}{categoryId}";
        
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _cacheExpiry;
            
            var subTypes = await _context.ParticleSubTypeDefinitions
                .AsNoTracking()
                .Where(st => st.Active == "1" && st.ParticleSubTypeCategoryId == categoryId)
                .OrderBy(st => st.SortOrder)
                .ToListAsync();

            return subTypes.Select(st => new ParticleSubTypeDefinitionDto
            {
                ParticleSubTypeCategoryId = st.ParticleSubTypeCategoryId,
                Value = st.Value,
                Description = st.Description,
                Active = st.Active == "1",
                SortOrder = st.SortOrder ?? 0
            }).ToList();
        }) ?? Enumerable.Empty<ParticleSubTypeDefinitionDto>();
    }

    public async Task<IEnumerable<ParticleSubTypeCategoryDefinitionDto>> GetParticleSubTypeCategoriesAsync()
    {
        return await _cache.GetOrCreateAsync(PARTICLE_CATEGORY_CACHE_KEY, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _cacheExpiry;
            
            // Get categories
            var categories = await _context.ParticleSubTypeCategoryDefinitions
                .AsNoTracking()
                .Where(c => c.Active == "1")
                .OrderBy(c => c.SortOrder)
                .ToListAsync();

            // Get sub-types for all categories
            var subTypes = await _context.ParticleSubTypeDefinitions
                .AsNoTracking()
                .Where(st => st.Active == "1")
                .OrderBy(st => st.SortOrder)
                .ToListAsync();

            // Build the result with sub-types grouped by category
            return categories.Select(category => new ParticleSubTypeCategoryDefinitionDto
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
        }) ?? Enumerable.Empty<ParticleSubTypeCategoryDefinitionDto>();
    }

    #endregion
}
