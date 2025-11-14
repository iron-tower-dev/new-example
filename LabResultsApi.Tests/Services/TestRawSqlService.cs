using LabResultsApi.Data;
using LabResultsApi.Models;
using LabResultsApi.Services;
using LabResultsApi.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LabResultsApi.Tests.Services;

/// <summary>
/// Test-specific implementation of IRawSqlService that works with in-memory databases
/// by using LINQ instead of raw SQL queries
/// </summary>
public class TestRawSqlService : IRawSqlService
{
    private readonly LabDbContext _context;
    private readonly ILogger<TestRawSqlService> _logger;

    public TestRawSqlService(LabDbContext context, ILogger<TestRawSqlService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<TestReading>> GetTestReadingsAsync(int sampleId, int testId)
    {
        return await _context.TestReadings
            .Where(tr => tr.SampleId == sampleId && tr.TestId == testId)
            .ToListAsync();
    }

    public async Task<int> SaveTestReadingAsync(TestReading reading)
    {
        try
        {
            // For keyless entities in tests, simulate saving
            _logger.LogInformation("Test reading saved for sample {SampleId}, test {TestId}", reading.SampleId, reading.TestId);
            return 1; // Return 1 to indicate success
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving test reading");
            return 0;
        }
    }

    public async Task<int> UpdateTestReadingAsync(TestReading reading)
    {
        try
        {
            _logger.LogInformation("Test reading updated for sample {SampleId}, test {TestId}", reading.SampleId, reading.TestId);
            return 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating test reading");
            return 0;
        }
    }

    public async Task<int> DeleteTestReadingsAsync(int sampleId, int testId)
    {
        try
        {
            var readings = await _context.TestReadings
                .Where(tr => tr.SampleId == sampleId && tr.TestId == testId)
                .ToListAsync();

            _context.TestReadings.RemoveRange(readings);
            await _context.SaveChangesAsync();
            return readings.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting test readings for sample {SampleId}, test {TestId}", sampleId, testId);
            return 0;
        }
    }

    public async Task<List<EmissionSpectroscopy>> GetEmissionSpectroscopyAsync(int sampleId, int testId)
    {
        return await _context.EmSpectro
            .Where(es => es.TestId == testId)
            .ToListAsync();
    }

    public async Task<int> SaveEmissionSpectroscopyAsync(EmissionSpectroscopy data)
    {
        try
        {
            _logger.LogInformation("Emission spectroscopy saved for test {TestId}", data.TestId);
            return 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving emission spectroscopy");
            return 0;
        }
    }

    public async Task<int> UpdateEmissionSpectroscopyAsync(EmissionSpectroscopy data)
    {
        try
        {
            _logger.LogInformation("Emission spectroscopy updated for test {TestId}", data.TestId);
            return 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating emission spectroscopy");
            return 0;
        }
    }

    public async Task<int> DeleteEmissionSpectroscopyAsync(int sampleId, int testId, int trialNum)
    {
        try
        {
            var emissions = await _context.EmSpectro
                .Where(es => es.TestId == testId)
                .ToListAsync();

            _context.EmSpectro.RemoveRange(emissions);
            await _context.SaveChangesAsync();
            return emissions.Count();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting emission spectroscopy for test {TestId}", testId);
            return 0;
        }
    }

    public async Task<int> ScheduleFerrographyAsync(int sampleId)
    {
        try
        {
            _logger.LogInformation("Ferrography scheduled for sample {SampleId}", sampleId);
            return 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling ferrography for sample {SampleId}", sampleId);
            return 0;
        }
    }

    public async Task<List<SampleHistoryDto>> GetSampleHistoryAsync(int sampleId, int testId)
    {
        // For test purposes, return mock history data
        var sample = await _context.UsedLubeSamples.FirstOrDefaultAsync(s => s.Id == sampleId);
        if (sample == null) return new List<SampleHistoryDto>();

        return new List<SampleHistoryDto>
        {
            new SampleHistoryDto
            {
                SampleId = sampleId,
                SampleDate = sample.SampleDate,
                TestName = "Test",
                Status = "Complete"
            }
        };
    }

    public async Task<ExtendedHistoryResultDto> GetExtendedSampleHistoryAsync(int sampleId, int testId, DateTime? fromDate, DateTime? toDate, int page, int pageSize, string? status)
    {
        var history = await GetSampleHistoryAsync(sampleId, testId);
        return new ExtendedHistoryResultDto
        {
            Results = history,
            TotalCount = history.Count,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<bool> TestDatabaseConnectionAsync()
    {
        try
        {
            await _context.Database.CanConnectAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> SampleExistsAsync(int sampleId)
    {
        return await _context.UsedLubeSamples.AnyAsync(s => s.Id == sampleId);
    }

    // Particle Analysis methods
    public async Task<List<ParticleType>> GetParticleTypesAsync(int sampleId, int testId)
    {
        return await _context.ParticleTypes
            .Where(pt => pt.SampleId == sampleId && pt.TestId == testId)
            .ToListAsync();
    }

    public async Task<List<ParticleSubType>> GetParticleSubTypesAsync(int sampleId, int testId)
    {
        return await _context.ParticleSubTypes
            .Where(pst => pst.SampleId == sampleId && pst.TestId == testId)
            .ToListAsync();
    }

    public async Task<InspectFilter?> GetInspectFilterAsync(int sampleId, int testId)
    {
        return await _context.InspectFilters
            .FirstOrDefaultAsync(inf => inf.TestId == testId);
    }

    public async Task<int> SaveParticleTypeAsync(ParticleType particleType)
    {
        try
        {
            _context.ParticleTypes.Add(particleType);
            await _context.SaveChangesAsync();
            return 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving particle type");
            return 0;
        }
    }

    public async Task<int> SaveParticleSubTypeAsync(ParticleSubType particleSubType)
    {
        try
        {
            _context.ParticleSubTypes.Add(particleSubType);
            await _context.SaveChangesAsync();
            return 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving particle sub type");
            return 0;
        }
    }

    public async Task<int> SaveInspectFilterAsync(InspectFilter inspectFilter)
    {
        try
        {
            _context.InspectFilters.Add(inspectFilter);
            await _context.SaveChangesAsync();
            return 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving inspect filter");
            return 0;
        }
    }

    public async Task<int> DeleteParticleTypesAsync(int sampleId, int testId)
    {
        try
        {
            var particleTypes = await _context.ParticleTypes
                .Where(pt => pt.SampleId == sampleId && pt.TestId == testId)
                .ToListAsync();

            _context.ParticleTypes.RemoveRange(particleTypes);
            await _context.SaveChangesAsync();
            return particleTypes.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting particle types");
            return 0;
        }
    }

    public async Task<int> DeleteParticleSubTypesAsync(int sampleId, int testId)
    {
        try
        {
            var particleSubTypes = await _context.ParticleSubTypes
                .Where(pst => pst.SampleId == sampleId && pst.TestId == testId)
                .ToListAsync();

            _context.ParticleSubTypes.RemoveRange(particleSubTypes);
            await _context.SaveChangesAsync();
            return particleSubTypes.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting particle sub types");
            return 0;
        }
    }

    public async Task<int> DeleteInspectFilterAsync(int sampleId, int testId)
    {
        try
        {
            var inspectFilter = await _context.InspectFilters
                .FirstOrDefaultAsync(inf => inf.TestId == testId);

            if (inspectFilter != null)
            {
                _context.InspectFilters.Remove(inspectFilter);
                await _context.SaveChangesAsync();
                return 1;
            }
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting inspect filter");
            return 0;
        }
    }
}