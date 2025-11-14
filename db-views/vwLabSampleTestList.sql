USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwLabSampleTestList]    Script Date: 10/28/2025 11:21:44 AM ******/
DROP VIEW [dbo].[vwLabSampleTestList]
GO

/****** Object:  View [dbo].[vwLabSampleTestList]    Script Date: 10/28/2025 11:21:44 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwLabSampleTestList]
AS
SELECT DISTINCT 
                      dbo.vwLabSampleTests.sampleID, dbo.vwLabSampleTests.TestID, dbo.vwLabSampleTests.TestName, dbo.vwLabSampleTests.TestAbbrev, 
                      dbo.vwLabSampleTests.ShortAbbrev, dbo.TestReadings.status AS TestStatusCode, dbo.TestStatusText(dbo.TestReadings.status) AS TestStatus
FROM         dbo.TestReadings RIGHT OUTER JOIN
                      dbo.vwLabSampleTests ON dbo.TestReadings.sampleID = dbo.vwLabSampleTests.sampleID AND 
                      dbo.TestReadings.testID = dbo.vwLabSampleTests.TestID

GO

