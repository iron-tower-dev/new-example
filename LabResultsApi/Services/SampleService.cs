using Microsoft.EntityFrameworkCore;
using LabResultsApi.Data;
using LabResultsApi.DTOs;

namespace LabResultsApi.Services;

public class SampleService : ISampleService
{
    private readonly LabDbContext _context;
    private readonly IRawSqlService _rawSqlService;
    private readonly ILogger<SampleService> _logger;

    public SampleService(LabDbContext context, IRawSqlService rawSqlService, ILogger<SampleService> logger)
    {
        _context = context;
        _rawSqlService = rawSqlService;
        _logger = logger;
    }

    public async Task<IEnumerable<SampleDto>> GetSamplesByTestAsync(int testId)
    {
        _logger.LogInformation("Getting samples for test {TestId}", testId);
        
        try
        {
            // First try to get samples that have test readings for the specific test
            var samplesWithReadings = await (from tr in _context.TestReadings
                                join s in _context.UsedLubeSamples on tr.SampleId equals s.Id
                                where tr.TestId == testId && tr.Status == "A"
                                select new SampleDto
                                {
                                    Id = s.Id,
                                    TagNumber = s.TagNumber,
                                    Component = s.Component,
                                    Location = s.Location,
                                    LubeType = s.LubeType,
                                    QualityClass = s.QualityClass,
                                    SampleDate = s.SampleDate,
                                    Status = s.Status
                                })
                                .Distinct()
                                .OrderBy(s => s.Id)
                                .ToListAsync();

            // If no samples found with readings, return recent samples that could be tested
            if (!samplesWithReadings.Any())
            {
                _logger.LogInformation("No samples found with test readings for test {TestId}, returning recent samples", testId);
                
                var recentSamples = await _context.UsedLubeSamples
                    .Where(s => s.Status == "A" && s.SampleDate >= DateTime.Now.AddDays(-30)) // Last 30 days
                    .OrderByDescending(s => s.SampleDate)
                    .Take(20) // Limit to 20 most recent samples
                    .Select(s => new SampleDto
                    {
                        Id = s.Id,
                        TagNumber = s.TagNumber,
                        Component = s.Component,
                        Location = s.Location,
                        LubeType = s.LubeType,
                        QualityClass = s.QualityClass,
                        SampleDate = s.SampleDate,
                        Status = s.Status
                    })
                    .ToListAsync();
                
                _logger.LogInformation("Found {Count} recent samples for test {TestId}", recentSamples.Count, testId);
                return recentSamples;
            }

            _logger.LogInformation("Found {Count} samples with readings for test {TestId}", samplesWithReadings.Count, testId);
            return samplesWithReadings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting samples for test {TestId}", testId);
            throw;
        }
    }

    public async Task<IEnumerable<SampleDto>> GetSamplesAsync(SampleFilterDto? filter = null)
    {
        _logger.LogInformation("Getting samples with filter");
        
        try
        {
            var query = _context.UsedLubeSamples.AsQueryable();

            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.TagNumber))
                    query = query.Where(s => s.TagNumber.Contains(filter.TagNumber));
                
                if (!string.IsNullOrEmpty(filter.Component))
                    query = query.Where(s => s.Component.Contains(filter.Component));
                
                if (!string.IsNullOrEmpty(filter.Location))
                    query = query.Where(s => s.Location.Contains(filter.Location));
                
                if (!string.IsNullOrEmpty(filter.LubeType))
                    query = query.Where(s => s.LubeType.Contains(filter.LubeType));
                
                if (filter.FromDate.HasValue)
                    query = query.Where(s => s.SampleDate >= filter.FromDate.Value);
                
                if (filter.ToDate.HasValue)
                    query = query.Where(s => s.SampleDate <= filter.ToDate.Value);
                
                if (filter.Status.HasValue)
                {
                    var statusText = GetStatusText(filter.Status.Value);
                    query = query.Where(s => s.Status == statusText);
                }
            }

            var samples = await query
                .OrderByDescending(s => s.SampleDate)
                .Take(200) // Limit results
                .Select(s => new SampleDto
                {
                    Id = s.Id,
                    TagNumber = s.TagNumber,
                    Component = s.Component,
                    Location = s.Location,
                    LubeType = s.LubeType,
                    QualityClass = s.QualityClass,
                    SampleDate = s.SampleDate,
                    Status = s.Status
                })
                .ToListAsync();

            return samples;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting samples with filter");
            throw;
        }
    }

    public async Task<SampleDto?> GetSampleAsync(int sampleId)
    {
        _logger.LogInformation("Getting sample {SampleId}", sampleId);
        
        try
        {
            var sample = await _context.UsedLubeSamples
                .Where(s => s.Id == sampleId)
                .Select(s => new SampleDto
                {
                    Id = s.Id,
                    TagNumber = s.TagNumber,
                    Component = s.Component,
                    Location = s.Location,
                    LubeType = s.LubeType,
                    QualityClass = s.QualityClass,
                    SampleDate = s.SampleDate,
                    Status = s.Status
                })
                .FirstOrDefaultAsync();

            return sample;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sample {SampleId}", sampleId);
            throw;
        }
    }

    public async Task<IEnumerable<SampleHistoryDto>> GetSampleHistoryAsync(int sampleId, int testId)
    {
        _logger.LogInformation("Getting history for sample {SampleId}, test {TestId}", sampleId, testId);
        
        try
        {
            // Use raw SQL to get historical data since TestReadings is keyless
            var historyData = await _rawSqlService.GetSampleHistoryAsync(sampleId, testId);
            return historyData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting history for sample {SampleId}, test {TestId}", sampleId, testId);
            throw;
        }
    }

    private static string GetStatusText(int status)
    {
        return status switch
        {
            0 => "Pending",
            1 => "Available", 
            2 => "In Progress",
            3 => "Complete",
            4 => "Cancelled",
            _ => "Unknown"
        };
    }
}