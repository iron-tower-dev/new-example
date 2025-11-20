# UsedLubeSamples Table Comparison: Legacy vs New API

## Database Schema Analysis

### Current Database Table Structure (UsedLubeSamples)
The table has **28 columns** as defined in `db-tables/UsedLubeSamples.sql`:

```sql
CREATE TABLE [dbo].[UsedLubeSamples](
    [ID] [int] NOT NULL,                          -- Primary Key
    [tagNumber] [nvarchar](22) NULL,              -- Equipment tag
    [component] [nvarchar](3) NULL,               -- Component code
    [location] [nvarchar](3) NULL,                -- Location code
    [lubeType] [nvarchar](30) NULL,               -- Lubricant type
    [woNumber] [nvarchar](16) NULL,               -- Work order number
    [trackingNumber] [nvarchar](12) NULL,         -- Tracking number
    [warehouseId] [nvarchar](10) NULL,            -- Warehouse ID
    [batchNumber] [nvarchar](30) NULL,            -- Batch number
    [classItem] [nvarchar](10) NULL,              -- Class item
    [sampleDate] [datetime] NULL,                 -- Sample date
    [receivedOn] [datetime] NULL,                 -- Received date
    [sampledBy] [nvarchar](50) NULL,              -- Sampled by person
    [status] [smallint] NULL,                     -- Status code (250 = active)
    [cmptSelectFlag] [tinyint] NULL,              -- Component select flag
    [newUsedFlag] [tinyint] NULL,                 -- New/Used flag
    [entryId] [nvarchar](5) NULL,                 -- Entry ID
    [validateId] [nvarchar](5) NULL,              -- Validation ID
    [testPricesId] [smallint] NULL,               -- Test prices ID
    [pricingPackageId] [smallint] NULL,           -- Pricing package ID
    [evaluation] [tinyint] NULL,                  -- Evaluation code
    [siteId] [int] NULL,                          -- Site ID
    [results_review_date] [datetime] NULL,        -- Results review date
    [results_avail_date] [datetime] NULL,         -- Results available date
    [results_reviewId] [nvarchar](5) NULL,        -- Results reviewer ID
    [storeSource] [nvarchar](100) NULL,           -- Store source
    [schedule] [nvarchar](1) NULL,                -- Schedule flag
    [returnedDate] [datetime] NULL                -- Returned date
) ON [PRIMARY]
```

### Current API Model (Sample.cs)
The model only has **8 columns**:

```csharp
[Table("UsedLubeSamples")]
public class Sample
{
    [Key]
    public int Id { get; set; }                    // Maps to ID
    
    [Column("tagNumber")]
    public string TagNumber { get; set; }          // ✓ Exists
    
    [Column("component")]
    public string Component { get; set; }          // ✓ Exists
    
    [Column("location")]
    public string Location { get; set; }           // ✓ Exists
    
    [Column("lubeType")]
    public string LubeType { get; set; }           // ✓ Exists
    
    [Column("sampleDate")]
    public DateTime SampleDate { get; set; }       // ✓ Exists
    
    [Column("status")]
    public string Status { get; set; }             // ✗ WRONG TYPE (should be smallint)
    
    [Column("qualityClass")]
    public string? QualityClass { get; set; }      // ✗ DOESN'T EXIST in table
}
```

## Critical Issues Found

### 1. ❌ Missing Columns (20 columns not in model)
The following columns exist in the database but are NOT in the API model:
- `woNumber` - Work order number
- `trackingNumber` - Tracking number
- `warehouseId` - Warehouse ID
- `batchNumber` - Batch number
- `classItem` - Class item
- `receivedOn` - Received date
- `sampledBy` - Sampled by person
- `cmptSelectFlag` - Component select flag
- `newUsedFlag` - New/Used flag
- `entryId` - Entry ID
- `validateId` - Validation ID
- `testPricesId` - Test prices ID
- `pricingPackageId` - Pricing package ID
- `evaluation` - Evaluation code
- `siteId` - Site ID
- `results_review_date` - Results review date
- `results_avail_date` - Results available date
- `results_reviewId` - Results reviewer ID
- `storeSource` - Store source
- `schedule` - Schedule flag
- `returnedDate` - Returned date

