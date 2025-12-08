using LabResultsApi.Models;
using LabResultsApi.DTOs;

namespace LabResultsApi.Services;

public interface IRawSqlService
{
    Task<List<TestReading>> GetTestReadingsAsync(int sampleId, int testId);
    Task<int> SaveTestReadingAsync(TestReading reading);
    Task<int> UpdateTestReadingAsync(TestReading reading);
    Task<int> DeleteTestReadingsAsync(int sampleId, int testId);
    Task<List<EmissionSpectroscopy>> GetEmissionSpectroscopyAsync(int sampleId, int testId);
    Task<int> SaveEmissionSpectroscopyAsync(EmissionSpectroscopy data);
    Task<int> UpdateEmissionSpectroscopyAsync(EmissionSpectroscopy data);
    Task<int> DeleteEmissionSpectroscopyAsync(int sampleId, int testId, int trialNum);
    Task<int> ScheduleFerrographyAsync(int sampleId);
    Task<List<SampleHistoryDto>> GetSampleHistoryAsync(int sampleId, int testId);
    Task<ExtendedHistoryResultDto> GetExtendedSampleHistoryAsync(int sampleId, int testId, DateTime? fromDate, DateTime? toDate, int page, int pageSize, string? status);
    Task<bool> TestDatabaseConnectionAsync();
    Task<bool> SampleExistsAsync(int sampleId);
    
    // Particle Analysis methods
    Task<List<ParticleType>> GetParticleTypesAsync(int sampleId, int testId);
    Task<List<ParticleSubType>> GetParticleSubTypesAsync(int sampleId, int testId);
    Task<InspectFilter?> GetInspectFilterAsync(int sampleId, int testId);
    Task<int> SaveParticleTypeAsync(ParticleType particleType);
    Task<int> SaveParticleSubTypeAsync(ParticleSubType particleSubType);
    Task<int> SaveInspectFilterAsync(InspectFilter inspectFilter);
    Task<int> DeleteParticleTypesAsync(int sampleId, int testId);
    Task<int> DeleteParticleSubTypesAsync(int sampleId, int testId);
    Task<int> DeleteInspectFilterAsync(int sampleId, int testId);
    
    // Filter Residue methods (Test ID 180)
    Task<FilterResidueResult?> GetFilterResidueAsync(int sampleId, int testId);
    Task<int> SaveFilterResidueAsync(FilterResidueResult filterResidue);
    Task<int> DeleteFilterResidueAsync(int sampleId, int testId);
    
    // Debris Identification methods (Test ID 240)
    Task<DebrisIdentificationResult?> GetDebrisIdentificationAsync(int sampleId, int testId);
    Task<int> SaveDebrisIdentificationAsync(DebrisIdentificationResult debrisId);
    Task<int> DeleteDebrisIdentificationAsync(int sampleId, int testId);
    
    // Stored procedure and function support
    Task<List<T>> ExecuteStoredProcedureAsync<T>(string procedureName, params object[] parameters) where T : class;
    Task<T?> ExecuteFunctionAsync<T>(string functionName, params object[] parameters);
    Task<int> ExecuteNonQueryAsync(string sql, params object[] parameters);
}