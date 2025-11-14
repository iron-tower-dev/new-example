USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwGoodnessLimitsAndResultsForSample]    Script Date: 10/28/2025 11:19:51 AM ******/
DROP VIEW [dbo].[vwGoodnessLimitsAndResultsForSample]
GO

/****** Object:  View [dbo].[vwGoodnessLimitsAndResultsForSample]    Script Date: 10/28/2025 11:19:51 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwGoodnessLimitsAndResultsForSample]
AS
SELECT     dbo.vwGoodnessLimitsForSamples.SampleID, dbo.vwGoodnessLimitsForSamples.group_name, dbo.vwGoodnessLimitsForSamples.tagNumber, 
                      dbo.vwGoodnessLimitsForSamples.component, dbo.vwGoodnessLimitsForSamples.location, dbo.vwGoodnessLimitsForSamples.testid, 
                      dbo.vwGoodnessLimitsForSamples.llim3, dbo.vwGoodnessLimitsForSamples.llim2, dbo.vwGoodnessLimitsForSamples.llim1, 
                      dbo.vwGoodnessLimitsForSamples.ulim1, dbo.vwGoodnessLimitsForSamples.ulim2, dbo.vwGoodnessLimitsForSamples.ulim3, 
                      dbo.vwGoodnessLimitsForSamples.gl3, dbo.vwGoodnessLimitsForSamples.gl2, dbo.vwGoodnessLimitsForSamples.gl1, 
                      dbo.vwGoodnessLimitsForSamples.gu1, dbo.vwGoodnessLimitsForSamples.gu2, dbo.vwGoodnessLimitsForSamples.gu3, 
                      dbo.vwGoodnessTestResults.Result, dbo.vwGoodnessTestResults.TestType
FROM         dbo.vwGoodnessTestResults INNER JOIN
                      dbo.vwGoodnessLimitsForSamples ON dbo.vwGoodnessTestResults.SampleID = dbo.vwGoodnessLimitsForSamples.SampleID AND 
                      dbo.vwGoodnessTestResults.testID = dbo.vwGoodnessLimitsForSamples.testid AND 
                      dbo.vwGoodnessTestResults.TestType = dbo.vwGoodnessLimitsForSamples.testname
WHERE     (dbo.vwGoodnessTestResults.Result IS NOT NULL)


GO

