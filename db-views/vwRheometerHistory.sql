USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwRheometerHistory]    Script Date: 10/28/2025 11:32:15 AM ******/
DROP VIEW [dbo].[vwRheometerHistory]
GO

/****** Object:  View [dbo].[vwRheometerHistory]    Script Date: 10/28/2025 11:32:15 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vwRheometerHistory]
AS
SELECT dbo.UsedLubeSamples.ID AS SampleID, dbo.UsedLubeSamples.tagNumber, dbo.UsedLubeSamples.component, dbo.UsedLubeSamples.location, 
               dbo.UsedLubeSamples.sampleDate, R5.[Yield stress], R4.[su max], R4.[su work], R4.[su flow], R3.[str% 10s], R3.[str% max], R3.[str% min], R3.[str% rcvry], 
               R1.[G` 30], R1.[G` 100], R6.[Tswp init], R6.Tswpfinal, R7.[G` 20a], R7.[G` 85], R7.[G` 20b], R2.[G` 0.1 r/s], R2.[G` 1 r/s], R2.[G` 10 r/s], R2.[G` 100 r/s], 
               R2.[1/Td 0.1 r/s], R2.[1/Td 100 r/s], R2.[eta* 0.1 r/s], R2.[eta' 0.1 r/s], dbo.UsedLubeSamples.newUsedFlag, dbo.UsedLubeSamples.lubeType, a.Comment AS remark
FROM  dbo.UsedLubeSamples INNER JOIN
               dbo.TestReadings ON dbo.UsedLubeSamples.ID = dbo.TestReadings.sampleID LEFT OUTER JOIN
               dbo.vwRheometerHistory6 R6 ON dbo.UsedLubeSamples.ID = R6.SampleID LEFT OUTER JOIN
               dbo.vwRheometerHistory1 R1 ON dbo.UsedLubeSamples.ID = R1.SampleID LEFT OUTER JOIN
               dbo.vwRheometerHistory7 R7 ON dbo.UsedLubeSamples.ID = R7.SampleID LEFT OUTER JOIN
               dbo.vwRheometerHistory5 R5 ON dbo.UsedLubeSamples.ID = R5.SampleID LEFT OUTER JOIN
               dbo.vwRheometerHistory4 R4 ON dbo.UsedLubeSamples.ID = R4.SampleID LEFT OUTER JOIN
               dbo.vwRheometerHistory3 R3 ON dbo.UsedLubeSamples.ID = R3.SampleID LEFT OUTER JOIN
               dbo.vwRheometerHistory2 R2 ON dbo.UsedLubeSamples.ID = R2.SampleID LEFT OUTER JOIN
               dbo.allsamplecomments a ON dbo.UsedLubeSamples.ID = a.SampleID
WHERE (dbo.TestReadings.testID = 270)

GO

