# Refactoring Plan: Remove RawSqlService

## Overview
Remove `RawSqlService.cs` and `OptimizedRawSqlService.cs` by moving their methods directly into the services/endpoints that use them.

## Current Dependencies

### Services Using IRawSqlService
1. **TestResultService** - Uses TestReadings methods
2. **SampleService** - Uses SampleExistsAsync
3. **TestSchedulingService** - Uses ScheduleFerrographyAsync
4. **FileUploadService** - Uses ExecuteNonQueryAsync
5. **QueryComparisonService** (Migration) - Uses ExecuteStoredProcedureAsync, ExecuteFunctionAsync

### Endpoints Using IRawSqlService
1. **ParticleTestEndpoints** - Uses Filter Residue and Debris ID methods (6 methods)
2. **EmissionSpectroscopyEndpoints** - Uses EmissionSpectroscopy methods (5 methods)
3. **HistoricalResultsEndpoints** - Uses history methods (2 methods)
4. **PerformanceEndpoints** - Uses TestDatabaseConnectionAsync

---

## Proposed New Structure

### 1. Create ParticleTestService
**Purpose:** Handle Filter Residue and Debris Identification tests

**Methods to Move:**
- `GetFilterResidueAsync(int sampleId, int testId)`
- `SaveFilterResidueAsync(FilterResidueResult filterResidue)`
- `DeleteFilterResidueAsync(int sampleId, int testId)`
- `GetDebrisIdentificationAsync(int sampleId, int testId)`
- `SaveDebrisIdentificationAsync(DebrisIdentificationResult debrisId)`
- `DeleteDebrisIdentificationAsync(int sampleId, int testId)`
- `DetermineOverallSeverity(List<ParticleType> particleTypes)` - helper

