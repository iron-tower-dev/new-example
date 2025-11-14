USE [LabResultsDb]
GO

/****** Object:  Table [dbo].[ParticleSubTypeDefinition]    Script Date: 10/7/2025 11:07:16 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ParticleSubTypeDefinition](
	[ParticleSubTypeCategoryID] [int] NOT NULL,
	[Value] [int] NOT NULL,
	[Description] [nvarchar](50) NOT NULL,
	[Active] [nvarchar](1) NULL,
	[SortOrder] [int] NULL,
 CONSTRAINT [PK_ParticleSubTypeDefinition] PRIMARY KEY CLUSTERED 
(
	[ParticleSubTypeCategoryID] ASC,
	[Value] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

