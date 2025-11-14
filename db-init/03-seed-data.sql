-- Seed Data Script for Lab Results Database
-- This script populates the database with initial test data

USE LabResultsDb;
GO

PRINT 'Seeding initial data for Lab Results Database...';
GO

-- Seed Test data
IF NOT EXISTS (SELECT 1 FROM Test WHERE TestName = 'TAN by Color Indication')
BEGIN
    INSERT INTO Test (TestName, TestDescription, Active) VALUES
    ('TAN by Color Indication', 'Total Acid Number test using color indication method', 1),
    ('Water-KF', 'Water content determination by Karl Fischer method', 1),
    ('TBN by Auto Titration', 'Total Base Number by automatic titration', 1),
    ('Viscosity @ 40°C', 'Kinematic viscosity measurement at 40 degrees Celsius', 1),
    ('Emission Spectroscopy', 'Elemental analysis by emission spectroscopy', 1),
    ('Flash Point', 'Flash point determination', 1),
    ('Pour Point', 'Pour point determination', 1),
    ('Particle Count', 'Particle contamination analysis', 1);
    
    PRINT 'Inserted test definitions.';
END
GO

-- Seed MTE Equipment data
IF NOT EXISTS (SELECT 1 FROM M_And_T_Equip WHERE EquipName = 'THERM-001')
BEGIN
    INSERT INTO M_And_T_Equip (EquipType, EquipName, exclude, testID, DueDate, Comments, Val1, Val2, Val3, Val4) VALUES
    -- Thermometers
    ('THERMOMETER', 'THERM-001', 0, NULL, DATEADD(month, 6, GETDATE()), 'Digital thermometer for viscosity tests', 0.1, NULL, NULL, NULL),
    ('THERMOMETER', 'THERM-002', 0, NULL, DATEADD(month, 3, GETDATE()), 'Analog thermometer for flash point', 0.2, NULL, NULL, NULL),
    ('THERMOMETER', 'THERM-003', 0, NULL, DATEADD(day, 15, GETDATE()), 'High precision thermometer', 0.05, NULL, NULL, NULL),
    ('THERMOMETER', 'THERM-004', 0, NULL, DATEADD(day, -5, GETDATE()), 'Standard lab thermometer (OVERDUE)', 0.15, NULL, NULL, NULL),
    
    -- Timers/Stopwatches
    ('TIMER', 'TIMER-001', 0, NULL, DATEADD(month, 8, GETDATE()), 'Digital stopwatch for viscosity', 0.01, NULL, NULL, NULL),
    ('TIMER', 'TIMER-002', 0, NULL, DATEADD(month, 4, GETDATE()), 'Precision timer', 0.005, NULL, NULL, NULL),
    ('TIMER', 'TIMER-003', 0, NULL, DATEADD(day, 25, GETDATE()), 'Lab stopwatch', 0.02, NULL, NULL, NULL),
    
    -- Viscometer tubes
    ('VISCOMETER', 'TUBE-A', 0, 4, DATEADD(month, 12, GETDATE()), 'Viscometer tube A - Size 100', 0.5023, NULL, NULL, NULL),
    ('VISCOMETER', 'TUBE-B', 0, 4, DATEADD(month, 10, GETDATE()), 'Viscometer tube B - Size 150', 0.5156, NULL, NULL, NULL),
    ('VISCOMETER', 'TUBE-C', 0, 4, DATEADD(month, 2, GETDATE()), 'Viscometer tube C - Size 200', 0.4987, NULL, NULL, NULL),
    ('VISCOMETER', 'TUBE-D', 0, 4, DATEADD(day, 10, GETDATE()), 'Viscometer tube D - Size 300', 0.5234, NULL, NULL, NULL),
    
    -- Barometers
    ('BAROMETER', 'BARO-001', 0, 6, DATEADD(month, 6, GETDATE()), 'Digital barometer for flash point tests', 1.0, NULL, NULL, NULL),
    ('BAROMETER', 'BARO-002', 0, 6, DATEADD(month, 9, GETDATE()), 'Precision barometer', 0.5, NULL, NULL, NULL),
    
    -- Deleterious test equipment
    ('DELETERIOUS', 'DEL-001', 0, NULL, DATEADD(month, 7, GETDATE()), 'Deleterious particle analysis equipment', 1.0, NULL, NULL, NULL);
    
    PRINT 'Inserted MTE equipment data.';
END
GO

