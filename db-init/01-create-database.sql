-- Create the LabResultsDb database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'LabResultsDb')
BEGIN
    CREATE DATABASE LabResultsDb;
    PRINT 'Database LabResultsDb created successfully.';
END
ELSE
BEGIN
    PRINT 'Database LabResultsDb already exists.';
END
GO

-- Use the database
USE LabResultsDb;
GO

PRINT 'Switched to LabResultsDb database.';
GO