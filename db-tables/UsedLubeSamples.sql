USE [LabResultsDb]
GO

/****** Object:  Table [dbo].[UsedLubeSamples]    Script Date: 10/7/2025 10:53:16 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[UsedLubeSamples](
	[ID] [int] NOT NULL,
	[tagNumber] [nvarchar](22) NULL,
	[component] [nvarchar](3) NULL,
	[location] [nvarchar](3) NULL,
	[lubeType] [nvarchar](30) NULL,
	[woNumber] [nvarchar](16) NULL,
	[trackingNumber] [nvarchar](12) NULL,
	[warehouseId] [nvarchar](10) NULL,
	[batchNumber] [nvarchar](30) NULL,
	[classItem] [nvarchar](10) NULL,
	[sampleDate] [datetime] NULL,
	[receivedOn] [datetime] NULL,
	[sampledBy] [nvarchar](50) NULL,
	[status] [smallint] NULL,
	[cmptSelectFlag] [tinyint] NULL,
	[newUsedFlag] [tinyint] NULL,
	[entryId] [nvarchar](5) NULL,
	[validateId] [nvarchar](5) NULL,
	[testPricesId] [smallint] NULL,
	[pricingPackageId] [smallint] NULL,
	[evaluation] [tinyint] NULL,
	[siteId] [int] NULL,
	[results_review_date] [datetime] NULL,
	[results_avail_date] [datetime] NULL,
	[results_reviewId] [nvarchar](5) NULL,
	[storeSource] [nvarchar](100) NULL,
	[schedule] [nvarchar](1) NULL,
	[returnedDate] [datetime] NULL
) ON [PRIMARY]
GO

