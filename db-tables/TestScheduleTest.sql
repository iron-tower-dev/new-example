USE [LabResultsDb]
GO

/****** Object:  Table [dbo].[TestScheduleTest]    Script Date: 10/7/2025 11:11:46 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TestScheduleTest](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[TestScheduleID] [int] NOT NULL,
	[TestID] [int] NOT NULL,
	[TestInterval] [tinyint] NULL,
	[MinimumInterval] [tinyint] NULL,
	[DuringMonth] [tinyint] NULL,
	[Details] [nvarchar](50) NULL
) ON [PRIMARY]
GO

