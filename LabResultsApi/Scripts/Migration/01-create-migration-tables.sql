-- Migration System Database Setup Script
-- Creates tables required for migration tracking and logging

USE [LabResultsDb]
GO

-- Create Migration History table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MigrationHistory]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[MigrationHistory](
        [MigrationId] [uniqueidentifier] NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [StartTime] [datetime2](7) NOT NULL DEFAULT GETUTCDATE(),
        [EndTime] [datetime2](7) NULL,
        [Status] [nvarchar](50) NOT NULL DEFAULT 'NotStarted',
        [CurrentOperation] [nvarchar](255) NULL,
        [ProgressPercentage] [decimal](5,2) NOT NULL DEFAULT 0,
        [EstimatedTimeRemaining] [time](7) NULL,
        [ConfigurationJson] [nvarchar](max) NULL,
        [ResultJson] [nvarchar](max) NULL,
        [CreatedBy] [nvarchar](100) NULL,
        [Environment] [nvarchar](50) NULL,
        [Version] [nvarchar](20) NULL
    )
    
    CREATE INDEX [IX_MigrationHistory_StartTime] ON [dbo].[MigrationHistory] ([StartTime] DESC)
    CREATE INDEX [IX_MigrationHistory_Status] ON [dbo].[MigrationHistory] ([Status])
    CREATE INDEX [IX_MigrationHistory_Environment] ON [dbo].[MigrationHistory] ([Environment])
    
    PRINT 'Created MigrationHistory table'
END
ELSE
BEGIN
    PRINT 'MigrationHistory table already exists'
END
GO

-- Create Migration Statistics table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MigrationStatistics]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[MigrationStatistics](
        [StatisticsId] [uniqueidentifier] NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [MigrationId] [uniqueidentifier] NOT NULL,
        [TablesProcessed] [int] NOT NULL DEFAULT 0,
        [TablesCreated] [int] NOT NULL DEFAULT 0,
        [RecordsProcessed] [bigint] NOT NULL DEFAULT 0,
        [RecordsInserted] [bigint] NOT NULL DEFAULT 0,
        [RecordsSkipped] [bigint] NOT NULL DEFAULT 0,
        [RecordsUpdated] [bigint] NOT NULL DEFAULT 0,
        [ErrorCount] [int] NOT NULL DEFAULT 0,
        [WarningCount] [int] NOT NULL DEFAULT 0,
        [ValidationQueriesRun] [int] NOT NULL DEFAULT 0,
        [ValidationQueriesMatched] [int] NOT NULL DEFAULT 0,
        [ValidationQueriesFailed] [int] NOT NULL DEFAULT 0,
        [AverageProcessingTimeMs] [decimal](10,2) NULL,
        [PeakMemoryUsageMB] [decimal](10,2) NULL,
        [TotalDurationMs] [bigint] NULL,
        [UpdatedAt] [datetime2](7) NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT [FK_MigrationStatistics_MigrationHistory] 
            FOREIGN KEY ([MigrationId]) REFERENCES [dbo].[MigrationHistory]([MigrationId])
            ON DELETE CASCADE
    )
    
    CREATE INDEX [IX_MigrationStatistics_MigrationId] ON [dbo].[MigrationStatistics] ([MigrationId])
    
    PRINT 'Created MigrationStatistics table'
END
ELSE
BEGIN
    PRINT 'MigrationStatistics table already exists'
END
GO

