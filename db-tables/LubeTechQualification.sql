USE [LabResultsDb]
GO

/****** Object:  Table [dbo].[LubeTechQualification]    Script Date: 10/7/2025 11:00:34 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[LubeTechQualification](
	[employeeID] [nvarchar](5) NULL,
	[testStandID] [smallint] NULL,
	[testStand] [nvarchar](50) NULL,
	[qualificationLevel] [nvarchar](10) NULL
) ON [PRIMARY]
GO

