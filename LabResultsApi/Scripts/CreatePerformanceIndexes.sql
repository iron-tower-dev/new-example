-- Performance Optimization Indexes for Laboratory Test Results System
-- Execute these indexes to improve query performance
-- Run during maintenance window as index creation can be resource intensive

USE [LabResultsDb]
GO

-- Check if indexes already exist before creating them
PRINT 'Creating performance optimization indexes...'

-- 1. TestReadings table indexes (Highest Priority)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TestReadings_SampleID_TestID')
BEGIN
    PRINT 'Creating IX_TestReadings_SampleID_TestID...'
    CREATE NONCLUSTERED INDEX IX_TestReadings_SampleID_TestID 
    ON TestReadings (sampleID, testID) 
    INCLUDE (trialNumber, status, entryDate, trialCalc, value1, value2, value3)
    WITH (ONLINE = ON, FILLFACTOR = 90)
END
ELSE
    PRINT 'IX_TestReadings_SampleID_TestID already exists'

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TestReadings_Status_EntryDate')
BEGIN
    PRINT 'Creating IX_TestReadings_Status_EntryDate...'
    CREATE NONCLUSTERED INDEX IX_TestReadings_Status_EntryDate 
    ON TestReadings (status, entryDate DESC)
    INCLUDE (sampleID, testID, trialNumber)
    WITH (ONLINE = ON, FILLFACTOR = 90)
END
ELSE
    PRINT 'IX_TestReadings_Status_EntryDate already exists'

-- 2. UsedLubeSamples table indexes (High Priority)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UsedLubeSamples_TagNumber_Component')
BEGIN
    PRINT 'Creating IX_UsedLubeSamples_TagNumber_Component...'
    CREATE NONCLUSTERED INDEX IX_UsedLubeSamples_TagNumber_Component 
    ON UsedLubeSamples (tagNumber, component) 
    INCLUDE (sampleDate, status, lubeType, qualityClass)
    WITH (ONLINE = ON, FILLFACTOR = 90)
END
ELSE
    PRINT 'IX_UsedLubeSamples_TagNumber_Component already exists'

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UsedLubeSamples_SampleDate')
BEGIN
    PRINT 'Creating IX_UsedLubeSamples_SampleDate...'
    CREATE NONCLUSTERED INDEX IX_UsedLubeSamples_SampleDate 
    ON UsedLubeSamples (sampleDate DESC) 
    INCLUDE (tagNumber, component, status, lubeType)
    WITH (ONLINE = ON, FILLFACTOR = 90)
END
ELSE
    PRINT 'IX_UsedLubeSamples_SampleDate already exists'

-- 3. EmSpectro table indexes (High Priority)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_EmSpectro_ID_TestID')
BEGIN
    PRINT 'Creating IX_EmSpectro_ID_TestID...'
    CREATE NONCLUSTERED INDEX IX_EmSpectro_ID_TestID 
    ON EmSpectro (ID, testID) 
    INCLUDE (trialNum, status, trialDate)
    WITH (ONLINE = ON, FILLFACTOR = 90)
END
ELSE
    PRINT 'IX_EmSpectro_ID_TestID already exists'

-- 4. Equipment table indexes (Medium Priority)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Equipment_EquipType')
BEGIN
    PRINT 'Creating IX_Equipment_EquipType...'
    CREATE NONCLUSTERED INDEX IX_Equipment_EquipType 
    ON M_And_T_Equip (equipType) 
    INCLUDE (equipName, calibrationValue, dueDate)
    WITH (ONLINE = ON, FILLFACTOR = 90)
END
ELSE
    PRINT 'IX_Equipment_EquipType already exists'

-- 5. NAS Lookup table indexes (Medium Priority)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_NAS_lookup_Channel')
BEGIN
    PRINT 'Creating IX_NAS_lookup_Channel...'
    CREATE NONCLUSTERED INDEX IX_NAS_lookup_Channel 
    ON NAS_lookup (channel) 
    INCLUDE (valLo, valHi, NAS)
    WITH (ONLINE = ON, FILLFACTOR = 95) -- Lookup tables change less frequently
END
ELSE
    PRINT 'IX_NAS_lookup_Channel already exists'

-- 6. Particle analysis indexes (Medium Priority)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ParticleType_SampleID_TestID')
BEGIN
    PRINT 'Creating IX_ParticleType_SampleID_TestID...'
    CREATE NONCLUSTERED INDEX IX_ParticleType_SampleID_TestID 
    ON ParticleType (SampleID, testID) 
    INCLUDE (ParticleTypeDefinitionID, Status, Comments)
    WITH (ONLINE = ON, FILLFACTOR = 90)
END
ELSE
    PRINT 'IX_ParticleType_SampleID_TestID already exists'

-- 7. Test table indexes (Lower Priority)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Test_TestName')
BEGIN
    PRINT 'Creating IX_Test_TestName...'
    CREATE NONCLUSTERED INDEX IX_Test_TestName 
    ON Test (testName) 
    INCLUDE (testID, testDescription)
    WITH (ONLINE = ON, FILLFACTOR = 95)
END
ELSE
    PRINT 'IX_Test_TestName already exists'

-- 8. Comments table indexes (Lower Priority)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Comments_Area_Type')
BEGIN
    PRINT 'Creating IX_Comments_Area_Type...'
    CREATE NONCLUSTERED INDEX IX_Comments_Area_Type 
    ON Comments (area, type) 
    INCLUDE (remark)
    WITH (ONLINE = ON, FILLFACTOR = 95)
END
ELSE
    PRINT 'IX_Comments_Area_Type already exists'

-- Update statistics on all tables to ensure optimal query plans
PRINT 'Updating statistics...'
UPDATE STATISTICS TestReadings
UPDATE STATISTICS UsedLubeSamples  
UPDATE STATISTICS EmSpectro
UPDATE STATISTICS M_And_T_Equip
UPDATE STATISTICS NAS_lookup
UPDATE STATISTICS ParticleType
UPDATE STATISTICS Test
UPDATE STATISTICS Comments

PRINT 'Performance optimization indexes created successfully!'

-- Query to verify all indexes were created
PRINT 'Verifying index creation...'
SELECT 
    t.name AS TableName,
    i.name AS IndexName,
    i.type_desc AS IndexType,
    CASE WHEN i.is_unique = 1 THEN 'Yes' ELSE 'No' END AS IsUnique,
    CASE WHEN i.fill_factor = 0 THEN 100 ELSE i.fill_factor END AS FillFactor
FROM sys.indexes i
INNER JOIN sys.tables t ON i.object_id = t.object_id
WHERE i.name IN (
    'IX_TestReadings_SampleID_TestID',
    'IX_TestReadings_Status_EntryDate',
    'IX_UsedLubeSamples_TagNumber_Component',
    'IX_UsedLubeSamples_SampleDate',
    'IX_EmSpectro_ID_TestID',
    'IX_Equipment_EquipType',
    'IX_NAS_lookup_Channel',
    'IX_ParticleType_SampleID_TestID',
    'IX_Test_TestName',
    'IX_Comments_Area_Type'
)
ORDER BY t.name, i.name

PRINT 'Index verification complete!'