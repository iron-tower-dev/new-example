-- FileUploads table for storing file upload information
CREATE TABLE FileUploads (
    Id int IDENTITY(1,1) PRIMARY KEY,
    FileName nvarchar(255) NOT NULL,
    OriginalFileName nvarchar(255) NOT NULL,
    ContentType nvarchar(100) NOT NULL,
    FileSize bigint NOT NULL,
    FilePath nvarchar(500) NOT NULL,
    SampleId int NOT NULL,
    TestId int NOT NULL,
    TrialNumber int NOT NULL,
    UploadDate datetime2 NOT NULL DEFAULT GETUTCDATE(),
    UploadedBy nvarchar(100) NOT NULL,
    Status nvarchar(20) NOT NULL DEFAULT 'Active',
    DeletedBy nvarchar(100) NULL,
    DeletedDate datetime2 NULL,
    Description nvarchar(500) NULL,
    
    -- Indexes for performance
    INDEX IX_FileUploads_Sample_Test (SampleId, TestId),
    INDEX IX_FileUploads_Sample_Test_Trial (SampleId, TestId, TrialNumber),
    INDEX IX_FileUploads_Status (Status),
    INDEX IX_FileUploads_UploadDate (UploadDate)
);

-- Add comments for documentation
EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Stores information about files uploaded for laboratory test samples', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'FileUploads';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Unique identifier for the file upload record', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'FileUploads',
    @level2type = N'COLUMN', @level2name = N'Id';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'System-generated unique filename for storage', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'FileUploads',
    @level2type = N'COLUMN', @level2name = N'FileName';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Original filename as uploaded by user', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'FileUploads',
    @level2type = N'COLUMN', @level2name = N'OriginalFileName';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'MIME content type of the uploaded file', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'FileUploads',
    @level2type = N'COLUMN', @level2name = N'ContentType';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Size of the uploaded file in bytes', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'FileUploads',
    @level2type = N'COLUMN', @level2name = N'FileSize';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Full file system path where the file is stored', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'FileUploads',
    @level2type = N'COLUMN', @level2name = N'FilePath';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Reference to the sample ID from UsedLubeSamples table', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'FileUploads',
    @level2type = N'COLUMN', @level2name = N'SampleId';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Reference to the test ID from Test table', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'FileUploads',
    @level2type = N'COLUMN', @level2name = N'TestId';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Trial number (1-4) that this file is associated with', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'FileUploads',
    @level2type = N'COLUMN', @level2name = N'TrialNumber';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'UTC timestamp when the file was uploaded', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'FileUploads',
    @level2type = N'COLUMN', @level2name = N'UploadDate';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'User ID or name of the person who uploaded the file', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'FileUploads',
    @level2type = N'COLUMN', @level2name = N'UploadedBy';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Status of the file: Active, Deleted, Archived', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'FileUploads',
    @level2type = N'COLUMN', @level2name = N'Status';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'User ID or name of the person who deleted the file', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'FileUploads',
    @level2type = N'COLUMN', @level2name = N'DeletedBy';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'UTC timestamp when the file was deleted', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'FileUploads',
    @level2type = N'COLUMN', @level2name = N'DeletedDate';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Optional description or notes about the uploaded file', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'FileUploads',
    @level2type = N'COLUMN', @level2name = N'Description';