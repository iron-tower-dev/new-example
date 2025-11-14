USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwSampleReleasePC]    Script Date: 10/28/2025 11:34:33 AM ******/
DROP VIEW [dbo].[vwSampleReleasePC]
GO

/****** Object:  View [dbo].[vwSampleReleasePC]    Script Date: 10/28/2025 11:34:33 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vwSampleReleasePC]
AS
SELECT     dbo.TestReadings.sampleID, dbo.TestReadings.testID, dbo.TestStatusText(dbo.TestReadings.status) AS TestStatusText, 
                      dbo.vwTestsForLab.TestName, dbo.vwTestsForLab.TestAbbrev, dbo.vwTestsForLab.ShortAbbrev, dbo.TestReadings.entryID, 
                      dbo.TestReadings.validateID, dbo.ParticleCount.micron_5_10, dbo.ParticleCount.micron_10_15, dbo.ParticleCount.micron_15_25, 
                      dbo.ParticleCount.micron_25_50, dbo.ParticleCount.micron_50_100, dbo.ParticleCount.micron_100, dbo.ParticleCount.nas_class
FROM         dbo.TestReadings INNER JOIN
                      dbo.vwTestsForLab ON dbo.TestReadings.testID = dbo.vwTestsForLab.TestID LEFT OUTER JOIN
                      dbo.ParticleCount ON dbo.TestReadings.sampleID = dbo.ParticleCount.ID
WHERE     (dbo.TestReadings.testID = 160)
GO

