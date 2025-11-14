USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwLocationForUsed]    Script Date: 10/28/2025 11:23:56 AM ******/
DROP VIEW [dbo].[vwLocationForUsed]
GO

/****** Object:  View [dbo].[vwLocationForUsed]    Script Date: 10/28/2025 11:23:56 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwLocationForUsed]
AS
SELECT     TOP 100 PERCENT site_id, code, name
FROM         dbo.Location
WHERE     (code <> N'000' AND NOT (code LIKE N'U%'))
ORDER BY name



GO

