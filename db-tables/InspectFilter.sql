USE [LabResultsDb]
GO

/****** Object:  Table [dbo].[InspectFilter]    Script Date: 10/7/2025 10:57:26 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[InspectFilter](
	[ID] [int] NULL,
	[testID] [smallint] NULL,
	[narrative] [nvarchar](4000) NULL,
	[major] [int] NULL,
	[minor] [int] NULL,
	[trace] [int] NULL
) ON [PRIMARY]
GO

