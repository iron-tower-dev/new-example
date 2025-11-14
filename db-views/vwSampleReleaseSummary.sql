USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwSampleReleaseSummary]    Script Date: 10/28/2025 11:35:34 AM ******/
DROP VIEW [dbo].[vwSampleReleaseSummary]
GO

/****** Object:  View [dbo].[vwSampleReleaseSummary]    Script Date: 10/28/2025 11:35:34 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vwSampleReleaseSummary]
AS
SELECT     dbo.UsedLubeSamples.ID AS SampleID, dbo.UsedLubeSamples.tagNumber, dbo.UsedLubeSamples.component AS ComponentCode, 
                      dbo.Component.name AS ComponentName, dbo.UsedLubeSamples.location AS LocationCode, dbo.Location.name AS LocationName, 
                      dbo.UsedLubeSamples.lubeType, dbo.UsedLubeSamples.sampleDate, dbo.UsedLubeSamples.receivedOn, dbo.UsedLubeSamples.sampledBy, 
                      dbo.UsedLubeSamples.woNumber, dbo.UsedLubeSamples.results_review_date, dbo.UsedLubeSamples.results_avail_date, 
                      dbo.UsedLubeSamples.newUsedFlag, dbo.UsedLubeSamples.storeSource
FROM         dbo.Component RIGHT OUTER JOIN
                      dbo.Location RIGHT OUTER JOIN
                      dbo.UsedLubeSamples LEFT OUTER JOIN
                      dbo.TestList ON dbo.UsedLubeSamples.status = dbo.TestList.Status ON dbo.Location.code = dbo.UsedLubeSamples.location ON 
                      dbo.Component.code = dbo.UsedLubeSamples.component

GO

