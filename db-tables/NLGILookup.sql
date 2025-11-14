USE [LabResultsDb]
GO

/****** Object:  Table [dbo].[NLGILookup]    Script Date: 10/7/2025 11:06:05 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[NLGILookup](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[lowerValue] [int] NOT NULL,
	[upperValue] [int] NOT NULL,
	[NLGIValue] [nvarchar](4) NOT NULL
) ON [PRIMARY]
GO

