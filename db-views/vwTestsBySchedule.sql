USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwTestsBySchedule]    Script Date: 10/28/2025 11:39:32 AM ******/
DROP VIEW [dbo].[vwTestsBySchedule]
GO

/****** Object:  View [dbo].[vwTestsBySchedule]    Script Date: 10/28/2025 11:39:32 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwTestsBySchedule]
AS
SELECT     TOP 100 PERCENT dbo.TestSchedule.ID AS TestScheduleID, dbo.TestSchedule.Tag, dbo.TestSchedule.ComponentCode, 
                      dbo.TestSchedule.LocationCode, dbo.TestSchedule.Material, dbo.vwTestsForScheduling.TestID, dbo.vwTestsForScheduling.TestName, 
                      dbo.vwTestsForScheduling.TestAbbrev
FROM         dbo.TestSchedule CROSS JOIN
                      dbo.vwTestsForScheduling
ORDER BY dbo.TestSchedule.ID

GO

