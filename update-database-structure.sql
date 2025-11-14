-- =============================================
-- Database Structure Update Script
-- This script will update the database to match the complete structure
-- from the SQL files in db-tables, db-functions, db-sp, and db-views
-- =============================================

USE [LabResultsDb]
GO

-- Set options for script execution
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET NOCOUNT ON
GO

PRINT 'Starting database structure update...'
PRINT 'Timestamp: ' + CONVERT(VARCHAR, GETDATE(), 120)
GO

-- =============================================
-- STEP 1: Drop existing views (they depend on tables/functions)
-- =============================================
PRINT 'Step 1: Dropping existing views...'
GO

-- Drop views in reverse dependency order
IF OBJECT_ID('dbo.vwActiveLE', 'V') IS NOT NULL DROP VIEW dbo.vwActiveLE
IF OBJECT_ID('dbo.vwActiveLimits', 'V') IS NOT NULL DROP VIEW dbo.vwActiveLimits
IF OBJECT_ID('dbo.vwAllMTEUsage', 'V') IS NOT NULL DROP VIEW dbo.vwAllMTEUsage
IF OBJECT_ID('dbo.vwAllResultsIKS', 'V') IS NOT NULL DROP VIEW dbo.vwAllResultsIKS
IF OBJECT_ID('dbo.vwComponentForUnique', 'V') IS NOT NULL DROP VIEW dbo.vwComponentForUnique
IF OBJECT_ID('dbo.vwComponentForUsed', 'V') IS NOT NULL DROP VIEW dbo.vwComponentForUsed
IF OBJECT_ID('dbo.vwEmSpectroIPDAS', 'V') IS NOT NULL DROP VIEW dbo.vwEmSpectroIPDAS
IF OBJECT_ID('dbo.vwEqLubricationPtIPDAS', 'V') IS NOT NULL DROP VIEW dbo.vwEqLubricationPtIPDAS
IF OBJECT_ID('dbo.vwExportTestData', 'V') IS NOT NULL DROP VIEW dbo.vwExportTestData
IF OBJECT_ID('dbo.vwFerrogramIPDAS', 'V') IS NOT NULL DROP VIEW dbo.vwFerrogramIPDAS
IF OBJECT_ID('dbo.vwFieldRecords', 'V') IS NOT NULL DROP VIEW dbo.vwFieldRecords
IF OBJECT_ID('dbo.vwFilterResidueIPDAS', 'V') IS NOT NULL DROP VIEW dbo.vwFilterResidueIPDAS
IF OBJECT_ID('dbo.vwFlashPtIPDAS', 'V') IS NOT NULL DROP VIEW dbo.vwFlashPtIPDAS
IF OBJECT_ID('dbo.vwFTIR', 'V') IS NOT NULL DROP VIEW dbo.vwFTIR
IF OBJECT_ID('dbo.vwFtirIPDAS', 'V') IS NOT NULL DROP VIEW dbo.vwFtirIPDAS
IF OBJECT_ID('dbo.vwGoodnessLimitsAndResultsForSample', 'V') IS NOT NULL DROP VIEW dbo.vwGoodnessLimitsAndResultsForSample
IF OBJECT_ID('dbo.vwGoodnessLimitsForSamples', 'V') IS NOT NULL DROP VIEW dbo.vwGoodnessLimitsForSamples
IF OBJECT_ID('dbo.vwGoodnessTestResults', 'V') IS NOT NULL DROP VIEW dbo.vwGoodnessTestResults
IF OBJECT_ID('dbo.vwGroupSamples', 'V') IS NOT NULL DROP VIEW dbo.vwGroupSamples
IF OBJECT_ID('dbo.vwLabOverall', 'V') IS NOT NULL DROP VIEW dbo.vwLabOverall
IF OBJECT_ID('dbo.vwLabSampleMaterials', 'V') IS NOT NULL DROP VIEW dbo.vwLabSampleMaterials
IF OBJECT_ID('dbo.vwLabSampleTestList', 'V') IS NOT NULL DROP VIEW dbo.vwLabSampleTestList
IF OBJECT_ID('dbo.vwLabSampleTests', 'V') IS NOT NULL DROP VIEW dbo.vwLabSampleTests
IF OBJECT_ID('dbo.vwLELimitsForSampleTests', 'V') IS NOT NULL DROP VIEW dbo.vwLELimitsForSampleTests
IF OBJECT_ID('dbo.vwLimitsGroups', 'V') IS NOT NULL DROP VIEW dbo.vwLimitsGroups
IF OBJECT_ID('dbo.vwLimitValuesAndResultForSamples', 'V') IS NOT NULL DROP VIEW dbo.vwLimitValuesAndResultForSamples
IF OBJECT_ID('dbo.vwLimitValuesForSamples', 'V') IS NOT NULL DROP VIEW dbo.vwLimitValuesForSamples
IF OBJECT_ID('dbo.vwLocationForUnique', 'V') IS NOT NULL DROP VIEW dbo.vwLocationForUnique
IF OBJECT_ID('dbo.vwLocationForUsed', 'V') IS NOT NULL DROP VIEW dbo.vwLocationForUsed
IF OBJECT_ID('dbo.vwLocationsIPDAS', 'V') IS NOT NULL DROP VIEW dbo.vwLocationsIPDAS
IF OBJECT_ID('dbo.vwMiscTestHistory', 'V') IS NOT NULL DROP VIEW dbo.vwMiscTestHistory
IF OBJECT_ID('dbo.vwMiscTestResults', 'V') IS NOT NULL DROP VIEW dbo.vwMiscTestResults
IF OBJECT_ID('dbo.vwMTE_UsageForSample', 'V') IS NOT NULL DROP VIEW dbo.vwMTE_UsageForSample
IF OBJECT_ID('dbo.vwMTEErrors', 'V') IS NOT NULL DROP VIEW dbo.vwMTEErrors
IF OBJECT_ID('dbo.vwMTEErrorsFixed', 'V') IS NOT NULL DROP VIEW dbo.vwMTEErrorsFixed
IF OBJECT_ID('dbo.vwOtherTests', 'V') IS NOT NULL DROP VIEW dbo.vwOtherTests
IF OBJECT_ID('dbo.vwParticleCount', 'V') IS NOT NULL DROP VIEW dbo.vwParticleCount
IF OBJECT_ID('dbo.vwParticleCountIPDAS', 'V') IS NOT NULL DROP VIEW dbo.vwParticleCountIPDAS
IF OBJECT_ID('dbo.vwParticleType', 'V') IS NOT NULL DROP VIEW dbo.vwParticleType
IF OBJECT_ID('dbo.vwParticleTypeDefinition', 'V') IS NOT NULL DROP VIEW dbo.vwParticleTypeDefinition
IF OBJECT_ID('dbo.vwParticleTypeDI', 'V') IS NOT NULL DROP VIEW dbo.vwParticleTypeDI
IF OBJECT_ID('dbo.vwParticleTypeFE', 'V') IS NOT NULL DROP VIEW dbo.vwParticleTypeFE
IF OBJECT_ID('dbo.vwParticleTypeFR', 'V') IS NOT NULL DROP VIEW dbo.vwParticleTypeFR
IF OBJECT_ID('dbo.vwParticleTypeIF', 'V') IS NOT NULL DROP VIEW dbo.vwParticleTypeIF
IF OBJECT_ID('dbo.vwPIPointData', 'V') IS NOT NULL DROP VIEW dbo.vwPIPointData
IF OBJECT_ID('dbo.vwRbotIPDAS', 'V') IS NOT NULL DROP VIEW dbo.vwRbotIPDAS
IF OBJECT_ID('dbo.vwResultsBySample', 'V') IS NOT NULL DROP VIEW dbo.vwResultsBySample
IF OBJECT_ID('dbo.vwRheometer', 'V') IS NOT NULL DROP VIEW dbo.vwRheometer
IF OBJECT_ID('dbo.vwRheometerHistory', 'V') IS NOT NULL DROP VIEW dbo.vwRheometerHistory
IF OBJECT_ID('dbo.vwRheometerHistory1', 'V') IS NOT NULL DROP VIEW dbo.vwRheometerHistory1
IF OBJECT_ID('dbo.vwRheometerHistory2', 'V') IS NOT NULL DROP VIEW dbo.vwRheometerHistory2
IF OBJECT_ID('dbo.vwRheometerHistory3', 'V') IS NOT NULL DROP VIEW dbo.vwRheometerHistory3
IF OBJECT_ID('dbo.vwRheometerHistory4', 'V') IS NOT NULL DROP VIEW dbo.vwRheometerHistory4
IF OBJECT_ID('dbo.vwRheometerHistory5', 'V') IS NOT NULL DROP VIEW dbo.vwRheometerHistory5
IF OBJECT_ID('dbo.vwRheometerHistory6', 'V') IS NOT NULL DROP VIEW dbo.vwRheometerHistory6
IF OBJECT_ID('dbo.vwRheometerHistory7', 'V') IS NOT NULL DROP VIEW dbo.vwRheometerHistory7
IF OBJECT_ID('dbo.vwSampleCommentsIKS', 'V') IS NOT NULL DROP VIEW dbo.vwSampleCommentsIKS
IF OBJECT_ID('dbo.vwSampleReleaseFerrogram', 'V') IS NOT NULL DROP VIEW dbo.vwSampleReleaseFerrogram
IF OBJECT_ID('dbo.vwSampleReleaseFTIR', 'V') IS NOT NULL DROP VIEW dbo.vwSampleReleaseFTIR
IF OBJECT_ID('dbo.vwSampleReleaseIF', 'V') IS NOT NULL DROP VIEW dbo.vwSampleReleaseIF
IF OBJECT_ID('dbo.vwSampleReleaseMisc', 'V') IS NOT NULL DROP VIEW dbo.vwSampleReleaseMisc
IF OBJECT_ID('dbo.vwSampleReleasePC', 'V') IS NOT NULL DROP VIEW dbo.vwSampleReleasePC
IF OBJECT_ID('dbo.vwSampleReleasePT', 'V') IS NOT NULL DROP VIEW dbo.vwSampleReleasePT
IF OBJECT_ID('dbo.vwSampleReleaseResults', 'V') IS NOT NULL DROP VIEW dbo.vwSampleReleaseResults
IF OBJECT_ID('dbo.vwSampleReleaseSpectro', 'V') IS NOT NULL DROP VIEW dbo.vwSampleReleaseSpectro
IF OBJECT_ID('dbo.vwSampleReleaseSummary', 'V') IS NOT NULL DROP VIEW dbo.vwSampleReleaseSummary
IF OBJECT_ID('dbo.vwSamplesInProgress', 'V') IS NOT NULL DROP VIEW dbo.vwSamplesInProgress
IF OBJECT_ID('dbo.vwScheduledSamplesByTest', 'V') IS NOT NULL DROP VIEW dbo.vwScheduledSamplesByTest
IF OBJECT_ID('dbo.vwSpectroscopy', 'V') IS NOT NULL DROP VIEW dbo.vwSpectroscopy
IF OBJECT_ID('dbo.vwTempLimits', 'V') IS NOT NULL DROP VIEW dbo.vwTempLimits
IF OBJECT_ID('dbo.vwTestAddCriteria', 'V') IS NOT NULL DROP VIEW dbo.vwTestAddCriteria
IF OBJECT_ID('dbo.vwTestDeleteCriteria', 'V') IS NOT NULL DROP VIEW dbo.vwTestDeleteCriteria
IF OBJECT_ID('dbo.vwTestEntryUsers', 'V') IS NOT NULL DROP VIEW dbo.vwTestEntryUsers
IF OBJECT_ID('dbo.vwTestIPDAS', 'V') IS NOT NULL DROP VIEW dbo.vwTestIPDAS
IF OBJECT_ID('dbo.vwTestListBySample', 'V') IS NOT NULL DROP VIEW dbo.vwTestListBySample
IF OBJECT_ID('dbo.vwTestResultBySampleAndTest', 'V') IS NOT NULL DROP VIEW dbo.vwTestResultBySampleAndTest
IF OBJECT_ID('dbo.vwTestResultIPDAS', 'V') IS NOT NULL DROP VIEW dbo.vwTestResultIPDAS
IF OBJECT_ID('dbo.vwTestResultsBySample', 'V') IS NOT NULL DROP VIEW dbo.vwTestResultsBySample
IF OBJECT_ID('dbo.vwTestRuleDefinition', 'V') IS NOT NULL DROP VIEW dbo.vwTestRuleDefinition
IF OBJECT_ID('dbo.vwTestRulesByEQID', 'V') IS NOT NULL DROP VIEW dbo.vwTestRulesByEQID
IF OBJECT_ID('dbo.vwTestRulesByTestID', 'V') IS NOT NULL DROP VIEW dbo.vwTestRulesByTestID
IF OBJECT_ID('dbo.vwTestsBySampleAndEQID', 'V') IS NOT NULL DROP VIEW dbo.vwTestsBySampleAndEQID
IF OBJECT_ID('dbo.vwTestsbySampleAndEQIDsub', 'V') IS NOT NULL DROP VIEW dbo.vwTestsbySampleAndEQIDsub
IF OBJECT_ID('dbo.vwTestsBySchedule', 'V') IS NOT NULL DROP VIEW dbo.vwTestsBySchedule
IF OBJECT_ID('dbo.vwTestScheduleBySample', 'V') IS NOT NULL DROP VIEW dbo.vwTestScheduleBySample
IF OBJECT_ID('dbo.vwTestScheduleDefinition', 'V') IS NOT NULL DROP VIEW dbo.vwTestScheduleDefinition
IF OBJECT_ID('dbo.vwTestScheduleDefinitionByEQID', 'V') IS NOT NULL DROP VIEW dbo.vwTestScheduleDefinitionByEQID
IF OBJECT_ID('dbo.vwTestScheduleDefinitionByMaterial', 'V') IS NOT NULL DROP VIEW dbo.vwTestScheduleDefinitionByMaterial
IF OBJECT_ID('dbo.vwTestScheduleDefinitionBySample', 'V') IS NOT NULL DROP VIEW dbo.vwTestScheduleDefinitionBySample
IF OBJECT_ID('dbo.vwTestsForLab', 'V') IS NOT NULL DROP VIEW dbo.vwTestsForLab
IF OBJECT_ID('dbo.vwTestsForScheduling', 'V') IS NOT NULL DROP VIEW dbo.vwTestsForScheduling
IF OBJECT_ID('dbo.vwUsedLubeSamplesIPDAS', 'V') IS NOT NULL DROP VIEW dbo.vwUsedLubeSamplesIPDAS
IF OBJECT_ID('dbo.vwViscosityIPDAS', 'V') IS NOT NULL DROP VIEW dbo.vwViscosityIPDAS

