USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwTestRulesByTestID]    Script Date: 10/28/2025 11:38:50 AM ******/
DROP VIEW [dbo].[vwTestRulesByTestID]
GO

/****** Object:  View [dbo].[vwTestRulesByTestID]    Script Date: 10/28/2025 11:38:50 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwTestRulesByTestID]
AS
SELECT DISTINCT 
                      dbo.limits_xref.valueat AS GroupID, vwTestsForScheduling_1.TestID, dbo.vwTestsForScheduling.TestID AS RuleTestID, 
                      dbo.vwTestsForScheduling.TestAbbrev, dbo.vwTestsForScheduling.TestName
FROM         dbo.vwTestsForScheduling CROSS JOIN
                      dbo.limits_xref CROSS JOIN
                      dbo.vwTestsForScheduling vwTestsForScheduling_1

GO

