USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwMiscTestResults]    Script Date: 10/28/2025 11:28:24 AM ******/
DROP VIEW [dbo].[vwMiscTestResults]
GO

/****** Object:  View [dbo].[vwMiscTestResults]    Script Date: 10/28/2025 11:28:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwMiscTestResults]
AS
SELECT     TOP 100 PERCENT dbo.UsedLubeSamples.ID AS SampleID, dbo.UsedLubeSamples.tagNumber, dbo.UsedLubeSamples.component, 
                      dbo.UsedLubeSamples.location, dbo.UsedLubeSamples.sampleDate, dbo.UsedLubeSamples.lubeType, dbo.TestReadings.testID, 
                      dbo.TestReadings.value1, dbo.Test.abbrev, dbo.UsedLubeSamples.status AS SampleStatus
FROM         dbo.Test INNER JOIN
                      dbo.TestReadings ON dbo.Test.ID = dbo.TestReadings.testID INNER JOIN
                      dbo.UsedLubeSamples ON dbo.TestReadings.sampleID = dbo.UsedLubeSamples.ID
WHERE     (dbo.Test.exclude IS NULL OR
                      dbo.Test.exclude = 'N') AND (dbo.Test.displayGroupId = 80) AND (dbo.TestReadings.trialNumber = 1)
ORDER BY dbo.TestReadings.testID, dbo.UsedLubeSamples.sampleDate DESC


GO

