# UsedLubeSamples Action Plan - Critical Fixes Required

## Executive Summary

After comparing the legacy VB ASP.NET application with the new .NET API, I've identified **critical mismatches** in how the UsedLubeSamples table is being used. The new API's Sample model is missing 20 columns and incorrectly maps the QualityClass field.

## Current Status

### ✅ What's Working
1. **Database Table**: UsedLubeSamples table exists with all 28 columns
2. **Data Seeding**: Successfully seeded with 1,000 real records (1993-2003)
3. **Basic Queries**: Simple SELECT queries work
4. **Table Structure**: Matches legacy database schema exactly

### ❌ What's Broken
1. **Model Mismatch**: Sample.cs only has 8 of 28 columns
2. **Wrong Data Type**: Status is string in model, smallint in database
3. **Missing JOIN**: QualityClass requires JOIN with Lube_Sampling_Point table
4. **Incomplete DTOs**: SampleDto missing critical fields
5. **Query Pattern**: Doesn't match legacy SQL pattern

## Critical Issues Detail

### Issue #1: Missing Columns in Sample Model
**Impact**: HIGH - Cannot access 71% of available data

The Sample model is missing these 20 columns:
```
woNumber, trackingNumber, warehouseId, batchNumber, classItem,
receivedOn, sampledBy, cmptSelectFlag, newUsedFlag, entryId,
validateId, testPricesId, pricingPackageId, evaluation, siteId,
results_review_date, results_avail_date, results_reviewId,
storeSource, schedule, returnedDate
```

**Used By:**
- vwUsedLubeSamplesIPDAS view (IPDAS integration)
- Work order tracking (woNumber)
- Results review workflow (results_review_date, results_reviewId)
- Site-specific filtering (siteId)

### Issue #2: QualityClass Mapping Error
**Impact**: HIGH - Incorrect data source

**Current (WRONG):**
```csharp
[Column("qualityClass")]
public string? QualityClass { get; set; }  // Column doesn't exist!
```

**Correct:**
```csharp
[NotMapped]
public string? QualityClass { get; set; }  // Must come from JOIN
```

**Legacy Pattern:**
```sql
SELECT u.*, l.qualityClass
FROM UsedLubeSamples u
LEFT JOIN Lube_Sampling_Point l 
    ON u.tagNumber = l.tagNumber 
    AND u.component = l.component 
    AND u.location = l.location
```

### Issue #3: Status Data Type Mismatch
**Impact**: MEDIUM - Potential runtime errors

**Current (WRONG):**
```csharp
[Column("status")]
public string Status { get; set; }  // Database has smallint!
```

**Correct:**
```csharp
[Column("status")]
public short? Status { get; set; }  // Match database type
```

**Status Values:**
- 250 = Active samples (used in vwUsedLubeSamplesIPDAS)
- Other values may exist for different states

## Required Actions

### Action 1: Update Sample Model ⚠️ CRITICAL
**File**: `LabResultsApi/Models/Sample.cs`
**Priority**: HIGH
**Effort**: 30 minutes

Add all 20 missing columns with correct data types:
- Change Status from string to short?
- Add woNumber, siteId, results_review_date, etc.
- Mark QualityClass as [NotMapped]
- Add proper MaxLength attributes

**See**: USEDLUBESAMPLES_COMPARISON.md for complete model definition

### Action 2: Create LubeSamplingPoint Model ⚠️ CRITICAL
**File**: `LabResultsApi/Models/LubeSamplingPoint.cs` (NEW)
**Priority**: HIGH
**Effort**: 20 minutes

```csharp
[Table("Lube_Sampling_Point")]
public class LubeSamplingPoint
{
    [Key]
    public int Id { get; set; }
    
    [Column("tagNumber")]
    public string TagNumber { get; set; }
    
    [Column("component")]
    public string Component { get; set; }
    
    [Column("location")]
    public string Location { get; set; }
    
    [Column("qualityClass")]
    public string? QualityClass { get; set; }
    
    // Add other fields as discovered
}
```

### Action 3: Fix SampleService Queries ⚠️ CRITICAL
**File**: `LabResultsApi/Services/SampleService.cs`
**Priority**: HIGH
**Effort**: 45 minutes

Update GetSamplesByTestAsync to include JOIN:
```csharp
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
        // Map all fields including QualityClass from lsp
    })
    .ToListAsync();
```

