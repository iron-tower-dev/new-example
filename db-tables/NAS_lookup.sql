USE [LabResultsDb]
GO

/****** Object:  Table [dbo].[NAS_lookup]    Script Date: 10/7/2025 11:04:51 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[NAS_lookup](
	[Channel] [int] NULL,
	[ValLo] [int] NULL,
	[ValHi] [int] NULL,
	[NAS] [int] NULL
) ON [PRIMARY]
GO