-- Create Migration Errors table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MigrationErrors]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[MigrationErrors](
        [ErrorId] [uniqueidentifier] NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [MigrationId] [uniqueidentifier] NOT NULL,
        [Timestamp] [datetime2](7) NOT NULL DEFAULT GETUTCDATE(),
        [Level] [nvarchar](20) NOT NULL,
        [Component] [nvarchar](100) NOT NULL,
        [Message] [nvarchar](max) NOT NULL,
        [Details] [nvarchar](max) NULL,
        [TableName] [nvarchar](128) NULL,
        [RecordNumber] [bigint] NULL,
        [StackTrace] [nvarchar](max) NULL,
        [InnerException] [nvarchar](max) NULL,
        [Resolution] [nvarchar](max) NULL,
        [IsResolved] [bit] NOT NULL DEFAULT 0
    )
    
    CREATE INDEX [IX_MigrationErrors_MigrationId] ON [dbo].[MigrationErrors] ([MigrationId])
    CREATE INDEX [IX_MigrationErrors_Timestamp] ON [dbo].[MigrationErrors] ([Timestamp] DESC)
    CREATE INDEX [IX_MigrationErrors_Level] ON [dbo].[MigrationErrors] ([Level])
    CREATE INDEX [IX_MigrationErrors_Component] ON [dbo].[MigrationErrors] ([Component])
    
    ALTER TABLE [dbo].[MigrationErrors] 
    ADD CONSTRAINT [FK_MigrationErrors_MigrationHistory] 
        FOREIGN KEY ([MigrationId]) REFERENCES [dbo].[MigrationHistory]([MigrationId])
        ON DELETE CASCADE
    
    PRINT 'Created MigrationErrors table'
END
ELSE
BEGIN
    PRINT 'MigrationErrors table already exists'
END
GO

-- Create Table Processing Status table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TableProcessingStatus]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[TableProcessingStatus](
        [StatusId] [uniqueidentifier] NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [MigrationId] [uniqueidentifier] NOT NULL,
        [TableName] [nvarchar](128) NOT NULL,
        [Status] [nvarchar](50) NOT NULL DEFAULT 'NotStarted',
        [StartTime] [datetime2](7) NULL,
        [EndTime] [datetime2](7) NULL,
        [RecordsProcessed] [bigint] NOT NULL DEFAULT 0,
        [RecordsInserted] [bigint] NOT NULL DEFAULT 0,
        [RecordsSkipped] [bigint] NOT NULL DEFAULT 0,
        [ErrorCount] [int] NOT NULL DEFAULT 0,
        [ProcessingTimeMs] [bigint] NULL,
        [CsvFilePath] [nvarchar](500) NULL,
        [SqlFilePath] [nvarchar](500) NULL,
        [LastError] [nvarchar](max) NULL,
        [CreatedAt] [datetime2](7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] [datetime2](7) NOT NULL DEFAULT GETUTCDATE()
    )
    
    CREATE INDEX [IX_TableProcessingStatus_MigrationId] ON [dbo].[TableProcessingStatus] ([MigrationId])
    CREATE INDEX [IX_TableProcessingStatus_TableName] ON [dbo].[TableProcessingStatus] ([TableName])
    CREATE INDEX [IX_TableProcessingStatus_Status] ON [dbo].[TableProcessingStatus] ([Status])
    
    ALTER TABLE [dbo].[TableProcessingStatus] 
    ADD CONSTRAINT [FK_TableProcessingStatus_MigrationHistory] 
        FOREIGN KEY ([MigrationId]) REFERENCES [dbo].[MigrationHistory]([MigrationId])
        ON DELETE CASCADE
    
    PRINT 'Created TableProcessingStatus table'
END
ELSE
BEGIN
    PRINT 'TableProcessingStatus table already exists'
END
GO

