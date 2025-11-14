USE [LabResultsDb]
GO

/****** Object:  Table [dbo].[System_Log]    Script Date: 10/7/2025 11:10:01 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[System_Log](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[EntryDate] [datetime] NULL,
	[EntryComment] [varchar](250) NULL
) ON [PRIMARY]
GO

