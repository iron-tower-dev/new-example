USE [LabResultsDb]
GO

/****** Object:  Table [dbo].[lcde_t]    Script Date: 10/7/2025 10:57:58 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[lcde_t](
	[eq_tag_num] [varchar](22) NOT NULL,
	[lube_component_code] [char](3) NOT NULL,
	[lube_location_code] [char](3) NOT NULL,
	[lcde_id] [char](12) NOT NULL,
	[sample_id] [int] NOT NULL,
	[disposition] [tinyint] NULL,
	[wr_num] [nvarchar](16) NULL,
	[comments] [text] NULL,
	[completed_date] [datetime] NULL,
	[enterID] [char](10) NULL,
	[reviewID] [char](10) NULL,
	[PDFFile] [char](250) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[lcde_t] ADD  CONSTRAINT [DF__lcde_t__enterID__2CBDA3B5]  DEFAULT (0) FOR [enterID]
GO

ALTER TABLE [dbo].[lcde_t] ADD  CONSTRAINT [DF__lcde_t__reviewID__2DB1C7EE]  DEFAULT (0) FOR [reviewID]
GO

