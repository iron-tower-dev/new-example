USE [LabResultsDb]
GO

/****** Object:  Table [dbo].[ParticleTypeDefinition]    Script Date: 10/7/2025 11:07:54 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ParticleTypeDefinition](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Type] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](500) NOT NULL,
	[Image1] [nvarchar](50) NOT NULL,
	[Image2] [nvarchar](50) NOT NULL,
	[Active] [nvarchar](1) NULL,
	[SortOrder] [int] NULL
) ON [PRIMARY]
GO

