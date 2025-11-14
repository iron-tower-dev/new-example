USE [LabResultsDb]
GO

/****** Object:  Table [dbo].[SWMSRecords]    Script Date: 10/7/2025 11:09:48 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[SWMSRecords](
	[tagNumber] [nvarchar](22) NOT NULL,
	[component] [nvarchar](3) NOT NULL,
	[location] [nvarchar](3) NOT NULL,
	[scheduledDate] [datetime] NULL,
	[woNumber] [nvarchar](16) NULL,
	[description] [nvarchar](2000) NULL
) ON [PRIMARY]
GO

