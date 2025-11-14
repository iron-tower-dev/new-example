USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwScheduledSamplesByTest]    Script Date: 10/28/2025 11:36:04 AM ******/
DROP VIEW [dbo].[vwScheduledSamplesByTest]
GO

/****** Object:  View [dbo].[vwScheduledSamplesByTest]    Script Date: 10/28/2025 11:36:04 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE VIEW [dbo].[vwScheduledSamplesByTest]
AS
SELECT     dbo.UsedLubeSamples.ID AS SampleID, dbo.TestReadings.testID
FROM         dbo.TestReadings RIGHT OUTER JOIN
                      dbo.UsedLubeSamples ON dbo.TestReadings.sampleID = dbo.UsedLubeSamples.ID
WHERE     (dbo.TestReadings.trialNumber = 1) AND (dbo.TestReadings.status = 'S')


GO

