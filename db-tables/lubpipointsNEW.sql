USE [LabResultsDb]
GO

/****** Object:  Table [dbo].[lubpipointsNEW]    Script Date: 10/7/2025 11:01:10 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[lubpipointsNEW](
	[PIPOINT] [nvarchar](4000) NOT NULL,
	[eqid] [varchar](22) NOT NULL,
	[comp] [varchar](30) NOT NULL,
	[loc] [varchar](30) NOT NULL,
	[measurement_type] [nvarchar](128) NULL,
	[colval] [varchar](100) NULL,
	[sampledate] [datetime] NULL
) ON [PRIMARY]
GO