**Dependencies Needed:**
- LabDbContext
- ILogger<ParticleTestService>
- Particle Analysis methods (see #4)
- TestReadings methods (see #3)

**Update:**
- ParticleTestEndpoints to inject IParticleTestService instead of IRawSqlService

---

### 2. Create EmissionSpectroscopyService
**Purpose:** Handle Emission Spectroscopy test data

**Methods to Move:**
- `GetEmissionSpectroscopyAsync(int sampleId, int testId)`
- `SaveEmissionSpectroscopyAsync(EmissionSpectroscopy data)`
- `UpdateEmissionSpectroscopyAsync(EmissionSpectroscopy data)`
- `DeleteEmissionSpectroscopyAsync(int sampleId, int testId, int trialNum)`
- `ScheduleFerrographyAsync(int sampleId)`

**Dependencies Needed:**
- LabDbContext
- ILogger<EmissionSpectroscopyService>

**Update:**
- EmissionSpectroscopyEndpoints to inject IEmissionSpectroscopyService
- TestSchedulingService to inject IEmissionSpectroscopyService (for ScheduleFerrographyAsync)

---

### 3. Expand TestResultService
**Purpose:** Already handles test results, expand to include TestReadings CRUD

**Methods to Move:**
- `GetTestReadingsAsync(int sampleId, int testId)`
- `SaveTestReadingAsync(TestReading reading)`
- `UpdateTestReadingAsync(TestReading reading)`
- `DeleteTestReadingsAsync(int sampleId, int testId)`

**Dependencies:** Already has LabDbContext, ILogger

**Update:**
- Remove IRawSqlService dependency from TestResultService
- Implement methods directly using LabDbContext

---

### 4. Create ParticleAnalysisService
**Purpose:** Handle particle analysis CRUD operations

**Methods to Move:**
- `GetParticleTypesAsync(int sampleId, int testId)`
- `GetParticleSubTypesAsync(int sampleId, int testId)`
- `GetInspectFilterAsync(int sampleId, int testId)`
- `SaveParticleTypeAsync(ParticleType particleType)`
- `SaveParticleSubTypeAsync(ParticleSubType particleSubType)`
- `SaveInspectFilterAsync(InspectFilter inspectFilter)`
- `DeleteParticleTypesAsync(int sampleId, int testId)`
- `DeleteParticleSubTypesAsync(int sampleId, int testId)`
- `DeleteInspectFilterAsync(int sampleId, int testId)`

**Dependencies Needed:**
- LabDbContext
- ILogger<ParticleAnalysisService>

**Used By:**
- ParticleTestService (new)
- Any other services that need particle analysis

---

### 5. Create HistoricalResultsService
**Purpose:** Handle sample history queries

**Methods to Move:**
- `GetSampleHistoryAsync(int sampleId, int testId)`
- `GetExtendedSampleHistoryAsync(int sampleId, int testId, DateTime? fromDate, DateTime? toDate, int page, int pageSize, string? status)`

**Dependencies Needed:**
- LabDbContext
- ILogger<HistoricalResultsService>

**Update:**
- HistoricalResultsEndpoints to inject IHistoricalResultsService

---

### 6. Expand SampleService
**Purpose:** Add utility methods for sample validation

**Methods to Move:**
- `SampleExistsAsync(int sampleId)`

**Update:**
- SampleService already has LabDbContext
- Add this simple method directly

---

### 7. Create DatabaseUtilityService (Optional)
**Purpose:** Generic database utilities

**Methods to Move (if still needed):**
- `TestDatabaseConnectionAsync()`
- `ExecuteStoredProcedureAsync<T>(string procedureName, params object[] parameters)`
- `ExecuteFunctionAsync<T>(string functionName, params object[] parameters)`
- `ExecuteNonQueryAsync(string sql, params object[] parameters)`

**Note:** These are generic utilities. Consider:
- Moving `TestDatabaseConnectionAsync` to a health check service
- Moving Execute* methods only if still used by migration services
- Otherwise, eliminate if not needed

**Dependencies Needed:**
- LabDbContext
- ILogger<DatabaseUtilityService>

---

## Implementation Steps

### Phase 1: Create New Services
1. Create `IParticleTestService` interface and `ParticleTestService` class
2. Create `IEmissionSpectroscopyService` interface and `EmissionSpectroscopyService` class
3. Create `IParticleAnalysisService` interface and `ParticleAnalysisService` class
4. Create `IHistoricalResultsService` interface and `HistoricalResultsService` class
5. Create `IDatabaseUtilityService` interface and `DatabaseUtilityService` class (if needed)

### Phase 2: Implement Methods
1. Copy methods from RawSqlService to respective new services
2. Update method implementations to directly use LabDbContext
3. Update cross-service dependencies:
   - ParticleTestService → inject IParticleAnalysisService and ITestResultService
   - EmissionSpectroscopyService → standalone
   - etc.

### Phase 3: Update Existing Services
1. TestResultService - add TestReadings methods, remove IRawSqlService dependency
2. SampleService - add SampleExistsAsync method, remove IRawSqlService dependency
3. TestSchedulingService - inject IEmissionSpectroscopyService instead
4. FileUploadService - inject IDatabaseUtilityService if needed

### Phase 4: Update Endpoints
1. ParticleTestEndpoints - inject IParticleTestService
2. EmissionSpectroscopyEndpoints - inject IEmissionSpectroscopyService
3. HistoricalResultsEndpoints - inject IHistoricalResultsService
4. PerformanceEndpoints - inject IDatabaseUtilityService or SampleService

### Phase 5: Update Dependency Injection
1. Update Program.cs:
   - Remove `services.AddScoped<IRawSqlService, RawSqlService>()`
   - Remove `services.AddScoped<IRawSqlService, OptimizedRawSqlService>()`
   - Add new service registrations

### Phase 6: Delete Old Files
1. Delete `Services/IRawSqlService.cs`
2. Delete `Services/RawSqlService.cs`
3. Delete `Services/OptimizedRawSqlService.cs`

### Phase 7: Build and Test
1. Compile project
2. Fix any remaining compilation errors
3. Run application and test endpoints
4. Verify all functionality works

---

## Files to Create (6-7 new files)

### Interfaces
1. `Services/IParticleTestService.cs`
2. `Services/IEmissionSpectroscopyService.cs`
3. `Services/IParticleAnalysisService.cs`
4. `Services/IHistoricalResultsService.cs`
5. `Services/IDatabaseUtilityService.cs` (optional)

### Implementations
6. `Services/ParticleTestService.cs`
7. `Services/EmissionSpectroscopyService.cs`
8. `Services/ParticleAnalysisService.cs`
9. `Services/HistoricalResultsService.cs`
10. `Services/DatabaseUtilityService.cs` (optional)

## Files to Modify (~10 files)

1. `Services/TestResultService.cs` - add TestReadings methods
2. `Services/SampleService.cs` - add SampleExistsAsync
3. `Services/TestSchedulingService.cs` - change IRawSqlService to IEmissionSpectroscopyService
4. `Services/FileUploadService.cs` - change to IDatabaseUtilityService or inline
5. `Endpoints/ParticleTestEndpoints.cs` - inject IParticleTestService
6. `Endpoints/EmissionSpectroscopyEndpoints.cs` - inject IEmissionSpectroscopyService
7. `Endpoints/HistoricalResultsEndpoints.cs` - inject IHistoricalResultsService
8. `Endpoints/PerformanceEndpoints.cs` - inject appropriate service
9. `Program.cs` - update DI registrations
10. `Services/Migration/QueryComparisonService.cs` - inject IDatabaseUtilityService if keeping generic execute methods

## Files to Delete (3 files)

1. `Services/IRawSqlService.cs`
2. `Services/RawSqlService.cs`
3. `Services/OptimizedRawSqlService.cs`

---

## Estimated Effort

- **New Code:** ~1,500-2,000 lines (interfaces + implementations)
- **Modified Code:** ~200-300 lines
- **Deleted Code:** ~1,200 lines
- **Net Change:** +500-1,000 lines (more modular, better separation of concerns)

## Risks and Considerations

1. **Circular Dependencies**: ParticleTestService needs IParticleAnalysisService and ITestResultService
   - Solution: Ensure proper interface segregation

2. **Database Context Sharing**: Multiple services using same DbContext
   - Solution: All services should be scoped, DbContext is already scoped

3. **Transaction Management**: Some methods call multiple other methods
   - Solution: Consider using Unit of Work pattern or explicit transactions if needed

4. **Migration Services**: QueryComparisonService uses generic Execute methods
   - Decision needed: Keep DatabaseUtilityService or refactor migration services differently

5. **Testing**: Each new service needs unit tests
   - Consider: Create corresponding test files for new services

---

## Benefits of This Refactoring

1. **Better Separation of Concerns**: Each service has a clear, focused responsibility
2. **Improved Testability**: Smaller services with fewer dependencies are easier to test
3. **Better Discoverability**: Developers know where to find particle test logic vs emission spectroscopy logic
4. **Reduced Coupling**: Services only depend on what they actually need
5. **Easier Maintenance**: Changes to particle tests don't affect emission spectroscopy code

---

## Alternative Approach: Keep Limited RawSqlService

If full refactoring is too large, consider a middle ground:

1. **Keep** a minimal `IDatabaseUtilityService` with only generic methods:
   - ExecuteStoredProcedureAsync
   - ExecuteFunctionAsync
   - ExecuteNonQueryAsync
   - TestDatabaseConnectionAsync
   - SampleExistsAsync

2. **Move domain-specific methods** to domain services:
   - Particle tests → ParticleTestService
   - Emission spectroscopy → EmissionSpectroscopyService
   - Test readings → TestResultService
   - History → HistoricalResultsService

This reduces the scope while still achieving better separation of concerns.

---

## Decision Required

**Option A: Full Refactoring** (as described above)
- Pros: Clean architecture, clear responsibilities
- Cons: More files, more changes, more testing

**Option B: Partial Refactoring** (keep DatabaseUtilityService)
- Pros: Smaller scope, less risk
- Cons: Still have one "utility" service

**Option C: Minimal Refactoring** (just rename and consolidate)
- Pros: Least work
- Cons: Doesn't fully address the architectural issue

**Recommendation:** Option A (Full Refactoring) for best long-term maintainability