USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwLabSampleMaterials]    Script Date: 10/28/2025 11:21:27 AM ******/
DROP VIEW [dbo].[vwLabSampleMaterials]
GO

/****** Object:  View [dbo].[vwLabSampleMaterials]    Script Date: 10/28/2025 11:21:27 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwLabSampleMaterials]
AS
SELECT DISTINCT 
                      dbo.UsedLubeSamples.ID AS sampleID, dbo.vwTestsForLab.TestID, dbo.vwTestsForLab.TestName, dbo.vwTestsForLab.TestAbbrev, 
                      dbo.vwTestsForLab.ShortAbbrev, dbo.UsedLubeSamples.lubeType AS Material
FROM         dbo.UsedLubeSamples CROSS JOIN
                      dbo.vwTestsForLab

GO