-- Create Query Validation Results table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[QueryValidationResults]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[QueryValidationResults](
        [ValidationId] [uniqueidentifier] NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [MigrationId] [uniqueidentifier] NOT NULL,
        [QueryName] [nvarchar](255) NOT NULL,
        [CurrentQuery] [nvarchar](max) NOT NULL,
        [LegacyQuery] [nvarchar](max) NOT NULL,
        [DataMatches] [bit] NOT NULL DEFAULT 0,
        [CurrentRowCount] [bigint] NULL,
        [LegacyRowCount] [bigint] NULL,
        [CurrentExecutionTimeMs] [bigint] NULL,
        [LegacyExecutionTimeMs] [bigint] NULL,
        [PerformanceDifferencePercent] [decimal](10,2) NULL,
        [DiscrepancyCount] [int] NOT NULL DEFAULT 0,
        [DiscrepanciesJson] [nvarchar](max) NULL,
        [ValidationStatus] [nvarchar](50) NOT NULL DEFAULT 'Pending',
        [ErrorMessage] [nvarchar](max) NULL,
        [ValidatedAt] [datetime2](7) NOT NULL DEFAULT GETUTCDATE()
    )
    
    CREATE INDEX [IX_QueryValidationResults_MigrationId] ON [dbo].[QueryValidationResults] ([MigrationId])
    CREATE INDEX [IX_QueryValidationResults_QueryName] ON [dbo].[QueryValidationResults] ([QueryName])
    CREATE INDEX [IX_QueryValidationResults_DataMatches] ON [dbo].[QueryValidationResults] ([DataMatches])
    CREATE INDEX [IX_QueryValidationResults_ValidationStatus] ON [dbo].[QueryValidationResults] ([ValidationStatus])
    
    ALTER TABLE [dbo].[QueryValidationResults] 
    ADD CONSTRAINT [FK_QueryValidationResults_MigrationHistory] 
        FOREIGN KEY ([MigrationId]) REFERENCES [dbo].[MigrationHistory]([MigrationId])
        ON DELETE CASCADE
    
    PRINT 'Created QueryValidationResults table'
END
ELSE
BEGIN
    PRINT 'QueryValidationResults table already exists'
END
GO

-- Create Migration Configuration table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MigrationConfiguration]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[MigrationConfiguration](
        [ConfigurationId] [uniqueidentifier] NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [ConfigurationName] [nvarchar](100) NOT NULL,
        [Environment] [nvarchar](50) NOT NULL,
        [ConfigurationJson] [nvarchar](max) NOT NULL,
        [IsActive] [bit] NOT NULL DEFAULT 0,
        [CreatedBy] [nvarchar](100) NULL,
        [CreatedAt] [datetime2](7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedBy] [nvarchar](100) NULL,
        [UpdatedAt] [datetime2](7) NOT NULL DEFAULT GETUTCDATE(),
        [Version] [int] NOT NULL DEFAULT 1,
        [Description] [nvarchar](500) NULL
    )
    
    CREATE UNIQUE INDEX [IX_MigrationConfiguration_Name_Environment] 
        ON [dbo].[MigrationConfiguration] ([ConfigurationName], [Environment])
    CREATE INDEX [IX_MigrationConfiguration_Environment] ON [dbo].[MigrationConfiguration] ([Environment])
    CREATE INDEX [IX_MigrationConfiguration_IsActive] ON [dbo].[MigrationConfiguration] ([IsActive])
    
    PRINT 'Created MigrationConfiguration table'
END
ELSE
BEGIN
    PRINT 'MigrationConfiguration table already exists'
END
GO

-- Create Authentication Backup table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AuthenticationBackup]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AuthenticationBackup](
        [BackupId] [uniqueidentifier] NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [BackupName] [nvarchar](100) NOT NULL,
        [MigrationId] [uniqueidentifier] NULL,
        [BackupPath] [nvarchar](500) NOT NULL,
        [BackupSize] [bigint] NULL,
        [FilesBackedUp] [int] NOT NULL DEFAULT 0,
        [BackupType] [nvarchar](50) NOT NULL DEFAULT 'Full',
        [Status] [nvarchar](50) NOT NULL DEFAULT 'InProgress',
        [CreatedAt] [datetime2](7) NOT NULL DEFAULT GETUTCDATE(),
        [CompletedAt] [datetime2](7) NULL,
        [ExpiresAt] [datetime2](7) NULL,
        [Description] [nvarchar](500) NULL,
        [RestoreInstructions] [nvarchar](max) NULL
    )
    
    CREATE INDEX [IX_AuthenticationBackup_MigrationId] ON [dbo].[AuthenticationBackup] ([MigrationId])
    CREATE INDEX [IX_AuthenticationBackup_Status] ON [dbo].[AuthenticationBackup] ([Status])
    CREATE INDEX [IX_AuthenticationBackup_CreatedAt] ON [dbo].[AuthenticationBackup] ([CreatedAt] DESC)
    
    ALTER TABLE [dbo].[AuthenticationBackup] 
    ADD CONSTRAINT [FK_AuthenticationBackup_MigrationHistory] 
        FOREIGN KEY ([MigrationId]) REFERENCES [dbo].[MigrationHistory]([MigrationId])
        ON DELETE SET NULL
    
    PRINT 'Created AuthenticationBackup table'
