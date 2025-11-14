USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwTestResultsBySample]    Script Date: 10/28/2025 11:38:13 AM ******/
DROP VIEW [dbo].[vwTestResultsBySample]
GO

/****** Object:  View [dbo].[vwTestResultsBySample]    Script Date: 10/28/2025 11:38:13 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE VIEW [dbo].[vwTestResultsBySample]
AS
SELECT     TOP 100 PERCENT dbo.UsedLubeSamples.ID AS SampleID, dbo.UsedLubeSamples.tagNumber, dbo.UsedLubeSamples.component, 
                      dbo.UsedLubeSamples.location, dbo.UsedLubeSamples.lubeType, dbo.UsedLubeSamples.sampleDate, dbo.UsedLubeSamples.receivedOn, 
                      dbo.UsedLubeSamples.status AS SampleStatusCode, dbo.TestList.Description AS SampleStatus, dbo.UsedLubeSamples.newUsedFlag, 
                      dbo.TestReadings.testID, dbo.TestReadings.trialNumber, dbo.TestReadings.value1, dbo.TestReadings.value2, dbo.TestReadings.value3, 
                      dbo.TestReadings.trialCalc, dbo.TestReadings.ID1, dbo.TestReadings.ID2, dbo.TestReadings.ID3, dbo.TestStatusText(dbo.TestReadings.status) 
                      AS TestStatus, dbo.TestReadings.schedType
FROM         dbo.TestList RIGHT OUTER JOIN
                      dbo.UsedLubeSamples ON dbo.TestList.Status = dbo.UsedLubeSamples.status LEFT OUTER JOIN
                      dbo.TestReadings ON dbo.UsedLubeSamples.ID = dbo.TestReadings.sampleID
ORDER BY dbo.UsedLubeSamples.ID DESC, dbo.TestReadings.testID, dbo.TestReadings.trialNumber


GO

