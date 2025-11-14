USE [LabResultsDb]
GO

/****** Object:  Table [dbo].[TestScheduleRule]    Script Date: 10/7/2025 11:11:25 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TestScheduleRule](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[GroupID] [int] NOT NULL,
	[TestID] [int] NOT NULL,
	[RuleTestID] [int] NOT NULL,
	[UpperRule] [bit] NOT NULL,
	[RuleAction] [nvarchar](1) NULL
) ON [PRIMARY]
GO