### 2. ❌ Wrong Data Type
- `Status` is defined as `string` in the model but is `smallint` in the database
- This could cause data conversion issues

### 3. ❌ Non-Existent Column
- `QualityClass` is in the model but does NOT exist in the UsedLubeSamples table
- This column exists in the `Lube_Sampling_Point` table instead

## Legacy Application Usage

### View: vwUsedLubeSamplesIPDAS
The legacy system uses a view that exposes specific columns:

```sql
CREATE VIEW [dbo].[vwUsedLubeSamplesIPDAS]
AS
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

**Key Points:**
- Status 250 = Active samples
- siteId must not be NULL for IPDAS integration
- Uses woNumber, results_review_date which are missing from our model

### Legacy Query Pattern
From SQL_OPERATIONS_COMPARISON.md, the legacy app uses:

```sql
SELECT distinct 
    t.sampleID,
    u.tagNumber,
    u.Component,
    u.Location,
    l.qualityClass  -- NOTE: From Lube_Sampling_Point, not UsedLubeSamples!
FROM TestReadings t 
INNER JOIN UsedLubeSamples u ON t.sampleID = u.ID 
LEFT OUTER JOIN Lube_Sampling_Point l 
    ON u.tagNumber = l.tagNumber 
    AND u.component = l.component 
    AND u.location = l.location 
WHERE t.status='A' AND t.testID={testID} 
ORDER BY t.sampleID
```

**Critical Finding:** `qualityClass` comes from a JOIN with `Lube_Sampling_Point`, not from UsedLubeSamples!

## Current API Query Pattern

From `SampleService.cs`:

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
        QualityClass = s.QualityClass,  // ❌ This doesn't exist in UsedLubeSamples!
        SampleDate = s.SampleDate,
        Status = s.Status
    })
    .Distinct()
    .OrderBy(s => s.Id)
    .ToListAsync();
```

**Issues:**
1. Missing JOIN with `Lube_Sampling_Point` to get `qualityClass`
2. Status comparison uses string "A" but database has smallint 250
3. Missing many columns that might be needed for full functionality

## Data Seeding Status

✅ **GOOD NEWS:** The UsedLubeSamples table has been successfully seeded with 1000 records:
- Date range: 1993-05-18 to 2003-02-19
- 444 unique tag numbers
- 47 unique components
- All 28 columns populated with real data

## Recommendations

### 1. Update Sample Model (HIGH PRIORITY)
Add all missing columns to match the database schema:

```csharp
[Table("UsedLubeSamples")]
public class Sample
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }
    
    [Column("tagNumber")]
    [MaxLength(22)]
    public string? TagNumber { get; set; }
    
    [Column("component")]
    [MaxLength(3)]
    public string? Component { get; set; }
    
    [Column("location")]
    [MaxLength(3)]
    public string? Location { get; set; }
    
    [Column("lubeType")]
    [MaxLength(30)]
    public string? LubeType { get; set; }
    
    [Column("woNumber")]
    [MaxLength(16)]
    public string? WoNumber { get; set; }
    
    [Column("trackingNumber")]
    [MaxLength(12)]
    public string? TrackingNumber { get; set; }
    
    [Column("warehouseId")]
    [MaxLength(10)]
    public string? WarehouseId { get; set; }
    
    [Column("batchNumber")]
    [MaxLength(30)]
    public string? BatchNumber { get; set; }
    
    [Column("classItem")]
    [MaxLength(10)]
    public string? ClassItem { get; set; }
    
    [Column("sampleDate")]
    public DateTime? SampleDate { get; set; }
    
    [Column("receivedOn")]
    public DateTime? ReceivedOn { get; set; }
    
    [Column("sampledBy")]
    [MaxLength(50)]
    public string? SampledBy { get; set; }
    
    [Column("status")]
    public short? Status { get; set; }  // Changed to short (smallint)
    
    [Column("cmptSelectFlag")]
    public byte? CmptSelectFlag { get; set; }
    
    [Column("newUsedFlag")]
    public byte? NewUsedFlag { get; set; }
    
    [Column("entryId")]
    [MaxLength(5)]
    public string? EntryId { get; set; }
    
    [Column("validateId")]
    [MaxLength(5)]
    public string? ValidateId { get; set; }
    
    [Column("testPricesId")]
    public short? TestPricesId { get; set; }
    
    [Column("pricingPackageId")]
    public short? PricingPackageId { get; set; }
    
    [Column("evaluation")]
    public byte? Evaluation { get; set; }
    
    [Column("siteId")]
    public int? SiteId { get; set; }
    
    [Column("results_review_date")]
    public DateTime? ResultsReviewDate { get; set; }
    
    [Column("results_avail_date")]
    public DateTime? ResultsAvailDate { get; set; }
    
    [Column("results_reviewId")]
    [MaxLength(5)]
    public string? ResultsReviewId { get; set; }
    
    [Column("storeSource")]
    [MaxLength(100)]
    public string? StoreSource { get; set; }
    
    [Column("schedule")]
    [MaxLength(1)]
    public string? Schedule { get; set; }
    
    [Column("returnedDate")]
    public DateTime? ReturnedDate { get; set; }
    
    // Navigation property for quality class (from Lube_Sampling_Point)
    [NotMapped]
    public string? QualityClass { get; set; }
}
```

