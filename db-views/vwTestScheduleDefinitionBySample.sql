USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwTestScheduleDefinitionBySample]    Script Date: 10/28/2025 11:40:37 AM ******/
DROP VIEW [dbo].[vwTestScheduleDefinitionBySample]
GO

/****** Object:  View [dbo].[vwTestScheduleDefinitionBySample]    Script Date: 10/28/2025 11:40:37 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwTestScheduleDefinitionBySample]
AS
SELECT DISTINCT 
                      TOP 100 PERCENT dbo.vwLabSampleTests.sampleID, dbo.vwLabSampleTests.TestID, dbo.vwLabSampleTests.TestName, 
                      dbo.vwLabSampleTests.TestAbbrev, dbo.vwLabSampleTests.tagNumber AS Tag, dbo.vwLabSampleTests.component AS ComponentCode, 
                      dbo.vwLabSampleTests.location AS LocationCode, dbo.vwTestScheduleDefinition.TestScheduleID, dbo.vwTestScheduleDefinition.Material, 
                      dbo.vwTestScheduleDefinition.TestInterval, dbo.vwTestScheduleDefinition.MinimumInterval, dbo.vwTestScheduleDefinition.DuringMonth, 
                      dbo.vwTestScheduleDefinition.Details
FROM         dbo.vwLabSampleTests LEFT OUTER JOIN
                      dbo.vwTestScheduleDefinition ON dbo.vwLabSampleTests.TestID = dbo.vwTestScheduleDefinition.TestID AND 
                      dbo.vwLabSampleTests.tagNumber = dbo.vwTestScheduleDefinition.Tag AND 
                      dbo.vwLabSampleTests.component = dbo.vwTestScheduleDefinition.ComponentCode AND 
                      dbo.vwLabSampleTests.location = dbo.vwTestScheduleDefinition.LocationCode
ORDER BY dbo.vwLabSampleTests.TestID


GO

