USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwAllMTEUsage]    Script Date: 10/28/2025 10:52:19 AM ******/
DROP VIEW [dbo].[vwAllMTEUsage]
GO

/****** Object:  View [dbo].[vwAllMTEUsage]    Script Date: 10/28/2025 10:52:19 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwAllMTEUsage]
AS
SELECT DISTINCT 
                      TOP 100 PERCENT dbo.UsedLubeSamples.ID, dbo.UsedLubeSamples.tagNumber, dbo.Component.name AS ComponentName, 
                      dbo.Location.name AS LocationName, dbo.TestReadings.testID, dbo.Test.abbrev, dbo.TestReadings.ID1 AS SerialNo, 
                      dbo.UsedLubeSamples.woNumber
FROM         dbo.UsedLubeSamples INNER JOIN
                      dbo.TestReadings ON dbo.UsedLubeSamples.ID = dbo.TestReadings.sampleID INNER JOIN
                      dbo.Test ON dbo.TestReadings.testID = dbo.Test.testStandID LEFT OUTER JOIN
                      dbo.Component ON dbo.UsedLubeSamples.component = dbo.Component.code LEFT OUTER JOIN
                      dbo.Location ON dbo.UsedLubeSamples.location = dbo.Location.code
WHERE     (dbo.TestReadings.testID = 50) AND (NOT (dbo.TestReadings.ID1 IS NULL)) OR
                      (dbo.TestReadings.testID = 60) AND (NOT (dbo.TestReadings.ID1 IS NULL))
UNION
SELECT DISTINCT 
                      TOP 100 PERCENT dbo.UsedLubeSamples.ID, dbo.UsedLubeSamples.tagNumber, dbo.Component.name AS ComponentName, 
                      dbo.Location.name AS LocationName, dbo.TestReadings.testID, dbo.Test.abbrev, dbo.TestReadings.ID2 AS SerialNo, 
                      dbo.UsedLubeSamples.woNumber
FROM         dbo.UsedLubeSamples INNER JOIN
                      dbo.TestReadings ON dbo.UsedLubeSamples.ID = dbo.TestReadings.sampleID INNER JOIN
                      dbo.Test ON dbo.TestReadings.testID = dbo.Test.testStandID LEFT OUTER JOIN
                      dbo.Component ON dbo.UsedLubeSamples.component = dbo.Component.code LEFT OUTER JOIN
                      dbo.Location ON dbo.UsedLubeSamples.location = dbo.Location.code
WHERE     (dbo.TestReadings.testID = 50) AND (NOT (dbo.TestReadings.ID2 IS NULL)) OR
                      (dbo.TestReadings.testID = 60) AND (NOT (dbo.TestReadings.ID2 IS NULL))
UNION
SELECT DISTINCT 
                      TOP 100 PERCENT dbo.UsedLubeSamples.ID, dbo.UsedLubeSamples.tagNumber, dbo.Component.name AS ComponentName, 
                      dbo.Location.name AS LocationName, dbo.TestReadings.testID, dbo.Test.abbrev, dbo.TestReadings.ID3 AS SerialNo, 
                      dbo.UsedLubeSamples.woNumber
FROM         dbo.UsedLubeSamples INNER JOIN
                      dbo.TestReadings ON dbo.UsedLubeSamples.ID = dbo.TestReadings.sampleID INNER JOIN
                      dbo.Test ON dbo.TestReadings.testID = dbo.Test.testStandID LEFT OUTER JOIN
                      dbo.Component ON dbo.UsedLubeSamples.component = dbo.Component.code LEFT OUTER JOIN
                      dbo.Location ON dbo.UsedLubeSamples.location = dbo.Location.code
WHERE     (dbo.TestReadings.testID = 50) AND (NOT (dbo.TestReadings.ID3 IS NULL)) OR
                      (dbo.TestReadings.testID = 60) AND (NOT (dbo.TestReadings.ID3 IS NULL))
UNION
SELECT DISTINCT 
                      TOP 100 PERCENT dbo.UsedLubeSamples.ID, dbo.UsedLubeSamples.tagNumber, dbo.Component.name AS ComponentName, 
                      dbo.Location.name AS LocationName, dbo.TestReadings.testID, dbo.Test.abbrev, dbo.TestReadings.ID1 AS SerialNo, 
                      dbo.UsedLubeSamples.woNumber
FROM         dbo.UsedLubeSamples INNER JOIN
                      dbo.TestReadings ON dbo.UsedLubeSamples.ID = dbo.TestReadings.sampleID INNER JOIN
                      dbo.Test ON dbo.TestReadings.testID = dbo.Test.testStandID LEFT OUTER JOIN
                      dbo.Component ON dbo.UsedLubeSamples.component = dbo.Component.code LEFT OUTER JOIN
                      dbo.Location ON dbo.UsedLubeSamples.location = dbo.Location.code
WHERE     (dbo.TestReadings.testID = 80) AND (NOT (dbo.TestReadings.ID1 IS NULL))
UNION
SELECT DISTINCT 
                      TOP 100 PERCENT dbo.UsedLubeSamples.ID, dbo.UsedLubeSamples.tagNumber, dbo.Component.name AS ComponentName, 
                      dbo.Location.name AS LocationName, dbo.TestReadings.testID, dbo.Test.abbrev, dbo.TestReadings.ID1 AS SerialNo, 
                      dbo.UsedLubeSamples.woNumber
FROM         dbo.UsedLubeSamples INNER JOIN
                      dbo.TestReadings ON dbo.UsedLubeSamples.ID = dbo.TestReadings.sampleID INNER JOIN
                      dbo.Test ON dbo.TestReadings.testID = dbo.Test.testStandID LEFT OUTER JOIN
                      dbo.Component ON dbo.UsedLubeSamples.component = dbo.Component.code LEFT OUTER JOIN
                      dbo.Location ON dbo.UsedLubeSamples.location = dbo.Location.code
WHERE     (dbo.TestReadings.testID = 140) AND (NOT (dbo.TestReadings.ID1 IS NULL))
UNION
SELECT DISTINCT 
                      TOP 100 PERCENT dbo.UsedLubeSamples.ID, dbo.UsedLubeSamples.tagNumber, dbo.Component.name AS ComponentName, 
                      dbo.Location.name AS LocationName, dbo.TestReadings.testID, dbo.Test.abbrev, dbo.TestReadings.ID2 AS SerialNo, 
                      dbo.UsedLubeSamples.woNumber
FROM         dbo.UsedLubeSamples INNER JOIN
                      dbo.TestReadings ON dbo.UsedLubeSamples.ID = dbo.TestReadings.sampleID INNER JOIN
                      dbo.Test ON dbo.TestReadings.testID = dbo.Test.testStandID LEFT OUTER JOIN
                      dbo.Component ON dbo.UsedLubeSamples.component = dbo.Component.code LEFT OUTER JOIN
                      dbo.Location ON dbo.UsedLubeSamples.location = dbo.Location.code
WHERE     (dbo.TestReadings.testID = 140) AND (NOT (dbo.TestReadings.ID2 IS NULL))
UNION
SELECT DISTINCT 
                      TOP 100 PERCENT dbo.UsedLubeSamples.ID, dbo.UsedLubeSamples.tagNumber, dbo.Component.name AS ComponentName, 
                      dbo.Location.name AS LocationName, dbo.TestReadings.testID, dbo.Test.abbrev, dbo.TestReadings.ID1 AS SerialNo, 
                      dbo.UsedLubeSamples.woNumber
FROM         dbo.UsedLubeSamples INNER JOIN
                      dbo.TestReadings ON dbo.UsedLubeSamples.ID = dbo.TestReadings.sampleID INNER JOIN
                      dbo.Test ON dbo.TestReadings.testID = dbo.Test.testStandID LEFT OUTER JOIN
                      dbo.Component ON dbo.UsedLubeSamples.component = dbo.Component.code LEFT OUTER JOIN
                      dbo.Location ON dbo.UsedLubeSamples.location = dbo.Location.code
WHERE     (dbo.TestReadings.testID = 170) AND (NOT (dbo.TestReadings.ID1 IS NULL))
UNION
SELECT DISTINCT 
                      TOP 100 PERCENT dbo.UsedLubeSamples.ID, dbo.UsedLubeSamples.tagNumber, dbo.Component.name AS ComponentName, 
                      dbo.Location.name AS LocationName, dbo.TestReadings.testID, dbo.Test.abbrev, dbo.TestReadings.ID1 AS SerialNo, 
                      dbo.UsedLubeSamples.woNumber
FROM         dbo.UsedLubeSamples INNER JOIN
                      dbo.TestReadings ON dbo.UsedLubeSamples.ID = dbo.TestReadings.sampleID INNER JOIN
                      dbo.Test ON dbo.TestReadings.testID = dbo.Test.testStandID LEFT OUTER JOIN
                      dbo.Component ON dbo.UsedLubeSamples.component = dbo.Component.code LEFT OUTER JOIN
                      dbo.Location ON dbo.UsedLubeSamples.location = dbo.Location.code
WHERE     (dbo.TestReadings.testID = 220) AND (NOT (dbo.TestReadings.ID1 IS NULL))
UNION
SELECT DISTINCT 
                      TOP 100 PERCENT dbo.UsedLubeSamples.ID, dbo.UsedLubeSamples.tagNumber, dbo.Component.name AS ComponentName, 
                      dbo.Location.name AS LocationName, dbo.TestReadings.testID, dbo.Test.abbrev, dbo.TestReadings.ID1 AS SerialNo, 
                      dbo.UsedLubeSamples.woNumber
FROM         dbo.UsedLubeSamples INNER JOIN
                      dbo.TestReadings ON dbo.UsedLubeSamples.ID = dbo.TestReadings.sampleID INNER JOIN
                      dbo.Test ON dbo.TestReadings.testID = dbo.Test.testStandID LEFT OUTER JOIN
                      dbo.Component ON dbo.UsedLubeSamples.component = dbo.Component.code LEFT OUTER JOIN
                      dbo.Location ON dbo.UsedLubeSamples.location = dbo.Location.code
WHERE     (dbo.TestReadings.testID = 230) AND (NOT (dbo.TestReadings.ID1 IS NULL))
UNION
SELECT DISTINCT 
                      TOP 100 PERCENT dbo.UsedLubeSamples.ID, dbo.UsedLubeSamples.tagNumber, dbo.Component.name AS ComponentName, 
                      dbo.Location.name AS LocationName, dbo.TestReadings.testID, dbo.Test.abbrev, dbo.TestReadings.ID1 AS SerialNo, 
                      dbo.UsedLubeSamples.woNumber
FROM         dbo.UsedLubeSamples INNER JOIN
                      dbo.TestReadings ON dbo.UsedLubeSamples.ID = dbo.TestReadings.sampleID INNER JOIN
                      dbo.Test ON dbo.TestReadings.testID = dbo.Test.testStandID LEFT OUTER JOIN
                      dbo.Component ON dbo.UsedLubeSamples.component = dbo.Component.code LEFT OUTER JOIN
                      dbo.Location ON dbo.UsedLubeSamples.location = dbo.Location.code
WHERE     (dbo.TestReadings.testID = 250) AND (NOT (dbo.TestReadings.ID1 IS NULL))
ORDER BY dbo.UsedLubeSamples.ID DESC

GO

