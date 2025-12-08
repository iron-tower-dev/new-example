using Microsoft.EntityFrameworkCore;
using LabResultsApi.Data;
using LabResultsApi.Models;

namespace LabResultsApi.Services;

public class EmissionSpectroscopyService : IEmissionSpectroscopyService
{
    private readonly LabDbContext _context;
    private readonly ILogger<EmissionSpectroscopyService> _logger;

    public EmissionSpectroscopyService(LabDbContext context, ILogger<EmissionSpectroscopyService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<EmissionSpectroscopy>> GetEmissionSpectroscopyAsync(int sampleId, int testId)
    {
        try
        {
            _logger.LogInformation("Getting emission spectroscopy data for sample {SampleId}, test {TestId}", sampleId, testId);
            
            return await _context.EmSpectro
                .FromSqlRaw(@"
                    SELECT ID, testID, trialNum, Na, Cr, Sn, Si, Mo, Ca, Al, Ba, Mg, 
                           Ni, Mn, Zn, P, Ag, Pb, H, B, Cu, Fe, trialDate, Sb
                    FROM EmSpectro 
                    WHERE ID = {0} AND testID = {1} 
                    ORDER BY trialNum", sampleId, testId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting emission spectroscopy data for sample {SampleId}, test {TestId}", sampleId, testId);
            throw;
        }
    }

    public async Task<int> SaveEmissionSpectroscopyAsync(EmissionSpectroscopy data)
    {
        try
        {
            _logger.LogInformation("Saving emission spectroscopy data for sample {SampleId}, test {TestId}", data.Id, data.TestId);
            
            return await _context.Database.ExecuteSqlRawAsync(@"
                INSERT INTO EmSpectro 
                (ID, testID, trialNum, Na, Cr, Sn, Si, Mo, Ca, Al, Ba, Mg, 
                 Ni, Mn, Zn, P, Ag, Pb, H, B, Cu, Fe, trialDate, Sb)
                VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, 
                        {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20}, {21}, {22}, {23})",
                data.Id, data.TestId, data.TrialNum, data.Na, data.Cr, data.Sn, data.Si,
                data.Mo, data.Ca, data.Al, data.Ba, data.Mg, data.Ni, data.Mn, data.Zn,
                data.P, data.Ag, data.Pb, data.H, data.B, data.Cu, data.Fe, data.TrialDate, data.Sb);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving emission spectroscopy data for sample {SampleId}, test {TestId}", data.Id, data.TestId);
            throw;
        }
    }

    public async Task<int> UpdateEmissionSpectroscopyAsync(EmissionSpectroscopy data)
    {
        try
        {
            _logger.LogInformation("Updating emission spectroscopy data for sample {SampleId}, test {TestId}, trial {TrialNum}", 
                data.Id, data.TestId, data.TrialNum);
            
            return await _context.Database.ExecuteSqlRawAsync(@"
                UPDATE EmSpectro 
                SET Na = {3}, Cr = {4}, Sn = {5}, Si = {6}, Mo = {7}, Ca = {8}, Al = {9}, Ba = {10}, 
                    Mg = {11}, Ni = {12}, Mn = {13}, Zn = {14}, P = {15}, Ag = {16}, Pb = {17}, 
                    H = {18}, B = {19}, Cu = {20}, Fe = {21}, Sb = {22}
                WHERE ID = {0} AND testID = {1} AND trialNum = {2}",
                data.Id, data.TestId, data.TrialNum, data.Na, data.Cr, data.Sn, data.Si,
                data.Mo, data.Ca, data.Al, data.Ba, data.Mg, data.Ni, data.Mn, data.Zn,
                data.P, data.Ag, data.Pb, data.H, data.B, data.Cu, data.Fe, data.Sb);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating emission spectroscopy data for sample {SampleId}, test {TestId}", 
                data.Id, data.TestId);
            throw;
        }
    }

    public async Task<int> DeleteEmissionSpectroscopyAsync(int sampleId, int testId, int trialNum)
    {
        try
        {
            _logger.LogInformation("Deleting emission spectroscopy data for sample {SampleId}, test {TestId}, trial {TrialNum}", 
                sampleId, testId, trialNum);
            
            return await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM EmSpectro WHERE ID = {0} AND testID = {1} AND trialNum = {2}",
                sampleId, testId, trialNum);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting emission spectroscopy data for sample {SampleId}, test {TestId}, trial {TrialNum}", 
                sampleId, testId, trialNum);
            throw;
        }
    }

    public async Task<int> ScheduleFerrographyAsync(int sampleId)
    {
        try
        {
            _logger.LogInformation("Scheduling Ferrography test for sample {SampleId}", sampleId);
            
            // Ferrography test ID is 210 based on the database seeding data
            const int ferrographyTestId = 210;
            
            // Check if Ferrography is already scheduled for this sample
            var existingCount = await _context.Database.SqlQueryRaw<int>(
                "SELECT COUNT(*) FROM TestReadings WHERE sampleID = {0} AND testID = {1}",
                sampleId, ferrographyTestId).FirstAsync();
            
            if (existingCount > 0)
            {
                _logger.LogInformation("Ferrography already scheduled for sample {SampleId}", sampleId);
                return 0; // Already scheduled
            }
            
            // Schedule Ferrography test by creating a TestReading entry with status 'X' (Pending)
            return await _context.Database.ExecuteSqlRawAsync(@"
                INSERT INTO TestReadings 
                (sampleID, testID, trialNumber, status, entryDate, entryID)
                VALUES ({0}, {1}, 1, 'X', {2}, 'SYSTEM')",
                sampleId, ferrographyTestId, DateTime.Now);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling Ferrography test for sample {SampleId}", sampleId);
            throw;
        }
    }

    public async Task<bool> SampleExistsAsync(int sampleId)
    {
        try
        {
            _logger.LogInformation("Checking if sample {SampleId} exists", sampleId);
            
            var count = await _context.Database.SqlQueryRaw<int>(
                "SELECT COUNT(*) FROM UsedLubeSamples WHERE ID = {0}", sampleId)
                .FirstAsync();
            
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if sample {SampleId} exists", sampleId);
            return false;
        }
    }
}
