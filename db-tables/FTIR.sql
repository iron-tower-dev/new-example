USE [LabResultsDb]
GO

/****** Object:  Table [dbo].[FTIR]    Script Date: 10/7/2025 10:57:13 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[FTIR](
	[sampleID] [int] NOT NULL,
	[anti_oxidant] [float] NULL,
	[oxidation] [float] NULL,
	[H2O] [float] NULL,
	[zddp] [float] NULL,
	[soot] [float] NULL,
	[fuel_dilution] [float] NULL,
	[mixture] [float] NULL,
	[NLGI] [float] NULL,
	[contam] [float] NULL
) ON [PRIMARY]
GO

