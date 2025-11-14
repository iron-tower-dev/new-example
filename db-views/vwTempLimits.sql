USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwTempLimits]    Script Date: 10/28/2025 11:36:26 AM ******/
DROP VIEW [dbo].[vwTempLimits]
GO

/****** Object:  View [dbo].[vwTempLimits]    Script Date: 10/28/2025 11:36:26 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwTempLimits]
AS
SELECT     dbo.vwGroupSamples.group_name, dbo.vwGroupSamples.valueat, dbo.vwGroupSamples.tagNumber, dbo.vwGroupSamples.component, 
                      dbo.vwGroupSamples.location, dbo.vwGroupSamples.SampleID, dbo.vwGroupSamples.sampleDate, dbo.limits.ulim3
FROM         dbo.vwGroupSamples INNER JOIN
                      dbo.limits ON dbo.vwGroupSamples.valueat = dbo.limits.limits_xref_id
WHERE     (dbo.limits.testid = 400)

GO

