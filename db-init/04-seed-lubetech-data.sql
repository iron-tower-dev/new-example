-- Seed LubeTech Data Script for Lab Results Database
-- This script creates LubeTech tables and loads user qualification data

USE LabResultsDb;
GO

PRINT 'Creating LubeTech tables and seeding user data...';
GO

-- Create LubeTechList table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='LubeTechList' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[LubeTechList](
        [employeeID] [nvarchar](5) NULL,
        [lastName] [nvarchar](22) NULL,
        [firstName] [nvarchar](14) NULL,
        [MI] [nvarchar](1) NULL,
        [qualificationPassword] [nvarchar](8) NULL
    ) ON [PRIMARY];
    PRINT 'Created LubeTechList table.';
END
GO

-- Create LubeTechQualification table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='LubeTechQualification' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[LubeTechQualification](
        [employeeID] [nvarchar](5) NULL,
        [testStandID] [smallint] NULL,
        [testStand] [nvarchar](50) NULL,
        [qualificationLevel] [nvarchar](10) NULL
    ) ON [PRIMARY];
    PRINT 'Created LubeTechQualification table.';
END
GO

-- Create TestStand table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TestStand' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[TestStand](
        [ID] [smallint] NOT NULL,
        [name] [nvarchar](50) NULL,
        CONSTRAINT [PK_TestStand] PRIMARY KEY CLUSTERED ([ID] ASC)
    ) ON [PRIMARY];
    PRINT 'Created TestStand table.';
END
GO

-- Clear existing data
DELETE FROM LubeTechList;
DELETE FROM LubeTechQualification;
DELETE FROM TestStand;
GO

-- Insert TestStand data
INSERT INTO TestStand (ID, name) VALUES
(10, 'TAN by Color Indication'),
(20, 'Water - KF'),
(30, 'Emission Spectroscopy - Standard'),
(40, 'Emission Spectroscopy - Large'),
(50, 'Viscosity @ 40'),
(60, 'Viscosity @ 100'),
(70, 'FT-IR'),
(80, 'Flash Point'),
(110, 'TBN by Auto Titration'),
(120, 'Inspect Filter'),
(130, 'Grease Penetration Worked'),
(140, 'Grease Dropping Point'),
(160, 'Particle Count'),
(170, 'RBOT'),
(180, 'Filter Residue'),
(210, 'Ferrography'),
(220, 'Rust'),
(230, 'TFOUT'),
(240, 'Debris Identification'),
(250, 'Deleterious'),
(270, 'Rheometer'),
(280, 'Misc. Tests'),
(284, 'D-inch'),
(285, 'Oil Content'),
(286, 'Varnish Potential Rating');
PRINT 'Inserted TestStand data.';
GO

-- Insert LubeTech users (including super user)
INSERT INTO LubeTechList (employeeID, lastName, firstName, MI, qualificationPassword) VALUES
('01076', 'ZALITIS', 'JOHN', NULL, 'j219s'),
('26675', 'CONNELL', 'CHARLIE', 'F', 'FRED'),
('32418', 'DICICCIO', 'PAUL', NULL, 'ROCK'),
('76780', 'RADNICH', 'STEVE', NULL, 'STEVER'),
('77442', 'RATTO', 'CARLOS', NULL, 'OTTAR'),
('96949', 'WARNER', 'GEANA', NULL, 'GEANA'),
('06175', 'YOUNG', 'RICHARD', NULL, 'dev'),
('99703', 'COPELAND', 'JIM', 'R', 'ROTIDE1'),
('16585', 'SOUTHWORTH', 'DERRICK', NULL, NULL),
('83046', 'HOLTZCLAW', 'MARY', NULL, 'HOLTZ'),
('74004', 'PECK', 'JOE', NULL, 'PECK'),
('43874', 'HAUTALA', 'DAN', NULL, 'DAN'),
('51636', 'HUCKEY', 'STEVE', NULL, 'ELK123'),
('02367', 'MCGATH', 'TAMMY', 'D', 'TEST'),
('99824', 'PIATEK', 'STAN', NULL, '8888'),
('17411', 'BRUCKER', 'MARLENE', NULL, 'FRED'),
('80533', 'ROWLAND', 'JIM', NULL, 'BRENDA'),
('99947', 'HOFMANN', 'STEVE', NULL, 'TUNDRA01'),
('C7162', 'HEM', 'SHARON', NULL, 'MONKEY'),
('C6334', 'REESER', 'HOWARD', NULL, NULL),
('01832', 'TEAL', 'DARLENE', NULL, 'TEAL'),
('09926', 'FRUSTI', 'ERIC', NULL, NULL),
('00551', 'BOWMAN', 'STEVE', NULL, 'dsatest'),
('02016', 'KOSS', 'PAUL', NULL, 'BOSS'),
('08326', 'SEATON', 'DON', NULL, 'TEST'),
('14897', 'SHAMABANSE', 'KUBALE', NULL, NULL),
('02964', 'ENGSTROM', 'HERBERT', NULL, 'DRDOOM'),
('00617', 'WOLTER', 'DENNIS', NULL, 'MASTS'),
('74362', 'PERKINS', 'JEFF', NULL, 'JP'),
('Q3811', 'HULL', 'BART', 'D', 'bart'),
('02409', 'VINH', 'THIEN', 'H', 'Thien'),
('56472', 'KISSLER', 'ROBERT', NULL, 'dev'),
('07446', 'MARTINEZ', 'CHRISTY', NULL, NULL),
('18598', 'BOWLING', 'THOMAS', NULL, NULL),
('99999', 'SUPERUSER', 'ADMIN', NULL, 'SUPER123');
PRINT 'Inserted LubeTech users including super user.';
GO

