USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwLimitValuesForSamples]    Script Date: 10/28/2025 11:23:33 AM ******/
DROP VIEW [dbo].[vwLimitValuesForSamples]
GO

/****** Object:  View [dbo].[vwLimitValuesForSamples]    Script Date: 10/28/2025 11:23:33 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwLimitValuesForSamples]
AS
SELECT     TOP 100 PERCENT dbo.UsedLubeSamples.ID AS SampleID, dbo.limits_xref.group_name, dbo.limits_xref.valueat, dbo.limits_xref.tagNumber, 
                      dbo.limits_xref.component, dbo.limits_xref.location, dbo.limits_xref.exclude, dbo.limits.testid, dbo.limits.testname, dbo.limits.llim3, dbo.limits.llim2, 
                      dbo.limits.llim1, dbo.limits.ulim1, dbo.limits.ulim2, dbo.limits.ulim3
FROM         dbo.UsedLubeSamples INNER JOIN
                      dbo.limits_xref ON dbo.UsedLubeSamples.tagNumber = dbo.limits_xref.tagNumber AND 
                      dbo.UsedLubeSamples.component = dbo.limits_xref.component AND dbo.UsedLubeSamples.location = dbo.limits_xref.location INNER JOIN
                      dbo.limits ON dbo.limits_xref.valueat = dbo.limits.limits_xref_id
WHERE     (dbo.limits_xref.exclude <> 'Y' OR
                      dbo.limits_xref.exclude IS NULL) AND (dbo.limits.llim1 IS NOT NULL) OR
                      (dbo.limits_xref.exclude <> 'Y' OR
                      dbo.limits_xref.exclude IS NULL) AND (dbo.limits.llim2 IS NOT NULL) OR
                      (dbo.limits_xref.exclude <> 'Y' OR
                      dbo.limits_xref.exclude IS NULL) AND (dbo.limits.llim3 IS NOT NULL) OR
                      (dbo.limits_xref.exclude <> 'Y' OR
                      dbo.limits_xref.exclude IS NULL) AND (dbo.limits.ulim1 IS NOT NULL) OR
                      (dbo.limits_xref.exclude <> 'Y' OR
                      dbo.limits_xref.exclude IS NULL) AND (dbo.limits.ulim2 IS NOT NULL) OR
                      (dbo.limits_xref.exclude <> 'Y' OR
                      dbo.limits_xref.exclude IS NULL) AND (dbo.limits.ulim3 IS NOT NULL)
ORDER BY dbo.limits.testid

GO

