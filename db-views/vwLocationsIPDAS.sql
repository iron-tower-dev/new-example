USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwLocationsIPDAS]    Script Date: 10/28/2025 11:24:11 AM ******/
DROP VIEW [dbo].[vwLocationsIPDAS]
GO

/****** Object:  View [dbo].[vwLocationsIPDAS]    Script Date: 10/28/2025 11:24:11 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwLocationsIPDAS]
AS
SELECT DISTINCT 
                      s.siteId AS site_id, s.component AS lube_component_code, s.location AS lube_location_code, c.name AS comp_name, l.name AS loc_name
FROM         dbo.UsedLubeSamples s INNER JOIN
                      dbo.Location l ON s.siteId = l.site_id AND s.siteId = l.site_id AND s.location = l.code INNER JOIN
                      dbo.Component c ON s.component = c.code
WHERE (s.component IS NOT NULL) AND (s.location IS NOT NULL) AND (c.name IS NOT NULL) AND (l.name IS NOT NULL)
GO

