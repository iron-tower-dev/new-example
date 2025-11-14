USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwFieldRecords]    Script Date: 10/28/2025 10:56:19 AM ******/
DROP VIEW [dbo].[vwFieldRecords]
GO

/****** Object:  View [dbo].[vwFieldRecords]    Script Date: 10/28/2025 10:56:19 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO






CREATE VIEW [dbo].[vwFieldRecords]
AS
SELECT DISTINCT 
                         TOP (100) PERCENT dbo.SWMSRecords.tagNumber, dbo.Component.name AS Component, dbo.Location.name AS Location, dbo.SWMSRecords.scheduledDate, dbo.SWMSRecords.woNumber, dbo.SWMSRecords.[description], DATEDIFF(day, dbo.SWMSRecords.[scheduledDate], GETDATE()) DaysOverdue
FROM            dbo.SWMSRecords INNER JOIN
                dbo.Component ON dbo.SWMSRecords.component = dbo.Component.code INNER JOIN
                dbo.Location ON dbo.SWMSRecords.location = dbo.Location.code LEFT OUTER JOIN
                dbo.UsedLubeSamples ON dbo.SWMSRecords.tagNumber = dbo.UsedLubeSamples.tagNumber AND 
                dbo.SWMSRecords.component = dbo.UsedLubeSamples.component AND dbo.SWMSRecords.location = dbo.UsedLubeSamples.location AND 
                dbo.SWMSRecords.woNumber = dbo.UsedLubeSamples.woNumber
WHERE        dbo.UsedLubeSamples.ID IS NULL
ORDER BY dbo.SWMSRecords.scheduledDate DESC





GO

