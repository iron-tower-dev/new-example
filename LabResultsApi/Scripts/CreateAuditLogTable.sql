-- Create AuditLog table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AuditLog' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[AuditLog] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [EmployeeId] nvarchar(50) NOT NULL,
        [Action] nvarchar(100) NOT NULL,
        [EntityType] nvarchar(100) NOT NULL,
        [EntityId] nvarchar(50) NULL,
        [OldValues] nvarchar(max) NULL,
        [NewValues] nvarchar(max) NULL,
        [Timestamp] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        [IpAddress] nvarchar(45) NULL,
        [UserAgent] nvarchar(500) NULL,
        [AdditionalInfo] nvarchar(500) NULL,
        CONSTRAINT [PK_AuditLog] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    
    -- Create indexes for better query performance
    CREATE NONCLUSTERED INDEX [IX_AuditLog_EmployeeId] ON [dbo].[AuditLog] ([EmployeeId]);
    CREATE NONCLUSTERED INDEX [IX_AuditLog_Timestamp] ON [dbo].[AuditLog] ([Timestamp] DESC);
    CREATE NONCLUSTERED INDEX [IX_AuditLog_EntityType] ON [dbo].[AuditLog] ([EntityType]);
    CREATE NONCLUSTERED INDEX [IX_AuditLog_Action] ON [dbo].[AuditLog] ([Action]);
    
    PRINT 'AuditLog table created successfully with indexes';
END
ELSE
BEGIN
    PRINT 'AuditLog table already exists';
END

-- Verify the table structure
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'AuditLog'
ORDER BY ORDINAL_POSITION;