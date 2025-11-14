USE [LabResultsDb]
GO

/****** Object:  Table [dbo].[EmSpectro]    Script Date: 10/7/2025 10:55:39 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[EmSpectro](
	[ID] [int] NULL,
	[testID] [smallint] NULL,
	[Na] [float] NULL,
	[Mo] [float] NULL,
	[Mg] [float] NULL,
	[P] [float] NULL,
	[B] [float] NULL,
	[H] [float] NULL,
	[Cr] [float] NULL,
	[Ca] [float] NULL,
	[Ni] [float] NULL,
	[Ag] [float] NULL,
	[Cu] [float] NULL,
	[Sn] [float] NULL,
	[Al] [float] NULL,
	[Mn] [float] NULL,
	[Pb] [float] NULL,
	[Fe] [float] NULL,
	[Si] [float] NULL,
	[Ba] [float] NULL,
	[Sb] [float] NULL,
	[Zn] [float] NULL,
	[trialDate] [datetime] NULL,
	[trialNum] [smallint] NULL
) ON [PRIMARY]
GO

