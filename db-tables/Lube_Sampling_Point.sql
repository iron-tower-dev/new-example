USE [LabResultsDb]
GO

/****** Object:  Table [dbo].[Lube_Sampling_Point]    Script Date: 10/7/2025 10:59:52 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Lube_Sampling_Point](
	[tagNumber] [nvarchar](22) NULL,
	[component] [nvarchar](3) NULL,
	[location] [nvarchar](3) NULL,
	[lubeClassItemNumber] [nvarchar](10) NULL,
	[lubeQuantityRequired] [float] NULL,
	[lubeUnitsOfMeasure] [nvarchar](3) NULL,
	[testCategory] [nvarchar](1) NULL,
	[qualityClass] [nvarchar](6) NULL,
	[pricingPackageId] [smallint] NULL,
	[testPricesId] [smallint] NULL,
	[lastSampleDate] [datetime] NULL,
	[changeTaskNumber] [nvarchar](6) NULL,
	[changeIntervalType] [nvarchar](1) NULL,
	[changeIntervalNumber] [tinyint] NULL,
	[lastChangeDate] [datetime] NULL,
	[inProgram] [bit] NULL,
	[testsScheduled] [bit] NULL,
	[applid] [int] NULL,
	[material_info] [nvarchar](500) NULL
) ON [PRIMARY]
GO

