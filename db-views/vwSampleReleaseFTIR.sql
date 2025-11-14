USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwSampleReleaseFTIR]    Script Date: 10/28/2025 11:33:58 AM ******/
DROP VIEW [dbo].[vwSampleReleaseFTIR]
GO

/****** Object:  View [dbo].[vwSampleReleaseFTIR]    Script Date: 10/28/2025 11:33:58 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vwSampleReleaseFTIR]
AS
SELECT     dbo.TestReadings.sampleID, dbo.TestReadings.testID, dbo.TestStatusText(dbo.TestReadings.status) AS TestStatusText, 
                      dbo.vwTestsForLab.TestName, dbo.vwTestsForLab.TestAbbrev, dbo.vwTestsForLab.ShortAbbrev, dbo.TestReadings.entryID, 
                      dbo.TestReadings.validateID, dbo.FTIR.anti_oxidant, dbo.FTIR.oxidation, dbo.FTIR.H2O, dbo.FTIR.zddp, dbo.FTIR.soot, dbo.FTIR.fuel_dilution, 
                      dbo.FTIR.mixture, dbo.FTIR.NLGI, dbo.FTIR.contam
FROM         dbo.TestReadings INNER JOIN
                      dbo.vwTestsForLab ON dbo.TestReadings.testID = dbo.vwTestsForLab.TestID LEFT OUTER JOIN
                      dbo.FTIR ON dbo.TestReadings.sampleID = dbo.FTIR.sampleID
WHERE     (dbo.TestReadings.testID = 70)
GO

