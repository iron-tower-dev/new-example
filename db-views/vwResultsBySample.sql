USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwResultsBySample]    Script Date: 10/28/2025 11:31:36 AM ******/
DROP VIEW [dbo].[vwResultsBySample]
GO

/****** Object:  View [dbo].[vwResultsBySample]    Script Date: 10/28/2025 11:31:36 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE VIEW [dbo].[vwResultsBySample]
AS
SELECT     sampleID, testID, dbo.TestResultValue(sampleID, testID) AS Result
FROM         dbo.TestReadings
WHERE     (trialNumber = 1)


GO

