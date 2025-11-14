USE [LabResultsDb]
GO

/****** Object:  Table [dbo].[Test]    Script Date: 10/7/2025 11:10:14 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Test](
	[ID] [smallint] NULL,
	[name] [nvarchar](40) NULL,
	[testStandID] [smallint] NULL,
	[sampleVolumeRequired] [smallint] NULL,
	[exclude] [char](1) NULL,
	[abbrev] [char](12) NULL,
	[displayGroupId] [smallint] NULL,
	[groupname] [char](30) NULL,
	[Lab] [bit] NULL,
	[Schedule] [bit] NULL,
	[ShortAbbrev] [nvarchar](6) NULL
) ON [PRIMARY]
GO

