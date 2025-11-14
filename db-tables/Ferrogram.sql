USE [LabResultsDb]
GO

/****** Object:  Table [dbo].[Ferrogram]    Script Date: 10/7/2025 10:56:47 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Ferrogram](
	[SampleID] [int] NULL,
	[FerrogramNumber] [nvarchar](20) NULL,
	[DilutionFactor] [nvarchar](20) NULL,
	[NormalRubbing] [real] NULL,
	[SevereWearSliding] [real] NULL,
	[SevereWearFatigue] [real] NULL,
	[CuttingPart] [real] NULL,
	[LaminarPart] [real] NULL,
	[Spheres] [real] NULL,
	[DarkMetalloOde] [real] NULL,
	[RedOde] [real] NULL,
	[CorrosiveWearPart] [real] NULL,
	[NonFerrousMetal] [real] NULL,
	[NonMetallicInorganic] [real] NULL,
	[BirefringentOrganic] [real] NULL,
	[NonMetallicAmporphous] [real] NULL,
	[FrictionPolymers] [real] NULL,
	[Fibers] [real] NULL,
	[Other] [real] NULL,
	[OtherText] [nvarchar](20) NULL,
	[CsdrdJdgWearSit] [real] NULL,
	[Comments] [nvarchar](300) NULL,
	[Magnification] [int] NULL
) ON [PRIMARY]
GO

