USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwLabSampleTests]    Script Date: 10/28/2025 11:21:59 AM ******/
DROP VIEW [dbo].[vwLabSampleTests]
GO

/****** Object:  View [dbo].[vwLabSampleTests]    Script Date: 10/28/2025 11:21:59 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwLabSampleTests]
AS
SELECT DISTINCT 
                      dbo.UsedLubeSamples.ID AS sampleID, dbo.vwTestsForLab.TestID, dbo.vwTestsForLab.TestName, dbo.vwTestsForLab.TestAbbrev, 
                      dbo.vwTestsForLab.ShortAbbrev, dbo.UsedLubeSamples.tagNumber, dbo.UsedLubeSamples.component, dbo.UsedLubeSamples.location
FROM         dbo.UsedLubeSamples LEFT OUTER JOIN
                      dbo.TestReadings ON dbo.UsedLubeSamples.ID = dbo.TestReadings.sampleID CROSS JOIN
                      dbo.vwTestsForLab

GO

