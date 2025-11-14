USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwTestRuleDefinition]    Script Date: 10/28/2025 11:38:25 AM ******/
DROP VIEW [dbo].[vwTestRuleDefinition]
GO

/****** Object:  View [dbo].[vwTestRuleDefinition]    Script Date: 10/28/2025 11:38:25 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwTestRuleDefinition]
AS
SELECT     dbo.vwTestRulesByTestID.GroupID, dbo.vwTestRulesByTestID.TestID, dbo.vwTestRulesByTestID.RuleTestID, dbo.vwTestRulesByTestID.TestAbbrev, 
                      dbo.vwTestRulesByTestID.TestName, dbo.TestScheduleRule.UpperRule, dbo.TestScheduleRule.RuleAction
FROM         dbo.vwTestRulesByTestID LEFT OUTER JOIN
                      dbo.TestScheduleRule ON dbo.vwTestRulesByTestID.GroupID = dbo.TestScheduleRule.GroupID AND 
                      dbo.vwTestRulesByTestID.RuleTestID = dbo.TestScheduleRule.RuleTestID AND dbo.vwTestRulesByTestID.TestID = dbo.TestScheduleRule.TestID

GO

