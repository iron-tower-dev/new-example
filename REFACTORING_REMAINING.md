# RawSqlService Refactoring - Remaining Work

## Completed So Far ✅

1. ✅ Extended IParticleAnalysisService with CRUD methods
2. ✅ Extended ParticleAnalysisService with CRUD implementations  
3. ✅ Created IEmissionSpectroscopyService interface
4. ✅ Created EmissionSpectroscopyService implementation
5. ✅ Created IParticleTestService interface

## Remaining Work

### Critical Files to Create (2 services)

1. **ParticleTestService.cs** - Implement IParticleTestService
2. **I/HistoricalResultsService.cs** - Interface and implementation

### Files to Modify (8-10 files)

Due to context/token limits, here's the systematic approach to complete:

1. Add TestReadings CRUD methods directly to TestResultService
2. Remove IRawSqlService dependency from TestResultService
3. Create ParticleTestService (depends on IParticleAnalysisService, ITestResultService)
4. Create HistoricalResultsService
5. Update EmissionSpectroscopyEndpoints to inject IEmissionSpectroscopyService
6. Update ParticleTestEndpoints to inject IParticleTestService  
7. Update HistoricalResultsEndpoints to inject IHistoricalResultsService
8. Update TestSchedulingService dependency
9. Update SampleService with SampleExistsAsync
10. Update Program.cs DI registrations
11. Delete old RawSqlService files

## Recommendation

Given token/context limits reached, here are your options:

**Option A: New Session**
- Start fresh session with this document
- I'll complete remaining work efficiently
- Benefit: Clean slate, full context

**Option B: You Complete Manually**
- Follow the detailed code below
- Copy/paste implementations
- Test incrementally

**Option C: Pause and Test**
- Test what we've done so far
- Continue in next session

I recommend **Option A** - start a new session and I'll complete the remaining work.

---

## Detailed Code for Remaining Files

### 1. ParticleTestService.cs

