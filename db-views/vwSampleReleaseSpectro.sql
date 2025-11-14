USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwSampleReleaseSpectro]    Script Date: 10/28/2025 11:35:20 AM ******/
DROP VIEW [dbo].[vwSampleReleaseSpectro]
GO

/****** Object:  View [dbo].[vwSampleReleaseSpectro]    Script Date: 10/28/2025 11:35:20 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vwSampleReleaseSpectro]
AS
SELECT DISTINCT 
                      dbo.TestReadings.sampleID, dbo.TestReadings.testID, dbo.TestStatusText(dbo.TestReadings.status) AS TestStatusText, 
                      dbo.EmSpectro.trialNum AS Trial, dbo.vwTestsForLab.TestName, dbo.vwTestsForLab.TestAbbrev, dbo.vwTestsForLab.ShortAbbrev, dbo.EmSpectro.Na, 
                      dbo.EmSpectro.Mo, dbo.EmSpectro.Mg, dbo.EmSpectro.P, dbo.EmSpectro.B, dbo.EmSpectro.Cr, dbo.EmSpectro.Ca, dbo.EmSpectro.Ni, 
                      dbo.EmSpectro.Ag, dbo.EmSpectro.Cu, dbo.EmSpectro.Sn, dbo.EmSpectro.Al, dbo.EmSpectro.Mn, dbo.EmSpectro.Pb, dbo.EmSpectro.Fe, 
                      dbo.EmSpectro.Si, dbo.EmSpectro.Ba, dbo.EmSpectro.Zn, dbo.EmSpectro.H, dbo.TestReadings.entryID, dbo.TestReadings.validateID
FROM         dbo.TestReadings INNER JOIN
                      dbo.vwTestsForLab ON dbo.TestReadings.testID = dbo.vwTestsForLab.TestID LEFT OUTER JOIN
                      dbo.EmSpectro ON dbo.TestReadings.testID = dbo.EmSpectro.testID AND dbo.TestReadings.sampleID = dbo.EmSpectro.ID
WHERE     (dbo.TestReadings.testID = 30) OR
                      (dbo.TestReadings.testID = 40)
GO