-- Seed Sample data
IF NOT EXISTS (SELECT 1 FROM UsedLubeSamples WHERE TagNumber = 'PUMP-001')
BEGIN
    INSERT INTO UsedLubeSamples (TagNumber, Component, Location, LubeType, QualityClass, SampleDate, Status) VALUES
    ('PUMP-001', 'Main Hydraulic Pump', 'Building A - Level 1', 'Hydraulic Oil', 'ISO VG 46', DATEADD(day, -7, GETDATE()), 'Pending'),
    ('PUMP-002', 'Backup Hydraulic Pump', 'Building A - Level 1', 'Hydraulic Oil', 'ISO VG 46', DATEADD(day, -5, GETDATE()), 'In Progress'),
    ('GEAR-001', 'Main Gearbox', 'Building B - Level 2', 'Gear Oil', 'ISO VG 220', DATEADD(day, -10, GETDATE()), 'Complete'),
    ('GEAR-002', 'Secondary Gearbox', 'Building B - Level 2', 'Gear Oil', 'ISO VG 320', DATEADD(day, -3, GETDATE()), 'Pending'),
    ('COMP-001', 'Air Compressor', 'Building C - Basement', 'Compressor Oil', 'ISO VG 100', DATEADD(day, -12, GETDATE()), 'Complete'),
    ('TURB-001', 'Steam Turbine', 'Building D - Level 3', 'Turbine Oil', 'ISO VG 32', DATEADD(day, -1, GETDATE()), 'Pending'),
    ('MOTOR-001', 'Electric Motor Bearing', 'Building A - Level 2', 'Bearing Grease', 'NLGI 2', DATEADD(day, -8, GETDATE()), 'In Progress'),
    ('TRANS-001', 'Transformer Oil', 'Substation', 'Transformer Oil', 'IEC 60296', DATEADD(day, -15, GETDATE()), 'Complete');
    
    PRINT 'Inserted sample data.';
END
GO

-- Seed some test results for demonstration
IF NOT EXISTS (SELECT 1 FROM TestReadings WHERE SampleID = 1)
BEGIN
    -- TAN test results for sample 1
    INSERT INTO TestReadings (SampleID, TestID, TrialNumber, FieldName, FieldValue, NumericValue, EntryID, Comments) VALUES
    (1, 1, 1, 'SampleWeight', '2.15', 2.15, 'SYSTEM', 'TAN test trial 1'),
    (1, 1, 1, 'FinalBuret', '3.2', 3.2, 'SYSTEM', 'TAN test trial 1'),
    (1, 1, 1, 'TanResult', '8.34', 8.34, 'SYSTEM', 'Calculated TAN result'),
    
    (1, 1, 2, 'SampleWeight', '2.08', 2.08, 'SYSTEM', 'TAN test trial 2'),
    (1, 1, 2, 'FinalBuret', '3.1', 3.1, 'SYSTEM', 'TAN test trial 2'),
    (1, 1, 2, 'TanResult', '8.36', 8.36, 'SYSTEM', 'Calculated TAN result'),
    
    -- Viscosity test results for sample 2
    (2, 4, 1, 'StopWatchTime', '245.6', 245.6, 'SYSTEM', 'Viscosity test'),
    (2, 4, 1, 'TubeCalibration', '0.5023', 0.5023, 'SYSTEM', 'Tube A calibration'),
    (2, 4, 1, 'ViscosityResult', '123.4', 123.4, 'SYSTEM', 'Calculated viscosity');
    
    PRINT 'Inserted sample test results.';
END
GO

-- Seed emission spectroscopy data
IF NOT EXISTS (SELECT 1 FROM EmSpectro WHERE SampleID = 3)
BEGIN
    INSERT INTO EmSpectro (SampleID, TestID, Element, Concentration, Units, EntryID) VALUES
    (3, 5, 'Fe', 25.6, 'ppm', 'SYSTEM'),
    (3, 5, 'Cu', 8.2, 'ppm', 'SYSTEM'),
    (3, 5, 'Al', 12.4, 'ppm', 'SYSTEM'),
    (3, 5, 'Cr', 3.1, 'ppm', 'SYSTEM'),
    (3, 5, 'Pb', 1.8, 'ppm', 'SYSTEM'),
    (3, 5, 'Sn', 0.5, 'ppm', 'SYSTEM'),
    (3, 5, 'Ni', 2.3, 'ppm', 'SYSTEM'),
    (3, 5, 'Si', 15.7, 'ppm', 'SYSTEM');
    
    PRINT 'Inserted emission spectroscopy data.';
END
GO

PRINT 'Data seeding completed successfully!';
PRINT 'Database is ready for use with the Lab Results API.';
GO

-- Display summary of seeded data
SELECT 'Tests' as TableName, COUNT(*) as RecordCount FROM Test
UNION ALL
SELECT 'Equipment' as TableName, COUNT(*) as RecordCount FROM M_And_T_Equip
UNION ALL
SELECT 'Samples' as TableName, COUNT(*) as RecordCount FROM UsedLubeSamples
UNION ALL
SELECT 'Test Results' as TableName, COUNT(*) as RecordCount FROM TestReadings
UNION ALL
SELECT 'Spectroscopy Data' as TableName, COUNT(*) as RecordCount FROM EmSpectro;
GO