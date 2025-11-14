USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwOtherTests]    Script Date: 10/28/2025 11:29:30 AM ******/
DROP VIEW [dbo].[vwOtherTests]
GO

/****** Object:  View [dbo].[vwOtherTests]    Script Date: 10/28/2025 11:29:30 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwOtherTests]
AS
SELECT     TOP 100 PERCENT dbo.vwGroupSamples.group_name, dbo.vwGroupSamples.valueat, dbo.vwGroupSamples.tagNumber, 
                      dbo.vwGroupSamples.component, dbo.vwGroupSamples.location, dbo.vwGroupSamples.SampleID, dbo.vwGroupSamples.sampleDate, 
                      dbo.TestReadings.testID, dbo.Test.abbrev AS TestType, dbo.TestReadings.value1, dbo.TestReadings.value2, dbo.TestReadings.value3, 
                      dbo.TestReadings.ID1, dbo.TestReadings.ID2, dbo.TestReadings.ID3, dbo.TestReadings.trialCalc
FROM         dbo.TestReadings INNER JOIN
                      dbo.Test ON dbo.TestReadings.testID = dbo.Test.ID INNER JOIN
                      dbo.vwGroupSamples ON dbo.TestReadings.sampleID = dbo.vwGroupSamples.SampleID
WHERE     (dbo.TestReadings.testID IN (10, 20, 50, 60, 80, 100, 110, 120, 130, 140, 170, 180, 220, 230, 240))
ORDER BY dbo.TestReadings.testID

GO

