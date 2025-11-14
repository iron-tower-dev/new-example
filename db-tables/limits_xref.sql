USE [LabResultsDb]
GO

/****** Object:  Table [dbo].[limits_xref]    Script Date: 10/7/2025 10:58:37 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[limits_xref](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[group_name] [char](40) NOT NULL,
	[valueat] [int] NOT NULL,
	[tagNumber] [varchar](22) NOT NULL,
	[component] [varchar](3) NOT NULL,
	[location] [varchar](3) NOT NULL,
	[exclude] [char](1) NULL,
	[excluded_on] [datetime] NULL,
	[fname] [nvarchar](250) NULL
) ON [PRIMARY]
GO

