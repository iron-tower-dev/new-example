USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwSampleReleaseResults]    Script Date: 10/28/2025 11:35:03 AM ******/
DROP VIEW [dbo].[vwSampleReleaseResults]
GO

/****** Object:  View [dbo].[vwSampleReleaseResults]    Script Date: 10/28/2025 11:35:03 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vwSampleReleaseResults]
AS
SELECT     TOP 100 PERCENT dbo.TestReadings.sampleID, dbo.TestReadings.testID, dbo.TestResultsForSampleSummary(dbo.TestReadings.sampleID, 
                      dbo.TestReadings.testID, dbo.TestReadings.trialNumber) AS Result, dbo.TestReadings.trialNumber AS Trial, dbo.TestReadings.ID1, 
                      dbo.TestReadings.ID2, dbo.TestReadings.ID3, dbo.TestReadings.entryID, dbo.TestReadings.validateID, dbo.TestReadings.status AS TestStatus, 
                      dbo.TestStatusText(dbo.TestReadings.status) AS TestStatusText, dbo.vwTestsForLab.TestName, dbo.vwTestsForLab.TestAbbrev, 
                      dbo.vwTestsForLab.ShortAbbrev
FROM         dbo.TestReadings INNER JOIN
                      dbo.vwTestsForLab ON dbo.TestReadings.testID = dbo.vwTestsForLab.TestID
WHERE     (NOT (dbo.TestReadings.testID IN (30, 40, 70, 120, 160, 210, 240)))
ORDER BY dbo.TestReadings.testID, dbo.TestReadings.trialNumber
GO

