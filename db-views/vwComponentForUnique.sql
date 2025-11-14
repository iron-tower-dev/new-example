USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwComponentForUnique]    Script Date: 10/28/2025 10:52:49 AM ******/
DROP VIEW [dbo].[vwComponentForUnique]
GO

/****** Object:  View [dbo].[vwComponentForUnique]    Script Date: 10/28/2025 10:52:49 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwComponentForUnique]
AS
SELECT     TOP 100 PERCENT site_id, code, name
FROM         dbo.Component
WHERE     (code LIKE N'U%')
ORDER BY name



GO

