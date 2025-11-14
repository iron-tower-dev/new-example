-- Create Test Users for Lab Results API
-- This script creates test authentication tables and users for development/testing

USE LabResultsDb;
GO

PRINT 'Creating authentication tables and test users...';
GO

-- Create LubeTechList table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='LubeTechList' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[LubeTechList](
        [employeeID] [nvarchar](5) NOT NULL,
        [lastName] [nvarchar](22) NULL,
        [firstName] [nvarchar](14) NULL,
        [MI] [nvarchar](1) NULL,
        [qualificationPassword] [nvarchar](8) NULL,
        CONSTRAINT [PK_LubeTechList] PRIMARY KEY CLUSTERED ([employeeID] ASC)
    );
    PRINT 'Created LubeTechList table.';
END
GO

-- Create ReviewerList table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ReviewerList' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[ReviewerList](
        [employeeID] [nvarchar](5) NOT NULL,
        [lastName] [nvarchar](22) NOT NULL,
        [firstName] [nvarchar](14) NOT NULL,
        [MI] [nvarchar](1) NULL,
        [reviewerPassword] [nvarchar](8) NULL,
        [level] [smallint] NOT NULL DEFAULT 1,
        CONSTRAINT [PK_ReviewerList] PRIMARY KEY CLUSTERED ([employeeID] ASC)
    );
    PRINT 'Created ReviewerList table.';
END
GO

-- Create LubeTechQualification table if it doesn't exist (keyless entity)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='LubeTechQualification' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[LubeTechQualification](
        [employeeID] [nvarchar](5) NOT NULL,
        [testStandID] [smallint] NULL,
        [testStand] [nvarchar](50) NULL,
        [qualificationLevel] [nvarchar](10) NULL
    );
    PRINT 'Created LubeTechQualification table.';
END
GO

-- Insert test technicians
IF NOT EXISTS (SELECT 1 FROM LubeTechList WHERE employeeID = '12345')
BEGIN
    INSERT INTO LubeTechList (employeeID, lastName, firstName, MI, qualificationPassword) VALUES
    ('12345', 'Doe', 'John', 'A', 'test123'),
    ('23456', 'Smith', 'Jane', 'B', 'test456'),
    ('34567', 'Johnson', 'Mike', 'C', 'test789');
    
    PRINT 'Inserted test technicians.';
END
GO

-- Insert test reviewers
IF NOT EXISTS (SELECT 1 FROM ReviewerList WHERE employeeID = '99999')
BEGIN
    INSERT INTO ReviewerList (employeeID, lastName, firstName, MI, reviewerPassword, level) VALUES
    ('99999', 'Wilson', 'Sarah', 'M', 'review1', 2),
    ('88888', 'Brown', 'David', 'L', 'review2', 1);
    
    PRINT 'Inserted test reviewers.';
END
GO

-- Insert test qualifications for technicians
IF NOT EXISTS (SELECT 1 FROM LubeTechQualification WHERE employeeID = '12345')
BEGIN
    INSERT INTO LubeTechQualification (employeeID, testStandID, testStand, qualificationLevel) VALUES
    -- John Doe (12345) - Senior technician with high qualifications
    ('12345', 1, 'TAN Test Station', 'MicrE'),
    ('12345', 2, 'Viscosity Station', 'MicrE'),
    ('12345', 3, 'Water Content Station', 'Q/QAG'),
    ('12345', 4, 'Flash Point Station', 'MicrE'),
    
    -- Jane Smith (23456) - Mid-level technician
    ('23456', 1, 'TAN Test Station', 'Q/QAG'),
    ('23456', 2, 'Viscosity Station', 'Q/QAG'),
    ('23456', 5, 'Particle Count Station', 'TRAIN'),
    
    -- Mike Johnson (34567) - Junior technician
    ('34567', 1, 'TAN Test Station', 'TRAIN'),
    ('34567', 6, 'Basic Testing Station', 'TRAIN');
    
    PRINT 'Inserted test qualifications.';
END
GO

PRINT 'Test users created successfully!';
PRINT '';
PRINT '=== TEST CREDENTIALS ===';
PRINT '';
PRINT 'TECHNICIANS:';
PRINT '  Employee ID: 12345, Password: test123 (John A Doe - Senior Tech with MicrE qualifications)';
PRINT '  Employee ID: 23456, Password: test456 (Jane B Smith - Mid-level Tech with Q/QAG qualifications)';
PRINT '  Employee ID: 34567, Password: test789 (Mike C Johnson - Junior Tech with TRAIN qualifications)';
PRINT '';
PRINT 'REVIEWERS:';
PRINT '  Employee ID: 99999, Password: review1 (Sarah M Wilson - Level 2 Reviewer)';
PRINT '  Employee ID: 88888, Password: review2 (David L Brown - Level 1 Reviewer)';
PRINT '';
PRINT 'Use these credentials to test the authentication system!';
GO

-- Display summary of created users
SELECT 'Technicians' as UserType, COUNT(*) as Count FROM LubeTechList
UNION ALL
SELECT 'Reviewers' as UserType, COUNT(*) as Count FROM ReviewerList
UNION ALL
SELECT 'Qualifications' as UserType, COUNT(*) as Count FROM LubeTechQualification;
GO