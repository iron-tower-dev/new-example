USE [LabResultsDb]
GO

/****** Object:  Table [dbo].[workmgmt]    Script Date: 10/7/2025 11:12:22 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[workmgmt](
	[entrydate] [datetime] NOT NULL,
	[days2] [int] NOT NULL,
	[days3_7] [int] NOT NULL,
	[days8_14] [int] NOT NULL,
	[days15_30] [int] NOT NULL,
	[days_over_30] [int] NOT NULL,
	[pending_total_samples] [int] NULL,
	[released_since_last_entry] [int] NULL,
	[released_ave] [numeric](10, 3) NULL,
	[testing_ave] [numeric](10, 3) NULL,
	[entry_ave] [numeric](10, 3) NULL,
	[fielddays_7_over] [numeric](18, 0) NULL,
	[availforeval] [numeric](18, 0) NULL,
	[open_ave] [numeric](18, 0) NULL,
	[comp_ave] [numeric](18, 0) NULL,
	[returnedTotal] [int] NULL,
	[returnedAve] [numeric](10, 3) NULL
) ON [PRIMARY]
GO

