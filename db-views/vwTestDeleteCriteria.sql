USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwTestDeleteCriteria]    Script Date: 10/28/2025 11:36:51 AM ******/
DROP VIEW [dbo].[vwTestDeleteCriteria]
GO

/****** Object:  View [dbo].[vwTestDeleteCriteria]    Script Date: 10/28/2025 11:36:51 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwTestDeleteCriteria]
AS
SELECT     dbo.UsedLubeSamples.ID AS SampleID, dbo.TestReadings.testID
FROM         dbo.UsedLubeSamples INNER JOIN
                      dbo.TestReadings ON dbo.UsedLubeSamples.ID = dbo.TestReadings.sampleID
WHERE     (NOT (dbo.TestReadings.status IN ('D', 'T', 'S')))

GO

