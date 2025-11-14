-- Seed TestReadings table to establish sample-test relationships
USE LabResultsDb;
GO

-- Clear existing data
DELETE FROM TestReadings;
GO

-- Insert test readings to establish which samples are available for which tests
-- This creates the relationship between samples and tests

-- Sample 1 (PUMP-001) - Hydraulic Oil - suitable for basic tests
INSERT INTO TestReadings (sampleID, testID, trialNumber, value1, value2, value3, trialCalc, ID1, ID2, ID3, trialComplete, status, schedType, entryID, validateID, entryDate, valiDate, MainComments) VALUES
(1, 10, 1, 2.5, 0.05, 0.025, NULL, NULL, NULL, NULL, 0, 'A', NULL, '01832', '01832', GETDATE(), GETDATE(), NULL), -- TAN
(1, 20, 1, 5, NULL, NULL, NULL, NULL, NULL, NULL, 0, 'A', NULL, '01832', '01832', GETDATE(), GETDATE(), NULL), -- Water-KF
(1, 50, 1, 320.5, NULL, 85.2, NULL, 'EG1001', 'J001', 'EG2001', 0, 'A', NULL, '01832', '01832', GETDATE(), GETDATE(), NULL), -- Viscosity @ 40
(1, 70, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 'A', NULL, '01832', '01832', GETDATE(), GETDATE(), NULL); -- FT-IR

-- Sample 2 (PUMP-002) - Hydraulic Oil - suitable for basic tests
INSERT INTO TestReadings (sampleID, testID, trialNumber, value1, value2, value3, trialCalc, ID1, ID2, ID3, trialComplete, status, schedType, entryID, validateID, entryDate, valiDate, MainComments) VALUES
(2, 10, 1, 3.2, 0.08, 0.035, NULL, NULL, NULL, NULL, 0, 'A', NULL, '01832', '01832', GETDATE(), GETDATE(), NULL), -- TAN
(2, 20, 1, 8, NULL, NULL, NULL, NULL, NULL, NULL, 0, 'A', NULL, '01832', '01832', GETDATE(), GETDATE(), NULL), -- Water-KF
(2, 50, 1, 298.7, NULL, 78.9, NULL, 'EG1002', 'J002', 'EG2002', 0, 'A', NULL, '01832', '01832', GETDATE(), GETDATE(), NULL), -- Viscosity @ 40
(2, 160, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 'A', NULL, '01832', '01832', GETDATE(), GETDATE(), NULL); -- Particle Count

-- Sample 3 (GEAR-001) - Gear Oil - suitable for gear oil tests
INSERT INTO TestReadings (sampleID, testID, trialNumber, value1, value2, value3, trialCalc, ID1, ID2, ID3, trialComplete, status, schedType, entryID, validateID, entryDate, valiDate, MainComments) VALUES
(3, 10, 1, 1.8, 0.03, 0.018, NULL, NULL, NULL, NULL, 0, 'A', NULL, '01832', '01832', GETDATE(), GETDATE(), NULL), -- TAN
(3, 50, 1, 450.2, NULL, 125.8, NULL, 'EG1003', 'J003', 'EG2003', 0, 'A', NULL, '01832', '01832', GETDATE(), GETDATE(), NULL), -- Viscosity @ 40
(3, 70, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 'A', NULL, '01832', '01832', GETDATE(), GETDATE(), NULL), -- FT-IR
(3, 210, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 'A', NULL, '01832', '01832', GETDATE(), GETDATE(), NULL); -- Ferrography

-- Sample 4 (GEAR-002) - Gear Oil - suitable for gear oil tests
INSERT INTO TestReadings (sampleID, testID, trialNumber, value1, value2, value3, trialCalc, ID1, ID2, ID3, trialComplete, status, schedType, entryID, validateID, entryDate, valiDate, MainComments) VALUES
(4, 10, 1, 2.1, 0.04, 0.022, NULL, NULL, NULL, NULL, 0, 'A', NULL, '01832', '01832', GETDATE(), GETDATE(), NULL), -- TAN
(4, 50, 1, 520.8, NULL, 142.3, NULL, 'EG1004', 'J004', 'EG2004', 0, 'A', NULL, '01832', '01832', GETDATE(), GETDATE(), NULL), -- Viscosity @ 40
(4, 210, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 'A', NULL, '01832', '01832', GETDATE(), GETDATE(), NULL); -- Ferrography

