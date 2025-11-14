-- Create Tables Script for Lab Results Database
-- This script creates the essential tables for the lab results system

USE LabResultsDb;
GO

PRINT 'Creating tables for Lab Results Database...';
GO

-- Create UsedLubeSamples table (main samples table)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UsedLubeSamples' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[UsedLubeSamples](
        [ID] [int] IDENTITY(1,1) NOT NULL,
        [TagNumber] [nvarchar](50) NOT NULL,
        [Component] [nvarchar](100) NULL,
        [Location] [nvarchar](100) NULL,
        [LubeType] [nvarchar](50) NULL,
        [QualityClass] [nvarchar](20) NULL,
        [SampleDate] [datetime] NOT NULL,
        [Status] [nvarchar](20) NOT NULL DEFAULT 'Pending',
        [CreatedDate] [datetime] NOT NULL DEFAULT GETDATE(),
        [ModifiedDate] [datetime] NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_UsedLubeSamples] PRIMARY KEY CLUSTERED ([ID] ASC)
    );
    PRINT 'Created UsedLubeSamples table.';
END
GO

-- Create Test table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Test' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[Test](
        [TestID] [int] IDENTITY(1,1) NOT NULL,
        [TestName] [nvarchar](100) NOT NULL,
        [TestDescription] [nvarchar](500) NULL,
        [Active] [bit] NOT NULL DEFAULT 1,
        [CreatedDate] [datetime] NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_Test] PRIMARY KEY CLUSTERED ([TestID] ASC)
    );
    PRINT 'Created Test table.';
END
GO

-- Create M_And_T_Equip table (MTE Equipment)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='M_And_T_Equip' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[M_And_T_Equip](
        [ID] [int] IDENTITY(1,1) NOT NULL,
        [EquipType] [nvarchar](30) NOT NULL,
        [EquipName] [nvarchar](30) NULL,
        [exclude] [bit] NULL,
        [testID] [smallint] NULL,
        [DueDate] [datetime] NULL,
        [Comments] [char](250) NULL,
        [Val1] [float] NULL,
        [Val2] [float] NULL,
        [Val3] [float] NULL,
        [Val4] [float] NULL,
        CONSTRAINT [PK_M_And_T_Equip] PRIMARY KEY CLUSTERED ([ID] ASC)
    );
    PRINT 'Created M_And_T_Equip table.';
END
GO

-- Create TestReadings table (keyless entity for test results)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TestReadings' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[TestReadings](
        [sampleID] [int] NULL,
        [testID] [smallint] NULL,
        [trialNumber] [smallint] NULL,
        [value1] [float] NULL,
        [value2] [float] NULL,
        [value3] [float] NULL,
        [trialCalc] [float] NULL,
        [ID1] [nvarchar](30) NULL,
        [ID2] [nvarchar](30) NULL,
        [ID3] [nvarchar](30) NULL,
        [trialComplete] [bit] NULL,
        [status] [nvarchar](1) NULL,
        [schedType] [char](10) NULL,
        [entryID] [nvarchar](5) NULL,
        [validateID] [nvarchar](5) NULL,
        [entryDate] [datetime] NULL,
        [valiDate] [datetime] NULL,
        [MainComments] [nvarchar](1000) NULL
    );
    PRINT 'Created TestReadings table.';
END
GO

-- Create EmSpectro table (keyless entity for emission spectroscopy)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='EmSpectro' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[EmSpectro](
        [ID] [int] NULL,
        [testID] [smallint] NULL,
        [Na] [float] NULL,
        [Mo] [float] NULL,
        [Mg] [float] NULL,
        [P] [float] NULL,
        [B] [float] NULL,
        [H] [float] NULL,
        [Cr] [float] NULL,
        [Ca] [float] NULL,
        [Ni] [float] NULL,
        [Ag] [float] NULL,
        [Cu] [float] NULL,
        [Sn] [float] NULL,
        [Al] [float] NULL,
        [Mn] [float] NULL,
        [Pb] [float] NULL,
        [Fe] [float] NULL,
        [Si] [float] NULL,
        [Ba] [float] NULL,
        [Sb] [float] NULL,
        [Zn] [float] NULL,
        [trialDate] [datetime] NULL,
        [trialNum] [smallint] NULL
    );
    PRINT 'Created EmSpectro table.';
END
GO

-- Create indexes for better performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UsedLubeSamples_TagNumber')
BEGIN
    CREATE INDEX IX_UsedLubeSamples_TagNumber ON UsedLubeSamples(TagNumber);
    PRINT 'Created index on UsedLubeSamples.TagNumber.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TestReadings_SampleID_TestID')
BEGIN
    CREATE INDEX IX_TestReadings_SampleID_TestID ON TestReadings(SampleID, TestID);
    PRINT 'Created index on TestReadings(SampleID, TestID).';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_M_And_T_Equip_EquipType')
BEGIN
    CREATE INDEX IX_M_And_T_Equip_EquipType ON M_And_T_Equip(EquipType);
    PRINT 'Created index on M_And_T_Equip.EquipType.';
END
GO

PRINT 'Table creation completed successfully!';
GO