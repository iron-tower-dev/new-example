USE [LabResultsDb]
GO

/****** Object:  Table [dbo].[ScheduleDeletions]    Script Date: 10/7/2025 11:09:21 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ScheduleDeletions](
	[SampleID] [int] NOT NULL,
	[TestID] [int] NOT NULL,
	[DeletionDate] [datetime] NULL,
	[Reason] [nvarchar](50) NULL
) ON [PRIMARY]
GO