-- Sample 5 (COMP-001) - Compressor Oil - suitable for compressor tests
INSERT INTO TestReadings (sampleID, testID, trialNumber, value1, value2, value3, trialCalc, ID1, ID2, ID3, trialComplete, status, schedType, entryID, validateID, entryDate, valiDate, MainComments) VALUES
(5, 10, 1, 1.5, 0.02, 0.015, NULL, NULL, NULL, NULL, 0, 'A', NULL, '01832', '01832', GETDATE(), GETDATE(), NULL), -- TAN
(5, 20, 1, 3, NULL, NULL, NULL, NULL, NULL, NULL, 0, 'A', NULL, '01832', '01832', GETDATE(), GETDATE(), NULL), -- Water-KF
(5, 50, 1, 385.4, NULL, 95.7, NULL, 'EG1005', 'J005', 'EG2005', 0, 'A', NULL, '01832', '01832', GETDATE(), GETDATE(), NULL); -- Viscosity @ 40

-- Sample 6 (TURB-001) - Turbine Oil - suitable for turbine tests including Ferrography
INSERT INTO TestReadings (sampleID, testID, trialNumber, value1, value2, value3, trialCalc, ID1, ID2, ID3, trialComplete, status, schedType, entryID, validateID, entryDate, valiDate, MainComments) VALUES
(6, 10, 1, 0.8, 0.01, 0.008, NULL, NULL, NULL, NULL, 0, 'A', NULL, '01832', '01832', GETDATE(), GETDATE(), NULL), -- TAN
(6, 20, 1, 2, NULL, NULL, NULL, NULL, NULL, NULL, 0, 'A', NULL, '01832', '01832', GETDATE(), GETDATE(), NULL), -- Water-KF
(6, 50, 1, 285.6, NULL, 72.1, NULL, 'EG1006', 'J006', 'EG2006', 0, 'A', NULL, '01832', '01832', GETDATE(), GETDATE(), NULL), -- Viscosity @ 40
(6, 70, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 'A', NULL, '01832', '01832', GETDATE(), GETDATE(), NULL), -- FT-IR
(6, 160, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 'A', NULL, '01832', '01832', GETDATE(), GETDATE(), NULL), -- Particle Count
(6, 210, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 'A', NULL, '01832', '01832', GETDATE(), GETDATE(), NULL); -- Ferrography

-- Sample 7 (MOTOR-001) - Bearing Grease - suitable for grease tests
INSERT INTO TestReadings (sampleID, testID, trialNumber, value1, value2, value3, trialCalc, ID1, ID2, ID3, trialComplete, status, schedType, entryID, validateID, entryDate, valiDate, MainComments) VALUES
(7, 130, 1, 285, NULL, NULL, NULL, NULL, NULL, NULL, 0, 'A', NULL, '01832', '01832', GETDATE(), GETDATE(), NULL), -- Grease Penetration Worked
(7, 140, 1, 195, NULL, NULL, NULL, NULL, NULL, NULL, 0, 'A', NULL, '01832', '01832', GETDATE(), GETDATE(), NULL); -- Grease Dropping Point

-- Sample 8 (TRANS-001) - Transformer Oil - suitable for electrical tests
INSERT INTO TestReadings (sampleID, testID, trialNumber, value1, value2, value3, trialCalc, ID1, ID2, ID3, trialComplete, status, schedType, entryID, validateID, entryDate, valiDate, MainComments) VALUES
(8, 10, 1, 0.05, 0.001, 0.0005, NULL, NULL, NULL, NULL, 0, 'A', NULL, '01832', '01832', GETDATE(), GETDATE(), NULL), -- TAN
(8, 20, 1, 1, NULL, NULL, NULL, NULL, NULL, NULL, 0, 'A', NULL, '01832', '01832', GETDATE(), GETDATE(), NULL), -- Water-KF
(8, 80, 1, 145, NULL, NULL, NULL, NULL, NULL, NULL, 0, 'A', NULL, '01832', '01832', GETDATE(), GETDATE(), NULL); -- Flash Point

PRINT 'Test readings seeded successfully!';

-- Show summary
SELECT 'Test Readings by Test' as Info, testID, COUNT(*) as SampleCount 
FROM TestReadings 
GROUP BY testID 
ORDER BY testID;

-- Show which samples are available for Ferrography (testID = 210)
SELECT 'Samples for Ferrography Test' as Info, sampleID 
FROM TestReadings 
WHERE testID = 210 
ORDER BY sampleID;

GO