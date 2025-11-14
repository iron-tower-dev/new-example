USE [LabResultsDb]
GO

/****** Object:  Table [dbo].[Lubricant]    Script Date: 10/7/2025 11:01:21 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Lubricant](
	[type] [nvarchar](30) NULL,
	[classItemNumber] [nvarchar](10) NULL,
	[cupNumber] [nvarchar](50) NULL,
	[healthHazard] [tinyint] NULL,
	[fireHazard] [tinyint] NULL,
	[specificHazard] [tinyint] NULL,
	[reactivity] [tinyint] NULL,
	[pricingPackageId] [smallint] NULL,
	[tube_size_vis40] [int] NULL,
	[tube_size_vis100] [int] NULL
) ON [PRIMARY]
GO

