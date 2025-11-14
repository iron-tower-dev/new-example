USE [LabResultsDb]
GO

/****** Object:  Table [dbo].[rheometer]    Script Date: 10/7/2025 11:08:37 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[rheometer](
	[SampleID] [int] NOT NULL,
	[testtype] [smallint] NOT NULL,
	[processed] [nvarchar](1) NOT NULL,
	[value1] [float] NULL,
	[value2] [float] NULL,
	[value3] [float] NULL,
	[value4] [float] NULL,
	[value5] [float] NULL,
	[value6] [float] NULL,
	[value7] [float] NULL,
	[value8] [float] NULL,
	[value9] [float] NULL,
	[value10] [float] NULL,
	[value11] [float] NULL,
	[value12] [float] NULL,
	[value13] [float] NULL
) ON [PRIMARY]
GO

