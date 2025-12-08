# Model-to-SQL Schema Alignment Summary

## Overview

This document summarizes the changes made to align C# entity models in `LabResultsApi/Models/` with their corresponding SQL table definitions in `db-tables/`.

## Changes Made

### 1. Test.cs - Complete Rewrite ✅

**Issue**: Model properties did not match the SQL table schema at all.

**SQL Table Columns** (from `db-tables/Test.sql`):
- ID (smallint, nullable)
- name (nvarchar(40), nullable)
- testStandID (smallint, nullable)
- sampleVolumeRequired (smallint, nullable)
- exclude (char(1), nullable)
- abbrev (char(12), nullable)
- displayGroupId (smallint, nullable)
- groupname (char(30), nullable)
- Lab (bit, nullable)
- Schedule (bit, nullable)
- ShortAbbrev (nvarchar(6), nullable)

**Previous Model** had:
- testID (int, Key)
- testName (string)
- testDescription (string)
- active (bool)

**Changes**:
- Removed all previous properties that didn't exist in SQL
- Added all 11 columns from SQL table with correct data types
- Changed to keyless entity (table has no primary key)
- Added appropriate MaxLength attributes

### 2. LubeSamplingPoint.cs - Major Refactor ✅

**Issue**: Model had a non-existent `Id` primary key and incorrect data types.

**SQL Table** (from `db-tables/Lube_Sampling_Point.sql`):
- No primary key defined
- 18 columns total

**Changes**:
- Removed non-existent `Id` property
- Marked as `[Keyless]` entity
- Fixed data types:
  - `LubeQuantityRequired`: decimal → double (matches SQL float)
  - `PricingPackageId`: int → short (matches SQL smallint)
  - `TestPricesId`: int → short (matches SQL smallint)
  - `ChangeIntervalNumber`: int → byte (matches SQL tinyint)
  - `TestsScheduled`: int → bool (matches SQL bit)
- Updated MaxLength attributes to match exact SQL lengths:
  - tagNumber: 50 → 22
  - component: 10 → 3
  - location: 10 → 3
  - qualityClass: 10 → 6
  - And others

### 3. EmissionSpectroscopy.cs - Minor Fix ✅

**Issue**: Model had an unmapped `Status` property that doesn't exist in SQL.

**Changes**:
- Removed the `Status` property (not in `db-tables/EmSpectro.sql`)

### 4. ParticleType.cs - Enhancement ✅

**Issue**: Missing MaxLength attributes for string columns.

**Changes**:
- Added `[MaxLength(20)]` to `Status` property
- Added `[MaxLength(500)]` to `Comments` property
- Added missing `using System.ComponentModel.DataAnnotations;`

### 5. InspectFilter.cs - Enhancement ✅

**Issue**: Missing MaxLength attribute for narrative column.

**Changes**:
- Added `[MaxLength(4000)]` to `Narrative` property
- Added missing `using System.ComponentModel.DataAnnotations;`

### 6. LabDbContext.cs - Configuration Updates ✅

**Changes**:
1. Moved `LubeSamplingPoint` from "Entities with primary keys" section to "Keyless entities" section
2. Added `modelBuilder.Entity<LubeSamplingPoint>().HasNoKey();` configuration
3. Updated `Test` entity configuration to use `HasNoKey()` instead of `HasKey(e => e.TestId)`
4. Added proper `ToTable("Test")` mapping for Test entity

## Models Verified as Correct ✅

The following models were verified to match their SQL schemas correctly:

1. **Sample.cs** ↔ `UsedLubeSamples.sql` - All 22 columns match
2. **Equipment.cs** ↔ `M_And_T_Equip.sql` - All 11 columns match
3. **TestReading.cs** ↔ `TestReadings.sql` - All 18 columns match
4. **LubeTech.cs** ↔ `LubeTechList.sql` - All 5 columns match
5. **Comment.cs** ↔ `Comments.sql` - All 4 columns match
6. **Reviewer.cs** ↔ `ReviewerList.sql` - All 6 columns match
7. **ParticleSubType.cs** ↔ `ParticleSubType.sql` - All 5 columns match