### Action 4: Update SampleDto ⚠️ IMPORTANT
**File**: `LabResultsApi/DTOs/SampleDto.cs`
**Priority**: MEDIUM
**Effort**: 15 minutes

Add missing fields:
- WoNumber
- SiteId
- ReceivedOn
- SampledBy
- ResultsReviewDate
- ResultsAvailDate
- etc.

### Action 5: Add DbSet for LubeSamplingPoint ⚠️ CRITICAL
**File**: `LabResultsApi/Data/LabDbContext.cs`
**Priority**: HIGH
**Effort**: 5 minutes

```csharp
public DbSet<LubeSamplingPoint> LubeSamplingPoints { get; set; }
```

### Action 6: Create/Verify View ⚠️ IMPORTANT
**File**: `db-views/vwUsedLubeSamplesIPDAS.sql`
**Priority**: MEDIUM
**Effort**: 10 minutes

Ensure view exists and is deployed:
```sql
CREATE VIEW [dbo].[vwUsedLubeSamplesIPDAS] AS
SELECT 
    siteId AS site_id,
    ID AS sample_id,
    tagNumber AS eq_tag_num,
    component AS lube_component_code,
    location AS lube_location_code,
    NULL AS lube_id,
    woNumber AS wo_num,
    sampleDate AS sample_date,
    results_review_date
FROM dbo.UsedLubeSamples
WHERE (status = 250) AND (siteId IS NOT NULL)
```

### Action 7: Update Tests ⚠️ IMPORTANT
**Files**: Test files in `LabResultsApi.Tests/`
**Priority**: MEDIUM
**Effort**: 30 minutes

Update unit tests to:
- Test new model properties
- Test JOIN with Lube_Sampling_Point
- Test status code filtering (250 vs "A")
- Verify data type conversions

## Testing Checklist

After implementing fixes, verify:

- [ ] Sample model can read all 28 columns from database
- [ ] QualityClass correctly populated from Lube_Sampling_Point JOIN
- [ ] Status filtering works with smallint values (250)
- [ ] GetSamplesByTestAsync returns correct samples
- [ ] vwUsedLubeSamplesIPDAS view accessible
- [ ] No runtime errors from data type mismatches
- [ ] DTOs serialize correctly with all fields
- [ ] Existing tests still pass
- [ ] New tests cover JOIN scenarios

## Verification Queries

Run these to verify fixes:

```sql
-- Verify sample with quality class
SELECT TOP 5 
    u.ID, u.tagNumber, u.component, u.location,
    u.status, u.siteId, u.woNumber,
    l.qualityClass
FROM UsedLubeSamples u
LEFT JOIN Lube_Sampling_Point l 
    ON u.tagNumber = l.tagNumber 
    AND u.component = l.component 
    AND u.location = l.location
WHERE u.status = 250

-- Verify IPDAS view
SELECT TOP 5 * FROM vwUsedLubeSamplesIPDAS

-- Count samples by status
SELECT status, COUNT(*) as count
FROM UsedLubeSamples
GROUP BY status
ORDER BY count DESC
```

## Timeline

**Total Estimated Effort**: 2.5 hours

1. **Phase 1** (1 hour): Update models
   - Update Sample.cs
   - Create LubeSamplingPoint.cs
   - Update LabDbContext.cs

2. **Phase 2** (1 hour): Update services and DTOs
   - Fix SampleService.cs queries
   - Update SampleDto.cs
   - Update related services

3. **Phase 3** (30 minutes): Testing and verification
   - Run verification queries
   - Update unit tests
   - Test API endpoints

## Risk Assessment

**If Not Fixed:**
- ❌ Cannot access work order information (woNumber)
- ❌ Cannot filter by site (siteId)
- ❌ Cannot track results review workflow
- ❌ IPDAS integration will fail
- ❌ Quality class data will be incorrect/missing
- ❌ Potential runtime errors from type mismatches

**After Fixing:**
- ✅ Full access to all sample data
- ✅ Correct quality class from proper JOIN
- ✅ IPDAS integration works
- ✅ Matches legacy application behavior
- ✅ No data type conversion errors

## Conclusion

The UsedLubeSamples table is properly seeded with data, but the API cannot fully utilize it due to model and query mismatches. These fixes are **CRITICAL** for production readiness and must be completed before the new API can replace the legacy system.

**Next Steps:**
1. Review this action plan
2. Implement fixes in order of priority
3. Run verification queries
4. Update tests
5. Deploy and verify in test environment
