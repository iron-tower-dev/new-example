USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwGroupSamples]    Script Date: 10/28/2025 11:21:01 AM ******/
DROP VIEW [dbo].[vwGroupSamples]
GO

/****** Object:  View [dbo].[vwGroupSamples]    Script Date: 10/28/2025 11:21:01 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE VIEW [dbo].[vwGroupSamples]
AS
SELECT DISTINCT 
                      dbo.limits_xref.group_name, dbo.limits_xref.valueat, dbo.UsedLubeSamples.tagNumber, dbo.UsedLubeSamples.component, 
                      dbo.UsedLubeSamples.location, dbo.UsedLubeSamples.ID AS SampleID, dbo.UsedLubeSamples.sampleDate
FROM         dbo.limits_xref INNER JOIN
                      dbo.UsedLubeSamples ON dbo.limits_xref.tagNumber = dbo.UsedLubeSamples.tagNumber AND 
                      dbo.limits_xref.component = dbo.UsedLubeSamples.component AND dbo.limits_xref.location = dbo.UsedLubeSamples.location
WHERE     (dbo.limits_xref.exclude IS NULL)


GO

