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
            // Match legacy query pattern: JOIN with Lube_Sampling_Point for quality class
            var samplesWithReadings = await (from tr in _context.TestReadings
                                join s in _context.UsedLubeSamples on tr.SampleId equals s.Id
                                join lsp in _context.LubeSamplingPoints 
                                    on new { s.TagNumber, s.Component, s.Location } 
                                    equals new { lsp.TagNumber, lsp.Component, lsp.Location } 
                                    into lspJoin
                                from lsp in lspJoin.DefaultIfEmpty()
                                where tr.TestId == testId && tr.Status == "A"
                                select new SampleDto
                                {
                                    Id = s.Id,
                                    TagNumber = s.TagNumber,
                                    Component = s.Component,
                                    Location = s.Location,
                                    LubeType = s.LubeType,
                                    WoNumber = s.WoNumber,
                                    SampleDate = s.SampleDate,
                                    ReceivedOn = s.ReceivedOn,
                                    SampledBy = s.SampledBy,
                                    Status = s.Status,
                                    SiteId = s.SiteId,
                                    ResultsReviewDate = s.ResultsReviewDate,
                                    ResultsAvailDate = s.ResultsAvailDate,
                                    QualityClass = lsp.QualityClass  // From Lube_Sampling_Point
                                })
                                .Distinct()
                                .OrderBy(s => s.Id)
                                .ToListAsync();

            // If no samples found with readings, return recent samples that could be tested
            if (!samplesWithReadings.Any())
            {
                _logger.LogInformation("No samples found with test readings for test {TestId}, returning recent samples", testId);
                
                // Status 250 = Active samples (from legacy pattern)
                var recentSamples = await (from s in _context.UsedLubeSamples
                                          join lsp in _context.LubeSamplingPoints 
                                              on new { s.TagNumber, s.Component, s.Location } 
                                              equals new { lsp.TagNumber, lsp.Component, lsp.Location } 
                                              into lspJoin
                                          from lsp in lspJoin.DefaultIfEmpty()
                                          where s.Status == 250 && s.SampleDate >= DateTime.Now.AddDays(-30)
                                          orderby s.SampleDate descending
                                          select new SampleDto
                                          {
                                              Id = s.Id,
                                              TagNumber = s.TagNumber,
                                              Component = s.Component,
                                              Location = s.Location,
                                              LubeType = s.LubeType,
                                              WoNumber = s.WoNumber,
                                              SampleDate = s.SampleDate,
                                              ReceivedOn = s.ReceivedOn,
                                              SampledBy = s.SampledBy,
                                              Status = s.Status,
                                              SiteId = s.SiteId,
                                              ResultsReviewDate = s.ResultsReviewDate,
                                              ResultsAvailDate = s.ResultsAvailDate,
                                              QualityClass = lsp.QualityClass
                                          })
                                          .Take(20)
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
            var query = from s in _context.UsedLubeSamples
                       join lsp in _context.LubeSamplingPoints 
                           on new { s.TagNumber, s.Component, s.Location } 
                           equals new { lsp.TagNumber, lsp.Component, lsp.Location } 
                           into lspJoin
                       from lsp in lspJoin.DefaultIfEmpty()
                       select new { Sample = s, QualityClass = lsp.QualityClass };

            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.TagNumber))
                    query = query.Where(x => x.Sample.TagNumber != null && x.Sample.TagNumber.Contains(filter.TagNumber));
                
                if (!string.IsNullOrEmpty(filter.Component))
                    query = query.Where(x => x.Sample.Component != null && x.Sample.Component.Contains(filter.Component));
                
                if (!string.IsNullOrEmpty(filter.Location))
                    query = query.Where(x => x.Sample.Location != null && x.Sample.Location.Contains(filter.Location));
                
                if (!string.IsNullOrEmpty(filter.LubeType))
                    query = query.Where(x => x.Sample.LubeType != null && x.Sample.LubeType.Contains(filter.LubeType));
                
                if (filter.FromDate.HasValue)
                    query = query.Where(x => x.Sample.SampleDate >= filter.FromDate.Value);
                
                if (filter.ToDate.HasValue)
                    query = query.Where(x => x.Sample.SampleDate <= filter.ToDate.Value);
                
                if (filter.Status.HasValue)
                {
                    short statusValue = (short)filter.Status.Value;
                    query = query.Where(x => x.Sample.Status == statusValue);
                }
            }

            var samples = await query
                .OrderByDescending(x => x.Sample.SampleDate)
                .Take(200) // Limit results
                .Select(x => new SampleDto
                {
                    Id = x.Sample.Id,
                    TagNumber = x.Sample.TagNumber,
                    Component = x.Sample.Component,
                    Location = x.Sample.Location,
                    LubeType = x.Sample.LubeType,
                    WoNumber = x.Sample.WoNumber,
                    SampleDate = x.Sample.SampleDate,
                    ReceivedOn = x.Sample.ReceivedOn,
                    SampledBy = x.Sample.SampledBy,
                    Status = x.Sample.Status,
                    SiteId = x.Sample.SiteId,
                    ResultsReviewDate = x.Sample.ResultsReviewDate,
                    ResultsAvailDate = x.Sample.ResultsAvailDate,
                    QualityClass = x.QualityClass
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
            var sample = await (from s in _context.UsedLubeSamples
                               join lsp in _context.LubeSamplingPoints 
                                   on new { s.TagNumber, s.Component, s.Location } 
                                   equals new { lsp.TagNumber, lsp.Component, lsp.Location } 
                                   into lspJoin
                               from lsp in lspJoin.DefaultIfEmpty()
                               where s.Id == sampleId
                               select new SampleDto
                               {
                                   Id = s.Id,
                                   TagNumber = s.TagNumber,
                                   Component = s.Component,
                                   Location = s.Location,
                                   LubeType = s.LubeType,
                                   WoNumber = s.WoNumber,
                                   TrackingNumber = s.TrackingNumber,
                                   WarehouseId = s.WarehouseId,
                                   BatchNumber = s.BatchNumber,
                                   ClassItem = s.ClassItem,
                                   SampleDate = s.SampleDate,
                                   ReceivedOn = s.ReceivedOn,
                                   SampledBy = s.SampledBy,
                                   Status = s.Status,
                                   CmptSelectFlag = s.CmptSelectFlag,
                                   NewUsedFlag = s.NewUsedFlag,
                                   EntryId = s.EntryId,
                                   ValidateId = s.ValidateId,
                                   TestPricesId = s.TestPricesId,
                                   PricingPackageId = s.PricingPackageId,
                                   Evaluation = s.Evaluation,
                                   SiteId = s.SiteId,
                                   ResultsReviewDate = s.ResultsReviewDate,
                                   ResultsAvailDate = s.ResultsAvailDate,
                                   ResultsReviewId = s.ResultsReviewId,
                                   StoreSource = s.StoreSource,
                                   Schedule = s.Schedule,
                                   ReturnedDate = s.ReturnedDate,
                                   QualityClass = lsp.QualityClass
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


}