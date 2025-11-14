USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwTestEntryUsers]    Script Date: 10/28/2025 11:37:04 AM ******/
DROP VIEW [dbo].[vwTestEntryUsers]
GO

/****** Object:  View [dbo].[vwTestEntryUsers]    Script Date: 10/28/2025 11:37:04 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwTestEntryUsers]
AS
SELECT DISTINCT 
                      TOP 100 PERCENT dbo.UsedLubeSamples.ID, dbo.UsedLubeSamples.tagNumber, dbo.Component.name AS ComponentName, 
                      dbo.Location.name AS LocationName, dbo.UsedLubeSamples.woNumber, dbo.TestReadings.testID, dbo.Test.abbrev, dbo.TestReadings.entryID, 
                      dbo.TestReadings.entryDate, dbo.Component.code AS ComponentCode, dbo.Location.code AS LocationCode, dbo.TestReadings.validateID
FROM         dbo.UsedLubeSamples INNER JOIN
                      dbo.TestReadings ON dbo.UsedLubeSamples.ID = dbo.TestReadings.sampleID INNER JOIN
                      dbo.Test ON dbo.TestReadings.testID = dbo.Test.testStandID LEFT OUTER JOIN
                      dbo.Location ON dbo.UsedLubeSamples.location = dbo.Location.code LEFT OUTER JOIN
                      dbo.Component ON dbo.UsedLubeSamples.component = dbo.Component.code
WHERE     (NOT (dbo.TestReadings.entryID IS NULL))
ORDER BY dbo.UsedLubeSamples.ID DESC



GO

