# SQL Verification Summary: Legacy vs New API

## Overview

This document summarizes the verification of SQL statements and data sources between the legacy VB ASP.NET application and the new .NET API, with special focus on the UsedLubeSamples table.

## Key Findings

### ✅ UsedLubeSamples Table - Data Status
- **Table Structure**: ✅ Correct (28 columns matching legacy)
- **Data Seeding**: ✅ Complete (1,000 records from 1993-2003)
- **Schema Match**: ✅ Exact match with legacy database
- **View Support**: ✅ vwUsedLubeSamplesIPDAS exists

### ❌ UsedLubeSamples Table - API Implementation
- **Model Completeness**: ❌ Only 8 of 28 columns (71% missing)
- **Data Type Accuracy**: ❌ Status is wrong type (string vs smallint)
- **Query Pattern**: ❌ Missing JOIN with Lube_Sampling_Point
- **QualityClass Source**: ❌ Incorrectly mapped to non-existent column

## Detailed Comparison

### 1. Table Structure ✅ VERIFIED

**Database Schema** (from `db-tables/UsedLubeSamples.sql`):
```
28 columns total:
- ID (int, PK)
- tagNumber, component, location, lubeType
- woNumber, trackingNumber, warehouseId, batchNumber, classItem
- sampleDate, receivedOn, sampledBy
- status, cmptSelectFlag, newUsedFlag
- entryId, validateId, testPricesId, pricingPackageId
- evaluation, siteId
- results_review_date, results_avail_date, results_reviewId
- storeSource, schedule, returnedDate
```

**Verification Query**:
```sql
SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'UsedLubeSamples'
-- Result: 28 columns ✅
```

### 2. Data Seeding ✅ VERIFIED

**Seeding Script**: `LabResultsApi/Scripts/generate_used_lube_samples_inserts.py`
**Source Data**: `db-seeding/UsedLubeSamples.csv`

**Verification Results**:
```sql
SELECT 
    COUNT(*) as TotalRecords,
    MIN(sampleDate) as EarliestSample,
    MAX(sampleDate) as LatestSample,
    COUNT(DISTINCT tagNumber) as UniqueTagNumbers,
    COUNT(DISTINCT component) as UniqueComponents
FROM UsedLubeSamples

Results:
- TotalRecords: 1000 ✅
- EarliestSample: 1993-05-18 ✅
- LatestSample: 2003-02-19 ✅
- UniqueTagNumbers: 444 ✅
- UniqueComponents: 47 ✅
```

### 3. Legacy Query Pattern 📋 DOCUMENTED

**From Legacy VB ASP.NET** (SQL_OPERATIONS_COMPARISON.md):
```sql
SELECT distinct 
    t.sampleID,
    u.tagNumber,
    u.Component,
    u.Location,
    l.qualityClass  -- ⚠️ From Lube_Sampling_Point, NOT UsedLubeSamples
FROM TestReadings t 
INNER JOIN UsedLubeSamples u ON t.sampleID = u.ID 
LEFT OUTER JOIN Lube_Sampling_Point l 
    ON u.tagNumber = l.tagNumber 
    AND u.component = l.component 
    AND u.location = l.location 
WHERE t.status='A' AND t.testID={testID} 
ORDER BY t.sampleID
```

**Key Points**:
- Uses 3-table JOIN
- QualityClass from Lube_Sampling_Point (not UsedLubeSamples)
- Filters on TestReadings.status = 'A'
- Filters on specific testID

### 4. Current API Query Pattern ❌ INCORRECT

**From SampleService.cs**:
```csharp
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
        QualityClass = s.QualityClass,  // ❌ WRONG: Column doesn't exist
        SampleDate = s.SampleDate,
        Status = s.Status
    })
    .Distinct()
    .OrderBy(s => s.Id)
    .ToListAsync();
```

**Issues**:
1. ❌ Missing JOIN with Lube_Sampling_Point
2. ❌ QualityClass accessed from wrong source
3. ❌ Only uses 8 of 28 available columns
4. ❌ Status type mismatch (string vs smallint)

### 5. View Usage ✅ VERIFIED

**vwUsedLubeSamplesIPDAS** (for IPDAS integration):
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

**Verification**:
```sql
SELECT COUNT(*) FROM vwUsedLubeSamplesIPDAS
-- View exists and is accessible ✅
```

**Critical Fields Used**:
- siteId ❌ Not in current API model
- woNumber ❌ Not in current API model
- results_review_date ❌ Not in current API model

### 6. Status Code Verification 📋 DOCUMENTED

**Different Status Schemes**:

