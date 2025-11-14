USE [LabResultsDb]
GO

/****** Object:  Table [dbo].[Component]    Script Date: 10/7/2025 10:55:08 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Component](
	[site_id] [int] NOT NULL,
	[code] [nvarchar](3) NULL,
	[name] [nvarchar](30) NULL
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Component] ADD  CONSTRAINT [DF__Component__site___4865BE2A]  DEFAULT (1) FOR [site_id]
GO

