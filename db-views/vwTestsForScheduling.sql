USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwTestsForScheduling]    Script Date: 10/28/2025 11:40:59 AM ******/
DROP VIEW [dbo].[vwTestsForScheduling]
GO

/****** Object:  View [dbo].[vwTestsForScheduling]    Script Date: 10/28/2025 11:40:59 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwTestsForScheduling]
AS
SELECT     ID AS TestID, name AS TestName, abbrev AS TestAbbrev
FROM         dbo.Test
WHERE     (Schedule = 1) AND (exclude IS NULL OR
                      exclude = 'N')

GO

