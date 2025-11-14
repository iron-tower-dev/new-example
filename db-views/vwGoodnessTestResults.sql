USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwGoodnessTestResults]    Script Date: 10/28/2025 11:20:48 AM ******/
DROP VIEW [dbo].[vwGoodnessTestResults]
GO

/****** Object:  View [dbo].[vwGoodnessTestResults]    Script Date: 10/28/2025 11:20:48 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwGoodnessTestResults]
AS
SELECT     SampleID, testID, '' AS TestType, dbo.TestResultValue(sampleID, testID) AS Result
FROM         dbo.TestReadings
WHERE     (trialNumber = 1) AND (NOT (testID IN (30, 40, 70, 160, 270)))
UNION
SELECT     SampleID, testID, 'Na' AS TestType, Na AS Result
FROM         dbo.vwSpectroscopy
WHERE     (testID IN (30, 40))
UNION
SELECT     SampleID, testID, 'Mo' AS TestType, Mo AS Result
FROM         dbo.vwSpectroscopy
WHERE     (testID IN (30, 40))
UNION
SELECT     SampleID, testID, 'Mg' AS TestType, Mg AS Result
FROM         dbo.vwSpectroscopy
WHERE     (testID IN (30, 40))
UNION
SELECT     SampleID, testID, 'P' AS TestType, P AS Result
FROM         dbo.vwSpectroscopy
WHERE     (testID IN (30, 40))
UNION
SELECT     SampleID, testID, 'B' AS TestType, B AS Result
FROM         dbo.vwSpectroscopy
WHERE     (testID IN (30, 40))
UNION
SELECT     SampleID, testID, 'Cr' AS TestType, Cr AS Result
FROM         dbo.vwSpectroscopy
WHERE     (testID IN (30, 40))
UNION
SELECT     SampleID, testID, 'Ca' AS TestType, Ca AS Result
FROM         dbo.vwSpectroscopy
WHERE     (testID IN (30, 40))
UNION
SELECT     SampleID, testID, 'Ni' AS TestType, Ni AS Result
FROM         dbo.vwSpectroscopy
WHERE     (testID IN (30, 40))
UNION
SELECT     SampleID, testID, 'Ag' AS TestType, Ag AS Result
FROM         dbo.vwSpectroscopy
WHERE     (testID IN (30, 40))
UNION
SELECT     SampleID, testID, 'Cu' AS TestType, Cu AS Result
FROM         dbo.vwSpectroscopy
WHERE     (testID IN (30, 40))
UNION
SELECT     SampleID, testID, 'Sn' AS TestType, Sn AS Result
FROM         dbo.vwSpectroscopy
WHERE     (testID IN (30, 40))
UNION
SELECT     SampleID, testID, 'Al' AS TestType, Al AS Result
FROM         dbo.vwSpectroscopy
WHERE     (testID IN (30, 40))
UNION
SELECT     SampleID, testID, 'Mn' AS TestType, Mn AS Result
FROM         dbo.vwSpectroscopy
WHERE     (testID IN (30, 40))
UNION
SELECT     SampleID, testID, 'Pb' AS TestType, Pb AS Result
FROM         dbo.vwSpectroscopy
WHERE     (testID IN (30, 40))
UNION
SELECT     SampleID, testID, 'Fe' AS TestType, Fe AS Result
FROM         dbo.vwSpectroscopy
WHERE     (testID IN (30, 40))
UNION
SELECT     SampleID, testID, 'Si' AS TestType, Si AS Result
FROM         dbo.vwSpectroscopy
WHERE     (testID IN (30, 40))
UNION
SELECT     SampleID, testID, 'Ba' AS TestType, Ba AS Result
FROM         dbo.vwSpectroscopy
WHERE     (testID IN (30, 40))
UNION
SELECT     SampleID, testID, 'Zn' AS TestType, Zn AS Result
FROM         dbo.vwSpectroscopy
WHERE     (testID IN (30, 40))
UNION
SELECT     id AS SampleID, 160 AS testID, '1m_5_10' AS TestType, micron_5_10 AS Result
FROM         particlecount
UNION
SELECT     id AS SampleID, 160 AS testID, '2m_10_15' AS TestType, micron_10_15 AS Result
FROM         particlecount
UNION
SELECT     id AS SampleID, 160 AS testID, '3m_15_25' AS TestType, micron_15_25 AS Result
FROM         particlecount
UNION
SELECT     id AS SampleID, 160 AS testID, '4m_25_50' AS TestType, micron_25_50 AS Result
FROM         particlecount
UNION
SELECT     id AS SampleID, 160 AS testID, '5m_50_100' AS TestType, micron_50_100 AS Result
FROM         particlecount
UNION
SELECT     id AS SampleID, 160 AS testID, '6m_>100' AS TestType, micron_100 AS Result
FROM         particlecount
UNION
SELECT     SampleID AS SampleID, 70 AS testID, '1AntiOxy' AS TestType, anti_oxidant AS Result
FROM         ftir
UNION
SELECT     SampleID AS SampleID, 70 AS testID, '2Oxidation' AS TestType, oxidation AS Result
FROM         ftir
UNION
SELECT     SampleID AS SampleID, 70 AS testID, '3H2O' AS TestType, h2o AS Result
FROM         ftir
UNION
SELECT     SampleID AS SampleID, 70 AS testID, '4Zddp' AS TestType, zddp AS Result
FROM         ftir
UNION
SELECT     SampleID AS SampleID, 70 AS testID, '5Soot' AS TestType, soot AS Result
FROM         ftir
UNION
SELECT     SampleID AS SampleID, 70 AS testID, '6FuelDilute' AS TestType, fuel_dilution AS Result
FROM         ftir
UNION
SELECT     SampleID AS SampleID, 70 AS testID, '7Mixture' AS TestType, mixture AS Result
FROM         ftir
UNION
SELECT     SampleID AS SampleID, 70 AS testID, '8NLGI' AS TestType, nlgi AS Result
FROM         ftir
UNION
SELECT     SampleID AS SampleID, 70 AS testID, '9Contam' AS TestType, contam AS Result
FROM         ftir
UNION
SELECT     SampleID, 270 AS testID, '2Yield stress' AS TestType, Calc1 AS Result
FROM         RheometerCalcs
WHERE     TestType = 5
UNION
SELECT     SampleID, 270 AS testID, '3su max' AS TestType, Calc1 AS Result
FROM         RheometerCalcs
WHERE     TestType = 4
UNION
SELECT     SampleID, 270 AS testID, '4su work' AS TestType, Calc2 AS Result
FROM         RheometerCalcs
WHERE     TestType = 4
UNION
SELECT     SampleID, 270 AS testID, '5su flow' AS TestType, Calc3 AS Result
FROM         RheometerCalcs
WHERE     TestType = 4
UNION
SELECT     SampleID, 270 AS testID, '6str% 10s' AS TestType, Calc1 AS Result
FROM         RheometerCalcs
WHERE     TestType = 3
UNION
SELECT     SampleID, 270 AS testID, '7str% max' AS TestType, Calc2 AS Result
FROM         RheometerCalcs
WHERE     TestType = 3
UNION
SELECT     SampleID, 270 AS testID, '8str% min' AS TestType, Calc3 AS Result
FROM         RheometerCalcs
WHERE     TestType = 3
UNION
SELECT     SampleID, 270 AS testID, '9str% rcvry' AS TestType, Calc4 AS Result
FROM         RheometerCalcs
WHERE     TestType = 3
UNION
SELECT     SampleID, 270 AS testID, 'aG` 30' AS TestType, Calc1 AS Result
FROM         RheometerCalcs
WHERE     TestType = 1
UNION
SELECT     SampleID, 270 AS testID, 'bG` 100' AS TestType, Calc2 AS Result
FROM         RheometerCalcs
WHERE     TestType = 1
UNION
SELECT     SampleID, 270 AS testID, 'cTswp init' AS TestType, Calc1 AS Result
FROM         RheometerCalcs
WHERE     TestType = 6
UNION
SELECT     SampleID, 270 AS testID, 'dTswp final' AS TestType, Calc2 AS Result
FROM         RheometerCalcs
WHERE     TestType = 6
UNION
SELECT     SampleID, 270 AS testID, 'eG` 20a' AS TestType, Calc1 AS Result
FROM         RheometerCalcs
WHERE     TestType = 7
UNION
SELECT     SampleID, 270 AS testID, 'fG` 85' AS TestType, Calc2 AS Result
FROM         RheometerCalcs
WHERE     TestType = 7
UNION
SELECT     SampleID, 270 AS testID, 'gG` 20b' AS TestType, Calc3 AS Result
FROM         RheometerCalcs
WHERE     TestType = 7
UNION
SELECT     SampleID, 270 AS testID, 'hG` 0.1 r/s' AS TestType, Calc1 AS Result
FROM         RheometerCalcs
WHERE     TestType = 2
UNION
SELECT     SampleID, 270 AS testID, 'iG` 1 r/s' AS TestType, Calc2 AS Result
FROM         RheometerCalcs
WHERE     TestType = 2
UNION
SELECT     SampleID, 270 AS testID, 'jG` 10 r/s' AS TestType, Calc3 AS Result
FROM         RheometerCalcs
WHERE     TestType = 2
UNION
SELECT     SampleID, 270 AS testID, 'kG` 100 r/s' AS TestType, Calc4 AS Result
FROM         RheometerCalcs
WHERE     TestType = 2

GO