```csharp
using Microsoft.EntityFrameworkCore;
using LabResultsApi.Data;
using LabResultsApi.Models;

namespace LabResultsApi.Services;

public class ParticleTestService : IParticleTestService
{
    private readonly LabDbContext _context;
    private readonly IParticleAnalysisService _particleAnalysisService;
    private readonly ITestResultService _testResultService;
    private readonly ILogger<ParticleTestService> _logger;

    public ParticleTestService(
        LabDbContext context,
        IParticleAnalysisService particleAnalysisService,
        ITestResultService testResultService,
        ILogger<ParticleTestService> logger)
    {
        _context = context;
        _particleAnalysisService = particleAnalysisService;
        _testResultService = testResultService;
        _logger = logger;
    }

    public async Task<FilterResidueResult?> GetFilterResidueAsync(int sampleId, int testId)
    {
        try
        {
            _logger.LogInformation("Getting filter residue data for sample {SampleId}, test {TestId}", sampleId, testId);
            
            var inspectFilter = await _particleAnalysisService.GetInspectFilterAsync(sampleId, testId);
            
            var testReadings = await _context.TestReadings
                .FromSqlRaw(@"
                    SELECT sampleID, testID, trialNumber, value1, value2, value3, trialCalc, 
                           ID1, ID2, ID3, trialComplete, status, schedType, entryID, validateID, 
                           entryDate, valiDate, MainComments
                    FROM TestReadings 
                    WHERE sampleID = {0} AND testID = {1} AND trialNumber = 1", sampleId, testId)
                .ToListAsync();
            
            var testReading = testReadings.FirstOrDefault();
            
            var particleTypes = await _particleAnalysisService.GetParticleTypeDataAsync(sampleId, testId);
            var particleSubTypes = await _particleAnalysisService.GetParticleSubTypeDataAsync(sampleId, testId);
            
            if (inspectFilter == null && testReading == null && particleTypes.Count == 0)
            {
                return null;
            }
            
            var result = new FilterResidueResult
            {
                SampleId = sampleId,
                TestId = (short)testId,
                Narrative = inspectFilter?.Narrative,
                Major = inspectFilter?.Major,
                Minor = inspectFilter?.Minor,
                Trace = inspectFilter?.Trace,
                SampleSize = testReading?.Value1,
                ResidueWeight = testReading?.Value3,
                FinalWeight = testReading?.Value2,
                OverallSeverity = DetermineOverallSeverity(particleTypes),
                ParticleTypes = particleTypes,
                ParticleSubTypes = particleSubTypes,
                EntryId = testReading?.EntryId,
                EntryDate = testReading?.EntryDate,
                Status = testReading?.Status
            };
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting filter residue data for sample {SampleId}, test {TestId}", sampleId, testId);
            throw;
        }
    }

    public async Task<int> SaveFilterResidueAsync(FilterResidueResult filterResidue)
    {
        try
        {
            _logger.LogInformation("Saving filter residue data for sample {SampleId}, test {TestId}", 
                filterResidue.SampleId, filterResidue.TestId);
            
            var rowsAffected = 0;
            
            await DeleteFilterResidueAsync(filterResidue.SampleId, filterResidue.TestId);
            
            var inspectFilter = new InspectFilter
            {
                Id = filterResidue.SampleId,
                TestId = filterResidue.TestId,
                Narrative = filterResidue.Narrative,
                Major = filterResidue.Major,
                Minor = filterResidue.Minor,
                Trace = filterResidue.Trace
            };
            rowsAffected += await _particleAnalysisService.SaveInspectFilterAsync(inspectFilter);
            
            var testReading = new TestReading
            {
                SampleId = filterResidue.SampleId,
                TestId = filterResidue.TestId,
                TrialNumber = 1,
                Value1 = filterResidue.SampleSize,
                Value3 = filterResidue.ResidueWeight,
                Value2 = filterResidue.FinalWeight,
                Status = filterResidue.Status ?? "E",
                EntryId = filterResidue.EntryId,
                EntryDate = filterResidue.EntryDate ?? DateTime.Now
            };
            rowsAffected += await _context.Database.ExecuteSqlRawAsync(@"
                INSERT INTO TestReadings 
                (sampleID, testID, trialNumber, value1, value2, value3, status, entryDate, entryID)
                VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8})",
                testReading.SampleId, testReading.TestId, testReading.TrialNumber, 
                testReading.Value1, testReading.Value2, testReading.Value3, 
                testReading.Status, testReading.EntryDate, testReading.EntryId);
            
            if (filterResidue.ParticleTypes != null)
            {
                foreach (var particleType in filterResidue.ParticleTypes)
                {
                    rowsAffected += await _particleAnalysisService.SaveParticleTypeAsync(particleType);
                }
            }
            
            if (filterResidue.ParticleSubTypes != null)
            {
                foreach (var particleSubType in filterResidue.ParticleSubTypes)
                {
                    rowsAffected += await _particleAnalysisService.SaveParticleSubTypeAsync(particleSubType);
                }
            }
            
            return rowsAffected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving filter residue data for sample {SampleId}, test {TestId}", 
                filterResidue.SampleId, filterResidue.TestId);
            throw;
        }
    }

    public async Task<int> DeleteFilterResidueAsync(int sampleId, int testId)
    {
        try
        {
            _logger.LogInformation("Deleting filter residue data for sample {SampleId}, test {TestId}", sampleId, testId);
            
            var rowsAffected = 0;
            
            rowsAffected += await _particleAnalysisService.DeleteInspectFilterAsync(sampleId, testId);
            rowsAffected += await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM TestReadings WHERE sampleID = {0} AND testID = {1}",
                sampleId, testId);
            rowsAffected += await _particleAnalysisService.DeleteParticleTypesAsync(sampleId, testId);
            rowsAffected += await _particleAnalysisService.DeleteParticleSubTypesAsync(sampleId, testId);
            
            return rowsAffected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting filter residue data for sample {SampleId}, test {TestId}", sampleId, testId);
            throw;
        }
    }

    public async Task<DebrisIdentificationResult?> GetDebrisIdentificationAsync(int sampleId, int testId)
    {
        try
        {
            _logger.LogInformation("Getting debris identification data for sample {SampleId}, test {TestId}", sampleId, testId);
            
            var inspectFilter = await _particleAnalysisService.GetInspectFilterAsync(sampleId, testId);
            
            var testReadings = await _context.Database.SqlQueryRaw<TestReading>(@"
                SELECT sampleID, testID, trialNumber, value1, value2, value3, trialCalc, 
                       ID1, ID2, ID3, trialComplete, status, schedType, entryID, validateID, 
                       entryDate, valiDate, MainComments
                FROM TestReadings 
                WHERE sampleID = {0} AND testID = {1} AND trialNumber = 1", sampleId, testId)
                .ToListAsync();
            
            var testReading = testReadings.FirstOrDefault();
            
            var particleTypes = await _particleAnalysisService.GetParticleTypeDataAsync(sampleId, testId);
            var particleSubTypes = await _particleAnalysisService.GetParticleSubTypeDataAsync(sampleId, testId);
            
            if (inspectFilter == null && testReading == null && particleTypes.Count == 0)
            {
                return null;
            }
            
            string? volumeOfOilUsed = testReading?.Id3;
            string? customVolume = null;
            
            if (volumeOfOilUsed == "custom" && !string.IsNullOrEmpty(testReading?.Id2))
            {
                customVolume = testReading.Id2;
            }
            
            var result = new DebrisIdentificationResult
            {
                SampleId = sampleId,
                TestId = (short)testId,
                Narrative = inspectFilter?.Narrative,
                Major = inspectFilter?.Major,
                Minor = inspectFilter?.Minor,
                Trace = inspectFilter?.Trace,
                VolumeOfOilUsed = volumeOfOilUsed,
                CustomVolume = customVolume,
                OverallSeverity = DetermineOverallSeverity(particleTypes),
                ParticleTypes = particleTypes,
                ParticleSubTypes = particleSubTypes,
                EntryId = testReading?.EntryId,
                EntryDate = testReading?.EntryDate,
                Status = testReading?.Status
            };
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting debris identification data for sample {SampleId}, test {TestId}", sampleId, testId);
            throw;
        }
    }

    public async Task<int> SaveDebrisIdentificationAsync(DebrisIdentificationResult debrisId)
    {
        try
        {
            _logger.LogInformation("Saving debris identification data for sample {SampleId}, test {TestId}", 
                debrisId.SampleId, debrisId.TestId);
            
            var rowsAffected = 0;
            
            await DeleteDebrisIdentificationAsync(debrisId.SampleId, debrisId.TestId);
            
            var inspectFilter = new InspectFilter
            {
                Id = debrisId.SampleId,
                TestId = debrisId.TestId,
                Narrative = debrisId.Narrative,
                Major = debrisId.Major,
                Minor = debrisId.Minor,
                Trace = debrisId.Trace
            };
            rowsAffected += await _particleAnalysisService.SaveInspectFilterAsync(inspectFilter);
            
            var testReading = new TestReading
            {
                SampleId = debrisId.SampleId,
                TestId = debrisId.TestId,
                TrialNumber = 1,
                Id3 = debrisId.VolumeOfOilUsed,
                Id2 = debrisId.CustomVolume,
                Status = debrisId.Status ?? "E",
                EntryId = debrisId.EntryId,
                EntryDate = debrisId.EntryDate ?? DateTime.Now
            };
            rowsAffected += await _context.Database.ExecuteSqlRawAsync(@"
                INSERT INTO TestReadings 
                (sampleID, testID, trialNumber, ID2, ID3, status, entryDate, entryID)
                VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})",
                testReading.SampleId, testReading.TestId, testReading.TrialNumber, 
                testReading.Id2, testReading.Id3, testReading.Status, 
                testReading.EntryDate, testReading.EntryId);
            
            if (debrisId.ParticleTypes != null)
            {
                foreach (var particleType in debrisId.ParticleTypes)
                {
                    rowsAffected += await _particleAnalysisService.SaveParticleTypeAsync(particleType);
                }
            }
            
            if (debrisId.ParticleSubTypes != null)
            {
                foreach (var particleSubType in debrisId.ParticleSubTypes)
                {
                    rowsAffected += await _particleAnalysisService.SaveParticleSubTypeAsync(particleSubType);
                }
            }
            
            return rowsAffected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving debris identification data for sample {SampleId}, test {TestId}", 
                debrisId.SampleId, debrisId.TestId);
            throw;
        }
    }

    public async Task<int> DeleteDebrisIdentificationAsync(int sampleId, int testId)
    {
        try
        {
            _logger.LogInformation("Deleting debris identification data for sample {SampleId}, test {TestId}", sampleId, testId);
            
            var rowsAffected = 0;
            
            rowsAffected += await _particleAnalysisService.DeleteInspectFilterAsync(sampleId, testId);
            rowsAffected += await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM TestReadings WHERE sampleID = {0} AND testID = {1}",
                sampleId, testId);
            rowsAffected += await _particleAnalysisService.DeleteParticleTypesAsync(sampleId, testId);
            rowsAffected += await _particleAnalysisService.DeleteParticleSubTypesAsync(sampleId, testId);
            
            return rowsAffected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting debris identification data for sample {SampleId}, test {TestId}", sampleId, testId);
            throw;
        }
    }

    private int DetermineOverallSeverity(List<ParticleType> particleTypes)
    {
        if (particleTypes == null || particleTypes.Count == 0)
        {
            return 0;
        }
        
        var maxSeverity = 0;
        
        foreach (var particleType in particleTypes)
        {
            if (!string.IsNullOrEmpty(particleType.Status) && int.TryParse(particleType.Status, out int severity))
            {
                if (severity > maxSeverity)
                {
                    maxSeverity = severity;
                }
            }
        }
        
        return maxSeverity;
    }
}
```

### 2. Update Program.cs DI Registrations

Find the section with service registrations and:

**Remove:**
```csharp
services.AddScoped<IRawSqlService, RawSqlService>();
// or
services.AddScoped<IRawSqlService, OptimizedRawSqlService>();
```

**Add:**
```csharp
services.AddScoped<IEmissionSpectroscopyService, EmissionSpectroscopyService>();
services.AddScoped<IParticleTestService, ParticleTestService>();
services.AddScoped<IHistoricalResultsService, HistoricalResultsService>();
```

### 3. Update Endpoints

Update all endpoint files to inject new services instead of IRawSqlService.

Continue in new session to complete remaining files...