END
ELSE
BEGIN
    PRINT 'AuthenticationBackup table already exists'
END
GO

-- Create stored procedures for migration management
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_GetMigrationStatus]') AND type in (N'P'))
BEGIN
    EXEC('
    CREATE PROCEDURE [dbo].[sp_GetMigrationStatus]
        @MigrationId uniqueidentifier = NULL
    AS
    BEGIN
        SET NOCOUNT ON;
        
        IF @MigrationId IS NULL
        BEGIN
            -- Get the most recent migration
            SELECT TOP 1 
                mh.*,
                ms.TablesProcessed,
                ms.RecordsProcessed,
                ms.ErrorCount,
                ms.WarningCount
            FROM [dbo].[MigrationHistory] mh
            LEFT JOIN [dbo].[MigrationStatistics] ms ON mh.MigrationId = ms.MigrationId
            ORDER BY mh.StartTime DESC
        END
        ELSE
        BEGIN
            -- Get specific migration
            SELECT 
                mh.*,
                ms.TablesProcessed,
                ms.RecordsProcessed,
                ms.ErrorCount,
                ms.WarningCount
            FROM [dbo].[MigrationHistory] mh
            LEFT JOIN [dbo].[MigrationStatistics] ms ON mh.MigrationId = ms.MigrationId
            WHERE mh.MigrationId = @MigrationId
        END
    END
    ')
    
    PRINT 'Created sp_GetMigrationStatus stored procedure'
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_CleanupOldMigrations]') AND type in (N'P'))
BEGIN
    EXEC('
    CREATE PROCEDURE [dbo].[sp_CleanupOldMigrations]
        @RetentionDays int = 30,
        @DryRun bit = 1
    AS
    BEGIN
        SET NOCOUNT ON;
        
        DECLARE @CutoffDate datetime2(7) = DATEADD(day, -@RetentionDays, GETUTCDATE())
        DECLARE @DeleteCount int = 0
        
        IF @DryRun = 1
        BEGIN
            SELECT 
                COUNT(*) as MigrationsToDelete,
                MIN(StartTime) as OldestMigration,
                MAX(StartTime) as NewestToDelete
            FROM [dbo].[MigrationHistory]
            WHERE StartTime < @CutoffDate
                AND Status IN (''Completed'', ''Failed'', ''Cancelled'')
        END
        ELSE
        BEGIN
            DELETE FROM [dbo].[MigrationHistory]
            WHERE StartTime < @CutoffDate
                AND Status IN (''Completed'', ''Failed'', ''Cancelled'')
            
            SET @DeleteCount = @@ROWCOUNT
            
            SELECT @DeleteCount as DeletedMigrations
        END
    END
    ')
    
    PRINT 'Created sp_CleanupOldMigrations stored procedure'
END
GO

-- Insert default configuration
IF NOT EXISTS (SELECT * FROM [dbo].[MigrationConfiguration] WHERE ConfigurationName = 'Default' AND Environment = 'Development')
BEGIN
    INSERT INTO [dbo].[MigrationConfiguration] 
    (ConfigurationName, Environment, ConfigurationJson, IsActive, CreatedBy, Description)
    VALUES 
    ('Default', 'Development', 
     '{"clearExistingData":true,"createMissingTables":true,"validateAgainstLegacy":false,"maxConcurrentOperations":2}',
     1, 'System', 'Default development configuration')
    
    PRINT 'Inserted default development configuration'
END
GO

PRINT 'Migration system database setup completed successfully!'
PRINT 'Tables created:'
PRINT '  - MigrationHistory'
PRINT '  - MigrationStatistics' 
PRINT '  - MigrationErrors'
PRINT '  - TableProcessingStatus'
PRINT '  - QueryValidationResults'
PRINT '  - MigrationConfiguration'
PRINT '  - AuthenticationBackup'
PRINT 'Stored procedures created:'
PRINT '  - sp_GetMigrationStatus'
PRINT '  - sp_CleanupOldMigrations'