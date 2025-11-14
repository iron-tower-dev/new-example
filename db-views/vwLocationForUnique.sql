USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwLocationForUnique]    Script Date: 10/28/2025 11:23:44 AM ******/
DROP VIEW [dbo].[vwLocationForUnique]
GO

/****** Object:  View [dbo].[vwLocationForUnique]    Script Date: 10/28/2025 11:23:44 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwLocationForUnique]
AS
SELECT     TOP 100 PERCENT site_id, code, name
FROM         dbo.Location
WHERE     (code LIKE N'U%')
ORDER BY name



GO

