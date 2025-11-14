USE [LabResultsDb]
GO

/****** Object:  Table [dbo].[lcde_limits]    Script Date: 10/7/2025 10:57:43 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[lcde_limits](
	[sample_id] [int] NOT NULL,
	[testid] [smallint] NOT NULL,
	[testname] [char](20) NULL,
	[lcde] [char](1) NOT NULL,
	[exclude] [char](1) NULL,
	[excluded_on] [datetime] NULL,
	[llim3] [real] NULL,
	[ulim3] [real] NULL,
	[llim2] [real] NULL,
	[ulim2] [real] NULL,
	[llim1] [real] NULL,
	[ulim1] [real] NULL,
	[tlcde] [char](1) NULL,
	[tl1] [char](1) NULL,
	[tl2] [char](1) NULL,
	[tl3] [char](1) NULL,
	[tu1] [char](1) NULL,
	[tu2] [char](1) NULL,
	[tu3] [char](1) NULL,
	[llcde] [char](1) NULL,
	[ll1] [char](1) NULL,
	[ll2] [char](1) NULL,
	[ll3] [char](1) NULL,
	[lu1] [char](1) NULL,
	[lu2] [char](1) NULL,
	[lu3] [char](1) NULL,
	[flcde] [char](1) NULL,
	[glcde] [char](1) NULL,
	[gl1] [smallint] NULL,
	[gl2] [smallint] NULL,
	[gl3] [smallint] NULL,
	[gu1] [smallint] NULL,
	[gu2] [smallint] NULL,
	[gu3] [smallint] NULL
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[lcde_limits] ADD  CONSTRAINT [DF_lcde_limits_lcde]  DEFAULT ('Y') FOR [lcde]
GO

