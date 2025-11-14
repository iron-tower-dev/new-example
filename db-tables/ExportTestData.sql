USE [LabResultsDb]
GO

/****** Object:  Table [dbo].[ExportTestData]    Script Date: 10/7/2025 10:56:28 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ExportTestData](
	[group_name] [varchar](40) NOT NULL,
	[valueat] [int] NOT NULL,
	[tagNumber] [varchar](22) NOT NULL,
	[component] [varchar](50) NOT NULL,
	[location] [varchar](50) NOT NULL,
	[sampleid] [int] NOT NULL,
	[sampledate] [datetime] NOT NULL,
	[testID] [smallint] NULL,
	[TestType] [varchar](40) NULL,
	[Val1] [numeric](38, 2) NULL
) ON [PRIMARY]
GO

