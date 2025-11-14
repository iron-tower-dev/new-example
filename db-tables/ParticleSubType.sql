USE [LabResultsDb]
GO

/****** Object:  Table [dbo].[ParticleSubType]    Script Date: 10/7/2025 11:06:36 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ParticleSubType](
	[SampleID] [int] NOT NULL,
	[testID] [smallint] NOT NULL,
	[ParticleTypeDefinitionID] [int] NOT NULL,
	[ParticleSubTypeCategoryID] [int] NOT NULL,
	[Value] [int] NULL
) ON [PRIMARY]
GO