### 2. Fix SampleService Queries (HIGH PRIORITY)
Update queries to match legacy pattern:

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
        Id = s.Id,
        TagNumber = s.TagNumber,
        Component = s.Component,
        Location = s.Location,
        LubeType = s.LubeType,
        QualityClass = lsp.QualityClass,  // From Lube_Sampling_Point
        SampleDate = s.SampleDate,
        Status = s.Status,
        WoNumber = s.WoNumber,
        SiteId = s.SiteId,
        // Add other needed fields
    })
    .Distinct()
    .OrderBy(s => s.Id)
    .ToListAsync();
```

### 3. Add Lube_Sampling_Point Model (HIGH PRIORITY)
Create model for the quality class lookup:

```csharp
[Table("Lube_Sampling_Point")]
public class LubeSamplingPoint
{
    [Key]
    public int Id { get; set; }
    
    public string TagNumber { get; set; }
    public string Component { get; set; }
    public string Location { get; set; }
    public string? QualityClass { get; set; }
    // Add other fields as needed
}
```

### 4. Update SampleDto (MEDIUM PRIORITY)
Add missing fields to DTO to support full functionality:

```csharp
public class SampleDto
{
    public int Id { get; set; }
    public string? TagNumber { get; set; }
    public string? Component { get; set; }
    public string? Location { get; set; }
    public string? LubeType { get; set; }
    public string? WoNumber { get; set; }
    public DateTime? SampleDate { get; set; }
    public DateTime? ReceivedOn { get; set; }
    public string? SampledBy { get; set; }
    public short? Status { get; set; }
    public int? SiteId { get; set; }
    public DateTime? ResultsReviewDate { get; set; }
    public DateTime? ResultsAvailDate { get; set; }
    public string? QualityClass { get; set; }  // From join
    // Add others as needed
}
```

### 5. Create vwUsedLubeSamplesIPDAS View (MEDIUM PRIORITY)
Ensure the view exists for IPDAS integration:

```sql
CREATE VIEW [dbo].[vwUsedLubeSamplesIPDAS]
AS
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

## Status Constants

Based on the data and view, status codes:
- **250** = Active/Available samples (used in vwUsedLubeSamplesIPDAS)
- **"A"** = Active test readings (used in TestReadings)

These are different tables with different status schemes!

## Conclusion

**CRITICAL FINDINGS:**
1. ✅ UsedLubeSamples table is properly seeded with 1000 records
2. ❌ Sample model is missing 20 columns from the database
3. ❌ QualityClass is incorrectly mapped (should come from Lube_Sampling_Point)
4. ❌ Status data type is wrong (string vs smallint)
5. ❌ Queries don't match legacy pattern (missing JOIN for quality class)

**IMPACT:** The new API cannot fully replicate legacy functionality without these fixes. Data is available but not accessible through the current model.

**PRIORITY:** HIGH - Fix model and queries before production use.
