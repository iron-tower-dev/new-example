USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwTestResultBySampleAndTest]    Script Date: 10/28/2025 11:37:42 AM ******/
DROP VIEW [dbo].[vwTestResultBySampleAndTest]
GO

/****** Object:  View [dbo].[vwTestResultBySampleAndTest]    Script Date: 10/28/2025 11:37:42 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwTestResultBySampleAndTest]
AS
SELECT     SampleID, testID, '' AS TestType, dbo.TestResultValue(sampleID, testID) AS Result
FROM         dbo.TestReadings
WHERE     (trialNumber = 1) AND (NOT (testID IN (30, 40)))
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

GO

