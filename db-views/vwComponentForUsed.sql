USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwComponentForUsed]    Script Date: 10/28/2025 10:53:06 AM ******/
DROP VIEW [dbo].[vwComponentForUsed]
GO

/****** Object:  View [dbo].[vwComponentForUsed]    Script Date: 10/28/2025 10:53:06 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwComponentForUsed]
AS
SELECT     TOP 100 PERCENT site_id, code, name
FROM         dbo.Component
WHERE     (code <> N'000') AND (NOT (code LIKE N'U%'))
ORDER BY name



GO

