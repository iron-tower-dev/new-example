USE [LabResultsDb]
GO

/****** Object:  Table [dbo].[Comments]    Script Date: 10/7/2025 10:54:42 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Comments](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[area] [nvarchar](4) NOT NULL,
	[type] [nvarchar](5) NULL,
	[remark] [nvarchar](80) NOT NULL
) ON [PRIMARY]
GO

