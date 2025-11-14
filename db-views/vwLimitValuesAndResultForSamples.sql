USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwLimitValuesAndResultForSamples]    Script Date: 10/28/2025 11:23:20 AM ******/
DROP VIEW [dbo].[vwLimitValuesAndResultForSamples]
GO

/****** Object:  View [dbo].[vwLimitValuesAndResultForSamples]    Script Date: 10/28/2025 11:23:20 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwLimitValuesAndResultForSamples]
AS
SELECT     dbo.vwLimitValuesForSamples.SampleID, dbo.vwLimitValuesForSamples.group_name, dbo.vwLimitValuesForSamples.tagNumber, 
                      dbo.vwLimitValuesForSamples.component, dbo.vwLimitValuesForSamples.location, dbo.vwLimitValuesForSamples.testid, 
                      dbo.vwLimitValuesForSamples.llim3, dbo.vwLimitValuesForSamples.llim2, dbo.vwLimitValuesForSamples.llim1, dbo.vwLimitValuesForSamples.ulim1, 
                      dbo.vwLimitValuesForSamples.ulim2, dbo.vwLimitValuesForSamples.ulim3, dbo.vwTestResultBySampleAndTest.Result, 
                      dbo.vwTestResultBySampleAndTest.TestType, dbo.Test.abbrev AS TestAbbrev
FROM         dbo.vwLimitValuesForSamples INNER JOIN
                      dbo.vwTestResultBySampleAndTest ON dbo.vwLimitValuesForSamples.SampleID = dbo.vwTestResultBySampleAndTest.SampleID AND 
                      dbo.vwLimitValuesForSamples.testid = dbo.vwTestResultBySampleAndTest.testID AND 
                      dbo.vwLimitValuesForSamples.testname = dbo.vwTestResultBySampleAndTest.TestType INNER JOIN
                      dbo.Test ON dbo.vwTestResultBySampleAndTest.testID = dbo.Test.ID
WHERE     (dbo.vwTestResultBySampleAndTest.Result IS NOT NULL)

GO

