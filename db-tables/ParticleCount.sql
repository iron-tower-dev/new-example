USE [LabResultsDb]
GO

/****** Object:  Table [dbo].[ParticleCount]    Script Date: 10/7/2025 11:06:21 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ParticleCount](
	[ID] [int] NULL,
	[micron_5_10] [float] NULL,
	[micron_10_15] [float] NULL,
	[micron_15_25] [float] NULL,
	[micron_25_50] [float] NULL,
	[micron_50_100] [float] NULL,
	[micron_100] [float] NULL,
	[testDate] [datetime] NULL,
	[iso_code] [char](5) NULL,
	[nas_class] [char](2) NULL
) ON [PRIMARY]
GO

