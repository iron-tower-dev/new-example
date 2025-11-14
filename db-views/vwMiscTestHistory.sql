USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwMiscTestHistory]    Script Date: 10/28/2025 11:24:56 AM ******/
DROP VIEW [dbo].[vwMiscTestHistory]
GO

/****** Object:  View [dbo].[vwMiscTestHistory]    Script Date: 10/28/2025 11:24:56 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwMiscTestHistory]
AS
SELECT DISTINCT 
                      dbo.UsedLubeSamples.ID, dbo.UsedLubeSamples.sampleDate, dbo.MiscTestResult(dbo.TestReadings.sampleID, 280) AS Resistivity, 
                      dbo.MiscTestResult(dbo.TestReadings.sampleID, 281) AS Chlorides, dbo.MiscTestResult(dbo.TestReadings.sampleID, 282) AS Amine, 
                      dbo.MiscTestResult(dbo.TestReadings.sampleID, 283) AS Phenol, dbo.UsedLubeSamples.tagNumber, dbo.UsedLubeSamples.component, 
                      dbo.UsedLubeSamples.location, 1 AS trial
FROM         dbo.TestReadings INNER JOIN
                      dbo.UsedLubeSamples ON dbo.TestReadings.sampleID = dbo.UsedLubeSamples.ID
WHERE     (dbo.TestReadings.testID IN (280, 281, 282, 283))

GO

