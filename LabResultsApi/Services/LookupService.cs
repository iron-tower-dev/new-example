using LabResultsApi.Data;
using LabResultsApi.Models;
using LabResultsApi.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using System.Collections;

namespace LabResultsApi.Services;

public class LookupService : ILookupService
{
    private readonly LabDbContext _context;
    private readonly ILogger<LookupService> _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly IPerformanceMonitoringService _performanceService;
    private readonly Dictionary<int, List<NasLookup>> _nasCache = new();
    private readonly List<NlgiLookup> _nlgiCache = new();
    private DateTime _lastCacheRefresh = DateTime.MinValue;
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromHours(1);

    // Cache keys
    private const string NAS_CACHE_KEY = "nas_lookup_data";
    private const string NLGI_CACHE_KEY = "nlgi_lookup_data";
    private const string EQUIPMENT_CACHE_KEY_PREFIX = "equipment_";

    public LookupService(
        LabDbContext context, 
        ILogger<LookupService> logger,
        IMemoryCache memoryCache,
        IPerformanceMonitoringService performanceService)
    {
        _context = context;
        _logger = logger;
        _memoryCache = memoryCache;
        _performanceService = performanceService;
    }

    /// <summary>
    /// Get NAS value for a specific channel and particle count
    /// </summary>
    public async Task<int> GetNasValueAsync(int channel, double particleCount)
    {
        var stopwatch = Stopwatch.StartNew();
        var cacheKey = $"{NAS_CACHE_KEY}_{channel}";
        
        try
        {
            // Try memory cache first
            if (_memoryCache.TryGetValue(cacheKey, out List<NasLookup>? cachedData))
            {
                _performanceService.RecordCacheOperation(cacheKey, true, stopwatch.Elapsed);
                return FindNasValue(cachedData!, particleCount);
            }

            await EnsureCacheIsCurrentAsync();

            if (!_nasCache.ContainsKey(channel))
            {
                _performanceService.RecordCacheOperation(cacheKey, false, stopwatch.Elapsed);
                return 0;
            }

            var channelData = _nasCache[channel].OrderBy(n => n.ValLo).ToList();
            
            // Cache in memory cache for faster subsequent access
            _memoryCache.Set(cacheKey, channelData, _cacheExpiry);
            _performanceService.RecordCacheOperation(cacheKey, false, stopwatch.Elapsed);
            
            return FindNasValue(channelData, particleCount);
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    private static int FindNasValue(List<NasLookup> channelData, double particleCount)
    {
        // Find the appropriate NAS value based on particle count
        for (int i = 0; i < channelData.Count; i++)
        {
            if (particleCount >= channelData[i].ValLo && particleCount <= channelData[i].ValHi)
            {
                return channelData[i].NAS ?? 0;
            }
        }

        // If particle count is higher than all thresholds, return the highest NAS value
        return channelData.LastOrDefault()?.NAS ?? 0;
    }

    /// <summary>
    /// Calculate the highest NAS value from multiple channels
    /// </summary>
    public async Task<NasCalculationResult> CalculateHighestNasAsync(Dictionary<int, double> particleCounts)
    {
        var result = new NasCalculationResult
        {
            ChannelNasValues = new Dictionary<int, int>(),
            IsValid = true
        };

        if (!particleCounts.Any())
        {
            result.IsValid = false;
            result.ErrorMessage = "No particle count data provided";
            return result;
        }

        foreach (var kvp in particleCounts)
        {
            if (kvp.Value < 0)
                continue; // Skip negative values

            var nasValue = await GetNasValueAsync(kvp.Key, kvp.Value);
            if (nasValue > 0)
            {
                result.ChannelNasValues[kvp.Key] = nasValue;
            }
        }

        result.HighestNas = result.ChannelNasValues.Values.DefaultIfEmpty(0).Max();
        return result;
    }

    /// <summary>
    /// Get NLGI grade for a penetration value
    /// </summary>
    public async Task<string> GetNlgiGradeAsync(double penetrationValue)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Try memory cache first
            if (_memoryCache.TryGetValue(NLGI_CACHE_KEY, out List<NlgiLookup>? cachedData))
            {
                _performanceService.RecordCacheOperation(NLGI_CACHE_KEY, true, stopwatch.Elapsed);
                return FindNlgiGrade(cachedData!, penetrationValue);
            }

            await EnsureCacheIsCurrentAsync();

            // Cache in memory cache for faster subsequent access
            _memoryCache.Set(NLGI_CACHE_KEY, _nlgiCache, _cacheExpiry);
            _performanceService.RecordCacheOperation(NLGI_CACHE_KEY, false, stopwatch.Elapsed);

            return FindNlgiGrade(_nlgiCache, penetrationValue);
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    private static string FindNlgiGrade(List<NlgiLookup> nlgiData, double penetrationValue)
    {
        if (penetrationValue <= 0)
            return "6"; // Highest grade for very low penetration

        var matchingGrade = nlgiData
            .FirstOrDefault(n => penetrationValue >= n.LowerValue && penetrationValue <= n.UpperValue);

        if (matchingGrade != null)
            return matchingGrade.NLGIValue ?? "2";

        // If no exact match, find the closest range
        if (penetrationValue < nlgiData.Min(n => n.LowerValue ?? 0))
            return "6"; // Highest grade

        if (penetrationValue > nlgiData.Max(n => n.UpperValue ?? 0))
            return "000"; // Lowest grade

        return "2"; // Default to grade 2 if no match found
    }

    /// <summary>
    /// Refresh the lookup cache
    /// </summary>
    public async Task RefreshCacheAsync()
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Refreshing lookup cache...");

            // Load NAS lookup data with performance monitoring
            var nasData = await _context.NasLookup.ToListAsync();
            _nasCache.Clear();
            
            foreach (var item in nasData)
            {
                if (item.Channel.HasValue)
                {
                    if (!_nasCache.ContainsKey(item.Channel.Value))
                        _nasCache[item.Channel.Value] = new List<NasLookup>();
                    
                    _nasCache[item.Channel.Value].Add(item);
                }
            }

            // Load NLGI lookup data
            var nlgiData = await _context.NlgiLookup.OrderBy(n => n.LowerValue).ToListAsync();
            _nlgiCache.Clear();
            _nlgiCache.AddRange(nlgiData);

            // Clear memory cache to force refresh
            _memoryCache.Remove(NAS_CACHE_KEY);
            _memoryCache.Remove(NLGI_CACHE_KEY);
            
            // Clear equipment cache
            var cacheKeys = new List<string>();
            if (_memoryCache is MemoryCache mc)
            {
                var field = typeof(MemoryCache).GetField("_coherentState", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field?.GetValue(mc) is object coherentState)
                {
                    var entriesCollection = coherentState.GetType()
                        .GetProperty("EntriesCollection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (entriesCollection?.GetValue(coherentState) is IDictionary<object, object> entries)
                    {
                        foreach (var entry in entries)
                        {
                            if (entry.Key.ToString()?.StartsWith(EQUIPMENT_CACHE_KEY_PREFIX) == true)
                            {
                                cacheKeys.Add(entry.Key.ToString()!);
                            }
                        }
                    }
                }
            }
            
            foreach (var key in cacheKeys)
            {
                _memoryCache.Remove(key);
            }

            _lastCacheRefresh = DateTime.UtcNow;
            stopwatch.Stop();
            
            _performanceService.RecordQueryExecution("RefreshLookupCache", stopwatch.Elapsed, true);
            _logger.LogInformation("Lookup cache refreshed successfully in {Duration}ms", stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _performanceService.RecordQueryExecution("RefreshLookupCache", stopwatch.Elapsed, false);
            _logger.LogError(ex, "Failed to refresh lookup cache");
            throw;
        }
    }

    private async Task EnsureCacheIsCurrentAsync()
    {
        if (_lastCacheRefresh == DateTime.MinValue || 
            DateTime.UtcNow - _lastCacheRefresh > _cacheExpiry)
        {
            await RefreshCacheAsync();
        }
    }

    // Interface implementations
    public async Task<NasLookupResult> CalculateNASAsync(NasLookupRequest request)
    {
        // Convert int dictionary to double dictionary for internal calculation
        var doubleParticleCounts = request.ParticleCounts.ToDictionary(kvp => kvp.Key, kvp => (double)kvp.Value);
        var result = await CalculateHighestNasAsync(doubleParticleCounts);
        return new NasLookupResult
        {
            HighestNAS = result.HighestNas,
            ChannelNASValues = result.ChannelNasValues,
            IsValid = result.IsValid,
            ErrorMessage = result.ErrorMessage
        };
    }

    public async Task<IEnumerable<NasLookupDto>> GetNASLookupTableAsync()
    {
        await EnsureCacheIsCurrentAsync();
        var allNasData = new List<NasLookupDto>();
        
        foreach (var channelData in _nasCache)
        {
            foreach (var item in channelData.Value)
            {
                allNasData.Add(new NasLookupDto
                {
                    Channel = item.Channel ?? 0,
                    ValLo = item.ValLo ?? 0,
                    ValHi = item.ValHi ?? 0,
                    NAS = item.NAS ?? 0
                });
            }
        }
        
        return allNasData.OrderBy(n => n.Channel).ThenBy(n => n.ValLo);
    }

    public async Task<int?> GetNASForParticleCountAsync(int channel, int particleCount)
    {
        var result = await GetNasValueAsync(channel, particleCount);
        return result > 0 ? result : null;
    }

    public async Task<bool> RefreshNASCacheAsync()
    {
        try
        {
            await RefreshCacheAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string?> GetNLGIForPenetrationAsync(int penetrationValue)
    {
        return await GetNlgiGradeAsync(penetrationValue);
    }

    public async Task<IEnumerable<NlgiLookupDto>> GetNLGILookupTableAsync()
    {
        await EnsureCacheIsCurrentAsync();
        return _nlgiCache.Select(n => new NlgiLookupDto
        {
            LowerValue = n.LowerValue ?? 0,
            UpperValue = n.UpperValue ?? 0,
            NLGIValue = n.NLGIValue ?? ""
        }).OrderBy(n => n.LowerValue);
    }

    public async Task<bool> RefreshNLGICacheAsync()
    {
        try
        {
            await RefreshCacheAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    // Stub implementations for equipment and other lookups
    public async Task<IEnumerable<EquipmentSelectionDto>> GetCachedEquipmentByTypeAsync(string equipType, short? testId = null)
    {
        // TODO: Implement equipment lookup
        return new List<EquipmentSelectionDto>();
    }

    public async Task<EquipmentCalibrationDto?> GetCachedEquipmentCalibrationAsync(int equipmentId)
    {
        // TODO: Implement equipment calibration lookup
        return null;
    }

    public async Task<bool> RefreshEquipmentCacheAsync()
    {
        // TODO: Implement equipment cache refresh
        return true;
    }

    public async Task<bool> RefreshEquipmentCacheByTypeAsync(string equipType)
    {
        // TODO: Implement equipment cache refresh by type
        return true;
    }

    public async Task<IEnumerable<ParticleTypeDefinitionDto>> GetParticleTypeDefinitionsAsync()
    {
        // TODO: Implement particle type definitions lookup
        return new List<ParticleTypeDefinitionDto>();
    }

    public async Task<IEnumerable<ParticleSubTypeDefinitionDto>> GetParticleSubTypeDefinitionsAsync(int categoryId)
    {
        // TODO: Implement particle sub-type definitions lookup
        return new List<ParticleSubTypeDefinitionDto>();
    }

    public async Task<IEnumerable<ParticleSubTypeCategoryDefinitionDto>> GetParticleSubTypeCategoriesAsync()
    {
        // TODO: Implement particle sub-type categories lookup
        return new List<ParticleSubTypeCategoryDefinitionDto>();
    }

    public async Task<bool> RefreshParticleTypeCacheAsync()
    {
        // TODO: Implement particle type cache refresh
        return true;
    }

    public async Task<IEnumerable<CommentDto>> GetCommentsByAreaAsync(string area)
    {
        // TODO: Implement comments lookup by area
        return new List<CommentDto>();
    }

    public async Task<IEnumerable<CommentDto>> GetCommentsByAreaAndTypeAsync(string area, string type)
    {
        // TODO: Implement comments lookup by area and type
        return new List<CommentDto>();
    }

    public async Task<IEnumerable<string>> GetCommentAreasAsync()
    {
        // TODO: Implement comment areas lookup
        return new List<string>();
    }

    public async Task<IEnumerable<string>> GetCommentTypesAsync(string area)
    {
        // TODO: Implement comment types lookup
        return new List<string>();
    }

    public async Task<bool> RefreshCommentCacheAsync()
    {
        // TODO: Implement comment cache refresh
        return true;
    }

    public async Task<bool> RefreshAllCachesAsync()
    {
        try
        {
            await RefreshCacheAsync();
            await RefreshEquipmentCacheAsync();
            await RefreshParticleTypeCacheAsync();
            await RefreshCommentCacheAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public Task<CacheStatusDto> GetCacheStatusAsync()
    {
        return Task.FromResult(new CacheStatusDto
        {
            CacheEntries = new Dictionary<string, CacheInfo>
            {
                { "NAS", new CacheInfo 
                    { 
                        IsLoaded = _nasCache.Any(), 
                        LastRefreshed = _lastCacheRefresh > DateTime.MinValue ? _lastCacheRefresh : null,
                        ItemCount = _nasCache.Values.Sum(v => v.Count),
                        ExpiresIn = _lastCacheRefresh > DateTime.MinValue ? _cacheExpiry - (DateTime.UtcNow - _lastCacheRefresh) : null
                    } 
                },
                { "NLGI", new CacheInfo 
                    { 
                        IsLoaded = _nlgiCache.Any(), 
                        LastRefreshed = _lastCacheRefresh > DateTime.MinValue ? _lastCacheRefresh : null,
                        ItemCount = _nlgiCache.Count,
                        ExpiresIn = _lastCacheRefresh > DateTime.MinValue ? _cacheExpiry - (DateTime.UtcNow - _lastCacheRefresh) : null
                    } 
                }
            }
        });
    }
}