USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwMTE_UsageForSample]    Script Date: 10/28/2025 11:00:10 AM ******/
DROP VIEW [dbo].[vwMTE_UsageForSample]
GO

/****** Object:  View [dbo].[vwMTE_UsageForSample]    Script Date: 10/28/2025 11:00:10 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER OFF
GO


CREATE VIEW [dbo].[vwMTE_UsageForSample]
AS
SELECT     dbo.UsedLubeSamples.ID, dbo.TestReadings.testID, dbo.TestReadings.ID1 AS SerialNo, dbo.UsedLubeSamples.woNumber
FROM         (dbo.UsedLubeSamples INNER JOIN
                      dbo.Lube_Sampling_Point ON (dbo.UsedLubeSamples.location = dbo.Lube_Sampling_Point.location) AND 
                      (dbo.UsedLubeSamples.component = dbo.Lube_Sampling_Point.component) AND 
                      (dbo.UsedLubeSamples.tagNumber = dbo.Lube_Sampling_Point.tagNumber)) INNER JOIN
                      dbo.TestReadings ON dbo.UsedLubeSamples.ID = dbo.TestReadings.sampleID
WHERE     ((dbo.TestReadings.testID IN (50, 60)) AND (dbo.UsedLubeSamples.woNumber IS NOT NULL) AND 
                      (dbo.Lube_Sampling_Point.qualityClass IN ('Q', 'QAG')) AND (dbo.TestReadings.ID1 IS NOT NULL))
UNION
SELECT     dbo.UsedLubeSamples.ID, dbo.TestReadings.testID, dbo.TestReadings.ID3 AS SerialNo, dbo.UsedLubeSamples.woNumber
FROM         (dbo.UsedLubeSamples INNER JOIN
                      dbo.Lube_Sampling_Point ON (dbo.UsedLubeSamples.location = dbo.Lube_Sampling_Point.location) AND 
                      (dbo.UsedLubeSamples.component = dbo.Lube_Sampling_Point.component) AND 
                      (dbo.UsedLubeSamples.tagNumber = dbo.Lube_Sampling_Point.tagNumber)) INNER JOIN
                      dbo.TestReadings ON dbo.UsedLubeSamples.ID = dbo.TestReadings.sampleID
WHERE     ((dbo.TestReadings.testID IN (50, 60)) AND (dbo.UsedLubeSamples.woNumber IS NOT NULL) AND 
                      (dbo.Lube_Sampling_Point.qualityClass IN ('Q', 'QAG')) AND (dbo.TestReadings.ID3 IS NOT NULL))
UNION
SELECT     dbo.UsedLubeSamples.ID, dbo.TestReadings.testID, dbo.TestReadings.ID1 AS SerialNo, dbo.UsedLubeSamples.woNumber
FROM         (dbo.UsedLubeSamples INNER JOIN
                      dbo.Lube_Sampling_Point ON (dbo.UsedLubeSamples.location = dbo.Lube_Sampling_Point.location) AND 
                      (dbo.UsedLubeSamples.component = dbo.Lube_Sampling_Point.component) AND 
                      (dbo.UsedLubeSamples.tagNumber = dbo.Lube_Sampling_Point.tagNumber)) INNER JOIN
                      dbo.TestReadings ON dbo.UsedLubeSamples.ID = dbo.TestReadings.sampleID
WHERE     ((dbo.TestReadings.testID = 80) AND (dbo.UsedLubeSamples.woNumber IS NOT NULL) AND 
                      (dbo.Lube_Sampling_Point.qualityClass IN ('Q', 'QAG')) AND (dbo.TestReadings.ID1 IS NOT NULL))
UNION
SELECT     dbo.UsedLubeSamples.ID, dbo.TestReadings.testID, dbo.TestReadings.ID2 AS SerialNo, dbo.UsedLubeSamples.woNumber
FROM         (dbo.UsedLubeSamples INNER JOIN
                      dbo.Lube_Sampling_Point ON (dbo.UsedLubeSamples.location = dbo.Lube_Sampling_Point.location) AND 
                      (dbo.UsedLubeSamples.component = dbo.Lube_Sampling_Point.component) AND 
                      (dbo.UsedLubeSamples.tagNumber = dbo.Lube_Sampling_Point.tagNumber)) INNER JOIN
                      dbo.TestReadings ON dbo.UsedLubeSamples.ID = dbo.TestReadings.sampleID
WHERE     ((dbo.TestReadings.testID = 80) AND (dbo.UsedLubeSamples.woNumber IS NOT NULL) AND 
                      (dbo.Lube_Sampling_Point.qualityClass IN ('Q', 'QAG')) AND (dbo.TestReadings.ID2 IS NOT NULL))
UNION
SELECT     dbo.UsedLubeSamples.ID, dbo.TestReadings.testID, dbo.TestReadings.ID1 AS SerialNo, dbo.UsedLubeSamples.woNumber
FROM         (dbo.UsedLubeSamples INNER JOIN
                      dbo.Lube_Sampling_Point ON (dbo.UsedLubeSamples.location = dbo.Lube_Sampling_Point.location) AND 
                      (dbo.UsedLubeSamples.component = dbo.Lube_Sampling_Point.component) AND 
                      (dbo.UsedLubeSamples.tagNumber = dbo.Lube_Sampling_Point.tagNumber)) INNER JOIN
                      dbo.TestReadings ON dbo.UsedLubeSamples.ID = dbo.TestReadings.sampleID
WHERE     ((dbo.TestReadings.testID = 140) AND (dbo.UsedLubeSamples.woNumber IS NOT NULL) AND 
                      (dbo.Lube_Sampling_Point.qualityClass IN ('Q', 'QAG')) AND (dbo.TestReadings.ID1 IS NOT NULL))
UNION
SELECT     dbo.UsedLubeSamples.ID, dbo.TestReadings.testID, dbo.TestReadings.ID2 AS SerialNo, dbo.UsedLubeSamples.woNumber
FROM         (dbo.UsedLubeSamples INNER JOIN
                      dbo.Lube_Sampling_Point ON (dbo.UsedLubeSamples.location = dbo.Lube_Sampling_Point.location) AND 
                      (dbo.UsedLubeSamples.component = dbo.Lube_Sampling_Point.component) AND 
                      (dbo.UsedLubeSamples.tagNumber = dbo.Lube_Sampling_Point.tagNumber)) INNER JOIN
                      dbo.TestReadings ON dbo.UsedLubeSamples.ID = dbo.TestReadings.sampleID
WHERE     ((dbo.TestReadings.testID = 140) AND (dbo.UsedLubeSamples.woNumber IS NOT NULL) AND 
                      (dbo.Lube_Sampling_Point.qualityClass IN ('Q', 'QAG')) AND (dbo.TestReadings.ID2 IS NOT NULL))
UNION
SELECT     dbo.UsedLubeSamples.ID, dbo.TestReadings.testID, dbo.TestReadings.ID1 AS SerialNo, dbo.UsedLubeSamples.woNumber
FROM         (dbo.UsedLubeSamples INNER JOIN
                      dbo.Lube_Sampling_Point ON (dbo.UsedLubeSamples.location = dbo.Lube_Sampling_Point.location) AND 
                      (dbo.UsedLubeSamples.component = dbo.Lube_Sampling_Point.component) AND 
                      (dbo.UsedLubeSamples.tagNumber = dbo.Lube_Sampling_Point.tagNumber)) INNER JOIN
                      dbo.TestReadings ON dbo.UsedLubeSamples.ID = dbo.TestReadings.sampleID
WHERE     ((dbo.TestReadings.testID = 170) AND (dbo.UsedLubeSamples.woNumber IS NOT NULL) AND 
                      (dbo.Lube_Sampling_Point.qualityClass IN ('Q', 'QAG')) AND (dbo.TestReadings.ID1 IS NOT NULL))
UNION
SELECT     dbo.UsedLubeSamples.ID, dbo.TestReadings.testID, dbo.TestReadings.ID1 AS SerialNo, dbo.UsedLubeSamples.woNumber
FROM         (dbo.UsedLubeSamples INNER JOIN
                      dbo.Lube_Sampling_Point ON (dbo.UsedLubeSamples.location = dbo.Lube_Sampling_Point.location) AND 
                      (dbo.UsedLubeSamples.component = dbo.Lube_Sampling_Point.component) AND 
                      (dbo.UsedLubeSamples.tagNumber = dbo.Lube_Sampling_Point.tagNumber)) INNER JOIN
                      dbo.TestReadings ON dbo.UsedLubeSamples.ID = dbo.TestReadings.sampleID
WHERE     ((dbo.TestReadings.testID = 220) AND (dbo.UsedLubeSamples.woNumber IS NOT NULL) AND 
                      (dbo.Lube_Sampling_Point.qualityClass IN ('Q', 'QAG')) AND (dbo.TestReadings.ID1 IS NOT NULL))
UNION
SELECT     dbo.UsedLubeSamples.ID, dbo.TestReadings.testID, dbo.TestReadings.ID1 AS SerialNo, dbo.UsedLubeSamples.woNumber
FROM         (dbo.UsedLubeSamples INNER JOIN
                      dbo.Lube_Sampling_Point ON (dbo.UsedLubeSamples.location = dbo.Lube_Sampling_Point.location) AND 
                      (dbo.UsedLubeSamples.component = dbo.Lube_Sampling_Point.component) AND 
                      (dbo.UsedLubeSamples.tagNumber = dbo.Lube_Sampling_Point.tagNumber)) INNER JOIN
                      dbo.TestReadings ON dbo.UsedLubeSamples.ID = dbo.TestReadings.sampleID
WHERE     ((dbo.TestReadings.testID = 230) AND (dbo.UsedLubeSamples.woNumber IS NOT NULL) AND 
                      (dbo.Lube_Sampling_Point.qualityClass IN ('Q', 'QAG')) AND (dbo.TestReadings.ID1 IS NOT NULL))
UNION
SELECT     dbo.UsedLubeSamples.ID, dbo.TestReadings.testID, dbo.TestReadings.ID1 AS SerialNo, dbo.UsedLubeSamples.woNumber
FROM         (dbo.UsedLubeSamples INNER JOIN
                      dbo.Lube_Sampling_Point ON (dbo.UsedLubeSamples.location = dbo.Lube_Sampling_Point.location) AND 
                      (dbo.UsedLubeSamples.component = dbo.Lube_Sampling_Point.component) AND 
                      (dbo.UsedLubeSamples.tagNumber = dbo.Lube_Sampling_Point.tagNumber)) INNER JOIN
                      dbo.TestReadings ON dbo.UsedLubeSamples.ID = dbo.TestReadings.sampleID
WHERE     ((dbo.TestReadings.testID = 250) AND (dbo.UsedLubeSamples.woNumber IS NOT NULL) AND 
                      (dbo.Lube_Sampling_Point.qualityClass IN ('Q', 'QAG')) AND (dbo.TestReadings.ID1 IS NOT NULL))
GO