PRINT 'Views dropped successfully.'
GO

-- =============================================
-- STEP 2: Drop existing stored procedures
-- =============================================
PRINT 'Step 2: Dropping existing stored procedures...'
GO

IF OBJECT_ID('dbo.getPoint', 'P') IS NOT NULL DROP PROCEDURE dbo.getPoint
IF OBJECT_ID('dbo.sp_EmptySWMSRecords', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_EmptySWMSRecords
IF OBJECT_ID('dbo.sp_EportTestData', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_EportTestData
IF OBJECT_ID('dbo.sp_FTIR', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_FTIR
IF OBJECT_ID('dbo.sp_OtherTests', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_OtherTests
IF OBJECT_ID('dbo.sp_ParticleCount', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_ParticleCount
IF OBJECT_ID('dbo.sp_Rheometer', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_Rheometer
IF OBJECT_ID('dbo.sp_SIMCAExportFerro', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_SIMCAExportFerro
IF OBJECT_ID('dbo.sp_SIMCAExportFTIR', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_SIMCAExportFTIR
IF OBJECT_ID('dbo.sp_SIMCAExportPC', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_SIMCAExportPC
IF OBJECT_ID('dbo.sp_SIMCAExportResults', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_SIMCAExportResults
IF OBJECT_ID('dbo.sp_SIMCAExportRheo', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_SIMCAExportRheo
IF OBJECT_ID('dbo.sp_SIMCAExportSpecLrg', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_SIMCAExportSpecLrg
IF OBJECT_ID('dbo.sp_SIMCAExportSpecStd', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_SIMCAExportSpecStd
IF OBJECT_ID('dbo.sp_SIMCAExportStub', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_SIMCAExportStub
IF OBJECT_ID('dbo.sp_Spectroscopy', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_Spectroscopy
IF OBJECT_ID('dbo.sp_TempLimits', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_TempLimits
IF OBJECT_ID('dbo.sp_WorkManagement', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_WorkManagement

PRINT 'Stored procedures dropped successfully.'
GO

-- =============================================
-- STEP 3: Drop existing functions
-- =============================================
PRINT 'Step 3: Dropping existing functions...'
GO

IF OBJECT_ID('dbo.LEScheduleByEQID', 'FN') IS NOT NULL DROP FUNCTION dbo.LEScheduleByEQID
IF OBJECT_ID('dbo.MiscTestResult', 'FN') IS NOT NULL DROP FUNCTION dbo.MiscTestResult
IF OBJECT_ID('dbo.TestLELimit', 'FN') IS NOT NULL DROP FUNCTION dbo.TestLELimit
IF OBJECT_ID('dbo.TestResultsForSampleSummary', 'FN') IS NOT NULL DROP FUNCTION dbo.TestResultsForSampleSummary
IF OBJECT_ID('dbo.TestResultValue', 'FN') IS NOT NULL DROP FUNCTION dbo.TestResultValue
IF OBJECT_ID('dbo.TestScheduleLimit', 'FN') IS NOT NULL DROP FUNCTION dbo.TestScheduleLimit
IF OBJECT_ID('dbo.TestStatusForDisplay', 'FN') IS NOT NULL DROP FUNCTION dbo.TestStatusForDisplay
IF OBJECT_ID('dbo.TestStatusText', 'FN') IS NOT NULL DROP FUNCTION dbo.TestStatusText

PRINT 'Functions dropped successfully.'
GO

-- =============================================
-- STEP 4: Create/Update Tables
-- =============================================
PRINT 'Step 4: Creating/updating tables...'
GO

-- Note: The actual table creation scripts will be executed by running each SQL file
-- This is a placeholder to show the structure. In practice, you would execute each file.

PRINT 'Tables will be created by executing individual SQL files from db-tables directory.'
PRINT 'Execute the following files in order:'
PRINT '  - Core tables first (UsedLubeSamples, Test, TestReadings, etc.)'
PRINT '  - Lookup tables'
PRINT '  - Dependent tables'
GO

-- =============================================
-- STEP 5: Create Functions
-- =============================================
PRINT 'Step 5: Creating functions...'
GO

PRINT 'Functions will be created by executing individual SQL files from db-functions directory.'
GO

-- =============================================
-- STEP 6: Create Stored Procedures
-- =============================================
PRINT 'Step 6: Creating stored procedures...'
GO

PRINT 'Stored procedures will be created by executing individual SQL files from db-sp directory.'
GO

-- =============================================
-- STEP 7: Create Views
-- =============================================
PRINT 'Step 7: Creating views...'
GO

PRINT 'Views will be created by executing individual SQL files from db-views directory.'
GO

-- =============================================
-- COMPLETION MESSAGE
-- =============================================
PRINT 'Database structure update preparation completed.'
PRINT 'Next steps:'
PRINT '1. Execute all table creation scripts from db-tables directory'
PRINT '2. Execute all function creation scripts from db-functions directory'
PRINT '3. Execute all stored procedure scripts from db-sp directory'
PRINT '4. Execute all view creation scripts from db-views directory'
PRINT 'Timestamp: ' + CONVERT(VARCHAR, GETDATE(), 120)
GO