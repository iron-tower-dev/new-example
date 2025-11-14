USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwTestScheduleDefinition]    Script Date: 10/28/2025 11:40:04 AM ******/
DROP VIEW [dbo].[vwTestScheduleDefinition]
GO

/****** Object:  View [dbo].[vwTestScheduleDefinition]    Script Date: 10/28/2025 11:40:04 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwTestScheduleDefinition]
AS
SELECT     dbo.vwTestsBySchedule.TestScheduleID, dbo.vwTestsBySchedule.TestID, dbo.vwTestsBySchedule.TestName, dbo.vwTestsBySchedule.TestAbbrev, 
                      dbo.vwTestsBySchedule.Tag, dbo.vwTestsBySchedule.ComponentCode, dbo.vwTestsBySchedule.LocationCode, dbo.vwTestsBySchedule.Material, 
                      dbo.TestScheduleTest.TestInterval, dbo.TestScheduleTest.MinimumInterval, dbo.TestScheduleTest.DuringMonth, dbo.TestScheduleTest.Details
FROM         dbo.TestScheduleTest RIGHT OUTER JOIN
                      dbo.vwTestsBySchedule ON dbo.TestScheduleTest.TestID = dbo.vwTestsBySchedule.TestID AND 
                      dbo.TestScheduleTest.TestScheduleID = dbo.vwTestsBySchedule.TestScheduleID

GO

