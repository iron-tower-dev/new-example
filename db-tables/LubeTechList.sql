USE [LabResultsDb]
GO

/****** Object:  Table [dbo].[LubeTechList]    Script Date: 10/7/2025 11:00:13 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[LubeTechList](
	[employeeID] [nvarchar](5) NULL,
	[lastName] [nvarchar](22) NULL,
	[firstName] [nvarchar](14) NULL,
	[MI] [nvarchar](1) NULL,
	[qualificationPassword] [nvarchar](8) NULL
) ON [PRIMARY]
GO

