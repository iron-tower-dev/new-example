-- Create Super User with access to ALL tests
-- This user can perform any test for easy testing

USE LabResultsDb;
GO

PRINT 'Creating super user with access to all tests...';
GO

-- Insert super user technician
IF NOT EXISTS (SELECT 1 FROM LubeTechList WHERE employeeID = 'ADMIN')
BEGIN
    INSERT INTO LubeTechList (employeeID, lastName, firstName, MI, qualificationPassword) VALUES
    ('ADMIN', 'Administrator', 'Super', 'U', 'admin123');
    
    PRINT 'Inserted super user technician.';
END
ELSE
BEGIN
    UPDATE LubeTechList 
    SET lastName = 'Administrator', firstName = 'Super', MI = 'U', qualificationPassword = 'admin123'
    WHERE employeeID = 'ADMIN';
    
    PRINT 'Updated existing super user technician.';
END
GO

-- Delete existing qualifications for ADMIN to avoid duplicates
DELETE FROM LubeTechQualification WHERE employeeID = 'ADMIN';
GO

-- Insert ALL test qualifications for super user with MicrE level (highest)
INSERT INTO LubeTechQualification (employeeID, testStandID, testStand, qualificationLevel) VALUES
-- Basic Chemical Tests
('ADMIN', 1, 'TAN Test Station', 'MicrE'),
('ADMIN', 2, 'Water Content Station', 'MicrE'),
('ADMIN', 3, 'TBN Test Station', 'MicrE'),

-- Viscosity Tests
('ADMIN', 4, 'Viscosity 40C Station', 'MicrE'),
('ADMIN', 5, 'Viscosity 100C Station', 'MicrE'),
('ADMIN', 6, 'Flash Point Station', 'MicrE'),

-- Spectroscopy Tests
('ADMIN', 7, 'Emission Spectroscopy Station', 'MicrE'),
('ADMIN', 8, 'Particle Count Station', 'MicrE'),

-- Grease Tests
('ADMIN', 9, 'Grease Penetration Station', 'MicrE'),
('ADMIN', 10, 'Grease Dropping Point Station', 'MicrE'),

-- Particle Analysis Tests
('ADMIN', 11, 'Inspect Filter Station', 'MicrE'),
('ADMIN', 12, 'Ferrography Station', 'MicrE'),

-- Specialized Tests
('ADMIN', 13, 'RBOT Station', 'MicrE'),
('ADMIN', 14, 'TFOUT Station', 'MicrE'),
('ADMIN', 15, 'Rust Test Station', 'MicrE'),
('ADMIN', 16, 'Deleterious Test Station', 'MicrE'),
('ADMIN', 17, 'D-inch Analysis Station', 'MicrE'),
('ADMIN', 18, 'Oil Content Station', 'MicrE'),
('ADMIN', 19, 'Varnish Potential Rating Station', 'MicrE');

PRINT 'Inserted ALL test qualifications for super user.';
GO

PRINT 'Super user created successfully!';
PRINT '';
PRINT '=== SUPER USER CREDENTIALS ===';
PRINT '';
PRINT 'SUPER USER (Access to ALL tests):';
PRINT '  Employee ID: ADMIN';
PRINT '  Password: admin123';
PRINT '  Name: Super U Administrator';
PRINT '  Qualification Level: MicrE (highest level for all tests)';
PRINT '';
PRINT 'This user can perform ANY test in the system!';
GO

-- Display qualifications for the super user
SELECT 
    ltq.employeeID,
    lt.firstName + ' ' + lt.lastName as FullName,
    ltq.testStandID,
    ltq.testStand,
    ltq.qualificationLevel
FROM LubeTechQualification ltq
JOIN LubeTechList lt ON ltq.employeeID = lt.employeeID
WHERE ltq.employeeID = 'ADMIN'
ORDER BY ltq.testStandID;
GO