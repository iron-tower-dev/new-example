USE [LabResultsDb]
GO

/****** Object:  Table [dbo].[Location]    Script Date: 10/7/2025 10:59:09 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Location](
	[site_id] [int] NOT NULL,
	[code] [nvarchar](3) NULL,
	[name] [nvarchar](30) NULL
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Location] ADD  CONSTRAINT [DF__Location__site_i__4959E263]  DEFAULT (1) FOR [site_id]
GO

