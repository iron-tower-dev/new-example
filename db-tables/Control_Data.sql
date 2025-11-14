USE [LabResultsDb]
GO

/****** Object:  Table [dbo].[Control_Data]    Script Date: 10/7/2025 10:55:28 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Control_Data](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](10) NOT NULL,
	[ControlValue] [nvarchar](200) NULL
) ON [PRIMARY]
GO

