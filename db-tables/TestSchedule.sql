USE [LabResultsDb]
GO

/****** Object:  Table [dbo].[TestSchedule]    Script Date: 10/7/2025 11:11:11 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TestSchedule](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Tag] [nvarchar](22) NULL,
	[ComponentCode] [nvarchar](3) NULL,
	[LocationCode] [nvarchar](3) NULL,
	[Material] [nvarchar](30) NULL
) ON [PRIMARY]
GO

