-- Create Test Stand to Test Mapping
-- This script creates a mapping between TestStand IDs (qualifications) and Test IDs (actual tests)

USE LabResultsDb;
GO

PRINT 'Creating test stand to test mapping...';
GO

-- Create mapping table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TestStandMapping' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[TestStandMapping](
        [TestStandId] [smallint] NOT NULL,
        [TestId] [int] NOT NULL,
        [IsActive] [bit] NOT NULL DEFAULT 1,
        CONSTRAINT [PK_TestStandMapping] PRIMARY KEY CLUSTERED ([TestStandId] ASC, [TestId] ASC)
    ) ON [PRIMARY];
    PRINT 'Created TestStandMapping table.';
END
GO

-- Clear existing mappings
DELETE FROM TestStandMapping;
GO

-- Insert mappings between TestStand IDs and Test IDs
-- Map the qualification test stands to actual test implementations
INSERT INTO TestStandMapping (TestStandId, TestId, IsActive) VALUES
-- TAN by Color Indication (TestStand 10) -> TAN by Color Indication (Test 1)
(10, 1, 1),

-- Water - KF (TestStand 20) -> Water-KF (Test 2)
(20, 2, 1),

-- TBN by Auto Titration (TestStand 110) -> TBN by Auto Titration (Test 3)
(110, 3, 1),

-- Viscosity @ 40 (TestStand 50) -> Viscosity @ 40°C (Test 4)
(50, 4, 1),

-- Emission Spectroscopy - Standard (TestStand 30) -> Emission Spectroscopy (Test 5)
(30, 5, 1),

-- Emission Spectroscopy - Large (TestStand 40) -> Emission Spectroscopy (Test 5)
(40, 5, 1),

-- Flash Point (TestStand 80) -> Flash Point (Test 6)
(80, 6, 1),

-- Particle Count (TestStand 160) -> Particle Count (Test 8)
(160, 8, 1);

PRINT 'Inserted test stand to test mappings.';
GO

-- Add more test definitions to match the test stands
-- Insert additional tests that don't exist yet but have qualifications
INSERT INTO Test (TestName, TestDescription, Active) VALUES
('Viscosity @ 100°C', 'Kinematic viscosity measurement at 100 degrees Celsius', 1),
('FT-IR', 'Fourier Transform Infrared Spectroscopy', 1),
('Inspect Filter', 'Visual inspection of filter elements', 1),
('Grease Penetration Worked', 'Worked penetration test for grease consistency', 1),
('Grease Dropping Point', 'Temperature at which grease drops from test apparatus', 1),
('RBOT', 'Rotating Bomb Oxidation Test', 1),
('Filter Residue', 'Analysis of filter residue content', 1),
('Ferrography', 'Microscopic analysis of wear particles', 1),
('Rust', 'Rust and corrosion analysis', 1),
('TFOUT', 'Thin Film Oxygen Uptake Test', 1),
('Debris Identification', 'Identification and classification of debris particles', 1),
('Deleterious', 'Analysis of deleterious particles', 1),
('Rheometer', 'Rheological properties measurement', 1),
('Misc. Tests', 'Miscellaneous laboratory tests', 1),
('D-inch', 'D-inch particle analysis', 1),
('Oil Content', 'Oil content determination', 1),
('Varnish Potential Rating', 'Varnish potential assessment', 1);

PRINT 'Added additional test definitions.';
GO

-- Add mappings for the new tests
INSERT INTO TestStandMapping (TestStandId, TestId, IsActive) VALUES
-- Get the TestIDs for the newly inserted tests
(60, (SELECT TestID FROM Test WHERE TestName = 'Viscosity @ 100°C'), 1),
(70, (SELECT TestID FROM Test WHERE TestName = 'FT-IR'), 1),
(120, (SELECT TestID FROM Test WHERE TestName = 'Inspect Filter'), 1),
(130, (SELECT TestID FROM Test WHERE TestName = 'Grease Penetration Worked'), 1),
(140, (SELECT TestID FROM Test WHERE TestName = 'Grease Dropping Point'), 1),
(170, (SELECT TestID FROM Test WHERE TestName = 'RBOT'), 1),
(180, (SELECT TestID FROM Test WHERE TestName = 'Filter Residue'), 1),
(210, (SELECT TestID FROM Test WHERE TestName = 'Ferrography'), 1),
(220, (SELECT TestID FROM Test WHERE TestName = 'Rust'), 1),
(230, (SELECT TestID FROM Test WHERE TestName = 'TFOUT'), 1),
(240, (SELECT TestID FROM Test WHERE TestName = 'Debris Identification'), 1),
(250, (SELECT TestID FROM Test WHERE TestName = 'Deleterious'), 1),
(270, (SELECT TestID FROM Test WHERE TestName = 'Rheometer'), 1),
(280, (SELECT TestID FROM Test WHERE TestName = 'Misc. Tests'), 1),
(284, (SELECT TestID FROM Test WHERE TestName = 'D-inch'), 1),
(285, (SELECT TestID FROM Test WHERE TestName = 'Oil Content'), 1),
(286, (SELECT TestID FROM Test WHERE TestName = 'Varnish Potential Rating'), 1);

PRINT 'Added mappings for all test stands.';
GO

PRINT 'Test stand to test mapping completed successfully!';
GO

-- Display summary
SELECT 'Total Tests' as Info, COUNT(*) as Count FROM Test
UNION ALL
SELECT 'Total Test Stand Mappings' as Info, COUNT(*) as Count FROM TestStandMapping
UNION ALL
SELECT 'Active Mappings' as Info, COUNT(*) as Count FROM TestStandMapping WHERE IsActive = 1;
GO

-- Show mapping details
SELECT 
    ts.ID as TestStandId,
    ts.name as TestStandName,
    t.TestID,
    t.TestName,
    tsm.IsActive
FROM TestStand ts
INNER JOIN TestStandMapping tsm ON ts.ID = tsm.TestStandId
INNER JOIN Test t ON tsm.TestId = t.TestID
ORDER BY ts.ID;
GO