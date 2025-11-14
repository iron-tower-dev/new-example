USE [LabResultsDb]
GO

/****** Object:  Table [dbo].[ParticleSubTypeCategoryDefinition]    Script Date: 10/7/2025 11:06:58 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ParticleSubTypeCategoryDefinition](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Description] [nvarchar](50) NOT NULL,
	[Active] [nvarchar](1) NULL,
	[SortOrder] [int] NULL
) ON [PRIMARY]
GO

