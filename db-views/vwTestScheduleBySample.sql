USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwTestScheduleBySample]    Script Date: 10/28/2025 11:39:50 AM ******/
DROP VIEW [dbo].[vwTestScheduleBySample]
GO

/****** Object:  View [dbo].[vwTestScheduleBySample]    Script Date: 10/28/2025 11:39:50 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwTestScheduleBySample]
AS
SELECT     dbo.vwTestListBySample.SampleID, dbo.vwTestListBySample.tagNumber, dbo.vwTestListBySample.component, dbo.vwTestListBySample.location, 
                      dbo.vwTestListBySample.SampleStatusCode, dbo.vwTestListBySample.SampleStatus, dbo.vwTestListBySample.receivedOn, 
                      dbo.vwTestListBySample.TestID, dbo.vwTestListBySample.TestName, dbo.vwTestListBySample.TestAbbrev, dbo.TestReadings.trialNumber, 
                      dbo.TestReadings.value1, dbo.TestReadings.value2, dbo.TestReadings.value3, dbo.TestReadings.trialCalc, dbo.TestReadings.ID1, 
                      dbo.TestReadings.ID2, dbo.TestReadings.ID3, dbo.TestReadings.status AS TestStatusCode, dbo.TestStatusText(dbo.TestReadings.status) 
                      AS TestStatus, dbo.TestReadings.schedType
FROM         dbo.vwTestListBySample LEFT OUTER JOIN
                      dbo.TestReadings ON dbo.vwTestListBySample.SampleID = dbo.TestReadings.sampleID AND 
                      dbo.vwTestListBySample.TestID = dbo.TestReadings.testID

GO

