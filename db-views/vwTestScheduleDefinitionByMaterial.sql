USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwTestScheduleDefinitionByMaterial]    Script Date: 10/28/2025 11:40:22 AM ******/
DROP VIEW [dbo].[vwTestScheduleDefinitionByMaterial]
GO

/****** Object:  View [dbo].[vwTestScheduleDefinitionByMaterial]    Script Date: 10/28/2025 11:40:22 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwTestScheduleDefinitionByMaterial]
AS
SELECT DISTINCT 
                      TOP 100 PERCENT dbo.vwTestScheduleDefinition.TestScheduleID, dbo.vwTestScheduleDefinition.TestInterval, 
                      dbo.vwTestScheduleDefinition.MinimumInterval, dbo.vwTestScheduleDefinition.DuringMonth, dbo.vwTestScheduleDefinition.Details, 
                      dbo.vwLabSampleMaterials.TestID, dbo.vwLabSampleMaterials.TestName, dbo.vwLabSampleMaterials.TestAbbrev, 
                      dbo.vwLabSampleMaterials.ShortAbbrev, dbo.vwLabSampleMaterials.Material
FROM         dbo.vwTestScheduleDefinition RIGHT OUTER JOIN
                      dbo.vwLabSampleMaterials ON dbo.vwTestScheduleDefinition.TestID = dbo.vwLabSampleMaterials.TestID AND 
                      dbo.vwTestScheduleDefinition.Material = dbo.vwLabSampleMaterials.Material

GO

