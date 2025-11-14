USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwSampleReleaseMisc]    Script Date: 10/28/2025 11:34:21 AM ******/
DROP VIEW [dbo].[vwSampleReleaseMisc]
GO

/****** Object:  View [dbo].[vwSampleReleaseMisc]    Script Date: 10/28/2025 11:34:21 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vwSampleReleaseMisc]
AS
SELECT     TOP 100 PERCENT dbo.TestReadings.sampleID, dbo.TestReadings.testID, dbo.TestResultsForSampleSummary(dbo.TestReadings.sampleID, 
                      dbo.TestReadings.testID, dbo.TestReadings.trialNumber) AS Result, dbo.TestReadings.trialNumber AS Trial, dbo.TestReadings.ID1, 
                      dbo.TestReadings.ID2, dbo.TestReadings.ID3, dbo.TestReadings.entryID, dbo.TestReadings.validateID, dbo.TestReadings.status AS TestStatus, 
                      dbo.TestStatusText(dbo.TestReadings.status) AS TestStatusText, dbo.Test.name AS TestName, dbo.Test.abbrev AS TestAbbrev, 
                      dbo.Test.ShortAbbrev
FROM         dbo.TestReadings INNER JOIN
                      dbo.Test ON dbo.TestReadings.testID = dbo.Test.ID
WHERE     (dbo.TestReadings.testID IN (280, 281, 282, 283))
ORDER BY dbo.TestReadings.testID, dbo.TestReadings.trialNumber
GO

