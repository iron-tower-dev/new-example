USE [LabResultsDb]
GO

/****** Object:  Table [dbo].[M_And_T_Equip]    Script Date: 10/7/2025 11:01:46 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[M_And_T_Equip](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[EquipType] [nvarchar](30) NOT NULL,
	[EquipName] [nvarchar](30) NULL,
	[exclude] [bit] NULL,
	[testID] [smallint] NULL,
	[DueDate] [datetime] NULL,
	[Comments] [char](250) NULL,
	[Val1] [float] NULL,
	[Val2] [float] NULL,
	[Val3] [float] NULL,
	[Val4] [float] NULL
) ON [PRIMARY]
GO

