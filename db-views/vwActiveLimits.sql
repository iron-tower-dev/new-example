USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwActiveLimits]    Script Date: 10/28/2025 10:52:02 AM ******/
DROP VIEW [dbo].[vwActiveLimits]
GO

/****** Object:  View [dbo].[vwActiveLimits]    Script Date: 10/28/2025 10:52:02 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwActiveLimits]
AS
SELECT     dbo.limits.limits_xref_id, dbo.limits.testid, dbo.limits.exclude, dbo.Test.abbrev AS TestAbbrev, dbo.TestScheduleLimit(dbo.limits.limits_xref_id, 
                      dbo.limits.testid, 'U') AS UpperLimit, dbo.TestScheduleLimit(dbo.limits.limits_xref_id, dbo.limits.testid, 'L') AS LowerLimit
FROM         dbo.limits INNER JOIN
                      dbo.Test ON dbo.limits.testid = dbo.Test.ID
WHERE     (dbo.limits.exclude IS NULL OR
                      dbo.limits.exclude = 'N') AND (dbo.Test.exclude IS NULL OR
                      dbo.Test.exclude = 'N') AND (dbo.limits.tl1 IS NOT NULL) AND (dbo.TestScheduleLimit(dbo.limits.limits_xref_id, dbo.limits.testid, 'U') IS NOT NULL) OR
                      (dbo.limits.exclude IS NULL OR
                      dbo.limits.exclude = 'N') AND (dbo.Test.exclude IS NULL OR
                      dbo.Test.exclude = 'N') AND (dbo.limits.tl1 IS NOT NULL) AND (dbo.TestScheduleLimit(dbo.limits.limits_xref_id, dbo.limits.testid, 'L') IS NOT NULL)

GO

