USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwTestListBySample]    Script Date: 10/28/2025 11:37:26 AM ******/
DROP VIEW [dbo].[vwTestListBySample]
GO

/****** Object:  View [dbo].[vwTestListBySample]    Script Date: 10/28/2025 11:37:26 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwTestListBySample]
AS
SELECT     TOP 100 PERCENT dbo.UsedLubeSamples.ID AS SampleID, dbo.UsedLubeSamples.tagNumber, dbo.UsedLubeSamples.component, 
                      dbo.UsedLubeSamples.location, dbo.UsedLubeSamples.status AS SampleStatusCode, dbo.TestList.Description AS SampleStatus, 
                      dbo.UsedLubeSamples.receivedOn, dbo.vwTestsForLab.TestID, dbo.vwTestsForLab.TestName, dbo.vwTestsForLab.TestAbbrev
FROM         dbo.vwTestsForLab CROSS JOIN
                      dbo.UsedLubeSamples LEFT OUTER JOIN
                      dbo.TestList ON dbo.UsedLubeSamples.status = dbo.TestList.Status
ORDER BY dbo.UsedLubeSamples.ID DESC

GO

