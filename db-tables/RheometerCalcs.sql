USE [LabResultsDb]
GO

/****** Object:  Table [dbo].[RheometerCalcs]    Script Date: 10/7/2025 11:09:00 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[RheometerCalcs](
	[SampleID] [int] NOT NULL,
	[TestType] [smallint] NOT NULL,
	[Calc1] [float] NULL,
	[Calc2] [float] NULL,
	[Calc3] [float] NULL,
	[Calc4] [float] NULL,
	[Calc5] [float] NULL,
	[Calc6] [float] NULL,
	[Calc7] [float] NULL,
	[Calc8] [float] NULL
) ON [PRIMARY]
GO

