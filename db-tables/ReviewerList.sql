USE [LabResultsDb]
GO

/****** Object:  Table [dbo].[ReviewerList]    Script Date: 10/7/2025 11:08:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ReviewerList](
	[employeeID] [char](5) NOT NULL,
	[lastName] [char](22) NOT NULL,
	[firstName] [char](14) NOT NULL,
	[MI] [char](1) NULL,
	[reviewerPassword] [char](8) NULL,
	[level] [smallint] NOT NULL
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ReviewerList] ADD  CONSTRAINT [DF_ReviewerList_level]  DEFAULT (1) FOR [level]
GO

