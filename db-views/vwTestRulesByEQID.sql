USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwTestRulesByEQID]    Script Date: 10/28/2025 11:38:38 AM ******/
DROP VIEW [dbo].[vwTestRulesByEQID]
GO

/****** Object:  View [dbo].[vwTestRulesByEQID]    Script Date: 10/28/2025 11:38:38 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwTestRulesByEQID]
AS
SELECT     dbo.limits_xref.tagNumber AS Tag, dbo.limits_xref.component AS ComponentCode, dbo.limits_xref.location AS LocationCode, 
                      dbo.vwActiveLimits.testid, dbo.vwActiveLimits.TestAbbrev, dbo.TestScheduleRule.RuleTestID, dbo.TestScheduleRule.UpperRule, 
                      dbo.TestScheduleRule.RuleAction, dbo.TestScheduleRule.GroupID, dbo.vwActiveLimits.UpperLimit, dbo.vwActiveLimits.LowerLimit
FROM         dbo.vwActiveLimits INNER JOIN
                      dbo.TestScheduleRule ON dbo.vwActiveLimits.limits_xref_id = dbo.TestScheduleRule.GroupID AND 
                      dbo.vwActiveLimits.testid = dbo.TestScheduleRule.TestID INNER JOIN
                      dbo.limits_xref ON dbo.vwActiveLimits.limits_xref_id = dbo.limits_xref.valueat
WHERE     (dbo.TestScheduleRule.RuleAction IS NOT NULL) AND (dbo.vwActiveLimits.exclude IS NULL OR
                      dbo.vwActiveLimits.exclude = 'N')

GO

