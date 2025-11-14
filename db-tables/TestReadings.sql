USE [LabResultsDb]
GO

/****** Object:  Table [dbo].[TestReadings]    Script Date: 10/7/2025 11:10:51 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

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
) ON [PRIMARY]
GO

