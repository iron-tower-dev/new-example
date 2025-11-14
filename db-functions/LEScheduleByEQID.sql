USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwLEScheduleByEQID]    Script Date: 10/28/2025 11:22:49 AM ******/
DROP VIEW [dbo].[vwLEScheduleByEQID]
GO

/****** Object:  View [dbo].[vwLEScheduleByEQID]    Script Date: 10/28/2025 11:22:49 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vwLEScheduleByEQID]
AS
SELECT DISTINCT TOP 100 PERCENT dbo.limits_xref.tagNumber, dbo.limits_xref.component, dbo.limits_xref.location, dbo.limits.testid
FROM         dbo.limits INNER JOIN
                      dbo.limits_xref ON dbo.limits.limits_xref_id = dbo.limits_xref.valueat INNER JOIN
                      dbo.vwTestsForLab ON dbo.limits.testid = dbo.vwTestsForLab.TestID
WHERE     (dbo.limits_xref.exclude IS NULL OR
                      dbo.limits_xref.exclude = 'N') AND (dbo.limits.exclude IS NULL OR
                      dbo.limits.exclude = 'N') AND (dbo.limits.lcde = 'Y')
ORDER BY dbo.limits_xref.tagNumber, dbo.limits_xref.component, dbo.limits_xref.location, dbo.limits.testid

GO

