USE [LabResultsDb]
GO

/****** Object:  Table [dbo].[eq_lubrication_pt_t]    Script Date: 10/7/2025 10:56:03 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[eq_lubrication_pt_t](
	[eq_tag_num] [varchar](22) NOT NULL,
	[lube_component_code] [char](3) NOT NULL,
	[lube_location_code] [char](3) NOT NULL,
	[applid] [int] NOT NULL,
	[lube_part_order_num] [varchar](30) NOT NULL,
	[lube_qty_reqd] [varchar](5) NULL,
	[lube_unit] [char](3) NULL,
	[in_pgm_indicator] [char](1) NULL,
	[pgm_status_code] [char](1) NULL,
	[lcde_reqd] [char](1) NULL,
	[first_lcde_freq] [char](1) NULL,
	[first_lcde_freq_intvl] [smallint] NULL,
	[next_lcde_override_freq] [char](1) NULL,
	[next_lcde_override_freq_intvl] [smallint] NULL,
	[sample_window_freq] [char](1) NULL,
	[sample_window_freq_intvl] [smallint] NULL,
	[last_lcde_date] [datetime] NULL,
	[next_lcde_date] [datetime] NULL,
	[sample_plug_installed] [char](1) NULL,
	[eq_group_id] [smallint] NULL,
	[last_sample_date] [datetime] NULL,
	[last_change_date] [datetime] NULL,
	[material_info] [text] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

