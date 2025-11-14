USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwParticleTypeDefinition]    Script Date: 10/28/2025 10:58:32 AM ******/
DROP VIEW [dbo].[vwParticleTypeDefinition]
GO

/****** Object:  View [dbo].[vwParticleTypeDefinition]    Script Date: 10/28/2025 10:58:32 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwParticleTypeDefinition]
AS
SELECT        ID, Type, Description, Image1, Image2, Active, SortOrder
FROM            dbo.ParticleTypeDefinition

GO

