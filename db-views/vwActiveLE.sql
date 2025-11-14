USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwActiveLE]    Script Date: 10/28/2025 10:51:22 AM ******/
DROP VIEW [dbo].[vwActiveLE]
GO

/****** Object:  View [dbo].[vwActiveLE]    Script Date: 10/28/2025 10:51:22 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwActiveLE]
AS
SELECT DISTINCT 
                      TOP 100 PERCENT dbo.limits.limits_xref_id, dbo.limits.lcde, dbo.limits.testid, dbo.limits.exclude, dbo.Test.abbrev AS TestAbbrev, dbo.limits.testname, 
                      dbo.TestLELimit(dbo.limits.limits_xref_id, dbo.limits.testid, dbo.limits.testname, 'U') AS UpperLimit, dbo.TestLELimit(dbo.limits.limits_xref_id, 
                      dbo.limits.testid, dbo.limits.testname, 'L') AS LowerLimit
FROM         dbo.limits INNER JOIN
                      dbo.Test ON dbo.limits.testid = dbo.Test.ID
WHERE     (dbo.limits.exclude IS NULL OR
                      dbo.limits.exclude = 'N') AND (dbo.Test.exclude IS NULL OR
                      dbo.Test.exclude = 'N') AND (dbo.TestLELimit(dbo.limits.limits_xref_id, dbo.limits.testid, dbo.limits.testname, 'U') IS NOT NULL) OR
                      (dbo.limits.exclude IS NULL OR
                      dbo.limits.exclude = 'N') AND (dbo.Test.exclude IS NULL OR
                      dbo.Test.exclude = 'N') AND (dbo.TestLELimit(dbo.limits.limits_xref_id, dbo.limits.testid, dbo.limits.testname, 'L') IS NOT NULL) OR
                      (dbo.limits.exclude IS NULL OR
                      dbo.limits.exclude = 'N') AND (dbo.Test.exclude IS NULL OR
                      dbo.Test.exclude = 'N') AND (dbo.TestLELimit(dbo.limits.limits_xref_id, dbo.limits.testid, dbo.limits.testname, 'U') IS NOT NULL) AND 
                      (dbo.limits.testid = 220)
ORDER BY dbo.limits.testid, dbo.limits.testname

GO

