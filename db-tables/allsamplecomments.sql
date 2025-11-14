USE [LabResultsDb]
GO

/****** Object:  Table [dbo].[allsamplecomments]    Script Date: 10/7/2025 10:54:16 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[allsamplecomments](
	[SiteID] [int] NOT NULL,
	[SampleID] [int] NOT NULL,
	[CommentArea] [nvarchar](4) NOT NULL,
	[CommentID] [int] NULL,
	[TestID] [smallint] NULL,
	[Comment] [nvarchar](2000) NOT NULL,
	[CommentDate] [datetime] NULL,
	[UserID] [nvarchar](10) NULL
) ON [PRIMARY]
GO

