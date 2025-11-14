using Microsoft.EntityFrameworkCore;
using LabResultsApi.Data;
using LabResultsApi.Models;
using LabResultsApi.DTOs;

namespace LabResultsApi.Services;

public class RawSqlService : IRawSqlService
{
    private readonly LabDbContext _context;
    private readonly ILogger<RawSqlService> _logger;

    public RawSqlService(LabDbContext context, ILogger<RawSqlService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<TestReading>> GetTestReadingsAsync(int sampleId, int testId)
    {
        try
        {
            _logger.LogInformation("Getting test readings for sample {SampleId}, test {TestId}", sampleId, testId);
            
            return await _context.TestReadings
                .FromSqlRaw(@"
                    SELECT sampleID, testID, trialNumber, value1, value2, value3, 
                           trialCalc, ID1, ID2, ID3, trialComplete, status, 
                           schedType, entryID, validateID, entryDate, valiDate, MainComments
                    FROM TestReadings 
                    WHERE sampleID = {0} AND testID = {1} 
                    ORDER BY trialNumber", sampleId, testId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting test readings for sample {SampleId}, test {TestId}", sampleId, testId);
            throw;
        }
    }

    public async Task<int> SaveTestReadingAsync(TestReading reading)
    {
        try
        {
            _logger.LogInformation("Saving test reading for sample {SampleId}, test {TestId}, trial {TrialNumber}", 
                reading.SampleId, reading.TestId, reading.TrialNumber);
            
            return await _context.Database.ExecuteSqlRawAsync(@"
                INSERT INTO TestReadings 
                (sampleID, testID, trialNumber, value1, value2, value3, trialCalc, 
                 ID1, ID2, ID3, status, entryDate, MainComments, entryID)
                VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13})",
                reading.SampleId, reading.TestId, reading.TrialNumber, 
                reading.Value1, reading.Value2, reading.Value3, reading.TrialCalc,
                reading.Id1, reading.Id2, reading.Id3, reading.Status, 
                reading.EntryDate, reading.MainComments, reading.EntryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving test reading for sample {SampleId}, test {TestId}", 
                reading.SampleId, reading.TestId);
            throw;
        }
    }

    public async Task<int> UpdateTestReadingAsync(TestReading reading)
    {
        try
        {
            _logger.LogInformation("Updating test reading for sample {SampleId}, test {TestId}, trial {TrialNumber}", 
                reading.SampleId, reading.TestId, reading.TrialNumber);
            
            return await _context.Database.ExecuteSqlRawAsync(@"
                UPDATE TestReadings 
                SET value1 = {3}, value2 = {4}, value3 = {5}, trialCalc = {6},
                    ID1 = {7}, ID2 = {8}, ID3 = {9}, status = {10}, 
                    MainComments = {11}, validateID = {12}, valiDate = {13}
                WHERE sampleID = {0} AND testID = {1} AND trialNumber = {2}",
                reading.SampleId, reading.TestId, reading.TrialNumber,
                reading.Value1, reading.Value2, reading.Value3, reading.TrialCalc,
                reading.Id1, reading.Id2, reading.Id3, reading.Status,
                reading.MainComments, reading.ValidateId, reading.ValidateDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating test reading for sample {SampleId}, test {TestId}", 
                reading.SampleId, reading.TestId);
            throw;
        }
    }

    public async Task<int> DeleteTestReadingsAsync(int sampleId, int testId)
    {
        try
        {
            _logger.LogInformation("Deleting test readings for sample {SampleId}, test {TestId}", sampleId, testId);
            
            return await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM TestReadings WHERE sampleID = {0} AND testID = {1}",
                sampleId, testId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting test readings for sample {SampleId}, test {TestId}", sampleId, testId);
            throw;
        }
    }

    public async Task<List<EmissionSpectroscopy>> GetEmissionSpectroscopyAsync(int sampleId, int testId)
    {
        try
        {
            _logger.LogInformation("Getting emission spectroscopy data for sample {SampleId}, test {TestId}", sampleId, testId);
            
            return await _context.EmSpectro
                .FromSqlRaw(@"
                    SELECT ID, testID, trialNum, Na, Cr, Sn, Si, Mo, Ca, Al, Ba, Mg, 
                           Ni, Mn, Zn, P, Ag, Pb, H, B, Cu, Fe, trialDate, status
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
                 Ni, Mn, Zn, P, Ag, Pb, H, B, Cu, Fe, trialDate, status)
                VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, 
                        {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20}, {21}, {22}, {23})",
                data.Id, data.TestId, data.TrialNum, data.Na, data.Cr, data.Sn, data.Si,
                data.Mo, data.Ca, data.Al, data.Ba, data.Mg, data.Ni, data.Mn, data.Zn,
                data.P, data.Ag, data.Pb, data.H, data.B, data.Cu, data.Fe, data.TrialDate, data.Status);
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
                    H = {18}, B = {19}, Cu = {20}, Fe = {21}, status = {22}
                WHERE ID = {0} AND testID = {1} AND trialNum = {2}",
                data.Id, data.TestId, data.TrialNum, data.Na, data.Cr, data.Sn, data.Si,
                data.Mo, data.Ca, data.Al, data.Ba, data.Mg, data.Ni, data.Mn, data.Zn,
                data.P, data.Ag, data.Pb, data.H, data.B, data.Cu, data.Fe, data.Status);
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

    public async Task<List<SampleHistoryDto>> GetSampleHistoryAsync(int sampleId, int testId)
    {
        try
        {
            _logger.LogInformation("Getting sample history for sample {SampleId}, test {TestId}", sampleId, testId);
            
            // Get last 12 results for the same equipment/component combination
            var sql = @"
                SELECT TOP 12 
                    s.ID as SampleId,
                    s.tagNumber as TagNumber,
                    s.sampleDate as SampleDate,
                    t.testName as TestName,
                    CASE 
                        WHEN tr.status = 'C' THEN 'Complete'
                        WHEN tr.status = 'E' THEN 'In Progress'
                        WHEN tr.status = 'X' THEN 'Pending'
                        ELSE 'Unknown'
                    END as Status,
                    tr.entryDate as EntryDate
                FROM UsedLubeSamples s
                INNER JOIN TestReadings tr ON s.ID = tr.sampleID
                INNER JOIN Test t ON tr.testID = t.testID
                WHERE s.tagNumber = (SELECT tagNumber FROM UsedLubeSamples WHERE ID = {0})
                    AND s.component = (SELECT component FROM UsedLubeSamples WHERE ID = {0})
                    AND tr.testID = {1}
                    AND s.ID <= {0}
                ORDER BY s.sampleDate DESC, s.ID DESC";

            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            
            // Create parameters properly
            var sampleIdParam = command.CreateParameter();
            sampleIdParam.ParameterName = "@sampleId";
            sampleIdParam.Value = sampleId;
            command.Parameters.Add(sampleIdParam);
            
            var testIdParam = command.CreateParameter();
            testIdParam.ParameterName = "@testId";
            testIdParam.Value = testId;
            command.Parameters.Add(testIdParam);
            
            var results = new List<SampleHistoryDto>();
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                results.Add(new SampleHistoryDto
                {
                    SampleId = reader.GetInt32(reader.GetOrdinal("SampleId")),
                    TagNumber = reader.GetString(reader.GetOrdinal("TagNumber")),
                    SampleDate = reader.GetDateTime(reader.GetOrdinal("SampleDate")),
                    TestName = reader.GetString(reader.GetOrdinal("TestName")),
                    Status = reader.GetString(reader.GetOrdinal("Status")),
                    EntryDate = reader.IsDBNull(reader.GetOrdinal("EntryDate")) ? null : reader.GetDateTime(reader.GetOrdinal("EntryDate"))
                });
            }
            
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sample history for sample {SampleId}, test {TestId}", sampleId, testId);
            throw;
        }
    }

    public async Task<ExtendedHistoryResultDto> GetExtendedSampleHistoryAsync(int sampleId, int testId, DateTime? fromDate, DateTime? toDate, int page, int pageSize, string? status)
    {
        try
        {
            _logger.LogInformation("Getting extended sample history for sample {SampleId}, test {TestId}, page {Page}, pageSize {PageSize}", 
                sampleId, testId, page, pageSize);

            // Build the WHERE clause dynamically based on filters
            var whereConditions = new List<string>
            {
                "s.tagNumber = (SELECT tagNumber FROM UsedLubeSamples WHERE ID = @sampleId)",
                "s.component = (SELECT component FROM UsedLubeSamples WHERE ID = @sampleId)",
                "tr.testID = @testId"
            };

            var parameters = new List<(string Name, object Value)>
            {
                ("@sampleId", sampleId),
                ("@testId", testId)
            };

            if (fromDate.HasValue)
            {
                whereConditions.Add("s.sampleDate >= @fromDate");
                parameters.Add(("@fromDate", fromDate.Value));
            }

            if (toDate.HasValue)
            {
                whereConditions.Add("s.sampleDate <= @toDate");
                parameters.Add(("@toDate", toDate.Value));
            }

            if (!string.IsNullOrEmpty(status))
            {
                whereConditions.Add("tr.status = @status");
                parameters.Add(("@status", status.ToUpper().Substring(0, 1))); // Convert to single character status
            }

            var whereClause = string.Join(" AND ", whereConditions);

            // Get total count first
            var countSql = $@"
                SELECT COUNT(DISTINCT s.ID)
                FROM UsedLubeSamples s
                INNER JOIN TestReadings tr ON s.ID = tr.sampleID
                INNER JOIN Test t ON tr.testID = t.testID
                WHERE {whereClause}";

            // Get paginated results
            var dataSql = $@"
                SELECT 
                    s.ID as SampleId,
                    s.tagNumber as TagNumber,
                    s.sampleDate as SampleDate,
                    t.testName as TestName,
                    CASE 
                        WHEN tr.status = 'C' THEN 'Complete'
                        WHEN tr.status = 'E' THEN 'In Progress'
                        WHEN tr.status = 'X' THEN 'Pending'
                        ELSE 'Unknown'
                    END as Status,
                    tr.entryDate as EntryDate
                FROM UsedLubeSamples s
                INNER JOIN TestReadings tr ON s.ID = tr.sampleID
                INNER JOIN Test t ON tr.testID = t.testID
                WHERE {whereClause}
                ORDER BY s.sampleDate DESC, s.ID DESC
                OFFSET @offset ROWS
                FETCH NEXT @pageSize ROWS ONLY";

            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            // Get total count
            int totalCount;
            using (var countCommand = connection.CreateCommand())
            {
                countCommand.CommandText = countSql;
                foreach (var param in parameters)
                {
                    var dbParam = countCommand.CreateParameter();
                    dbParam.ParameterName = param.Name;
                    dbParam.Value = param.Value;
                    countCommand.Parameters.Add(dbParam);
                }

                var countResult = await countCommand.ExecuteScalarAsync();
                totalCount = Convert.ToInt32(countResult);
            }

            // Get paginated data
            var results = new List<SampleHistoryDto>();
            using (var dataCommand = connection.CreateCommand())
            {
                dataCommand.CommandText = dataSql;
                
                foreach (var param in parameters)
                {
                    var dbParam = dataCommand.CreateParameter();
                    dbParam.ParameterName = param.Name;
                    dbParam.Value = param.Value;
                    dataCommand.Parameters.Add(dbParam);
                }

                // Add pagination parameters
                var offsetParam = dataCommand.CreateParameter();
                offsetParam.ParameterName = "@offset";
                offsetParam.Value = (page - 1) * pageSize;
                dataCommand.Parameters.Add(offsetParam);

                var pageSizeParam = dataCommand.CreateParameter();
                pageSizeParam.ParameterName = "@pageSize";
                pageSizeParam.Value = pageSize;
                dataCommand.Parameters.Add(pageSizeParam);

                using var reader = await dataCommand.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    results.Add(new SampleHistoryDto
                    {
                        SampleId = reader.GetInt32(reader.GetOrdinal("SampleId")),
                        TagNumber = reader.GetString(reader.GetOrdinal("TagNumber")),
                        SampleDate = reader.GetDateTime(reader.GetOrdinal("SampleDate")),
                        TestName = reader.GetString(reader.GetOrdinal("TestName")),
                        Status = reader.GetString(reader.GetOrdinal("Status")),
                        EntryDate = reader.IsDBNull(reader.GetOrdinal("EntryDate")) ? null : reader.GetDateTime(reader.GetOrdinal("EntryDate"))
                    });
                }
            }

            return new ExtendedHistoryResultDto
            {
                Results = results,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting extended sample history for sample {SampleId}, test {TestId}", sampleId, testId);
            throw;
        }
    }

    public async Task<bool> TestDatabaseConnectionAsync()
    {
        try
        {
            _logger.LogInformation("Testing database connection");
            
            // Simple query to test connection
            await _context.Database.ExecuteSqlRawAsync("SELECT 1");
            
            _logger.LogInformation("Database connection test successful");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database connection test failed: {ErrorMessage}", ex.Message);
            return false;
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

    // Particle Analysis methods
    public async Task<List<ParticleType>> GetParticleTypesAsync(int sampleId, int testId)
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

    public async Task<List<ParticleSubType>> GetParticleSubTypesAsync(int sampleId, int testId)
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

    public async Task<List<T>> ExecuteStoredProcedureAsync<T>(string procedureName, params object[] parameters) where T : class
    {
        try
        {
            _logger.LogInformation("Executing stored procedure {ProcedureName}", procedureName);
            
            var parameterString = string.Join(", ", parameters.Select((_, i) => $"{{{i}}}"));
            var sql = $"EXEC {procedureName} {parameterString}";
            
            return await _context.Set<T>().FromSqlRaw(sql, parameters).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing stored procedure {ProcedureName}", procedureName);
            throw;
        }
    }

    public async Task<T?> ExecuteFunctionAsync<T>(string functionName, params object[] parameters)
    {
        try
        {
            _logger.LogInformation("Executing function {FunctionName}", functionName);
            
            var parameterString = string.Join(", ", parameters.Select((_, i) => $"{{{i}}}"));
            var sql = $"SELECT {functionName}({parameterString})";
            
            var result = await _context.Database.SqlQueryRaw<T>(sql, parameters).FirstOrDefaultAsync();
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing function {FunctionName}", functionName);
            throw;
        }
    }

    public async Task<int> ExecuteNonQueryAsync(string sql, params object[] parameters)
    {
        try
        {
            _logger.LogInformation("Executing non-query SQL: {Sql}", sql);
            
            return await _context.Database.ExecuteSqlRawAsync(sql, parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing non-query SQL: {Sql}", sql);
            throw;
        }
    }
}