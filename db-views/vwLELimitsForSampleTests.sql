USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwLELimitsForSampleTests]    Script Date: 10/28/2025 10:59:48 AM ******/
DROP VIEW [dbo].[vwLELimitsForSampleTests]
GO

/****** Object:  View [dbo].[vwLELimitsForSampleTests]    Script Date: 10/28/2025 10:59:48 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwLELimitsForSampleTests]
AS
SELECT     dbo.UsedLubeSamples.ID, dbo.UsedLubeSamples.tagNumber, dbo.UsedLubeSamples.component, dbo.UsedLubeSamples.location, 
                      dbo.vwActiveLE.lcde, dbo.vwActiveLE.testid, dbo.vwActiveLE.TestAbbrev, dbo.vwActiveLE.testname, dbo.vwActiveLE.UpperLimit, 
                      dbo.vwActiveLE.LowerLimit
FROM         dbo.UsedLubeSamples INNER JOIN
                      dbo.limits_xref ON dbo.UsedLubeSamples.tagNumber = dbo.limits_xref.tagNumber AND 
                      dbo.UsedLubeSamples.component = dbo.limits_xref.component AND dbo.UsedLubeSamples.location = dbo.limits_xref.location INNER JOIN
                      dbo.vwActiveLE ON dbo.limits_xref.valueat = dbo.vwActiveLE.limits_xref_id

GO

