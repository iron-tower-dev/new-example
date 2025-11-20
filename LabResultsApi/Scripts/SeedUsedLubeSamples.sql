-- Seed script for UsedLubeSamples table
-- This script ensures the table exists and populates it with data from UsedLubeSamples.csv

USE [LabResultsDb]
GO

-- Check if table exists, if not create it
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'UsedLubeSamples')
BEGIN
    PRINT 'Creating UsedLubeSamples table...'
    
    CREATE TABLE [dbo].[UsedLubeSamples](
        [ID] [int] NOT NULL PRIMARY KEY,
        [tagNumber] [nvarchar](22) NULL,
        [component] [nvarchar](3) NULL,
        [location] [nvarchar](3) NULL,
        [lubeType] [nvarchar](30) NULL,
        [woNumber] [nvarchar](16) NULL,
        [trackingNumber] [nvarchar](12) NULL,
        [warehouseId] [nvarchar](10) NULL,
        [batchNumber] [nvarchar](30) NULL,
        [classItem] [nvarchar](10) NULL,
        [sampleDate] [datetime] NULL,
        [receivedOn] [datetime] NULL,
        [sampledBy] [nvarchar](50) NULL,
        [status] [smallint] NULL,
        [cmptSelectFlag] [tinyint] NULL,
        [newUsedFlag] [tinyint] NULL,
        [entryId] [nvarchar](5) NULL,
        [validateId] [nvarchar](5) NULL,
        [testPricesId] [smallint] NULL,
        [pricingPackageId] [smallint] NULL,
        [evaluation] [tinyint] NULL,
        [siteId] [int] NULL,
        [results_review_date] [datetime] NULL,
        [results_avail_date] [datetime] NULL,
        [results_reviewId] [nvarchar](5) NULL,
        [storeSource] [nvarchar](100) NULL,
        [schedule] [nvarchar](1) NULL,
        [returnedDate] [datetime] NULL
    ) ON [PRIMARY]
    
    PRINT 'UsedLubeSamples table created successfully.'
END
ELSE
BEGIN
    PRINT 'UsedLubeSamples table already exists.'
END
GO

-- Clear existing data (optional - comment out if you want to preserve existing data)
PRINT 'Clearing existing data from UsedLubeSamples table...'
DELETE FROM UsedLubeSamples;
GO

-- Bulk insert data from CSV file
PRINT 'Importing data from UsedLubeSamples.csv...'

BULK INSERT UsedLubeSamples
FROM '/home/derrick/projects/testing/new-example/db-seeding/UsedLubeSamples.csv'
WITH (
    FIRSTROW = 2,  -- Skip header row
    FIELDTERMINATOR = ',',
    ROWTERMINATOR = '\n',
    TABLOCK,
    KEEPNULLS,
    ERRORFILE = '/tmp/UsedLubeSamples_errors.txt'
);
GO

-- Verify the data was inserted correctly
PRINT 'Verifying data import...'
SELECT 
    COUNT(*) as TotalRecords,
    MIN(sampleDate) as EarliestSample,
    MAX(sampleDate) as LatestSample,
    COUNT(DISTINCT tagNumber) as UniqueTagNumbers
FROM UsedLubeSamples;
GO

-- Show sample records
PRINT 'Sample records from UsedLubeSamples:'
SELECT TOP 10 
    ID,
    tagNumber,
    component,
    location,
    lubeType,
    sampleDate,
    status
FROM UsedLubeSamples
ORDER BY ID;
GO

PRINT 'UsedLubeSamples seeding complete!'
GO