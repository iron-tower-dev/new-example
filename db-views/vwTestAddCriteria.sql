USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwTestAddCriteria]    Script Date: 10/28/2025 11:36:37 AM ******/
DROP VIEW [dbo].[vwTestAddCriteria]
GO

/****** Object:  View [dbo].[vwTestAddCriteria]    Script Date: 10/28/2025 11:36:37 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwTestAddCriteria]
AS
SELECT     dbo.UsedLubeSamples.ID AS SampleID, dbo.UsedLubeSamples.status, dbo.TestReadings.testID, dbo.ScheduleDeletions.Reason
FROM         dbo.ScheduleDeletions RIGHT OUTER JOIN
                      dbo.TestReadings ON dbo.ScheduleDeletions.SampleID = dbo.TestReadings.sampleID AND 
                      dbo.ScheduleDeletions.TestID = dbo.TestReadings.testID RIGHT OUTER JOIN
                      dbo.UsedLubeSamples ON dbo.TestReadings.sampleID = dbo.UsedLubeSamples.ID

GO

