USE [LabResultsDb]
GO

/****** Object:  Table [dbo].[ParticleType]    Script Date: 10/7/2025 11:07:34 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ParticleType](
	[SampleID] [int] NOT NULL,
	[testID] [smallint] NOT NULL,
	[ParticleTypeDefinitionID] [int] NOT NULL,
	[Status] [nvarchar](20) NULL,
	[Comments] [nvarchar](500) NULL
) ON [PRIMARY]
GO

