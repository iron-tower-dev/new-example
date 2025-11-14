USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwTestsForLab]    Script Date: 10/28/2025 11:40:47 AM ******/
DROP VIEW [dbo].[vwTestsForLab]
GO

/****** Object:  View [dbo].[vwTestsForLab]    Script Date: 10/28/2025 11:40:47 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwTestsForLab]
AS
SELECT     TOP 100 PERCENT ID AS TestID, name AS TestName, abbrev AS TestAbbrev, ShortAbbrev
FROM         dbo.Test
WHERE     (exclude IS NULL OR
                      exclude = 'N') AND (Lab = 1)
ORDER BY ID

GO