-- Insert qualifications for super user (99999) with all tests at highest levels
INSERT INTO LubeTechQualification (employeeID, testStandID, testStand, qualificationLevel) VALUES
('99999', 10, 'TAN by Color Indication', 'Q/QAG'),
('99999', 20, 'Water - KF', 'Q/QAG'),
('99999', 30, 'Emission Spectroscopy - Standard', 'Q/QAG'),
('99999', 40, 'Emission Spectroscopy - Large', 'Q/QAG'),
('99999', 50, 'Viscosity @ 40', 'Q/QAG'),
('99999', 60, 'Viscosity @ 100', 'Q/QAG'),
('99999', 70, 'FT-IR', 'Q/QAG'),
('99999', 80, 'Flash Point', 'Q/QAG'),
('99999', 110, 'TBN by Auto Titration', 'Q/QAG'),
('99999', 120, 'Inspect Filter', 'MicrE'),
('99999', 130, 'Grease Penetration Worked', 'Q/QAG'),
('99999', 140, 'Grease Dropping Point', 'Q/QAG'),
('99999', 160, 'Particle Count', 'MicrE'),
('99999', 170, 'RBOT', 'Q/QAG'),
('99999', 180, 'Filter Residue', 'MicrE'),
('99999', 210, 'Ferrography', 'MicrE'),
('99999', 220, 'Rust', 'Q/QAG'),
('99999', 230, 'TFOUT', 'Q/QAG'),
('99999', 240, 'Debris Identification', 'MicrE'),
('99999', 250, 'Deleterious', 'Q/QAG'),
('99999', 270, 'Rheometer', 'Q/QAG'),
('99999', 280, 'Misc. Tests', 'Q/QAG'),
('99999', 284, 'D-inch', 'Q/QAG'),
('99999', 285, 'Oil Content', 'Q/QAG'),
('99999', 286, 'Varnish Potential Rating', 'Q/QAG');
PRINT 'Inserted super user qualifications for all test stands.';
GO

-- Insert sample qualifications for other key users
-- User 96949 (WARNER, GEANA) - comprehensive qualifications
INSERT INTO LubeTechQualification (employeeID, testStandID, testStand, qualificationLevel) VALUES
('96949', 10, 'TAN by Color Indication', 'Q/QAG'),
('96949', 20, 'Water - KF', 'Q/QAG'),
('96949', 30, 'Emission Spectroscopy - Standard', 'Q/QAG'),
('96949', 40, 'Emission Spectroscopy - Large', 'Q/QAG'),
('96949', 50, 'Viscosity @ 40', 'Q/QAG'),
('96949', 60, 'Viscosity @ 100', 'Q/QAG'),
('96949', 70, 'FT-IR', 'Q/QAG'),
('96949', 80, 'Flash Point', 'Q/QAG'),
('96949', 110, 'TBN by Auto Titration', 'Q/QAG'),
('96949', 120, 'Inspect Filter', 'MicrE'),
('96949', 130, 'Grease Penetration Worked', 'Q/QAG'),
('96949', 140, 'Grease Dropping Point', 'Q/QAG'),
('96949', 160, 'Particle Count', 'Q/QAG'),
('96949', 170, 'RBOT', 'Q/QAG'),
('96949', 180, 'Filter Residue', 'MicrE'),
('96949', 210, 'Ferrography', 'MicrE'),
('96949', 220, 'Rust', 'Q/QAG'),
('96949', 230, 'TFOUT', 'Q/QAG'),
('96949', 240, 'Debris Identification', 'MicrE'),
('96949', 250, 'Deleterious', 'Q/QAG'),
('96949', 270, 'Rheometer', 'Q/QAG'),
('96949', 285, 'Oil Content', 'TRAIN'),
('96949', 286, 'Varnish Potential Rating', 'Q/QAG');