## Critical Insights

### Keyless Entities

The following entities are now properly marked as keyless (no primary key in SQL):

1. **Test** - No primary key defined in SQL
2. **LubeSamplingPoint** - No primary key defined in SQL
3. **TestReadings** - Already correctly marked as keyless
4. **EmSpectro** - Already correctly marked as keyless
5. **ParticleType** - Already correctly marked as keyless
6. **ParticleSubType** - Already correctly marked as keyless
7. **InspectFilter** - Already correctly marked as keyless

**Important**: Keyless entities cannot use navigation properties in Entity Framework Core. Use raw SQL or explicit joins instead.

## Data Type Mappings

The following SQL to C# type mappings were applied:

| SQL Server Type | C# Type | Notes |
|----------------|---------|-------|
| int | int | |
| smallint | short | |
| tinyint | byte | |
| bit | bool | |
| nvarchar(n) | string | With `[MaxLength(n)]` |
| char(n) | string | With `[MaxLength(n)]` |
| float | double | |
| datetime | DateTime | |

## Breaking Changes

⚠️ **Warning**: These changes may break existing code that references the old properties.

### Test Model Breaking Changes

Any code referencing the old `Test` model properties will need to be updated:

- `TestId` → `Id`
- `TestName` → `Name`
- `TestDescription` → *removed* (doesn't exist in SQL)
- `Active` → *removed* (doesn't exist in SQL)

New properties available:
- `TestStandId`
- `SampleVolumeRequired`
- `Exclude`
- `Abbrev`
- `DisplayGroupId`
- `GroupName`
- `Lab`
- `Schedule`
- `ShortAbbrev`

### LubeSamplingPoint Breaking Changes

- Removed: `Id` property (never existed in database)
- Changed types for several properties

## Build Status

✅ **BUILD SUCCESSFUL** - All compilation errors have been resolved!

- 0 errors
- 196 warnings (mostly async/await and nullability warnings, non-critical)
- Successfully compiled to `/home/derrick/projects/testing/new-example/LabResultsApi/bin/Debug/net8.0/LabResultsApi.dll`

## Next Steps

1. ✅ **Build Verification**: COMPLETED - Project builds successfully
2. ✅ **Update Service Layer**: COMPLETED - Services updated to use correct Test model properties
3. **Update DTOs**: Ensure DTOs that map from these models are updated (if needed)
4. **Update Endpoints**: Review endpoints that use `Test` or `LubeSamplingPoint` models (mostly done)
5. **Run Tests**: Execute `dotnet test ../LabResultsApi.Tests/LabResultsApi.Tests.csproj`
6. **Database Verification**: Test queries against actual database to ensure models work correctly

## Files Modified

1. `LabResultsApi/Models/Test.cs`
2. `LabResultsApi/Models/LubeSamplingPoint.cs`
3. `LabResultsApi/Models/EmissionSpectroscopy.cs`
4. `LabResultsApi/Models/ParticleType.cs`
5. `LabResultsApi/Models/InspectFilter.cs`
6. `LabResultsApi/Data/LabDbContext.cs`

## Recommendations

1. **Search for References**: Use your IDE to search for references to `TestId`, `TestName`, etc. in the `Test` model
2. **Update AutoMapper Profiles**: Check `LabResultsApi/Mappings/` for any mappings that need updating
3. **Review Queries**: Check for any LINQ queries or raw SQL that assume `Test` has a primary key
4. **Update Swagger/API Docs**: API documentation may need updates for changed property names
5. **Consider Migration Strategy**: If this is a live system, plan a deployment strategy for the breaking changes

## Additional Notes

- The SQL files in `db-tables/` are the source of truth for table schemas
- Some tables have nullable columns for what appear to be required fields - this matches the SQL definitions
- Character fields use `char(n)` or `nvarchar(n)` in SQL - both map to C# `string` with `[MaxLength]` attributes
