USE [LabResultsDb]
GO

/****** Object:  Table [dbo].[site]    Script Date: 10/7/2025 11:09:34 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[site](
	[site_id] [int] NOT NULL,
	[name] [char](30) NOT NULL,
	[abbrev] [char](10) NULL,
	[contact1] [char](50) NULL,
	[contact2] [char](50) NULL,
	[contact3] [char](50) NULL,
	[exclude] [char](1) NULL
) ON [PRIMARY]
GO