-- User 83046 (HOLTZCLAW, MARY) - extensive qualifications
INSERT INTO LubeTechQualification (employeeID, testStandID, testStand, qualificationLevel) VALUES
('83046', 10, 'TAN by Color Indication', 'Q/QAG'),
('83046', 20, 'Water - KF', 'Q/QAG'),
('83046', 30, 'Emission Spectroscopy - Standard', 'Q/QAG'),
('83046', 40, 'Emission Spectroscopy - Large', 'Q/QAG'),
('83046', 50, 'Viscosity @ 40', 'Q/QAG'),
('83046', 60, 'Viscosity @ 100', 'Q/QAG'),
('83046', 70, 'FT-IR', 'Q/QAG'),
('83046', 80, 'Flash Point', 'Q/QAG'),
('83046', 110, 'TBN by Auto Titration', 'Q/QAG'),
('83046', 120, 'Inspect Filter', 'Q/QAG'),
('83046', 130, 'Grease Penetration Worked', 'Q/QAG'),
('83046', 140, 'Grease Dropping Point', 'TRAIN'),
('83046', 160, 'Particle Count', 'Q/QAG'),
('83046', 170, 'RBOT', 'Q/QAG'),
('83046', 180, 'Filter Residue', 'MicrE'),
('83046', 210, 'Ferrography', 'MicrE'),
('83046', 220, 'Rust', 'Q/QAG'),
('83046', 230, 'TFOUT', 'Q/QAG'),
('83046', 240, 'Debris Identification', 'MicrE'),
('83046', 250, 'Deleterious', 'Q/QAG'),
('83046', 270, 'Rheometer', 'Q/QAG'),
('83046', 285, 'Oil Content', 'Q/QAG'),
('83046', 286, 'Varnish Potential Rating', 'Q/QAG');

-- User 74362 (PERKINS, JEFF) - moderate qualifications
INSERT INTO LubeTechQualification (employeeID, testStandID, testStand, qualificationLevel) VALUES
('74362', 10, 'TAN by Color Indication', 'Q/QAG'),
('74362', 20, 'Water - KF', 'Q/QAG'),
('74362', 30, 'Emission Spectroscopy - Standard', 'TRAIN'),
('74362', 40, 'Emission Spectroscopy - Large', 'TRAIN'),
('74362', 50, 'Viscosity @ 40', 'Q/QAG'),
('74362', 60, 'Viscosity @ 100', 'Q/QAG'),
('74362', 70, 'FT-IR', 'TRAIN'),
('74362', 110, 'TBN by Auto Titration', 'Q/QAG'),
('74362', 140, 'Grease Dropping Point', 'Q/QAG'),
('74362', 170, 'RBOT', 'TRAIN'),
('74362', 180, 'Filter Residue', 'Q/QAG'),
('74362', 240, 'Debris Identification', 'TRAIN'),
('74362', 270, 'Rheometer', 'Q/QAG'),
('74362', 286, 'Varnish Potential Rating', 'Q/QAG');

-- User 06175 (YOUNG, RICHARD) - comprehensive qualifications
INSERT INTO LubeTechQualification (employeeID, testStandID, testStand, qualificationLevel) VALUES
('06175', 10, 'TAN by Color Indication', 'Q/QAG'),
('06175', 20, 'Water - KF', 'Q/QAG'),
('06175', 30, 'Emission Spectroscopy - Standard', 'Q/QAG'),
('06175', 40, 'Emission Spectroscopy - Large', 'Q/QAG'),
('06175', 50, 'Viscosity @ 40', 'Q/QAG'),
('06175', 60, 'Viscosity @ 100', 'Q/QAG'),
('06175', 70, 'FT-IR', 'Q/QAG'),
('06175', 80, 'Flash Point', 'Q/QAG'),
('06175', 110, 'TBN by Auto Titration', 'Q/QAG'),
('06175', 120, 'Inspect Filter', 'Q/QAG'),
('06175', 140, 'Grease Dropping Point', 'Q/QAG'),
('06175', 160, 'Particle Count', 'Q/QAG'),
('06175', 170, 'RBOT', 'Q/QAG'),
('06175', 180, 'Filter Residue', 'Q/QAG'),
('06175', 210, 'Ferrography', 'Q/QAG'),
('06175', 220, 'Rust', 'Q/QAG'),
('06175', 230, 'TFOUT', 'Q/QAG'),
('06175', 240, 'Debris Identification', 'MicrE'),
('06175', 250, 'Deleterious', 'Q/QAG'),
('06175', 270, 'Rheometer', 'Q/QAG'),
('06175', 286, 'Varnish Potential Rating', 'Q/QAG');

PRINT 'Inserted sample qualifications for key users.';
GO

PRINT 'LubeTech data seeding completed successfully!';
PRINT 'Super user (99999) has been created with all qualifications.';
GO

-- Display summary of seeded LubeTech data
SELECT 'LubeTech Users' as TableName, COUNT(*) as RecordCount FROM LubeTechList
UNION ALL
SELECT 'User Qualifications' as TableName, COUNT(*) as RecordCount FROM LubeTechQualification
UNION ALL
SELECT 'Test Stands' as TableName, COUNT(*) as RecordCount FROM TestStand;
GO

-- Show super user details
SELECT 'Super User Details:' as Info;
SELECT employeeID, lastName, firstName, qualificationPassword 
FROM LubeTechList 
WHERE employeeID = '99999';

SELECT 'Super User Qualifications Count:' as Info;
SELECT COUNT(*) as QualificationCount 
FROM LubeTechQualification 
WHERE employeeID = '99999';
GO