USE [LabResultsDb]
GO

/****** Object:  Table [dbo].[LookupList]    Script Date: 10/7/2025 10:59:28 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[LookupList](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Type] [nvarchar](10) NOT NULL,
	[Description] [nvarchar](50) NOT NULL,
	[SortOrder] [int] NULL
) ON [PRIMARY]
GO

