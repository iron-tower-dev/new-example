-- =============================================
-- Create Missing Database Views
-- These views replicate the functionality from the legacy VB ASP.NET system
-- =============================================

-- 1. vwLELimitsForSampleTests - Limits evaluation for sample tests
IF NOT EXISTS (SELECT * FROM sys.views WHERE name = 'vwLELimitsForSampleTests')
BEGIN
    EXEC('
    CREATE VIEW vwLELimitsForSampleTests AS
    SELECT DISTINCT
        s.ID,
        tr.testID,
        t.testName,
        s.tagNumber,
        s.component,
        s.location,
        s.qualityClass,
        l.parameter,
        l.highLimit,
        l.lowLimit,
        l.highWarning,
        l.lowWarning
    FROM UsedLubeSamples s
    INNER JOIN TestReadings tr ON s.ID = tr.sampleID
    INNER JOIN Test t ON tr.testID = t.testID
    LEFT JOIN limits l ON tr.testID = l.testID 
        AND (l.tagNumber = s.tagNumber OR l.tagNumber IS NULL OR l.tagNumber = '''')
        AND (l.component = s.component OR l.component IS NULL OR l.component = '''')
        AND (l.location = s.location OR l.location IS NULL OR l.location = '''')
        AND l.isActive = 1
    WHERE tr.status IN (''A'', ''E'', ''C'', ''D'')
    ')
END

-- 2. vwSpectroscopy - Spectroscopy results view
IF NOT EXISTS (SELECT * FROM sys.views WHERE name = 'vwSpectroscopy')
BEGIN
    EXEC('
    CREATE VIEW vwSpectroscopy AS
    SELECT 
        es.ID as SampleID,
        es.testID,
        es.trialNum,
        es.Na, es.Cr, es.Sn, es.Si, es.Mo, es.Ca, es.Al, es.Ba,
        es.Mg, es.Ni, es.Mn, es.Zn, es.P, es.Ag, es.Pb, es.H, es.B, es.Cu, es.Fe,
        es.trialDate,
        s.tagNumber,
        s.component,
        s.location,
        s.sampleDate
    FROM EmSpectro es
    INNER JOIN UsedLubeSamples s ON es.ID = s.ID
    ')
END

-- 3. vwFTIR - FTIR results view
IF NOT EXISTS (SELECT * FROM sys.views WHERE name = 'vwFTIR')
BEGIN
    EXEC('
    CREATE VIEW vwFTIR AS
    SELECT 
        f.sampleID as SampleID,
        f.contam,
        f.anti_oxidant,
        f.oxidation,
        f.h2o,
        f.zddp,
        f.soot,
        f.fuel_dilution,
        f.mixture,
        f.nlgi,
        s.tagNumber,
        s.component,
        s.location,
        s.sampleDate
    FROM FTIR f
    INNER JOIN UsedLubeSamples s ON f.sampleID = s.ID
    ')
END

-- 4. vwParticleCount - Particle count results view
IF NOT EXISTS (SELECT * FROM sys.views WHERE name = 'vwParticleCount')
BEGIN
    EXEC('
    CREATE VIEW vwParticleCount AS
    SELECT 
        pc.id as SampleID,
        pc.micron_5_10,
        pc.micron_10_15,
        pc.micron_15_25,
        pc.micron_25_50,
        pc.micron_50_100,
        pc.micron_100,
        pc.nas_class,
        pc.testDate,
        s.tagNumber,
        s.component,
        s.location,
        s.sampleDate
    FROM ParticleCount pc
    INNER JOIN UsedLubeSamples s ON pc.id = s.ID
    ')
END

-- 5. vwResultsBySample - General results view
IF NOT EXISTS (SELECT * FROM sys.views WHERE name = 'vwResultsBySample')
BEGIN
    EXEC('
    CREATE VIEW vwResultsBySample AS
    SELECT 
        tr.sampleID,
        tr.testID,
        tr.trialNumber,
        COALESCE(tr.trialCalc, tr.value1, tr.value2, tr.value3) as Result,
        tr.status,
        tr.entryDate,
        tr.entryID,
        tr.validateID,
        tr.valiDate,
        t.testName,
        s.tagNumber,
        s.component,
        s.location,
        s.sampleDate
    FROM TestReadings tr
    INNER JOIN Test t ON tr.testID = t.testID
    INNER JOIN UsedLubeSamples s ON tr.sampleID = s.ID
    WHERE tr.status IN (''C'', ''D'')
    ')
END

-- 6. vwTestScheduleDefinitionByEQID - Test scheduling by equipment
IF NOT EXISTS (SELECT * FROM sys.views WHERE name = 'vwTestScheduleDefinitionByEQID')
BEGIN
    EXEC('
    CREATE VIEW vwTestScheduleDefinitionByEQID AS
    SELECT 
        ts.testID,
        t.testName,
        lsp.tagNumber as Tag,
        lsp.component as ComponentCode,
        lsp.location as LocationCode,
        ts.minimumInterval as MinimumInterval,
        ts.scheduleType,
        ts.isActive,
        lsp.qualityClass
    FROM TestSchedule ts
    INNER JOIN Test t ON ts.testID = t.testID
    INNER JOIN Lube_Sampling_Point lsp ON ts.samplingPointID = lsp.ID
    WHERE ts.isActive = 1 AND t.active = 1
    ')
END

-- 7. vwTestRulesByEQID - Test scheduling rules by equipment
IF NOT EXISTS (SELECT * FROM sys.views WHERE name = 'vwTestRulesByEQID')
BEGIN
    EXEC('
    CREATE VIEW vwTestRulesByEQID AS
    SELECT 
        tsr.testID,
        t.testName,
        lsp.tagNumber as Tag,
        lsp.component as ComponentCode,
        lsp.location as LocationCode,
        tsr.ruleType as RuleType,
        tsr.condition as Condition,
        tsr.thresholdValue as ThresholdValue,
        tsr.thresholdOperator as ThresholdOperator,
        tsr.triggerTestID as TriggerTestID,
        tsr.minimumInterval as MinimumInterval,
        tsr.isActive as IsActive
    FROM TestScheduleRule tsr
    INNER JOIN Test t ON tsr.testID = t.testID
    INNER JOIN Lube_Sampling_Point lsp ON tsr.samplingPointID = lsp.ID
    WHERE tsr.isActive = 1
    ')
END

-- 8. vwTestDeleteCriteria - Criteria for removing tests
IF NOT EXISTS (SELECT * FROM sys.views WHERE name = 'vwTestDeleteCriteria')
BEGIN
    EXEC('
    CREATE VIEW vwTestDeleteCriteria AS
    SELECT 
        tr.sampleID as sampleid,
        tr.testID as testid,
        tr.status,
        s.tagNumber,
        s.component,
        s.location,
        ''Can be removed'' as reason
    FROM TestReadings tr
    INNER JOIN UsedLubeSamples s ON tr.sampleID = s.ID
    WHERE tr.status = ''X'' -- Only pending tests can be removed
        AND NOT EXISTS (
            SELECT 1 FROM TestScheduleRule tsr
            INNER JOIN Lube_Sampling_Point lsp ON tsr.samplingPointID = lsp.ID
            WHERE tsr.testID = tr.testID 
                AND lsp.tagNumber = s.tagNumber
                AND lsp.component = s.component
                AND lsp.location = s.location
                AND tsr.ruleType = ''REQUIRED''
                AND tsr.isActive = 1
        )
    ')
END

-- 9. vwTestAddCriteria - Criteria for adding tests
IF NOT EXISTS (SELECT * FROM sys.views WHERE name = 'vwTestAddCriteria')
BEGIN
    EXEC('
    CREATE VIEW vwTestAddCriteria AS
    SELECT 
        s.ID as sampleid,
        tsr.testID as testid,
        s.tagNumber,
        s.component,
        s.location,
        tr.status,
        CASE 
            WHEN tr.sampleID IS NULL THEN ''''
            ELSE ''Already scheduled''
        END as reason
    FROM UsedLubeSamples s
    CROSS JOIN TestScheduleRule tsr
    INNER JOIN Lube_Sampling_Point lsp ON tsr.samplingPointID = lsp.ID
        AND lsp.tagNumber = s.tagNumber
        AND lsp.component = s.component
        AND lsp.location = s.location
    LEFT JOIN TestReadings tr ON s.ID = tr.sampleID AND tsr.testID = tr.testID
    WHERE tsr.ruleType = ''ADD'' AND tsr.isActive = 1
    ')
END

-- 10. vwTestResultBySampleAndTest - Specific test results
IF NOT EXISTS (SELECT * FROM sys.views WHERE name = 'vwTestResultBySampleAndTest')
BEGIN
    EXEC('
    CREATE VIEW vwTestResultBySampleAndTest AS
    SELECT 
        tr.sampleID as sampleid,
        tr.testID as testid,
        COALESCE(tr.trialCalc, tr.value1, tr.value2, tr.value3) as result,
        tr.status,
        tr.entryDate,
        s.tagNumber,
        s.component,
        s.location
    FROM TestReadings tr
    INNER JOIN UsedLubeSamples s ON tr.sampleID = s.ID
    WHERE tr.status IN (''C'', ''D'')
    ')
END

-- 11. vwLabOverall - Overall lab status view
IF NOT EXISTS (SELECT * FROM sys.views WHERE name = 'vwLabOverall')
BEGIN
    EXEC('
    CREATE VIEW vwLabOverall AS
    SELECT 
        tr.sampleID as SampleID,
        tr.testID as TestID,
        t.testName as TestName,
        tr.status,
        CASE 
            WHEN tr.status = ''C'' THEN ''Complete''
            WHEN tr.status = ''D'' THEN ''Complete''
            WHEN tr.status = ''E'' THEN ''In Progress''
            WHEN tr.status = ''A'' THEN ''Available''
            WHEN tr.status = ''X'' THEN ''Pending''
            ELSE ''Unknown''
        END as TestDisplayStatus,
        tr.entryDate,
        tr.entryID,
        s.tagNumber,
        s.component,
        s.location,
        s.sampleDate
    FROM TestReadings tr
    INNER JOIN Test t ON tr.testID = t.testID
    INNER JOIN UsedLubeSamples s ON tr.sampleID = s.ID
    ')
END

-- 12. vwMTE_UsageForSample - M&TE usage tracking
IF NOT EXISTS (SELECT * FROM sys.views WHERE name = 'vwMTE_UsageForSample')
BEGIN
    EXEC('
    CREATE VIEW vwMTE_UsageForSample AS
    SELECT 
        tr.sampleID as ID,
        tr.testID as TESTID,
        tr.ID1 as EquipmentID1,
        tr.ID2 as EquipmentID2,
        tr.ID3 as EquipmentID3,
        e1.equipName as Equipment1Name,
        e2.equipName as Equipment2Name,
        e3.equipName as Equipment3Name,
        e1.dueDate as Equipment1DueDate,
        e2.dueDate as Equipment2DueDate,
        e3.dueDate as Equipment3DueDate,
        tr.entryDate
    FROM TestReadings tr
    LEFT JOIN Equipment e1 ON tr.ID1 = CAST(e1.ID as VARCHAR(50))
    LEFT JOIN Equipment e2 ON tr.ID2 = CAST(e2.ID as VARCHAR(50))
    LEFT JOIN Equipment e3 ON tr.ID3 = CAST(e3.ID as VARCHAR(50))
    WHERE tr.status IN (''C'', ''D'', ''E'')
    ')
END

PRINT 'All missing database views have been created successfully.'