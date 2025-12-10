# TODO Comments in Codebase

**Date:** December 10, 2024  
**Total TODOs Found:** 7 in active code (excluding documentation)  
**Completed:** 1  
**Remaining:** 6

## Backend (.NET API)

### 1. HistoricalResultsEndpoints.cs (Line 90)
```csharp
// TODO: Implement GetExtendedSampleHistoryAsync in ISampleService
// For now, return basic history
var history = await sampleService.GetSampleHistoryAsync(sampleId, testId);
```

**Context:** The `GetExtendedHistoricalResults` endpoint is using basic history instead of extended history with pagination, date range filtering, and status filtering.

**Priority:** Medium - Currently works but lacks advanced filtering features.

**Recommendation:** Add `GetExtendedSampleHistoryAsync` to `ISampleService` that supports:
- Date range filtering (`fromDate`, `toDate`)
- Status filtering
- Pagination (`page`, `pageSize`)
- Should return `(IEnumerable<SampleHistoryDto> results, int totalCount)` tuple

---

### 2. TestEndpoints.cs (Line 76) ✅ COMPLETED
```csharp
// For SSO migration: return all tests since authentication is removed
// TODO: Implement proper user qualification checking with Active Directory
var allTests = await testResultService.GetTestsAsync();
```

**Context:** The `GetQualifiedTests` endpoint currently returns all tests because JWT authentication was removed during SSO migration. It should integrate with Active Directory to check actual user qualifications.

**Priority:** High - This is a security/access control concern.

**Status:** ✅ **COMPLETED** - December 10, 2024

**Implementation:**
- Updated endpoint to use `IQualificationService.GetQualifiedTestsAsync(employeeId)`
- Gets current user's employeeId from JWT claims via `IAuthorizationService.GetEmployeeId(user)`
- Queries `LubeTechQualification` table to filter tests by user's qualifications
- Cross-references with `Test.testStandID` column to return only qualified tests
- Added authentication check to return 401 if user is not authenticated
- No Active Directory integration required - uses existing database tables (`LubeTechList`, `LubeTechQualification`)

---

## Frontend (Angular)

### 3. auth.service.ts (Line 96)
```typescript
// TODO: Re-enable token validation later
// this.validateStoredToken(token).subscribe({ ... });
```

**Context:** Token validation is temporarily disabled during initialization. The service assumes any stored token is valid.

**Priority:** High - Security concern.

**Recommendation:** Re-enable token validation once SSO/Active Directory authentication is implemented. The validation logic is already written (commented out lines 97-116).

---

### 4. ftir-test-entry.component.ts (Line 651)
```typescript
// In legacy system, macro is determined from vwTestScheduleDefinitionBySample or ByMaterial
// For now, set a placeholder - this should be fetched from API
if (lubeType) {
    // TODO: Fetch actual macro from TestSchedulingService
    this.macroType.set('STANDARD');
}
```

**Context:** Macro type determination is hardcoded to 'STANDARD' instead of being fetched from the database.

**Priority:** Low-Medium - Depends on whether different macro types affect test behavior.

**Recommendation:**
1. Add endpoint to TestSchedulingService: `GET /api/test-scheduling/macro/{sampleId}/{testId}`
2. Query `vwTestScheduleDefinitionBySample` or `vwTestScheduleDefinitionByMaterial` views
3. Return actual macro configuration

---

### 5. ftir-test-entry.component.ts (Line 692)
```typescript
onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
        this.uploadedFileName.set(file.name);
        // TODO: Implement file parsing logic
        // This should call FileUploadService to parse the file
        // and populate the form fields
        this.snackBar.open('File upload functionality will be implemented', 'Close', {
            duration: 3000
        });
    }
}
```

**Context:** File upload for FT-IR data files (`.dat`, `.txt`, `.csv`) is not implemented. The UI allows file selection but doesn't process it.

**Priority:** Medium - Would improve user experience by auto-populating values.

**Recommendation:**
1. Implement file parser in FileUploadService for FT-IR instrument output formats
2. Parse key fields: deltaArea, antiOxidant, oxidation, h2o, antiWear, soot, fuelDilution, mixture, weakAcid
3. Auto-populate form fields with parsed values
4. Allow user to review/modify before saving

---

### 6. ftir-test-entry.component.ts (Line 764)
```typescript
private scheduleFollowOnTests(sampleId: number): void {
    // TODO: Implement AutoAddRemoveTests logic from legacy system
    // This would analyze the FT-IR results and schedule additional tests if needed
}
```

**Context:** The legacy system had auto-scheduling logic that would trigger additional tests based on FT-IR results (e.g., if contamination is high, schedule particle count).

**Priority:** Medium - Business logic feature.

**Recommendation:**
1. Review legacy `AutoAddRemoveTests` logic in VB/ASP code
2. Define business rules for when to schedule additional tests
3. Implement in TestSchedulingService
4. Call from frontend after successful FT-IR save

---

### 7. historical-result-details-dialog.component.ts (Line 395)
```typescript
exportResult(): void {
    // TODO: Implement export functionality
    const result = this.resultDetails();
    if (result) {
        console.log('Exporting result:', result);
        // Could implement CSV, JSON, or PDF export here
    }
}
```

**Context:** Export button exists in historical results detail dialog but doesn't do anything.

**Priority:** Low - Nice-to-have feature.

**Recommendation:**
1. Implement CSV export as most useful format
2. Include all test values, trial data, sample info, and metadata
3. Use filename format: `test-result-{sampleId}-{testId}-{date}.csv`

---

### 8. historical-results.component.ts (Line 473)
```typescript
exportData(): void {
    // TODO: Implement export functionality
    console.log('Export functionality to be implemented');
}
```

**Context:** Export button exists in historical results table but doesn't do anything.

**Priority:** Low - Nice-to-have feature.

**Recommendation:**
1. Implement CSV export for current table view (basic or extended)
2. Include all visible columns
3. Respect current filters/pagination state
4. Use filename format: `historical-results-{sampleId}-{testId}-{date}.csv`

---

## Priority Summary

### High Priority (Security/Access Control):
1. ~~**TestEndpoints.cs** - Implement Active Directory user qualification checking~~ ✅ COMPLETED
2. **auth.service.ts** - Re-enable token validation

### Medium Priority (Business Logic):
1. **HistoricalResultsEndpoints.cs** - Implement extended history with filtering
2. **ftir-test-entry.component.ts** - Implement file parsing for FT-IR data files
3. **ftir-test-entry.component.ts** - Implement auto-scheduling logic for follow-on tests
4. **ftir-test-entry.component.ts** - Fetch actual macro type from API

### Low Priority (Nice-to-have):
1. **historical-result-details-dialog.component.ts** - Implement result export
2. **historical-results.component.ts** - Implement table export

---

## Notes

- The jQuery library (`vb-asp/includes/jquery-3.7.1.js`) contains internal TODOs, but these are part of the library itself and not actionable
- Documentation files contain TODO markers but are referencing completed work or historical status
- The high-priority items relate to the SSO/Active Directory migration that was mentioned in the authentication removal work

## Next Steps

1. **Prioritize authentication work:** Implement Active Directory integration and re-enable token validation
2. **Complete FT-IR functionality:** Add file parsing and auto-scheduling logic
3. **Enhance historical results:** Add extended filtering capabilities
4. **Add export features:** Implement CSV exports for historical data (if users request it)
