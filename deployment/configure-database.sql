-- Database configuration script for Lab Results System Production Deployment
-- Run this script on the production SQL Server before deploying the application

USE master;
GO

-- Create database if it doesn't exist
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

USE LabResultsDb;
GO

-- Create application user if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = 'LabResultsUser')
BEGIN
    CREATE LOGIN LabResultsUser WITH PASSWORD = 'REPLACE_WITH_SECURE_PASSWORD';
    PRINT 'Login LabResultsUser created successfully.';
END
ELSE
BEGIN
    PRINT 'Login LabResultsUser already exists.';
END
GO

-- Create database user
IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = 'LabResultsUser')
BEGIN
    CREATE USER LabResultsUser FOR LOGIN LabResultsUser;
    PRINT 'User LabResultsUser created successfully.';
END
ELSE
BEGIN
    PRINT 'User LabResultsUser already exists.';
END
GO

-- Grant necessary permissions
ALTER ROLE db_datareader ADD MEMBER LabResultsUser;
ALTER ROLE db_datawriter ADD MEMBER LabResultsUser;
ALTER ROLE db_ddladmin ADD MEMBER LabResultsUser;
GRANT EXECUTE TO LabResultsUser;
PRINT 'Permissions granted to LabResultsUser.';
GO

-- Create performance indexes if they don't exist
IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = 'IX_UsedLubeSamples_SampleDate')
BEGIN
    CREATE NONCLUSTERED INDEX IX_UsedLubeSamples_SampleDate 
    ON UsedLubeSamples (sampleDate DESC)
    INCLUDE (tagNumber, component, location);
    PRINT 'Index IX_UsedLubeSamples_SampleDate created.';
END
GO

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = 'IX_TestReadings_SampleTest')
BEGIN
    CREATE NONCLUSTERED INDEX IX_TestReadings_SampleTest 
    ON TestReadings (sampleID, testID, trialNumber)
    INCLUDE (value1, value2, value3, trialCalc, status);
    PRINT 'Index IX_TestReadings_SampleTest created.';
END
GO

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = 'IX_EmSpectro_SampleTest')
BEGIN
    CREATE NONCLUSTERED INDEX IX_EmSpectro_SampleTest 
    ON EmSpectro (id, testId, trialNum)
    INCLUDE (Na, Mo, Mg, Ca, Al, Ba, Ni, Mn, Zn, P, Ag, Pb, H, B, Cu, Fe, Cr, Sn, Si);
    PRINT 'Index IX_EmSpectro_SampleTest created.';
END
GO

-- Create backup job (optional - adjust schedule as needed)
DECLARE @BackupPath NVARCHAR(500) = 'C:\DatabaseBackups\LabResultsDb_' + FORMAT(GETDATE(), 'yyyyMMdd_HHmmss') + '.bak';

-- Ensure backup directory exists (you may need to create this manually)
BACKUP DATABASE LabResultsDb 
TO DISK = @BackupPath
WITH FORMAT, INIT, COMPRESSION;
PRINT 'Database backup created at: ' + @BackupPath;
GO

-- Set database options for production
ALTER DATABASE LabResultsDb SET RECOVERY FULL;
ALTER DATABASE LabResultsDb SET AUTO_CLOSE OFF;
ALTER DATABASE LabResultsDb SET AUTO_SHRINK OFF;
ALTER DATABASE LabResultsDb SET AUTO_CREATE_STATISTICS ON;
ALTER DATABASE LabResultsDb SET AUTO_UPDATE_STATISTICS ON;
ALTER DATABASE LabResultsDb SET AUTO_UPDATE_STATISTICS_ASYNC ON;
PRINT 'Database options configured for production.';
GO

-- Update statistics on all tables
EXEC sp_updatestats;
PRINT 'Statistics updated on all tables.';
GO

PRINT 'Database configuration completed successfully!';
PRINT 'Remember to:';
PRINT '1. Replace REPLACE_WITH_SECURE_PASSWORD with a strong password';
PRINT '2. Update connection strings in appsettings.Production.json';
PRINT '3. Ensure backup directory C:\DatabaseBackups exists';
PRINT '4. Configure regular backup maintenance plans';
GO