**UsedLubeSamples.status** (smallint):
- 250 = Active samples (used in vwUsedLubeSamplesIPDAS)
- Other values may exist for different states

**TestReadings.status** (string):
- "A" = Active test readings
- Other values for different states

**Current API Issue**:
```csharp
[Column("status")]
public string Status { get; set; }  // ❌ WRONG TYPE
```

**Should Be**:
```csharp
[Column("status")]
public short? Status { get; set; }  // ✅ CORRECT TYPE
```

## Missing Columns Impact Analysis

### Critical Missing Columns (HIGH IMPACT)

1. **siteId** (int)
   - Used by: vwUsedLubeSamplesIPDAS
   - Impact: IPDAS integration will fail
   - Usage: Site-specific filtering

2. **woNumber** (nvarchar(16))
   - Used by: vwUsedLubeSamplesIPDAS, work order tracking
   - Impact: Cannot link samples to work orders
   - Usage: Maintenance workflow integration

3. **results_review_date** (datetime)
   - Used by: vwUsedLubeSamplesIPDAS, review workflow
   - Impact: Cannot track review status
   - Usage: Results approval process

4. **results_reviewId** (nvarchar(5))
   - Used by: Review workflow
   - Impact: Cannot track who reviewed results
   - Usage: Audit trail

### Important Missing Columns (MEDIUM IMPACT)

5. **receivedOn** (datetime)
   - Impact: Cannot track sample receipt
   - Usage: Turnaround time calculations

6. **sampledBy** (nvarchar(50))
   - Impact: Cannot track who took sample
   - Usage: Quality control, audit trail

7. **entryId** (nvarchar(5))
   - Impact: Cannot track data entry person
   - Usage: Audit trail

8. **validateId** (nvarchar(5))
   - Impact: Cannot track validator
   - Usage: Quality assurance workflow

### Supporting Missing Columns (LOW IMPACT)

9-28. Other fields (trackingNumber, warehouseId, batchNumber, etc.)
   - Impact: Reduced functionality
   - Usage: Various tracking and reporting features

## Lube_Sampling_Point Table ✅ VERIFIED

**Table Exists**: Yes
**Has Data**: Yes
**Used For**: Quality class lookup

**Sample Data**:
```
tagNumber | component | location | qualityClass
----------|-----------|----------|-------------
00        | 000       | 000      | Q
0678788   | 000       | 000      | Q
0678789   | 000       | 000      | Q
```

**Current API Status**: ❌ No model exists for this table

## Recommendations Summary

### Immediate Actions (Before Production)

1. **Update Sample Model** ⚠️ CRITICAL
   - Add all 20 missing columns
   - Fix Status data type (string → short?)
   - Mark QualityClass as [NotMapped]

2. **Create LubeSamplingPoint Model** ⚠️ CRITICAL
   - New model for quality class lookup
   - Add to DbContext

3. **Fix SampleService Queries** ⚠️ CRITICAL
   - Add JOIN with Lube_Sampling_Point
   - Update to use all available columns
   - Fix status filtering

4. **Update DTOs** ⚠️ IMPORTANT
   - Add missing fields to SampleDto
   - Support full data transfer

5. **Verify View Access** ⚠️ IMPORTANT
   - Ensure vwUsedLubeSamplesIPDAS is accessible
   - Test IPDAS integration

### Testing Requirements

Before deployment, verify:
- [ ] All 28 columns accessible through API
- [ ] QualityClass correctly populated from JOIN
- [ ] Status filtering works with smallint 250
- [ ] vwUsedLubeSamplesIPDAS returns data
- [ ] Work order linking functional
- [ ] Results review workflow supported
- [ ] No runtime type conversion errors

## Conclusion

### What's Working ✅
- Database structure is correct
- Data is properly seeded
- Views exist and are accessible
- Table matches legacy schema exactly

### What Needs Fixing ❌
- API model incomplete (71% of columns missing)
- Query pattern doesn't match legacy
- QualityClass incorrectly sourced
- Data type mismatches
- Missing Lube_Sampling_Point integration

### Risk Level: 🔴 HIGH

The new API **cannot** fully replace the legacy system without these fixes. While the database is ready, the API layer cannot access or utilize the data correctly.

### Estimated Fix Time: 2.5 hours

See `USEDLUBESAMPLES_ACTION_PLAN.md` for detailed implementation steps.

---

**Generated**: 2025-11-20
**Verified By**: Database schema comparison, data seeding verification, legacy code analysis
**Status**: ❌ CRITICAL ISSUES IDENTIFIED - FIXES REQUIRED
